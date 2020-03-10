using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FirePDF.StreamPartFunctions
{
    public class StreamTree
    {
        public readonly Node<StreamPart> root;

        public StreamTree(Operation[] operations)
        {
            root = new Node<StreamPart>();
            BuildStackTree(operations, root, 0);
        }

        public List<StreamPart> GetAllLeafNodes()
        {
            return root.GetAllLeafNodes();
        }

        private static int BuildStackTree(Operation[] operations, Node<StreamPart> root, int i)
        {
            StreamPart part = new StreamPart();

            for (; i < operations.Length; i++)
            {
                Operation operation = operations[i];
                switch (operation.operatorName)
                {
                    case "q":
                        if (part.operations.Count > 0)
                        {
                            root.AddChildNode(part);
                            part = new StreamPart();
                        }

                        Node<StreamPart> stackRoot = new Node<StreamPart>();
                        i = BuildStackTree(operations, stackRoot, i + 1);
                        stackRoot.ConvertNodeFromLeafToBranch();

                        root.AddChildNode(stackRoot);
                        break;
                    case "Q":
                        if (part.operations.Count > 0)
                        {
                            root.AddChildNode(part);
                        }

                        return i;
                    default:
                        part.AddOperation(operation);
                        break;
                }
            }

            if (part.operations.Count > 0)
            {
                root.AddChildNode(part);
            }

            return i;
        }

        /// <summary>
        /// the swapper is called for each leaf in the tree
        /// each leaf is replaced by the returned value
        /// </summary>
        public void SwapParts(Func<StreamPart, StreamPart> swapper)
        {
            root.SwapLeaves(swapper);
        }

        public void RemoveLeafNodes(Func<StreamPart, bool> test)
        {
            root.RemoveLeaves(test);
        }

        public List<Operation> ConvertToOperations()
        {
            List<Operation> operations = new List<Operation>();
            ConvertTreeToOperations(root, operations);

            return operations;
        }

        private static void ConvertTreeToOperations(Node<StreamPart> root, List<Operation> operations)
        {
            if (root.Value != null)
            {
                operations.AddRange(root.Value.operations);
            }
            else
            {
                foreach (Node<StreamPart> part in root.GetChildren())
                {
                    if (part.Value == null)
                    {
                        operations.Add(new Operation("q", new List<object>()));
                        ConvertTreeToOperations(part, operations);
                        operations.Add(new Operation("Q", new List<object>()));
                    }
                    else
                    {
                        ConvertTreeToOperations(part, operations);
                    }
                }
            }
        }

        public List<StreamPart> GetTreeAsContiguousStreamParts()
        {
            List<StreamPart> parts = new List<StreamPart>();
            GetTreeAsContiguousStreamParts(parts, root);

            return parts;
        }

        private static void GetTreeAsContiguousStreamParts(ICollection<StreamPart> streamParts, Node<StreamPart> root)
        {
            if (root.Value != null)
            {
                streamParts.Add(root.Value);
            }
            else
            {
                foreach (Node<StreamPart> part in root.GetChildren())
                {
                    if (part.Value == null)
                    {
                        streamParts.Add(new StreamPart(new List<Operation>
                        {
                            new Operation("q", new List<object>())
                        }));

                        GetTreeAsContiguousStreamParts(streamParts, part);

                        streamParts.Add(new StreamPart(new List<Operation>
                        {
                            new Operation("Q", new List<object>())
                        }));
                    }
                    else
                    {
                        streamParts.Add(part.Value);
                    }
                }
            }
        }

        public string ToVerboseString()
        {
            return ToVerboseString(root, "");
        }

        private static string ToVerboseString(Node<StreamPart> root, string indent)
        {
            string s = "";

            if (root.Value != null)
            {
                if (root.Value.tags.Any())
                {
                    s += indent + "//tags: " + string.Join(", ", root.Value.tags.ToArray()) + "\n";
                }

                if (root.Value.variables.Any())
                {
                    s += indent + "//variables: " + string.Join(", ", root.Value.variables.Select(x => x.Key + "=" + x.Value).ToArray()) + "\n";
                }

                foreach (Operation operation in root.Value.operations)
                {
                    s += indent + operation.ToString() + "\n";
                }

                return s;
            }

            indent += "    ";
            foreach (Node<StreamPart> node in root.GetChildren())
            {
                if (node.Value == null)
                {
                    s += indent + "q\n";
                    s += ToVerboseString(node, indent);
                    s += indent + "Q\n";
                }
                else
                {
                    s += ToVerboseString(node, indent);
                }
            }

            return s;
        }
    }
}
