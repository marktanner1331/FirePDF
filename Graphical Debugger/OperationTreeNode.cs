using FirePDF.Model;
using FirePDF.StreamPartFunctions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Graphical_Debugger
{
    internal class OperationTreeNode : TreeNode
    {
        private readonly Operation operation;

        public OperationTreeNode(Node<StreamPart> root)
        {
            Text = root.GetAllLeafNodes().Sum(x => x.operations.Count) + " operations";

            if (root.Value != null)
            {
                foreach (Operation operation in root.Value.operations)
                {
                    Nodes.Add(new OperationTreeNode(operation));
                }
            }
            else
            {
                foreach (Node<StreamPart> node in root.GetChildren())
                {
                    OperationTreeNode subNode = new OperationTreeNode(node);

                    if (node.Value == null)
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
        public List<bool> GetCheckedMap()
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
                return Nodes.Cast<OperationTreeNode>().SelectMany(x => x.GetCheckedMap()).ToList();
            }
        }

        public List<Operation> GetOperations()
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
                return Nodes.Cast<OperationTreeNode>().SelectMany(x => x.GetOperations()).ToList();
            }
        }

        public OperationTreeNode(Operation operation)
        {
            Text = operation.ToString();
            this.operation = operation;
        }
    }
}
