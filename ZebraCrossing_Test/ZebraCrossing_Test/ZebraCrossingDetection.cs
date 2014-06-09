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
using ZebraCrossing_Test;
namespace ZebraCrossingDetection
{
    public enum LineQuantification { 
        ANGLE_170_TO_180 = 0,
        L_ANGLE_160_TO_170,
        L_ANGLE_150_TO_160,
        L_ANGLE_140_TO_150,
        L_ANGLE_130_TO_140,
        L_ANGLE_120_TO_130,
        L_ANGLE_110_TO_120,
        L_ANGLE_100_TO_110,
        L_ANGLE_90_TO_100,
        R_ANGLE_160_TO_170,
        R_ANGLE_150_TO_160,
        R_ANGLE_140_TO_150,
        R_ANGLE_130_TO_140,
        R_ANGLE_120_TO_130,
        R_ANGLE_110_TO_120,
        R_ANGLE_100_TO_110,
        R_ANGLE_90_TO_100
    };


    public class ZebraCrossingDetection
    {
      
        public ZebraCrossingDetection() {
            
        }
        //開始偵測
        public static bool StartToDetect(string filename) {

            return false;
        }

        //1.載入圖片
        public static Image<Bgr, byte> LoadImg(string filename)
        {
            if (filename != null)
            {

                Image<Bgr, byte> source = new Image<Bgr, byte>(filename);
                source = source.Resize(480, 360, INTER.CV_INTER_LINEAR);
                return source;
            }
            else {
                return null;
            }
        }
        //2.剪裁圖片
        public static Image<Bgr, byte> ToCrop(Image<Bgr, byte> source)
        {
            if (source != null)
            {
                source = source.Copy();
                source.ROI = new Rectangle(new Point(0, source.Height / 2), new Size(source.Width, source.Height / 2));
                return source;
            }
            else
            {
                return null;
            }
        }
        //3.灰階
        public static Image<Gray, byte> ToGray(Image<Bgr, byte> source)
        {
            if (source != null)
            {
                Image<Gray, byte> processImg = source.Convert<Gray, byte>();
                return processImg;
            }
            else
            {
                return null;
            }
        }

        //4.Mask白色
        public static Image<Gray, byte> MaskWhite(Image<Gray, byte> source)
        {

            //oriImg 與 grayImg 測試
            if (source != null)
            {
                source = new Image<Gray, byte>(new Size(source.Width, source.Height));
                CvInvoke.cvInRangeS(source, new MCvScalar(160, 160, 160), new MCvScalar(255, 255, 255), source);
                return source;
                
            }
            else
            {
                return null;
            }
        }

        //模糊
        public static Image<Gray, byte> Smooth(Image<Gray, byte> source)
        {

            if (source != null)
            {
                CvInvoke.cvSmooth(source, source, SMOOTH_TYPE.CV_GAUSSIAN, 13, 13, 1.5, 1);
                return source;
            }
            else
                return null;
        }

        //去胡椒鹽
        public static Image<Gray, byte> PepperFilter(Image<Gray, byte> source)
        {
            if (source != null)
            {
                //用中值濾波去雜訊
                source = source.SmoothMedian(3);
                return source;
            }
            else
            {
                return null;
            }
        }

        #region 偵測線段
        //5.找尋直線
        public static LinkedList<LineEquation> DetectHoughLine(Image<Gray, byte> source)
        {
            LinkedList<LineEquation> candidateLineEquations = new LinkedList<LineEquation>();
          
            if (source != null)
            {
                Image<Bgr, byte> onlyLineImg = new Image<Bgr, byte>(source.Width, source.Height, new Bgr(0, 0, 0));

                //Hough transform for line detection
                LineSegment2D[][] lines = source.HoughLines(
                    new Gray(125),  //Canny algorithm low threshold
                    new Gray(260),  //Canny algorithm high threshold
                    1,              //rho parameter
                    Math.PI / 180.0,  //theta parameter 
                    100,            //threshold
                    1,             //min length for a line
                    20);            //max allowed gap along the line

                foreach (var line in lines[0])
                {
                    //計算並取得線段的直線方程式
                    LineEquation eqation = LineEquation.GetLineEquation(line);
                    candidateLineEquations.AddLast(eqation);
                }

                return candidateLineEquations;

            }
            else
                return null;

          
        }
        #endregion

