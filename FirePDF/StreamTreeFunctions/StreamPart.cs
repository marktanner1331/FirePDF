using FirePDF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.StreamTreeFunctions
{
    public class StreamPart
    {
        public List<Operation> operations;
        private HashSet<string> tags;

        public StreamPart()
        {
            operations = new List<Operation>();
            tags = new HashSet<string>();
        }

        public StreamPart(List<Operation> operations)
        {
            this.operations = operations;
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
