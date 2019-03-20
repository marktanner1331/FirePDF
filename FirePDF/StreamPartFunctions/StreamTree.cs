using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamPartFunctions
{
    public class StreamTree
    {
        private Node<StreamPart> root;

        public StreamTree(List<Operation> operations)
        {
            root = new Node<StreamPart>();
            buildStackTree(operations, root, 0);
        }

        public List<StreamPart> getAllLeafNodes()
        {
            return root.getAllLeafNodes();
        }
        
        private static int buildStackTree(List<Operation> operations, Node<StreamPart> root, int i)
        {
            StreamPart part = new StreamPart();

            for (; i < operations.Count; i++)
            {
                Operation operation = operations[i];
                switch (operation.operatorName)
                {
                    case "q":
                        if (part.operations.Count > 0)
                        {
                            root.addChildNode(part);
                            part = new StreamPart();
                        }

                        Node<StreamPart> stackRoot = new Node<StreamPart>();
                        i = buildStackTree(operations, stackRoot, i + 1);
                        stackRoot.convertNodeFromLeafToBranch();

                        root.addChildNode(stackRoot);
                        break;
                    case "Q":
                        if (part.operations.Count > 0)
                        {
                            root.addChildNode(part);
                        }

                        return i;
                    default:
                        part.addOperation(operation);
                        break;
                }
            }

            if (part.operations.Count > 0)
            {
                root.addChildNode(part);
            }

            return i;
        }

        /// <summary>
        /// the swapper is called for each leaf in the tree
        /// each leaf is replaced by the returned value
        /// </summary>
        public void swapParts(Func<StreamPart, StreamPart> swapper)
        {
            root.swapLeaves(swapper);
        }

        public void removeLeafNodes(Func<StreamPart, bool> test)
        {
            root.removeLeaves(test);
        }

        public List<Operation> convertToOperations()
        {
            List<Operation> operations = new List<Operation>();
            convertTreeToOperations(root, operations);

            return operations;
        }

        private void convertTreeToOperations(Node<StreamPart> root, List<Operation> operations)
        {
            if (root.value != null)
            {
                operations.AddRange(root.value.operations);
            }
            else
            {
                foreach (Node<StreamPart> part in root.getChildren())
                {
                    if (part.value == null)
                    {
                        operations.Add(new Operation("q", new List<object>()));
                        convertTreeToOperations(part, operations);
                        operations.Add(new Operation("Q", new List<object>()));
                    }
                    else
                    {
                        convertTreeToOperations(part, operations);
                    }
                }
            }
        }

        public String toVerboseString()
        {
            return toVerboseString(root, "");
        }

        private String toVerboseString(Node<StreamPart> root, String indent)
        {
            String s = "";
            
            if (root.value != null)
            {
                if(root.value.tags.Any())
                {
                    s += indent + "//tags: " + string.Join(", ", root.value.tags) + "\n";
                }

                if(root.value.variables.Any())
                {
                    s += indent + "//variables: " + string.Join(", ", root.value.variables.Select(x => x.Key + "=" + x.Value)) + "\n";
                }

                foreach (Operation operation in root.value.operations)
                {
                    s += indent + operation.ToString() + "\n";
                }

                return s;
            }

            indent += "    ";
            foreach (Node<StreamPart> node in root.getChildren())
            {
                if (node.value == null)
                {
                    s += indent + "q\n";
                    s += toVerboseString(node, indent);
                    s += indent + "Q\n";
                }
                else
                {
                    s += toVerboseString(node, indent);
                }
            }
            if (s.Length == 0)
            {

            }
            return s;
        }
    }
}
