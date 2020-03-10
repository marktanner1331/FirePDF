using FirePDF.Model;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    internal class OperationListBox : TreeView
    {
        public event Action OnCheckChanged;

        public OperationListBox()
        {
            CheckBoxes = true;
            AfterCheck += treeView1_AfterCheck;
        }

        public void SetOperations(List<Operation> operations)
        {
            StreamTree tree = new StreamTree(operations);
            Controls.Clear();
            Nodes.Clear();
            OperationTreeNode root = new OperationTreeNode(tree.root);
            Nodes.Add(root);
        }
        
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            AfterCheck -= treeView1_AfterCheck;
            SelectChildren(e.Node);
            AfterCheck += treeView1_AfterCheck;

            OnCheckChanged?.Invoke();
        }

        public List<bool> GetCheckedMap() => ((OperationTreeNode)Nodes[0]).GetCheckedMap();

        public List<Operation> GetCheckedOperations() => ((OperationTreeNode)Nodes[0]).GetOperations();

        private static void SelectChildren(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = node.Checked;
                SelectChildren(child);
            }
        }
    }
}
