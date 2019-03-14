using FirePDF.Model;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class PDFResources
    {
        private PDF pdf;
        private Dictionary<string, object> underlyingDict;

        public PDFResources(PDF pdf, Dictionary<string, object> underlyingDict)
        {
            this.pdf = pdf;
            this.underlyingDict = underlyingDict;
        }

        /// <summary>
        /// returns the object at the given path, or null if it cannot be found
        /// automatically resolves any indirect references before returning them
        /// </summary>
        public object getObjectAtPath(params string[] path)
        {
            object root = underlyingDict;
            foreach (string part in path)
            {
                if (root is Dictionary<string, object>)
                {
                    Dictionary<string, object> temp = (Dictionary<string, object>)root;
                    if (temp.ContainsKey(part))
                    {
                        root = temp[part];
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (root is List<object>)
                {
                    int index;
                    if (int.TryParse(part, out index) == false)
                    {
                        return null;
                    }

                    List<object> temp = (List<object>)root;
                    if (temp.Count <= index)
                    {
                        return null;
                    }

                    root = temp[index];
                }

                if (root is ObjectReference)
                {
                    root = PDFReaderLayer1.readIndirectObject(pdf, (ObjectReference)root);
                }
            }

            return root;
        }
    }
}
