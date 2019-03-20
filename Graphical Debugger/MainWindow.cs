using FirePDF;
using FirePDF.Distilling;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using FirePDF.Rendering;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    public partial class MainWindow : Form
    {
        private SplitContainer splitter;

        private PDFRenderer pdfRenderer;
        private ListBox listBox;

        private IStreamOwner owner;
        private List<Operation> operations;

        public MainWindow()
        {
            InitializeComponent();
            Text = "FirePDF Graphical Debugger";

            splitter = new SplitContainer();
            splitter.Size = ClientSize;
            splitter.SplitterDistance = 200;
            Controls.Add(splitter);

            string file = @"C:\Users\Mark Tanner\scratch\kuier 1.pdf";
            //string file = @"C:\Users\Mark Tanner\scratch\page 2.pdf";
            PDF pdf = new PDF(file);

            Page page = pdf.getPage(1);

            List<XObjectForm> forms = page.resources.getXObjectForms().ToList();
            XObjectForm form = forms.First();
            owner = form;

            Stream s = form.readContentStream();
            operations = ContentStreamReader.readOperationsFromStream(s);

            StreamTree tree = new StreamTree(operations);
            StreamTreeClassifier.classifyStreamTree(form, tree);
            tree.removeLeafNodes(x => x.variables["type"] == "clippingPath");
            operations = tree.convertToOperations();

            pdfRenderer = new PDFRenderer();
            pdfRenderer.render(form, operations.Take(0));
            splitter.Panel2.Controls.Add(pdfRenderer);
            pdfRenderer.Size = splitter.Panel2.ClientSize;

            listBox = new ListBox();
            splitter.Panel1.Controls.Add(listBox);
            splitter.SplitterMoved += Splitter_SplitterMoved;
            listBox.Size = splitter.Panel1.ClientSize;
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            foreach(Operation operation in operations)
            {
                listBox.Items.Add(operation);
            }
        }

        private void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            listBox.Size = splitter.Panel1.ClientSize;
            pdfRenderer.Size = splitter.Panel2.ClientSize;
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox.SelectedIndex;
            pdfRenderer.render(owner, operations.Take(index + 1));
        }
    }
}
