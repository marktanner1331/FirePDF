using FirePDF.Model;
using FirePDF.Writing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FirePDF
{
    public class Page : HaveUnderlyingDict, IStreamOwner
    {
        public PdfResources Resources { get; }
        public RectangleF BoundingBox { get; }

        //TODO: do we need to check if the underlying dict is dirty too?
        public override bool IsDirty() => UnderlyingDict.IsDirty() || Resources.IsDirty();
        public bool IsOrphan { get; internal set; }

        Pdf IStreamOwner.Pdf => Pdf;

        public Page(Pdf owner, Size pageSize) : base(new PdfDictionary(owner, new Dictionary<Name, object>()))
        {
            IsOrphan = true;

            Resources = new PdfResources(new PdfDictionary(Pdf));
            UnderlyingDict.Set("Resources", Pdf.store.Add(Resources.UnderlyingDict));

            UnderlyingDict.Set("Type", (Name)"Page");
            UnderlyingDict.Set("MediaBox", new PdfList(Pdf, 0, 0, pageSize.Width, pageSize.Height));
        }

        internal Page(PdfDictionary pageDictionary) : this(pageDictionary, false) { }

        internal Page(PdfDictionary pageDictionary, bool isOrphan) : base(pageDictionary)
        {
            Resources = new PdfResources(UnderlyingDict.Get<PdfDictionary>("Resources"));
            IsOrphan = isOrphan;
            BoundingBox = UnderlyingDict.Get<PdfList>("MediaBox").AsRectangle();
        }

        public IEnumerable<ObjectReference> ListImages(bool includeFormXObjects)
        {
            HashSet<ObjectReference> forms = new HashSet<ObjectReference>();
            HashSet<ObjectReference> images = new HashSet<ObjectReference>();

            void processResources(PdfResources resources)
            {
                foreach(ObjectReference imageReference in resources.ListImages())
                {
                    images.Add(imageReference);
                }

                if(includeFormXObjects)
                {
                    foreach (ObjectReference formReference in resources.ListFormXObjects())
                    {
                        if (forms.Contains(formReference))
                        {
                            continue;
                        }

                        forms.Add(formReference);

                        XObjectForm form = formReference.Get<XObjectForm>();
                        processResources(form.Resources);
                    }
                }
            };

            processResources(this.Resources);

            return images;
        }

        /// <summary>
        /// returns references to all IStreamOwners used on the page
        /// this includes the page itself + any form xObjects
        /// </summary>
        public IEnumerable<IStreamOwner> listIStreamOwners()
        {
            HashSet<IStreamOwner> streamOwners = new HashSet<IStreamOwner>();
            streamOwners.Add(this);

            HashSet<ObjectReference> processedForms = new HashSet<ObjectReference>();

            void processResources(PdfResources resources)
            {
                foreach (ObjectReference formReference in resources.ListFormXObjects())
                {
                    if (processedForms.Contains(formReference))
                    {
                        continue;
                    }

                    processedForms.Add(formReference);

                    XObjectForm form = formReference.Get<XObjectForm>();
                    streamOwners.Add(form);

                    processResources(form.Resources);
                }
            };

            return streamOwners;
        }

        /// <summary>
        /// returns a list of references to the  content streams for the page
        /// </summary>
        /// <param name="includeFormXObjects">When set to true, the form XObject streams will also be returned</param>
        public IEnumerable<ObjectReference> ListContentStreams(bool includeFormXObjects)
        {
            HashSet<ObjectReference> refs = new HashSet<ObjectReference>();

            object contents = UnderlyingDict.Get("Contents", false);
            if (contents is ObjectReference)
            {
                refs.Add(contents as ObjectReference);
            }
            else if (contents is PdfList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    refs.Add(list.Get<ObjectReference>(i, false));
                }
            }
            else
            {
                throw new Exception();
            }

            if(includeFormXObjects == false)
            {
                return refs;
            }

            void processResources(PdfResources resources)
            {
                foreach(ObjectReference formReference in resources.ListFormXObjects())
                {
                    if(refs.Contains(formReference))
                    {
                        continue;
                    }

                    refs.Add(formReference);

                    XObjectForm form = formReference.Get<XObjectForm>();
                    processResources(form.Resources);
                }
            };

            processResources(this.Resources);

            return refs;
        }

        public ObjectReference Serialize(PdfWriter writer)
        {
            throw new Exception("not handling saving yet");
            //if (resources.IsDirty)
            //{
            //    foreach (string dirtyObjectPath in resources.dirtyObjects)
            //    {
            //        string[] path = dirtyObjectPath.Split('/');
            //        object obj = resources.getObjectAtPath(path);

            //        ObjectReference objectRef = writer.writeIndirectObjectUsingNextFreeNumber(obj);
            //        resources.setObjectAtPath(objectRef, path);
            //    }

            //    resources.dirtyObjects.Clear();

            //    underlyingDict["Resources"] = resources.underlyingDict;
            //}

            //return writer.writeIndirectObjectUsingNextFreeNumber(underlyingDict);
        }

        public Stream GetStream()
        {
            MemoryStream compositeStream = new MemoryStream();

            object contents = UnderlyingDict.Get("Contents", true);

            if (contents is PdfStream)
            {
                using (Stream stream = (contents as PdfStream).GetDecompressedStream())
                {
                    stream.CopyTo(compositeStream);
                }
            }
            else if (contents is PdfList)
            {
                foreach (PdfStream pdfStream in (contents as PdfList).Cast<PdfStream>())
                {
                    using (Stream stream = pdfStream.GetDecompressedStream())
                    {
                        stream.CopyTo(compositeStream);
                    }
                }
            }
            else
            {
                throw new Exception();
            }

            compositeStream.Position = 0;
            return compositeStream;
        }
    }
}
