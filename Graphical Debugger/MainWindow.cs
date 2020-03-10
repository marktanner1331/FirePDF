using FirePDF;
using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.Reading;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    public partial class MainWindow : Form
    {
        private readonly SplitContainer splitter;

        private readonly PdfRenderer pdfRenderer;
        private readonly OperationListBox listBox;

        private IStreamOwner owner;
        private List<Operation> operations;

        public MainWindow()
        {
            BackColor = Color.Black;

            InitializeComponent();
            Text = "FirePDF Graphical Debugger";

            splitter = new SplitContainer();
            splitter.Size = ClientSize;
            splitter.SplitterDistance = 200;
            Controls.Add(splitter);
            
            pdfRenderer = new PdfRenderer();
            splitter.Panel2.Controls.Add(pdfRenderer);
            pdfRenderer.Size = splitter.Panel2.ClientSize;

            listBox = new OperationListBox();
            splitter.Panel1.Controls.Add(listBox);
            splitter.SplitterMoved += Splitter_SplitterMoved;
            listBox.OnCheckChanged += ListBox_onCheckChanged;
            listBox.Size = splitter.Panel1.ClientSize;
            
            LoadPdf(@"C:\Users\Mark Tanner\scratch\press herald 2020-03-09\3.Pdf");
        }

        private void ListBox_onCheckChanged()
        {
            pdfRenderer.Render(owner, listBox.GetCheckedMap());
        }

        private void LoadPdf(string fullPathToFile)
        {
            Pdf pdf = new Pdf(fullPathToFile);

            Page page = pdf.GetPage(1);
            owner = page;
            StreamCollector collector = new StreamCollector();
            RecursiveStreamReader reader = new RecursiveStreamReader(collector);

            reader.ReadStreamRecursively(page);
            operations = collector.operations;
            
            listBox.SetOperations(operations);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            if(splitter != null)
            {
                splitter.Size = ClientSize;
            }
        }

        private void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            listBox.Size = splitter.Panel1.ClientSize;
            pdfRenderer.Size = splitter.Panel2.ClientSize;
        }
    }
}
