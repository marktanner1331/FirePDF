using FirePDF.Model;
using FirePDF.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace FirePDF.Distilling
{
    class TextDistiller
    {
        public static void removeAllTextFromPage(Page page)
        {
            Distiller.removeOperationsFromPage(page, (x, resources) =>
            {
                return TextProcessor.isTextOperation(x);
            });

            foreach (IStreamOwner streamOwner in page.listIStreamOwners())
            {
                List<Name> fontNames = streamOwner.Resources.ListFontResourceNames();
                foreach (Name fontName in fontNames)
                {
                    streamOwner.Resources.removeFont(fontName);
                }
            }
        }
    }
}
