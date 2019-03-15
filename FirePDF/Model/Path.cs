using FirePDF.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirePDF.Model
{
    public class Path
    {
        public enum SegmentType
        {
            CLOSE,
            CUBIC_TO,
            LINE_TO,
            MOVE_TO
        }

        [DefaultValue(WindingRule.NON_ZERO)]
        public WindingRule windingRule;

        private List<Tuple<SegmentType, double[]>> segments;

        public Path()
        {
            segments = new List<Tuple<SegmentType, double[]>>();
        }

        public void reset()
        {
            segments.Clear();
            windingRule = WindingRule.NON_ZERO;
        }

        public PointD getCurrentPoint()
        {
            Tuple<SegmentType, double[]> last = segments.Last();
            switch (last.Item1)
            {
                case SegmentType.CLOSE:
                    return null;
                case SegmentType.CUBIC_TO:
                    return new PointD(last.Item2[4], last.Item2[5]);
                case SegmentType.LINE_TO:
                    return new PointD(last.Item2[0], last.Item2[1]);
                case SegmentType.MOVE_TO:
                    return new PointD(last.Item2[0], last.Item2[1]);
                default:
                    throw new Exception();
            }
        }

        public void closePath()
        {
            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.CLOSE, null));
        }

        public void cubicCurveTo(IEnumerable<double> coefficients)
        {
            double[] array = coefficients.ToArray();
            if (array.Length != 6)
            {
                throw new Exception("coefficients must have 6 values");
            }

            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.CUBIC_TO, array));
        }

        public void cubicCurveTo(double anchor1X, double anchor1Y, double anchor2X, double anchor2Y, double endX, double endY)
        {
            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.CUBIC_TO, new double[] { anchor1X, anchor1Y, anchor2X, anchor2Y, endX, endY }));
        }

        public void lineTo(IEnumerable<double> coefficients)
        {
            double[] array = coefficients.ToArray();
            if (array.Length != 2)
            {
                throw new Exception("coefficients must have 2 values");
            }

            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.LINE_TO, array));
        }

        public void lineTo(double x, double y)
        {
            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.LINE_TO, new double[] { x, y }));
        }

        public void moveTo(IEnumerable<double> coefficients)
        {
            double[] array = coefficients.ToArray();
            if (array.Length != 2)
            {
                throw new Exception("coefficients must have 2 values");
            }

            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.MOVE_TO, array));
        }

        public void moveTo(double x, double y)
        {
            segments.Add(new Tuple<SegmentType, double[]>(SegmentType.MOVE_TO, new double[] { x, y }));
        }

        public GraphicsPath toGraphicsPath()
        {
            throw new NotImplementedException();
        }
    }
}

