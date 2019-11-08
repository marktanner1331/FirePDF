using FirePDF;
using FirePDF.Model;
using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flattener
{
    class Splitter : IRenderer
    {
        //concept is layers
        //that alternate by type
        //and we try and add the element to the lowest layer possible of the same type
        //we can add it to a layer if nothing in the layers above it intersect it
        //  for both raster and vector

        private List<Layer> layers;

        public Splitter()
        {
            layers = new List<Layer>();

            //we start with a vector layer just for fun
            //as its probably the first thing that will be added to the page
            //but a vector layer would work just as well
            //if this layer ends up not getting used then its fine as it will be cleaned up before exporting
            layers.Add(new Layer(Layer.LayerType.vector));
        }

        public void exportAsSinglePage()
        {
            PDF pdf = new PDF();
        }

        private void addElementToLayers(Layer.LayerType layerType, Layer.LayerElement element)
        {
            int targetLayerIndex = 0;

            //we do an i > 0 here because there is no point checking if we intersect the bottom layer
            //regardless of whether we do or not we can add it
            for (int i = layers.Count - 1; i > 0; i--)
            {
                Layer layer = layers[i];
                if (layer.doesIntersect(element.boundingBox))
                {
                    //if we intersect then we can't put on a lower layer
                    targetLayerIndex = i;
                    break;
                }
            }
            
            //we can either put it on the current layer if the layer types match
            //or the layer above if they dont match
            if (layers[targetLayerIndex].type != layerType)
            {
                targetLayerIndex++;
            }

            //check that the layer already exists, if it doesn't then create it
            if(layers.Count == targetLayerIndex)
            {
                layers.Add(new Layer(layerType));
            }

            //finally, now that we have found the correct layer, we can add the element to it
            layers[targetLayerIndex].addElement(element);
        }

        public override void drawImage(XObjectImage image)
        {
            FirePDF.Model.GraphicsState gs = getGraphicsState();

            Matrix temp = gs.currentTransformationMatrix;

            //i still don't understand how the negative vertical space works
            //im also not sure it matters in this case
            //as we are simply checking if bounds interect (in addElementToLayers())
            //so as long as every element has the same negative/positive vertical transform
            //it will be ok (i think)

            //temp.Scale(1, -1);
            //temp.Translate(0, -1);

            PointF[] points = new PointF[] { new PointF(0, 0), new PointF(1, 1) };
            temp.TransformPoints(points);

            //the ctm might have rotated us
            //so we need to find the bounding box from the points the hard way
            float left = Math.Min(points[0].X, points[1].X);
            float right = Math.Max(points[0].X, points[1].X);
            float top = Math.Min(points[0].Y, points[1].Y);
            float bottom = Math.Max(points[0].Y, points[1].Y);

            RectangleF bounds = new RectangleF(left, top, right - left, bottom - top);

            addElementToLayers(Layer.LayerType.raster, new Layer.LayerElement
            {
                boundingBox = bounds,
                element = image,
                graphicsState = gs,
                type = Layer.LayerElement.ElementType.Image
            });
        }

        public override void drawText(byte[] text)
        {
            FirePDF.Model.GraphicsState gs = getGraphicsState();
            FirePDF.Model.Font font = gs.font;
            Matrix temp = gs.currentTransformationMatrix;

            SizeF size = font.measureText(text, gs);
            
            PointF[] points = new PointF[] {
                new PointF(gs.textMatrix.Elements[4], gs.textMatrix.Elements[5]),
                new PointF(gs.textMatrix.Elements[4] + size.Width, gs.textMatrix.Elements[5] + size.Height) };

            temp.TransformPoints(points);

            //the ctm might have rotated us
            //so we need to find the bounding box from the points the hard way
            float left = Math.Min(points[0].X, points[1].X);
            float right = Math.Max(points[0].X, points[1].X);
            float top = Math.Min(points[0].Y, points[1].Y);
            float bottom = Math.Max(points[0].Y, points[1].Y);

            addElementToLayers(Layer.LayerType.vector, new Layer.LayerElement
            {
                boundingBox = new RectangleF(left, top, right - left, bottom - top),
                element = text,
                graphicsState = gs,
                type = Layer.LayerElement.ElementType.Text
            });
        }
        
        public override void fillAndStrokePath(GraphicsPath path)
        {
            FirePDF.Model.GraphicsState gs = getGraphicsState();

            addElementToLayers(Layer.LayerType.vector, new Layer.LayerElement
            {
                boundingBox = path.GetBounds(gs.currentTransformationMatrix),
                element = path,
                graphicsState = gs,
                type = Layer.LayerElement.ElementType.FillAndStrokePath
            });
        }

        public override void fillPath(GraphicsPath path)
        {
            FirePDF.Model.GraphicsState gs = getGraphicsState();

            addElementToLayers(Layer.LayerType.vector, new Layer.LayerElement
            {
                boundingBox = path.GetBounds(gs.currentTransformationMatrix),
                element = path,
                graphicsState = gs,
                type = Layer.LayerElement.ElementType.FillPath
            });
        }

        public override void strokePath(GraphicsPath path)
        {
            FirePDF.Model.GraphicsState gs = getGraphicsState();

            addElementToLayers(Layer.LayerType.vector, new Layer.LayerElement
            {
                boundingBox = path.GetBounds(gs.currentTransformationMatrix),
                element = path,
                graphicsState = gs,
                type = Layer.LayerElement.ElementType.StrokePath
            });
        }
    }
}
