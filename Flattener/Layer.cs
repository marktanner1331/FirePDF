using System.Collections.Generic;
using System.Drawing;

namespace Flattener
{
    public class Layer
    {
        public enum LayerType
        {
            Raster,
            Vector
        }

        public class LayerElement
        {
            public enum ElementType
            {
                Image,
                Text,
                StrokePath,
                FillPath,
                FillAndStrokePath
            }

            public ElementType type;

            public object element;

            /// <summary>
            /// bounding box is in device space
            /// </summary>
            public RectangleF boundingBox;
            public FirePDF.Model.GraphicsState graphicsState;
        }

        public readonly LayerType type;
        public readonly List<LayerElement> elements;

        public Layer(LayerType type)
        {
            this.type = type;
            elements = new List<LayerElement>();
        }

        internal bool DoesIntersect(RectangleF boundingBox)
        {
            foreach(LayerElement element in elements)
            {
                if(element.boundingBox.IntersectsWith(boundingBox))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddElement(LayerElement element)
        {
            elements.Add(element);
        }
    }
}
