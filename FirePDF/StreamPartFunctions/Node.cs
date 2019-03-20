using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamPartFunctions
{
    public class Node<X> where X : class
    {
        private List<Node<X>> children;
        public X value { get; private set; }

        private Node<X> parent;
        public readonly int uniqueID;

        private static int uniqueIDCounter = 0;

        public Node()
        {
            uniqueID = uniqueIDCounter++;
        }

        public Node(X value)
        {
            this.value = value;
            uniqueID = uniqueIDCounter++;
        }

        public void removeChild(int index)
        {
            children?.RemoveAt(index);
        }

        public void clearChildren()
        {
            children?.Clear();
        }

        public int getNumChildren()
        {
            return children?.Count ?? (value == null ? 0 : 1);
        }

        public List<X> getAllLeafNodes()
        {
            List<X> nodes = new List<X>();

            getAllLeafNodes(nodes);

            return nodes;
        }

        private void getAllLeafNodes(List<X> nodes)
        {
            if (value != null)
            {
                nodes.Add(value);
            }
            else
            {
                foreach (Node<X> node in children)
                {
                    node.getAllLeafNodes(nodes);
                }
            }
        }

        public void addChildNode(X child)
        {
            if (value == null && children == null)
            {
                value = child;
            }
            else
            {
                addChildNode(new Node<X>(child));
            }
        }

        public void convertNodeFromLeafToBranch()
        {
            if(value == null)
            {
                return;
            }

            Node<X> node = new Node<X>(value);
            node.parent = this;

            children = new List<Node<X>>();
            children.Add(node);
            value = null;
        }

        public void addChildNode(Node<X> child)
        {
            if (children == null)
            {
                children = new List<Node<X>>();
            }

            if (value != null)
            {
                Node<X> node = new Node<X>(value);
                node.parent = this;

                children.Add(node);
                value = null;
            }

            child.parent = this;
            children.Add(child);
        }

        public void removeLeaves(Func<X, bool> test)
        {
            if (value != null)
            {
                if(test(value))
                {
                    parent.children.Remove(this);
                }
            }
            else
            {
                foreach (Node<X> node in children.ToList())
                {
                    node.removeLeaves(test);
                }

                if(children.Count == 0 && parent != null)
                {
                    parent.children.Remove(this);
                }
            }
        }

        public List<Node<X>> getChildren()
        {
            if (children == null)
            {
                return new List<Node<X>>();
            }

            return children;
        }

        public void setChildren(List<Node<X>> children)
        {
            if (value != null)
            {
                throw new Exception("trying to set children when node has a value");
            }
            
            this.children = children;
            foreach (Node<X> child in children)
            {
                child.parent = this;
            }
        }

        public String toFullString()
        {
            return toFullString("");
        }

        private String toFullString(String indent)
        {
            if (value != null)
            {
                String s2 = value.ToString();
                String[] lines = s2.Split('\n');

                String t = "";
                foreach(String line in lines)
                {
                    if (string.IsNullOrEmpty(t) == false)
                    {
                        t += "\n";
                    }

                    t += indent + line;
                }

                return t;
            }

            String s = "";
            foreach (Node<X> node in children)
            {
                if (string.IsNullOrEmpty(s) == false)
                {
                    s += "\n";
                }

                s += node.toFullString(indent + "    ");
            }

            return s;
        }
    }
}
