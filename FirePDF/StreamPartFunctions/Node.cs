using System;
using System.Collections.Generic;
using System.Linq;

namespace FirePDF.StreamPartFunctions
{
    public class Node<TX> where TX : class
    {
        private List<Node<TX>> children;
        public TX Value { get; private set; }

        private Node<TX> parent;
        public readonly int uniqueId;

        private static int _uniqueIdCounter = 0;

        public Node()
        {
            uniqueId = _uniqueIdCounter++;
        }

        public Node(TX value)
        {
            Value = value;
            uniqueId = _uniqueIdCounter++;
        }

        public void RemoveChild(int index)
        {
            children?.RemoveAt(index);
        }

        public void ClearChildren()
        {
            children?.Clear();
        }

        public int GetNumChildren()
        {
            return children?.Count ?? (Value == null ? 0 : 1);
        }

        public List<TX> GetAllLeafNodes()
        {
            List<TX> nodes = new List<TX>();

            GetAllLeafNodes(nodes);

            return nodes;
        }

        private void GetAllLeafNodes(ICollection<TX> nodes)
        {
            if (Value != null)
            {
                nodes.Add(Value);
            }
            else
            {
                foreach (Node<TX> node in children)
                {
                    node.GetAllLeafNodes(nodes);
                }
            }
        }

        public void AddChildNode(TX child)
        {
            if (Value == null && children == null)
            {
                Value = child;
            }
            else
            {
                AddChildNode(new Node<TX>(child));
            }
        }

        public void ConvertNodeFromLeafToBranch()
        {
            if(Value == null)
            {
                return;
            }

            Node<TX> node = new Node<TX>(Value);
            node.parent = this;

            children = new List<Node<TX>>();
            children.Add(node);
            Value = null;
        }

        public void AddChildNode(Node<TX> child)
        {
            if (children == null)
            {
                children = new List<Node<TX>>();
            }

            if (Value != null)
            {
                Node<TX> node = new Node<TX>(Value);
                node.parent = this;

                children.Add(node);
                Value = null;
            }

            child.parent = this;
            children.Add(child);
        }

        public void SwapLeaves(Func<TX, TX> swapper)
        {
            if (Value != null)
            {
                Value = swapper(Value);
            }
            else
            {
                foreach (Node<TX> node in children.ToList())
                {
                    node.SwapLeaves(swapper);
                }
            }
        }

        public void RemoveLeaves(Func<TX, bool> test)
        {
            if (Value != null)
            {
                if(test(Value))
                {
                    parent.children.Remove(this);
                }
            }
            else
            {
                foreach (Node<TX> node in children.ToList())
                {
                    node.RemoveLeaves(test);
                }

                if(children.Count == 0 && parent != null)
                {
                    parent.children.Remove(this);
                }
            }
        }

        public List<Node<TX>> GetChildren()
        {
            if (children == null)
            {
                return new List<Node<TX>>();
            }

            return children;
        }

        public void SetChildren(List<Node<TX>> children)
        {
            if (Value != null)
            {
                throw new Exception("trying to set children when node has a value");
            }
            
            this.children = children;
            foreach (Node<TX> child in children)
            {
                child.parent = this;
            }
        }

        public string ToFullString()
        {
            return ToFullString("");
        }

        private string ToFullString(string indent)
        {
            if (Value != null)
            {
                string s2 = Value.ToString();
                string[] lines = s2.Split('\n');

                string t = "";
                foreach(string line in lines)
                {
                    if (string.IsNullOrEmpty(t) == false)
                    {
                        t += "\n";
                    }

                    t += indent + line;
                }

                return t;
            }

            string s = "";
            foreach (Node<TX> node in children)
            {
                if (string.IsNullOrEmpty(s) == false)
                {
                    s += "\n";
                }

                s += node.ToFullString(indent + "    ");
            }

            return s;
        }
    }
}
