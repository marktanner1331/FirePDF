using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class IHaveUnderlyingDict : IHavePDF, IHaveChildren
    {
        public PDFDictionary underlyingDict { get; protected set; }

        public IHaveUnderlyingDict(PDFDictionary underlyingDict) : base(underlyingDict.pdf)
        {
            this.underlyingDict = underlyingDict;
        }

        internal IHaveUnderlyingDict() : base(null)
        {

        }

        public virtual bool isDirty() => underlyingDict.isDirty();

        public static IHaveUnderlyingDict fromDictionary(PDFDictionary dict)
        {
            //TODO: the underlyingDict will store a reference to the pdf, so w dont need to pass it in here
            switch (dict.get<Name>("Type"))
            {
                case "Font":
                    return Model.Font.loadExistingFontFromPDF(dict);
                case "Catalog":
                    return new Catalog(dict);
                case "Pages":
                    return new PageTreeNode(dict);
                case "Page":
                    return new Page(dict, false);
                case "FontDescriptor":
                    return new FontDescriptor(dict);
                case "ExtGState":
                    return new ExtGState(dict);
                case "Annot":
                    return new PDFAnnot(dict);
                case "Outlines":
                    return new PDFOutlines(dict);
                case "StructTreeRoot":
                    return new StructTreeRoot(dict);
                case "Group":
                    return new PDFGroup(dict);
                case "Encoding":
                    return new PDFEncoding(dict);
                case "Metadata":
                    return new PDFMetaDataDictionary(dict);
                case "Mask":
                    return new Mask(dict);
                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerable<ObjectReference> GetObjectReferences() => underlyingDict.GetObjectReferences();

        public void swapReferences(Func<ObjectReference, ObjectReference> callback) => underlyingDict.swapReferences(callback);
    }
}
