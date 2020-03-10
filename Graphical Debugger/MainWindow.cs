using FirePDF;
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
        private OperationListBox listBox;

        private IStreamOwner owner;
        private List<Operation> operations;

        public MainWindow()
        {
            this.BackColor = Color.Black;

            InitializeComponent();
            Text = "FirePDF Graphical Debugger";

            splitter = new SplitContainer();
            splitter.Size = ClientSize;
            splitter.SplitterDistance = 200;
            Controls.Add(splitter);
            
            pdfRenderer = new PDFRenderer();
            splitter.Panel2.Controls.Add(pdfRenderer);
            pdfRenderer.Size = splitter.Panel2.ClientSize;

            listBox = new OperationListBox();
            splitter.Panel1.Controls.Add(listBox);
            splitter.SplitterMoved += Splitter_SplitterMoved;
            listBox.onCheckChanged += ListBox_onCheckChanged;
            listBox.Size = splitter.Panel1.ClientSize;
            
            loadPDF(@"C:\Users\Mark Tanner\scratch\press herald 2020-03-09\3.pdf");
        }

        private void ListBox_onCheckChanged()
        {
            pdfRenderer.render(owner, listBox.getCheckedMap());
        }

        private void loadPDF(string fullPathToFile)
        {
            PDF pdf = new PDF(fullPathToFile);

            Page page = pdf.getPage(1);
            owner = page;
            StreamCollector collector = new StreamCollector();
            RecursiveStreamReader reader = new RecursiveStreamReader(collector);

            reader.readStreamRecursively(page);
            operations = collector.operations;
            
            listBox.setOperations(operations);
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
