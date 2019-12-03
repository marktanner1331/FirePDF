using FirePDF.Model;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    class OperationListBox : TreeView
    {
        public event Action onCheckChanged;

        public OperationListBox()
        {
            this.CheckBoxes = true;
            this.AfterCheck += treeView1_AfterCheck;
        }

        public void setOperations(List<Operation> operations)
        {
            StreamTree tree = new StreamTree(operations);
            this.Controls.Clear();
            this.Nodes.Clear();
            OperationTreeNode root = new OperationTreeNode(tree.root);
            this.Nodes.Add(root);
        }
        
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            this.AfterCheck -= treeView1_AfterCheck;
            selectChildren(e.Node);
            this.AfterCheck += treeView1_AfterCheck;

            onCheckChanged?.Invoke();
        }

        public List<bool> getCheckedMap() => ((OperationTreeNode)this.Nodes[0]).getCheckedMap();

        public List<Operation> getCheckedOperations() => ((OperationTreeNode)this.Nodes[0]).getOperations();

        private void selectChildren(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = node.Checked;
                selectChildren(child);
            }
        }
    }
}
