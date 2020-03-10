using FirePDF.Model;
using System.Collections.Generic;
using System.Linq;

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
            tags = new HashSet<string>();
            variables = new Dictionary<string, string>();
        }

        public void AddOperation(Operation operation)
        {
            operations.Add(operation);
        }

        public void AddTag(string tag)
        {
            tags.Add(tag);
        }

        public void RemoveTags(params string[] parameters)
        {
            foreach(string tag in parameters)
            {
                tags.Remove(tag);
            }
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }
        
        public override string ToString()
        {
            return string.Join("\n", operations.Select(x => x.ToString()).ToArray());
        }
    }
}
