using FirePDF;
using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BosEdit
{
    public partial class MainWindow : Form
    {
        private DataGridView objectsBox = new DataGridView();
        private ObjectView objectView;
        private Pdf pdf;

        public MainWindow()
        {
            InitializeComponent();
            this.Text = "BosEdit";

            this.MainMenuStrip = new MenuStrip();
            ToolStripItem open = MainMenuStrip.Items.Add("Open");
            open.Click += Open_Click;

            ToolStripItem save = MainMenuStrip.Items.Add("Save");
            save.Click += Save_Click;

            MainMenuStrip.Items.Add(new ToolStripSeparator());
            
            ToolStripItem back = MainMenuStrip.Items.Add("Back");
            back.Click += Back_Click;

            Controls.Add(this.MainMenuStrip);

            objectsBox = new DataGridView();
            objectsBox.ColumnCount = 3;
            objectsBox.Columns[0].Name = "Obj Num";
            objectsBox.Columns[1].Name = "Gen Num";
            objectsBox.Columns[2].Name = "Offset";
            objectsBox.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            objectsBox.MultiSelect = false;
            objectsBox.RowHeadersVisible = false;
            objectsBox.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            objectsBox.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            objectsBox.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            objectsBox.SelectionChanged += ObjectsBox_SelectionChanged;
            objectsBox.AutoResizeColumns();
            this.Controls.Add(objectsBox);

            objectView = new ObjectView();
            this.Controls.Add(objectView);

            this.ClientSizeChanged += MainWindow_ClientSizeChanged;
            MainWindow_ClientSizeChanged(this, null);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if(pdf == null)
            {
                return;
            }

            string fileName;
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.AddExtension = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            pdf.Save(fileName, SaveType.Fresh);
        }

        private void Back_Click(object sender, EventArgs e)
        {
            objectView.back();
        }

        private void ObjectsBox_SelectionChanged(object sender, EventArgs e)
        {
            if(pdf == null)
            {
                return;
            }

            DataGridViewRow row = objectsBox.Rows[objectsBox.CurrentCell.RowIndex];
            if(row.Cells.Count == 0)
            {
                return;
            }

            if (row.Cells[0].Value == null)
            {
                return;
            }

            int objectNumber = int.Parse(row.Cells[0].Value.ToString());
            int generationNumber = int.Parse(row.Cells[1].Value.ToString());
            
            object obj = pdf.Get<object>(objectNumber, generationNumber);
            objectView.loadObject(obj);
        }

        private void MainWindow_ClientSizeChanged(object sender, EventArgs e)
        {
            objectsBox.Location = new Point(0, this.MainMenuStrip.Bottom);
            objectsBox.Size = new Size(GetPreferredWidth(objectsBox), ClientSize.Height - objectsBox.Top);

            objectView.Location = new Point(objectsBox.Right, this.MainMenuStrip.Bottom);
            objectView.Size = new Size(ClientSize.Width - objectView.Left, ClientSize.Height - objectView.Top);
        }

        private int GetPreferredWidth(DataGridView grid)
        {
            var border = 0;
            if (grid.BorderStyle == BorderStyle.FixedSingle)
                border = 2 * SystemInformation.BorderSize.Width;
            var vscrollWidth = 0;
            var vscroll = grid.Controls.OfType<VScrollBar>().FirstOrDefault();
            if (vscroll != null && vscroll.Visible)
                border += vscroll.Width;
            var columnsWidth = grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible);
            var rowHeadersWidth = 0;
            if (grid.RowHeadersVisible)
                rowHeadersWidth = grid.RowHeadersWidth;
            return columnsWidth + vscrollWidth + rowHeadersWidth + border + 1;
        }

        private void Open_Click(object sender, EventArgs e)
        {
            string fileName;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.AddExtension = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                }
                else
                {
                    return;
                }
            }

            pdf = new Pdf(fileName);

            objectsBox.Rows.Clear();
            foreach (XrefTable.XrefRecord record in pdf.ListObjects())
            {
                objectsBox.Rows.Add(record.objectNumber, record.generation, record.offset);
            }
        }
    }
}
