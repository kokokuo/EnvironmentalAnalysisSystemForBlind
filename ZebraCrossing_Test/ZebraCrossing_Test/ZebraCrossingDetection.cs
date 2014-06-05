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
    public class ZebraCrossingDetection
    {
        //原始影像
        Image<Bgr, byte> oriImg; 
        //處理影像
        Image<Gray, byte> processImg;
        LinkedList<LineEquation> candidateHoughLineEquations;
     
        public ZebraCrossingDetection() {

        }
        public Image<Bgr, byte> LoadImg(string filename)
        {
            if (filename != null)
            {
                oriImg = new Image<Bgr, byte>(filename);
                oriImg = oriImg.Resize(480, 360, INTER.CV_INTER_LINEAR);
                return oriImg;
            }
            else {
                return null;
            }
        }

        public Image<Gray, byte> ToGray(Image<Bgr, byte> source) {
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

        public Image<Bgr, byte> ToCrop(Image<Bgr, byte> source)
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

        public Image<Gray, byte> MaskWhite(Image<Gray, byte> source)
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


        public Image<Gray, byte> Smooth(Image<Gray, byte> source)
        {

            if (source != null)
            {
                CvInvoke.cvSmooth(source, source, SMOOTH_TYPE.CV_GAUSSIAN, 13, 13, 1.5, 1);
                return source;
            }
            else
                return null;
        }

        #region 偵測線段
        public LinkedList<LineEquation> DetectHoughLine(Image<Gray, byte> source, bool showExperientResult)
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
        

        #region 量化分類
        private Dictionary<string, LinkedList<LineEquation>> QuquantifyLinesByAngle(LinkedList<LineEquation>  candidateHoughLineEquations)
        {
            Dictionary<string, LinkedList<LineEquation>> linesHistogram = new Dictionary<string, LinkedList<LineEquation>>();
            //統計線段
            foreach (LineEquation line in candidateHoughLineEquations)
            {
                //量化分類
                if (170 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) <= 180)
                {
                    if (!linesHistogram.ContainsKey("0"))
                    {
                        linesHistogram.Add("0", new LinkedList<LineEquation>());
                        linesHistogram["0"].AddLast(line);
                    }
                    else
                        linesHistogram["0"].AddLast(line);
                }
                else if (160 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 170 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("1"))
                    {
                        linesHistogram.Add("1", new LinkedList<LineEquation>());
                        linesHistogram["1"].AddLast(line);
                    }
                    else
                        linesHistogram["1"].AddLast(line);
                }
                else if (160 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 170 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("2"))
                    {
                        linesHistogram.Add("2", new LinkedList<LineEquation>());
                        linesHistogram["2"].AddLast(line);
                    }
                    else
                        linesHistogram["2"].AddLast(line);
                }
                else if (150 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 160 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("3"))
                    {
                        linesHistogram.Add("3", new LinkedList<LineEquation>());
                        linesHistogram["3"].AddLast(line);
                    }
                    else
                        linesHistogram["3"].AddLast(line);
                }
                else if (140 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 150 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("4"))
                    {
                        linesHistogram.Add("4", new LinkedList<LineEquation>());
                        linesHistogram["4"].AddLast(line);
                    }
                    else
                        linesHistogram["4"].AddLast(line);
                }
                else if (130 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 140 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("5"))
                    {
                        linesHistogram.Add("5", new LinkedList<LineEquation>());
                        linesHistogram["5"].AddLast(line);
                    }
                    else
                        linesHistogram["5"].AddLast(line);
                }
                else if (120 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 130 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("6"))
                    {
                        linesHistogram.Add("6", new LinkedList<LineEquation>());
                        linesHistogram["6"].AddLast(line);
                    }
                    else
                        linesHistogram["6"].AddLast(line);
                }
                else if (110 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 120 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("7"))
                    {
                        linesHistogram.Add("7", new LinkedList<LineEquation>());
                        linesHistogram["7"].AddLast(line);
                    }
                    else
                        linesHistogram["7"].AddLast(line);
                }
                else if (100 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 110 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("8"))
                    {
                        linesHistogram.Add("8", new LinkedList<LineEquation>());
                        linesHistogram["8"].AddLast(line);
                    }
                    else
                        linesHistogram["8"].AddLast(line);
                }
                else if (90 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 100 && line.Direction == 1)
                {
                    if (!linesHistogram.ContainsKey("9"))
                    {
                        linesHistogram.Add("9", new LinkedList<LineEquation>());
                        linesHistogram["9"].AddLast(line);
                    }
                    else
                        linesHistogram["9"].AddLast(line);
                }
                else if (160 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 170 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("10"))
                    {
                        linesHistogram.Add("10", new LinkedList<LineEquation>());
                        linesHistogram["10"].AddLast(line);
                    }
                    else
                        linesHistogram["10"].AddLast(line);
                }
                else if (160 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 170 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("11"))
                    {
                        linesHistogram.Add("11", new LinkedList<LineEquation>());
                        linesHistogram["11"].AddLast(line);
                    }
                    else
                        linesHistogram["11"].AddLast(line);
                }
                else if (150 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 160 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("12"))
                    {
                        linesHistogram.Add("12", new LinkedList<LineEquation>());
                        linesHistogram["12"].AddLast(line);
                    }
                    else
                        linesHistogram["12"].AddLast(line);
                }
                else if (140 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 150 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("13"))
                    {
                        linesHistogram.Add("13", new LinkedList<LineEquation>());
                        linesHistogram["13"].AddLast(line);
                    }
                    else
                        linesHistogram["13"].AddLast(line);
                }
                else if (130 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 140 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("14"))
                    {
                        linesHistogram.Add("14", new LinkedList<LineEquation>());
                        linesHistogram["14"].AddLast(line);
                    }
                    else
                        linesHistogram["14"].AddLast(line);
                }
                else if (120 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 130 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("15"))
                    {
                        linesHistogram.Add("15", new LinkedList<LineEquation>());
                        linesHistogram["15"].AddLast(line);
                    }
                    else
                        linesHistogram["15"].AddLast(line);
                }
                else if (110 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 120 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("16"))
                    {
                        linesHistogram.Add("16", new LinkedList<LineEquation>());
                        linesHistogram["16"].AddLast(line);
                    }
                    else
                        linesHistogram["16"].AddLast(line);
                }
                else if (100 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 110 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("17"))
                    {
                        linesHistogram.Add("17", new LinkedList<LineEquation>());
                        linesHistogram["17"].AddLast(line);
                    }
                    else
                        linesHistogram["17"].AddLast(line);
                }
                else if (90 <= Math.Abs(line.Angle) && Math.Abs(line.Angle) < 100 && line.Direction == -1)
                {
                    if (!linesHistogram.ContainsKey("18"))
                    {
                        linesHistogram.Add("18", new LinkedList<LineEquation>());
                        linesHistogram["18"].AddLast(line);
                    }
                    else
                        linesHistogram["18"].AddLast(line);
                }
            }

            return linesHistogram;
        }
        #endregion

        #region 過濾線段
        public Dictionary<string, LinkedList<LineEquation>> MainGroupLineFilter(LinkedList<LineEquation> candidateHoughLineEquations, ref int mainDirectionLineGroupId)
        {
            Dictionary<string, LinkedList<LineEquation>> linesHistogram =  QuquantifyLinesByAngle(candidateHoughLineEquations);

            float mainDirectionRatio = 0;
            //過濾線段
            int total = candidateHoughLineEquations.Count;
            Console.WriteLine("Total Lines = " + total);
            for (int i = 0; i < 19; i++)
            {
                if (linesHistogram.ContainsKey(i.ToString()))
                {
                    Console.WriteLine("line[" + i + "]:" + linesHistogram[i.ToString()].Count);
                    float ratio = (linesHistogram[i.ToString()].Count / (float)total);
                    if (mainDirectionRatio < ratio)
                    {
                        mainDirectionRatio = ratio;
                        mainDirectionLineGroupId = i;
                    }
                    Console.WriteLine("佔全部線段的比例 =>" + ratio);
                }
                else
                    Console.WriteLine("line[" + i + "]:" + 0);
            }
            Console.WriteLine("主方向群為:" + mainDirectionLineGroupId + "佔全部線段的比例 =>" + mainDirectionRatio);

            //過濾過短的線段
            //1.找最大
            var maxLengthLine = (from selectLine in linesHistogram[mainDirectionLineGroupId.ToString()]
                                 select selectLine).Max(line => line.Line.Length);
            Console.WriteLine("最長的線段為:" + maxLengthLine);
            //2.依照比例把過段的濾掉
            LinkedListNode<LineEquation> node = linesHistogram[mainDirectionLineGroupId.ToString()].First;
            while (node != null)
            {
                if (node.Value.Line.Length < (maxLengthLine / 3))
                {
                    linesHistogram[mainDirectionLineGroupId.ToString()].Remove(node);
                    Console.WriteLine("移除的線段為:\nLength:" + node.Value.Line.Length + ",p1:" + node.Value.Line.P1 + ",p2:" + node.Value.Line.P2);
                }
                node = node.Next;
            }

            return linesHistogram;
        }
        #endregion

        #region 修復線段
        public LinkedList<LineEquation> RepairedLines(LinkedList<LineEquation> candidateHoughLineEquations, Image<Bgr, byte> source)
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
                    int x = 0, y = 0;
                    //是否共線或是相交的線段
                    LineSegment2D repairedLine = new LineSegment2D();
                    intersect = Intersect(node.Value, matchedNode.Value, out p, ref repairedLine, source);
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
        private static bool Intersect(LineEquation line1, LineEquation line2, out Point intersectP, ref LineSegment2D repaiedLine, Image<Bgr, byte> source)
        {
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
        
    }
}
