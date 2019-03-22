using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.StreamPartFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Distilling
{
    public class ModificationEngine
    {
        /// <summary>
        /// when set to true, this method increases the width and height of each image on the page by 1 pixel
        /// </summary>
        public bool increaseImageDimensionsByOnePixel = false;
        public bool removeClippingPaths = false;

        public List<Operation> run(IStreamOwner streamOwner, List<Operation> operations)
        {
            if (increaseImageDimensionsByOnePixel)
            {
                GraphicsStateProcessor gsp = new GraphicsStateProcessor(streamOwner);
                Operation[] array = operations.ToArray();

                for (int i = 0; i < array.Length - 1; i++)
                {
                    if (array[i].operatorName == "cm" && array[i + 1].operatorName == "Do")
                    {
                        GraphicsState gs = gsp.getCurrentState();
                        Operation cm = array[i];

                        cm.operands[0] = cm.getOperandAsFloat(0) + 1 / gs.currentTransformationMatrix.Elements[0];
                        cm.operands[3] = cm.getOperandAsFloat(3) + 1 / gs.currentTransformationMatrix.Elements[3];
                        cm.operands[4] = cm.getOperandAsFloat(4) - 1 / gs.currentTransformationMatrix.Elements[0];
                        cm.operands[5] = cm.getOperandAsFloat(5) - 1 / gs.currentTransformationMatrix.Elements[3];
                    }

                    gsp.processOperation(array[i]);
                }

                operations = array.ToList();
            }

            if (removeClippingPaths)
            {
                StreamTree tree = new StreamTree(operations);
                StreamTreeClassifier.classifyStreamTree(streamOwner, tree);

                tree.removeLeafNodes(x => x.variables["type"] == "clippingPath");

                operations = tree.convertToOperations();
            }

            return operations;
        }
    }
}
