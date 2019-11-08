using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flattener
{
    class Layer
    {
        public enum LayerType
        {
            raster,
            vector
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
            this.elements = new List<LayerElement>();
        }

        internal bool doesIntersect(RectangleF boundingBox)
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

        public void addElement(LayerElement element)
        {
            elements.Add(element);
        }
    }
}