        #region 修復線段
        public static LinkedList<LineEquation> RepairedLines(LinkedList<LineEquation> candidateHoughLineEquations, Image<Bgr, byte> source)
        {

            int i = 0;
            Point p;
            //ToArray避開刪除後的List長度問題
            LinkedListNode<LineEquation> node = candidateHoughLineEquations.First;
            while (node != null)
            {
                LinkedListNode<LineEquation> matchedNode = node.Next;
                bool intersect = false;
                while (matchedNode != null)
                {
                    //是否共線或是相交的線段
                    LineSegment2D repairedLine = new LineSegment2D();
                    intersect = CheckIntersectOrNot(node.Value, matchedNode.Value, out p, ref repairedLine, source);
                    if (intersect)
                    {
                        //改掉原先的線段
                        node.Value.Line = repairedLine;
                        //並刪除被比較的線段
                        LinkedListNode<LineEquation> remove = matchedNode;
                        matchedNode = matchedNode.Next;
                        candidateHoughLineEquations.Remove(remove.Value);

                    }
                    else
                        matchedNode = matchedNode.Next;
                }
                //下一個Node
                node = node.Next;
                i++;
            }
            return candidateHoughLineEquations;
        }

        //重建接近水平線段
        private static LineSegment2D RepaiedHorizontalHoughLine(Point[] ps)
        {
            Point leftPoint = ps[0];
            Point rightPoint = ps[0];
            for (int i = 1; i < ps.Length; i++)
            {
                if (ps[i].X <= leftPoint.X)
                    leftPoint = ps[i];
                if (ps[i].X >= rightPoint.X)
                    rightPoint = ps[i];
            }
            return new LineSegment2D(leftPoint, rightPoint);
        }

        //重建接近垂直線段
        private static LineSegment2D RepaiedVerticalHoughLine(Point[] ps)
        {
            Point topPoint = ps[0];
            Point bottomPoint = ps[0];
            for (int i = 1; i < ps.Length; i++)
            {
                if (ps[i].Y <= topPoint.Y)
                    topPoint = ps[i];
                if (ps[i].Y >= bottomPoint.Y)
                    bottomPoint = ps[i];
            }
            return new LineSegment2D(topPoint, bottomPoint);
        }
        //判斷接近水平相交的座標點是否在兩條線段內
        private static bool CheckHorizontalIntersectionPoint(Point[] ps, int intersect_x, int intersect_y)
        {
            Point leftPoint = ps[0];
            Point rightPoint = ps[0];
            for (int i = 1; i < ps.Length; i++)
            {
                if (ps[i].X <= leftPoint.X)
                    leftPoint = ps[i];
                if (ps[i].X >= rightPoint.X)
                    rightPoint = ps[i];
            }
            return leftPoint.X < intersect_x && rightPoint.X > intersect_x;
        }

        //判斷接近垂直相交的座標點是否在兩條線段內
        private static bool CheckVerticalIntersectionPoint(Point[] ps, int intersect_x, int intersect_y)
        {
            Point topPoint = ps[0];
            Point bottomPoint = ps[0];
            for (int i = 1; i < ps.Length; i++)
            {
                if (ps[i].Y <= topPoint.Y)
                    topPoint = ps[i];
                if (ps[i].Y >= bottomPoint.Y)
                    bottomPoint = ps[i];
            }
            return topPoint.X < intersect_y && bottomPoint.X > intersect_y;
        }

