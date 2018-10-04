using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF
{
    public class Page
    {
        private PDF pdf;
        private Dictionary<string, object> underlyingDict;

        public Page(PDF pdf)
        {
            this.pdf = pdf;
        }

        public void fromDictionary(Dictionary<string, object> dict)
        {
            this.underlyingDict = dict;
        }

        public IEnumerable<Image> getImages()
        {
            if(underlyingDict.ContainsKey("Resources") == false)
            {
                //resources are inherited from tree
                throw new NotImplementedException();
            }

            Dictionary<string, object> resources = (Dictionary<string, object>)underlyingDict["Resources"];
            if(resources.ContainsKey("XObject") == false)
            {
                yield break;
            }

            Dictionary<string, object> xObjects = (Dictionary<string, object>)resources["XObject"];
            foreach(ObjectReference objectReference in xObjects.Values)
            {
                Dictionary<string, object> xObjectDict = PDFReaderLevel1.readIndirectDictionary(pdf, objectReference);
                if((string)xObjectDict["Subtype"] != "Image")
                {
                    continue;
                }

                XObjectImage xObject = new XObjectImage(pdf);
                xObject.fromDictionary(xObjectDict);

                yield return xObject.getImage();
            }
        }
    }
}
