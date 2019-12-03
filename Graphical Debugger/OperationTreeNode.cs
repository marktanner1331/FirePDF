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
    class OperationTreeNode : TreeNode
    {
        private Operation operation;

        public OperationTreeNode(Node<StreamPart> root)
        {
            this.Text = root.getAllLeafNodes().Sum(x => x.operations.Count) + " operations";

            if (root.value != null)
            {
                foreach (Operation operation in root.value.operations)
                {
                    Nodes.Add(new OperationTreeNode(operation));
                }
            }
            else
            {
                foreach (Node<StreamPart> node in root.getChildren())
                {
                    OperationTreeNode subNode = new OperationTreeNode(node);

                    if (node.value == null)
                    {
                        subNode.Nodes.Insert(0, new OperationTreeNode(new Operation("q", new List<object>())));
                        subNode.Nodes.Add(new OperationTreeNode(new Operation("Q", new List<object>())));
                    }

                    Nodes.Add(subNode);
                }
            }
        }

        /// <summary>
        /// returns the operations as a list of booleans depending on whether they are checked or not
        /// </summary>
        public List<bool> getCheckedMap()
        {
            if (operation != null)
            {
                return new List<bool>
                {
                    Checked
                };
            }
            else
            {
                return Nodes.Cast<OperationTreeNode>().SelectMany(x => x.getCheckedMap()).ToList();
            }
        }

        public List<Operation> getOperations()
        {
            if (operation != null)
            {
                if (Checked)
                {
                    return new List<Operation>
                    {
                        operation
                    };
                }
                else
                {
                    return new List<Operation>();
                }
            }
            else
            {
                return Nodes.Cast<OperationTreeNode>().SelectMany(x => x.getOperations()).ToList();
            }
        }

        public OperationTreeNode(Operation operation)
        {
            base.Text = operation.ToString();
            this.operation = operation;
        }
    }
}
