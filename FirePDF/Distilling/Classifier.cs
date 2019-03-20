using FirePDF.Model;
using FirePDF.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Distilling
{
    public static class Classifier
    {
        public static bool isDrawImage(IEnumerable<Operation> operations)
        {
            Operation[] array = operations.ToArray();
            if(array.Length != 2)
            {
                return false;
            }

            if(array[0].operatorName != "cm")
            {
                return false;
            }

            if (array[1].operatorName != "Do")
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// returns true if the given operations contains a clipping path and nothing else
        /// </summary>
        public static bool isClippingPath(IEnumerable<Operation> operations)
        {
            Operation[] array = operations.ToArray();
            if(array.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < array.Length - 2; i++)
            {
                if(LineProcessor.isPathDrawingCommand(array[i].operatorName) == false)
                {
                    return false;
                }
            }

            if(ClippingProcessor.isClippingCommand(array[array.Length - 2].operatorName) == false)
            {
                return false;
            }

            if(array[array.Length - 1].operatorName != "n")
            {
                return false;
            }

            return true;
        }
    }
}