        //共線或是相交
        private static bool CheckIntersectOrNot(LineEquation line1, LineEquation line2, out Point intersectP, ref LineSegment2D repaiedLine, Image<Bgr, byte> source)
        {
             intersectP = new Point();
            //檢查共線(檢查向量 A,B,C三點,A->B 與B->C兩條向量會是比例關係 or A-B 與 A-C的斜率會依樣 or 向量叉積 A)
            //使用 x1(y2- y3) + x2(y3- y1) + x3(y1- y2) = 0 面積公式 http://math.tutorvista.com/geometry/collinear-points.html
            int x1 = line1.Line.P1.X;
            int y1 = line1.Line.P1.Y;
            int x2 = line1.Line.P2.X;
            int y2 = line1.Line.P2.Y;
            int x3 = line2.Line.P1.X;
            int y3 = line2.Line.P1.Y;
            int x4 = line2.Line.P2.X;
            int y4 = line2.Line.P2.Y;
            float v = (line1.A * line2.B) - line2.A * line1.B;
            //Console.WriteLine("line1 slope = " + line1.Slope + ", line 2 slope = " + line2.Slope);
            //Console.WriteLine("line1 P1 = " + line1.Line.P1 + ",line1 P2 = " + line1.Line.P2 + ", length = " + line1.Line.Length);
            //Console.WriteLine("line2 P1 = " + line2.Line.P1 + ",line2 P2 = " + line2.Line.P2 + ", length = " + line2.Line.Length);
            //不太可能會有共線,需要給一個Range
            //面積公式來看 1/2 * x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2) 如果小於1000可以是
            //or 用A-B 與 A-C的斜率去看斜率誤差 約接近0表示共線高
            Console.WriteLine("面積公式1 = " + Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) + " y3 -y1 = " + Math.Abs(y3 - y1));
            Console.WriteLine("面積公式2 = " + Math.Abs(x1 * (y2 - y4) + x2 * (y4 - y1) + x4 * (y1 - y2)) + " y4 -y1 = " + Math.Abs(y4 - y1));
            //float p1p2Slope = (y2 - y1) / (float)(x2 - x1);
            //float p1p3Slope = (y3 - y1) / (float)(x3 - x1);
            //Console.WriteLine("Slope p1 -> p2 = " +p1p2Slope+ ", Slope p1 -> p3 ="+ p1p3Slope + "差距值 = " + Math.Abs(Math.Abs(p1p2Slope) - Math.Abs(p1p3Slope)));
           
