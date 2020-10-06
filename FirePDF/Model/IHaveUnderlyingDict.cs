using FirePDF.Text;
using System;
using System.Collections.Generic;

namespace FirePDF.Model
{
    public class HaveUnderlyingDict : HavePdf, IHaveChildren
    {
        public PdfDictionary UnderlyingDict { get; protected set; }

        public HaveUnderlyingDict(PdfDictionary underlyingDict) : base(underlyingDict.Pdf)
        {
            UnderlyingDict = underlyingDict;
        }

        internal HaveUnderlyingDict() : base(null)
        {

        }

        public virtual bool IsDirty() => UnderlyingDict.IsDirty();

        public static HaveUnderlyingDict FromDictionary(PdfDictionary dict)
        {
            //TODO: the underlyingDict will store a reference to the Pdf, so w don''t need to pass it in here
            switch (dict.Get<Name>("Type"))
            {
                case "Font":
                    return Font.LoadExistingFontFromPdf(dict);
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
                    return new PdfAnnot(dict);
                case "Outlines":
                    return new PdfOutlines(dict);
                case "StructTreeRoot":
                    return new StructTreeRoot(dict);
                case "Group":
                    return new PdfGroup(dict);
                case "Encoding":
                    return new PDFEncoding(dict);
                case "Metadata":
                    return new PdfMetaDataDictionary(dict);
                case "Mask":
                    return new Mask(dict);
                case "Pattern":
                    return new PatternDictionary(dict);
                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerable<ObjectReference> GetObjectReferences() => UnderlyingDict.GetObjectReferences();

        public void SwapReferences(Func<ObjectReference, ObjectReference> callback) => UnderlyingDict.SwapReferences(callback);
    }
}
