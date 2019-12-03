using FirePDF.Model;
using FirePDF.Processors;
using FirePDF.StreamPartFunctions;
using FirePDF.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace FirePDF.Distilling
{
    public class ModificationEngine
    {
        /// <summary>
        /// when set to true, this method increases the width and height of each image on the page by 1 pixel
        /// </summary>
        public bool increaseImageDimensionsByOnePixel = false;

        public bool increaseClippingPathsByOnePixel = false;

        public bool removeClippingPaths = false;

        public bool removeImageClippingPaths = false;

        public List<Operation> run(IStreamOwner streamOwner, List<Operation> operations)
        {
            throw new Exception();
            //if (increaseImageDimensionsByOnePixel)
            //{
            //    GraphicsStateProcessor gsp = new GraphicsStateProcessor(streamOwner);
            //    Operation[] array = operations.ToArray();

            //    for (int i = 0; i < array.Length - 1; i++)
            //    {
            //        if (array[i].operatorName == "cm" && array[i + 1].operatorName == "Do")
            //        {
            //            GraphicsState gs = gsp.getCurrentState();
            //            Operation cm = array[i];

            //            cm.operands[0] = cm.getOperandAsFloat(0) + 1 / gs.currentTransformationMatrix.Elements[0];
            //            cm.operands[3] = cm.getOperandAsFloat(3) + 1 / gs.currentTransformationMatrix.Elements[3];
            //            cm.operands[4] = cm.getOperandAsFloat(4) - 1 / gs.currentTransformationMatrix.Elements[0];
            //            cm.operands[5] = cm.getOperandAsFloat(5) - 1 / gs.currentTransformationMatrix.Elements[3];
            //        }

            //        gsp.processOperation(array[i]);
            //    }

            //    operations = array.ToList();
            //}

            //if (increaseClippingPathsByOnePixel)
            //{
            //    StreamTree tree = new StreamTree(operations);
            //    StreamTreeClassifier.classifyStreamTree(streamOwner, tree);

            //    GraphicsStateProcessor gsp = new GraphicsStateProcessor(streamOwner);

            //    List<StreamPart> parts = tree.getTreeAsContiguousStreamParts();
            //    StreamPart[] partsArray = parts.ToArray();

            //    for (int i = 0; i < partsArray.Length; i++)
            //    {
            //        StreamPart part = partsArray[i];

            //        if (part is ClippingPathStreamPart && parts[i + 2] is DrawImageStreamPart)
            //        {
            //            GraphicsState gs = gsp.getCurrentState();

            //            LineProcessor lp = new LineProcessor();
            //            ClippingProcessor cp = new ClippingProcessor(gsp.getCurrentState, lp);

            //            foreach (Operation subOp in part.operations)
            //            {
            //                cp.processOperation(subOp);
            //                lp.processOperation(subOp);
            //            }
                        
            //            var realBounds = streamOwner.boundingBox;

            //            Operation cm = new Operation("cm", new List<object> {
            //                    (realBounds.Width + 1) / realBounds.Width,
            //                    0,
            //                    0,
            //                    (realBounds.Height + 1) / realBounds.Height,
            //                    0,
            //                    0
            //                });

            //            //cm.operands[0] = cm.getOperandAsFloat(0) + 1 / gs.currentTransformationMatrix.Elements[0];
            //            //cm.operands[3] = cm.getOperandAsFloat(3) + 1 / gs.currentTransformationMatrix.Elements[3];
            //            //cm.operands[4] = cm.getOperandAsFloat(4) - 1 / gs.currentTransformationMatrix.Elements[0];
            //            //cm.operands[5] = cm.getOperandAsFloat(5) - 1 / gs.currentTransformationMatrix.Elements[3];

            //            part.operations.Insert(0, cm);

            //            cm = new Operation("cm", new List<object> {
            //                    realBounds.Width / (realBounds.Width + 1),
            //                    0,
            //                    0,
            //                    realBounds.Height / (realBounds.Height + 1),
            //                    0,
            //                    0
            //                });

            //            //part.operations.Add(cm);
            //        }

            //        part.operations.ForEach(x => gsp.processOperation(x));
            //    }

            //    operations = tree.convertToOperations();
            //}

            //if (removeImageClippingPaths)
            //{
            //    StreamTree tree = new StreamTree(operations);
            //    StreamTreeClassifier.classifyStreamTree(streamOwner, tree);

            //    GraphicsStateProcessor gsp = new GraphicsStateProcessor(streamOwner);

            //    List<StreamPart> parts = tree.getTreeAsContiguousStreamParts();
            //    StreamPart[] partsArray = parts.ToArray();

            //    for (int i = 0; i < partsArray.Length; i++)
            //    {
            //        StreamPart part = partsArray[i];

            //        if (part is ClippingPathStreamPart && parts[i + 2] is DrawImageStreamPart)
            //        {
            //            part.operations.Clear();
            //        }

            //        part.operations.ForEach(x => gsp.processOperation(x));
            //    }

            //    operations = tree.convertToOperations();
            //}

            //if (removeClippingPaths)
            //{
            //    StreamTree tree = new StreamTree(operations);
            //    StreamTreeClassifier.classifyStreamTree(streamOwner, tree);

            //    tree.removeLeafNodes(x => x.variables["type"] == "clippingPath");

            //    operations = tree.convertToOperations();
            //}

            //return operations;
        }
    }
}
