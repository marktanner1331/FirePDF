# FirePDF

FirePDF is the only library that gives you proper low level access to PDF's for reading and writing. 

## Basic editing
Splitting pages into separate files:
```
using (Pdf pdf = new Pdf(inputFile))
{
    int i = 1;
    foreach (Page page in pdf)
    {
        Console.WriteLine($"Running for page {i} of {pdf.NumPages()}");

        using (Pdf newPdf = new Pdf())
        {
            newPdf.AddPage(page);
            newPdf.Save(outputFolder + i + ".Pdf");
        }

        i++;
    }
}
```

## Accessing PDF Structures
You can also get access to the internal dictionaries. For example, we can find all the masks:
```
using (Pdf pdf = new Pdf(file))
{
    HashSet<ObjectReference> maskRefs = new HashSet<ObjectReference>();

    Page page = pdf.GetPage(1);
    foreach (IStreamOwner owner in page.listIStreamOwners())
    {
        foreach (ExtGState state in owner.Resources.GetAllExtGStates())
        {
            if (state.HasSoftMask(out SoftMask mask))
            {
                ObjectReference maskRef = pdf.ReverseGet(mask);
                maskRefs.Add(maskRef);
            }
        }
    }
}
```

## Modifying streams
You can even modify the underlying operation streams:
```
using (Pdf pdf = new Pdf(file))
{
    Page page = pdf.GetPage(1);
    var k = page.listIStreamOwners();

    foreach (var g in k)
    {
        using (Stream s = g.GetStream())
        {
            List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(pdf, s);
            int originalSize = operations.Count;

            operations = operations.Where(x =>
            {
                if (x.operatorName == "scn" && x.operands[0] is Name && (Name)x.operands[0] == "R15")
                {
                    return false;
                }
                
                if (x.operatorName == "cs" && x.operands[0] is Name && (Name)x.operands[0] == "R16")
                {
                    return false;
                }

                return true;
            }).ToList();

            if (originalSize == operations.Count)
            {
                continue;
            }

            //gotta keep this stream open until we hit save
            MemoryStream ms = new MemoryStream();

            ContentStreamWriter.WriteOperationsToStream(ms, operations);
            ms.Seek(0, SeekOrigin.Begin);
            
            if (g is XObjectForm form)
            {
                form.UpdateStream(ms);
            }
        }
    }

    pdf.Save(@"", SaveType.Fresh);
}
```

## Saving
FirePDF automatically tracks what changes have been made, and can either save just those changes, or resave the entire PDF as fresh.