            //尋找兩端點
            Point[] points = new Point[] { line1.Line.P1, line1.Line.P2, line2.Line.P1, line2.Line.P2 };
            float area1 = Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));
            float area2 = Math.Abs(x1 * (y2 - y4) + x2 * (y4 - y1) + x4 * (y1 - y2));
            if (area1 <= 1000 && area2 <= 1000 && Math.Abs(y3 - y1) < 6 && Math.Abs(y4 - y1) < 6)
            // Math.Abs(y3 - y1) < 8 => y3 - y1表示距離
            {
                Console.WriteLine("共線" + "\n");
                intersectP.X = -1;
                intersectP.Y = -1;
                repaiedLine = RepaiedHorizontalHoughLine(points);
                return true;
            }
            else if (v != 0)
            {  //代表兩條線段方程式不是平行,y3 - y1表示距離
                Console.Write("相交");
                //Console.WriteLine("v = " + v);
                //兩條線相似
                Console.WriteLine("線段一原角度：" + line1.Angle + ",修正角度:" + line1.AdjustAngle + "\n線段原二角度：" + line2.Angle + ",修正角度" + line2.AdjustAngle);
                double angleDifference = Math.Abs(line1.AdjustAngle - line2.AdjustAngle);
                Console.WriteLine("兩條線的角度差為：" + angleDifference);
                if (angleDifference < 15)
                {
                    Console.WriteLine("兩條線可能是可以銜接");
                    float delta_x = (line1.C * line2.B) - line2.C * line1.B;
                    float delta_y = (line1.A * line2.C) - line2.A * line1.C;

                    intersectP.X = Convert.ToInt32(delta_x / v);
                    intersectP.Y = Convert.ToInt32(delta_y / v);


                    if ((intersectP.X < 0 || intersectP.X > source.Width || intersectP.Y < 0 || intersectP.Y > source.Height))
                    {
                        Console.WriteLine("所以超出畫面");
                        intersectP.X = -1;
                        intersectP.Y = -1;
                        return false;
                    }
                    else
                    {
                        //判斷角度有用原角度取絕對值(因為角度的方位比較特別)
                        double line1_angle = Math.Abs(line1.Angle);
                        double line2_angle = Math.Abs(line2.Angle);

                        //接近平行線(角度都大於150),並且方向一致
                        if (line1_angle > 150 && line2_angle > 150 && line1.Direction == line2.Direction)
                        {
                            if (!CheckHorizontalIntersectionPoint(points, intersectP.X, intersectP.Y))
                                return false;
                            Console.WriteLine("接近水平的線段,且交點有在兩線段內");
                        }
                        else if (line1_angle <= 150 && line1_angle >= 120 && line2_angle <= 150 && line2_angle >= 120 && line1.Direction == line2.Direction)
                        {
                            //斜45度
                            if (!CheckVerticalIntersectionPoint(points, intersectP.X, intersectP.Y) && !CheckHorizontalIntersectionPoint(points, intersectP.X, intersectP.Y))
                                return false;
                            Console.WriteLine("接近斜45度或135度的線段,且交點有在兩線段內");
                        }
                        else if (line1_angle < 120 && line2_angle < 120 && line1.Direction == line2.Direction) //接近垂直
                        {
                            if (!CheckVerticalIntersectionPoint(points, intersectP.X, intersectP.Y))
                                return false;
                            Console.WriteLine("接近垂直的線段,且交點有在兩線段內");
                        }

                        Console.Write("\n");
                        repaiedLine = RepaiedHorizontalHoughLine(points);
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("但是角度差異過大，研判不是\n");
                    intersectP.X = -1;
                    intersectP.Y = -1;
                    return false;
                }

                //Console.WriteLine("intersect x = " + x + ",intersect y = " + y + "\n");

            }
            else
            {
                intersectP.X = -1;
                intersectP.Y = -1;
                Console.WriteLine("平行" + "\n");
                return false;
            }

        }
        #endregion

        #region 量化分類
        private static Dictionary<LineQuantification, LinkedList<LineEquation>> QuquantifyLinesByAngle(LinkedList<LineEquation> candidateHoughLineEquations)
        {
            Dictionary<LineQuantification, LinkedList<LineEquation>> linesHistogram = new Dictionary<LineQuantification, LinkedList<LineEquation>>();
            //統計線段
            foreach (LineEquation line in candidateHoughLineEquations)
            {
                //量化分類
                if (170 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) <= 180)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.ANGLE_170_TO_180))
                    {
                        linesHistogram.Add(LineQuantification.ANGLE_170_TO_180, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.ANGLE_170_TO_180].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.ANGLE_170_TO_180].AddLast(line);
                }
                else if (160 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 170 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_160_TO_170))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_160_TO_170, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_160_TO_170].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_160_TO_170].AddLast(line);
                }
                else if (150 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 160 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_150_TO_160))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_150_TO_160, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_150_TO_160].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_150_TO_160].AddLast(line);
                }
                else if (140 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 150 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_140_TO_150))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_140_TO_150, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_140_TO_150].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_140_TO_150].AddLast(line);
                }
                else if (130 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 140 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_130_TO_140))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_130_TO_140, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_130_TO_140].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_130_TO_140].AddLast(line);
                }
                else if (120 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 130 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_120_TO_130))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_120_TO_130, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_120_TO_130].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_120_TO_130].AddLast(line);
                }
                else if (110 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 120 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_110_TO_120))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_110_TO_120, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_110_TO_120].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_110_TO_120].AddLast(line);
                }
                else if (100 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 110 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_100_TO_110))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_100_TO_110, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_100_TO_110].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_100_TO_110].AddLast(line);
                }
                else if (90 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 100 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.L_ANGLE_90_TO_100))
                    {
                        linesHistogram.Add(LineQuantification.L_ANGLE_90_TO_100, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.L_ANGLE_90_TO_100].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.L_ANGLE_90_TO_100].AddLast(line);
                }
                else if (160 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 170 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_160_TO_170))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_160_TO_170, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_160_TO_170].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_160_TO_170].AddLast(line);
                }
               
                else if (150 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 160 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_150_TO_160))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_150_TO_160, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_150_TO_160].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_150_TO_160].AddLast(line);
                }
                else if (140 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 150 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_140_TO_150))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_140_TO_150, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_140_TO_150].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_140_TO_150].AddLast(line);
                }
                else if (130 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 140 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_130_TO_140))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_130_TO_140, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_130_TO_140].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_130_TO_140].AddLast(line);
                }
                else if (120 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 130 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_120_TO_130))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_120_TO_130, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_120_TO_130].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_120_TO_130].AddLast(line);
                }
                else if (110 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 120 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_110_TO_120))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_110_TO_120, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_110_TO_120].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_110_TO_120].AddLast(line);
                }
                else if (100 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 110 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_100_TO_110))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_100_TO_110, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_100_TO_110].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_100_TO_110].AddLast(line);
                }
                else if (90 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 100 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey(LineQuantification.R_ANGLE_90_TO_100))
                    {
                        linesHistogram.Add(LineQuantification.R_ANGLE_90_TO_100, new LinkedList<LineEquation>());
                        linesHistogram[LineQuantification.R_ANGLE_90_TO_100].AddLast(line);
                    }
                    else
                        linesHistogram[LineQuantification.R_ANGLE_90_TO_100].AddLast(line);
                }
            }

            return linesHistogram;
        }
        #endregion

        #region 過濾線段
        public static Dictionary<LineQuantification, LinkedList<LineEquation>> MainGroupLineFilter(LinkedList<LineEquation> candidateHoughLineEquations, ref int mainDirectionLineGroupId)
        {
            Dictionary<LineQuantification, LinkedList<LineEquation>> linesHistogram =  QuquantifyLinesByAngle(candidateHoughLineEquations);

            float mainDirectionRatio = 0;
            //過濾線段
            int total = candidateHoughLineEquations.Count;
            Console.WriteLine("Total Lines = " + total);
            foreach(LineQuantification quantification in Enum.GetValues(typeof(LineQuantification)) ){
                if (linesHistogram.ContainsKey(quantification))
                {
                    Console.WriteLine("line[" + quantification.ToString() + "]:" + linesHistogram[quantification].Count);
                    float ratio = (linesHistogram[quantification].Count / (float)total);
                    if (mainDirectionRatio < ratio)
                    {
                        mainDirectionRatio = ratio;
                        mainDirectionLineGroupId = (int)quantification;
                    }
                    Console.WriteLine("佔全部線段的比例 =>" + ratio);
                }
                else
                    Console.WriteLine("line[" + quantification.ToString() + "]:" + 0);
            }
                
         
            Console.WriteLine("主方向群為:" + mainDirectionLineGroupId + "佔全部線段的比例 =>" + mainDirectionRatio);

            //過濾過短的線段
            //1.找最大
            var maxLengthLine = (from selectLine in linesHistogram[(LineQuantification)mainDirectionLineGroupId]
                                 select selectLine).Max(line => line.Line.Length);
            Console.WriteLine("最長的線段為:" + maxLengthLine);
            //2.依照比例把過段的濾掉
            LinkedListNode<LineEquation> node = linesHistogram[(LineQuantification)mainDirectionLineGroupId].First;
            while (node != null)
            {
                if (node.Value.Line.Length < (maxLengthLine / 3))
                {
                    linesHistogram[(LineQuantification)mainDirectionLineGroupId].Remove(node);
                    Console.WriteLine("移除的線段為:\nLength:" + node.Value.Line.Length + ",p1:" + node.Value.Line.P1 + ",p2:" + node.Value.Line.P2);
                }
                node = node.Next;
            }

            return linesHistogram;
        }
        #endregion

        #region 分析黑白紋路
        public static void AnalyzeZebraCrossingTexture(int mainDirectionLineGroupId, Dictionary<LineQuantification, LinkedList<LineEquation>> linesHistogram, Image<Gray, byte> source, Image<Bgr, byte> oriImg)
        {
            //紀錄斑馬線之間白色連結起來的線段
            List<LineSegment2DF> crossingConnectionlines = new List<LineSegment2DF>();  

            Point prePoint = new Point();
            Point currentPoint = new Point();
            Image<Bgr, byte> scanLineImg = oriImg.Clone();

            IntensityPoint current, previous;
            current = new IntensityPoint();
            previous = new IntensityPoint();

            int index = 0;
            List<LineEquation> orderedLines = new List<LineEquation>();
            //角度幾乎呈垂直,所以排序用x軸
            if ((17 <= mainDirectionLineGroupId && mainDirectionLineGroupId <= 18) || (7 <= mainDirectionLineGroupId && mainDirectionLineGroupId <= 9))
            {
                var orderedMainLines = from line in linesHistogram[(LineQuantification)mainDirectionLineGroupId] orderby (line.Line.P1.X + line.Line.P2.X) / 2 select line;
                foreach (LineEquation line in orderedMainLines)
                {
                    orderedLines.Add(line);
                }
            }
            else
            {
                var orderedMainLines = from line in linesHistogram[(LineQuantification)mainDirectionLineGroupId] orderby (line.Line.P1.Y + line.Line.P2.Y) / 2 select line;
                foreach (LineEquation line in orderedMainLines)
                {
                    orderedLines.Add(line);
                }
            }

            foreach (LineEquation line in orderedLines)
            {
                int lineCenterY = (line.Line.P1.Y + line.Line.P2.Y) / 2;
                int lineCenterX = (line.Line.P1.X + line.Line.P2.X) / 2;

                if (!currentPoint.IsEmpty)
                    prePoint = currentPoint;
                currentPoint = new Point(lineCenterX, lineCenterY);
                //兩點 =>存放線條,並繪製
                if (!currentPoint.IsEmpty && !prePoint.IsEmpty)
                {
                    LineSegment2DF scanline = new LineSegment2DF(prePoint, currentPoint);
                    //記錄每一條線段
                    crossingConnectionlines.Add(scanline);
                    Console.WriteLine("draw Line:direction ,x = " + scanline.Direction.X + "y =" + scanline.Direction.Y + ",point p1.x =" + prePoint.X + ",p1.y = " + prePoint.Y + ", p2.x =" + currentPoint.X + ",p2.y = " + currentPoint.Y);
                    
                }

                Console.WriteLine("-------------------------------------");
                index++;
            }
            ImageViewer showScanLine = new ImageViewer(scanLineImg, "Scan Line");
            showScanLine.Show();

            Image<Bgr, byte> stasticDst = new Image<Bgr, byte>(640, 480, new Bgr(Color.White));
            //統計黑白像素與判斷是否每條線段為白黑白的特徵
            bool isBlackWhiteCrossing = DoBlackWhiteStatisticsByScanLine(crossingConnectionlines, source, stasticDst);

            if (isBlackWhiteCrossing && linesHistogram[(LineQuantification)mainDirectionLineGroupId].Count > 3)
            {
                //MessageBox.Show("找到斑馬線");
            }
        
        }
        /// <summary>
        /// 判斷紋路是否黑白交錯
        /// </summary>
        /// <param name="checkPoints">記錄每一條線的黑白是否交錯的格式</param>
        /// <param name="intensity">當前像素亮度</param>
        /// <param name="lineIndex">目前的線段索引</param>
        /// <param name="pixelSum">黑或白的像素總和(有連續兩個)</param>
        /// <param name="previousPixelValue">前一個像素的值(來判斷當下與前一個像素的數值一致否)</param>
        private static void CheckBlackWhiteTexture(string checkPoints, double intensity, ref int pixelSum, ref int previousPixelValue, ref int previousCheckIntentisty, ref string checkBlackWhiteCrossingPoint)
        {

            if (intensity == 255)
            {
                if (previousPixelValue != -1)
                {
                    if (intensity == previousPixelValue)
                        pixelSum++;
                    else
                    {
                        previousPixelValue = 255;
                        pixelSum = 1;
                    }
                }
                else
                {
                    previousPixelValue = 255;
                    pixelSum++;
                }
            }
            else
            {
                if (previousPixelValue != -1)
                {
                    if (intensity == previousPixelValue)
                        pixelSum++;
                    else
                    {
                        previousPixelValue = 0;
                        pixelSum = 1;
                    }
                }
                else
                {
                    previousPixelValue = 0;
                    pixelSum++;
                }
            }
            //有連續5個同樣像素及判斷為是此紋路
            if (pixelSum == 5)
            {
                Console.WriteLine("---------------\nIntensity: " + intensity + " has accumulated!");
                //如果是第一次判斷紋路或是前一個紋路與線段的紋路不相同,紀錄
                if (previousCheckIntentisty != intensity || previousCheckIntentisty == -1)
                {
                    //白色用true
                    if (intensity == 255)
                    {
                        checkBlackWhiteCrossingPoint += "1";
                        previousCheckIntentisty = 255;
                    }
                    else  //黑色用true
                    {
                        checkBlackWhiteCrossingPoint += "0";
                        previousCheckIntentisty = 0;
                    }
                }
                Console.WriteLine("Previous intensity = " + previousCheckIntentisty + "\n---------------");

                pixelSum = 0;
            }

        }

        private static bool DoBlackWhiteStatisticsByScanLine(List<LineSegment2DF> lines, Image<Gray, byte> source, Image<Bgr, byte> stasticDst)
        {
            string checkBlackWhiteCrossingPoint = "";
            Image<Bgr, byte> blackWhiteCurveImg = new Image<Bgr, byte>(480, 300, new Bgr(Color.White));
            int x = 0; // 要尋訪的起點
            IntensityPoint current, previous;
            current = new IntensityPoint();
            previous = new IntensityPoint();

            //記錄每一條線段的像素統計用的索引

            int pixelSum = 0;
            int previousPixelValue = -1;
            int previousCheckIntentisty = -1;

            //計算線段通過pixel
            foreach (LineSegment2DF line in lines)
            {
                float nextX;
                float nextY = line.P1.Y;

                //如果尋訪小於線段結束點的y軸，則不斷尋訪
                while (nextY < line.P2.Y)
                {

                    nextX = GetXPositionFromLineEquations(line.P1, line.P2, nextY);

                    //抓灰階 or 二值化做測試
                    Gray pixel = source[Convert.ToInt32(nextY), Convert.ToInt32(nextX)];
                    CheckBlackWhiteTexture(checkBlackWhiteCrossingPoint, pixel.Intensity, ref pixelSum, ref previousPixelValue, ref previousCheckIntentisty, ref checkBlackWhiteCrossingPoint);

                    //取得目前掃描線步進的素值
                    current.SetData(new PointF(nextX, nextY), pixel.Intensity);

                    DrawBlackWhiteCurve(stasticDst, current, previous, x);
                    //設定前一筆
                    previous.SetData(current.GetLocation(), current.GetIntensity());

                    Console.WriteLine("x:" + nextX + ",y:" + nextY + ",Intensity:" + pixel.Intensity);

                    //步進Y
                    nextY++;
                    //繪製用的步進值
                    x += 5;
                }

            }

            ImageViewer showBlackWhiteCurve = new ImageViewer(blackWhiteCurveImg, "Show Black and White Curve");
            showBlackWhiteCurve.Show();



            ////顯示所有check的狀況
            Console.WriteLine(checkBlackWhiteCrossingPoint);
            if (checkBlackWhiteCrossingPoint.Contains("010101") || checkBlackWhiteCrossingPoint.Contains("101010"))
            {
                Console.WriteLine("有交錯");
                return true;
            }
            else
                return false;
        }
        //計算直線方程式，並求x座標來取出圖片像素
        private static float GetXPositionFromLineEquations(PointF p1, PointF p2, float y)
        {
            float m = (p2.Y - p1.Y) / (float)(p2.X - p1.X);
            // y - y0 = m(x - x0)
            float x = ((y - p2.Y) / m) + p2.X;
            //Console.WriteLine("y =" + y + "and find x=" + x);
            return x;
        }
        #endregion



        /// <summary>
        /// 繪製黑白交錯的曲線(縱軸是Intensity,橫軸是排序的線段從最前面的線段到最下面的線段的座標)
        /// </summary>
        /// <param name="drawImg"></param>
        /// <param name="current">現在的座標像素</param>
        /// <param name="previous">前一個點的座標像素</param>
        /// <param name="x">x軸步近的值</param>
        private static void DrawBlackWhiteCurve(Image<Bgr, byte> drawImg, IntensityPoint current, IntensityPoint previous, int x)
        {
            //繪製呈現用，斑馬線黑白像素經過的圖形
            int projectY = Math.Abs((int)current.GetIntensity() - 300);
            if (!current.IsEmpty() && !previous.IsEmpty())
            {
                float prevPorjectY = Math.Abs((float)previous.GetIntensity() - 300);
                drawImg.Draw(new LineSegment2DF(new PointF(x - 2, projectY), new PointF(x, prevPorjectY)), new Bgr(Color.Green), 1);
            }
            else
            {
                drawImg.Draw(new LineSegment2DF(new PointF(0, 300), new PointF(x, projectY)), new Bgr(Color.Red), 1);
            }
            drawImg.Draw(new CircleF(new PointF(x, projectY), 1), new Bgr(Color.Blue), 2);

        }
    }
}
