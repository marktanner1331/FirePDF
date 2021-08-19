using FirePDF.Model;
using FirePDF.Reading;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BosEdit
{
    class ObjectView : DataGridView
    {
        private object currentObject;
        private Stack<object> history;

        public ObjectView()
        {
            history = new Stack<object>();

            this.ColumnCount = 2;
            this.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Columns[0].Name = "Key";
            this.Columns[1].Name = "Value";
            this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            this.Columns.Insert(0, new DataGridViewImageColumn());
            ((DataGridViewImageColumn)this.Columns[0]).DefaultCellStyle.NullValue = null;
            this.Columns[0].Name = "Icon";
            this.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.MultiSelect = false;
            this.AllowUserToOrderColumns = false;
            this.ReadOnly = true;
            this.RowHeadersVisible = false;
            this.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            this.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            this.AllowUserToAddRows = false;

            this.CellDoubleClick += ObjectView_CellDoubleClick;
            this.CellClick += ObjectView_CellClick;
            this.AutoResizeColumns();
        }

        private void ObjectView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(this.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell)
            {
                ObjectView_CellDoubleClick(sender, e);
            }
        }

        private void ObjectView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = this.Rows[e.RowIndex];
            object clickedObject;

            if(currentObject is PdfStream stream && row.Cells[1] is DataGridViewButtonCell buttonCell)
            {
                switch(buttonCell.Value)
                {
                    case "Open in Notepad":
                        ObjectReference objRef = stream.Pdf.ReverseGet(stream);
                        using (StreamReader reader = new StreamReader(stream.GetDecompressedStream()))
                        {
                            OpenInNotepad.openInNotepad(objRef.ToString(), reader.ReadToEnd());
                        }
                        break;
                    case "Open in Notepad Indented":
                        ObjectReference objRef2 = stream.Pdf.ReverseGet(stream);

                        List<Operation> operations = ContentStreamReader.ReadOperationsFromStream(stream.Pdf, stream.GetDecompressedStream());
                        StreamTree tree = new StreamTree(operations.ToArray());
                        
                        OpenInNotepad.openInNotepad(objRef2.ToString(), tree.ToVerboseString());
                        break;
                    case "Save to File":
                        if(tryAskUserForSavePath(out string savePath))
                        {
                            if(currentObject is XObjectImage image)
                            {
                                image.GetImage().Save(savePath);
                            }
                            else
                            {
                                using (Stream objectStream = stream.GetDecompressedStream())
                                {
                                    using (FileStream fs = File.OpenWrite(savePath))
                                    {
                                        objectStream.CopyTo(fs);
                                        objectStream.Flush();
                                        fs.Flush();
                                    }
                                }
                            }
                        }
                        break;
                    case "Update from File":
                        if (tryAskUserForOpenPath(out string openPath))
                        {
                            stream.UpdateStream(File.OpenRead(openPath));
                        }
                        break;
                }
                
                return;
            }
            else if (currentObject is HaveUnderlyingDict underlyingDict)
            {
                clickedObject = underlyingDict.UnderlyingDict.Get((Name)row.Cells[1].Value, true);
            }
            else if (currentObject is PdfList list)
            {
                clickedObject = list.Get<object>(int.Parse((Name)row.Cells[1].Value), true);
            }
            else
            {
                clickedObject = null;
            }

            if(clickedObject != null)
            {
                if (clickedObject is HaveUnderlyingDict || clickedObject is PdfList)
                {
                    loadObject(clickedObject);
                }
            }
        }

        private bool tryAskUserForSavePath(out string savePath)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    savePath = dialog.FileName;
                    return true;
                }
                else
                {
                    savePath = null;
                    return false;
                }
            }
        }

        private bool tryAskUserForOpenPath(out string openPath)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    openPath = dialog.FileName;
                    return true;
                }
                else
                {
                    openPath = null;
                    return false;
                }
            }
        }

        public void back()
        {
            if(history.Count > 1)
            {
                history.Pop();
                loadObject(history.Pop());
            }
            
        }

        public void loadObject(object obj)
        {
            history.Push(obj);

            currentObject = obj;
            this.Rows.Clear();

            if (obj is PdfStream streamOwner)
            {
                foreach (var entry in streamOwner.UnderlyingDict)
                {
                    AdRow(entry.Key, entry.Value);
                }

                AddButtonRow("Open in Notepad");
                AddButtonRow("Open in Notepad Indented");
                AddButtonRow("Save to File");
                AddButtonRow("Update from File");
            }
            else if (obj is HaveUnderlyingDict underlyingDict)
            {
                foreach (var entry in underlyingDict.UnderlyingDict)
                {
                    AdRow(entry.Key, entry.Value);
                }
            }
            else if(obj is PdfList list)
            {
                this.Rows.Clear();
                int index = 0;
                foreach (var entry in list)
                {
                    AdRow(index.ToString(), entry);
                    index++;
                }
            }
            else
            {
                AdRow("Value", obj.ToString());
            }

            this.AutoResizeColumns();
        }

        private void AddButtonRow(string buttonText)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.Cells.Add(new DataGridViewTextBoxCell());
            DataGridViewButtonCell cell = new DataGridViewButtonCell();

            cell.Value = buttonText;

            row.Cells.Add(cell);
            this.Rows.Add(row);
        }

        private void AdRow(Name key, object value)
        {
            switch (value)
            {
                case PdfList _:
                    this.Rows.Add(DefaultIcons.FolderSmall, key, "Array");
                    break;
                case PdfDictionary _:
                    this.Rows.Add(DefaultIcons.FolderSmall, key, "Dictionary");
                    break;
                case ObjectReference _:
                    this.Rows.Add(DefaultIcons.FolderSmall, key, value);
                    break;
                default:
                    this.Rows.Add(null, key, value);
                    break;
            }
        }
    }
}
