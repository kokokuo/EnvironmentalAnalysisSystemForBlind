using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace ZebraCrossing_Test
{
    public class LineEquation
    {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float Slope { get; set; }
        public double Angle { get; set; }
        public double AdjustAngle { get; set; }
        public int Direction { get; set; }
        public LineSegment2D Line { get; set; }

        public static LineEquation GetLineEquation(LineSegment2D line)
        {
            float m = (line.P2.Y - line.P1.Y) / (float)(line.P2.X - line.P1.X);
            // y - y1 = m(x - x1)
            //ax + by = c 
            //a = m, b = -1, c = mx1 - y1
            float a = m;
            float b = -1;
            float c = m * line.P1.X - line.P1.Y;
            PointF vector = line.Direction;
            double angle = Math.Atan2(vector.Y, vector.X) * 180.0 / Math.PI;
            double adjustAngle = angle;
            int direction = 1;
            if (angle < 0)
            {
                adjustAngle = 360 + angle;
                //如果原西角度是負的 ,設定方向為負 -1
                direction = -1;
            }
            //http://dufu.math.ncu.edu.tw/calculus/calculus_bus/node11.html
            //Console.WriteLine("a =" + a + ",b = " + b + ",c = " + c);
            return new LineEquation() { A = a, B = b, C = c, Slope = m, Angle = angle, AdjustAngle = adjustAngle, Direction = direction, Line = line };
        }
    }
}
