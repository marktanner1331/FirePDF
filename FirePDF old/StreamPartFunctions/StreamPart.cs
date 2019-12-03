using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamPartFunctions
{
    public class StreamPart
    {
        public List<Operation> operations;

        public readonly HashSet<string> tags;
        public readonly Dictionary<string, string> variables;

        public StreamPart() : this(new List<Operation>())
        {

        }

        public StreamPart(List<Operation> operations)
        {
            this.operations = operations;
            this.tags = new HashSet<string>();
            this.variables = new Dictionary<string, string>();
        }

        public void addOperation(Operation operation)
        {
            operations.Add(operation);
        }

        public void addTag(string tag)
        {
            tags.Add(tag);
        }

        public void removeTags(params string[] parameters)
        {
            foreach(string tag in parameters)
            {
                tags.Remove(tag);
            }
        }

        public bool hasTag(string tag)
        {
            return tags.Contains(tag);
        }
        
        public override string ToString()
        {
            return string.Join("\n", operations);
        }
    }
}
