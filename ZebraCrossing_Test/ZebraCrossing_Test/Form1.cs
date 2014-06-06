using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;

namespace ZebraCrossing_Test
{
    public partial class Form1 : Form
    {
        //接近水平線的線段修復，線段與線段之間的y高度的差距誤差(避免斑馬線上是兩個不同的y線段，但是因為拍攝的視角關係，被誤認為是同一條)
        const int HORIZATON_REPAIR_HOUGH_LINE_HEIGHT_THRESHOLD = 7;

        DirectoryInfo dir;
        Image<Bgr, byte> oriImg;
        Image<Gray, byte> grayImg;
        Image<Gray, byte> maskWhiteImg;
        ImageViewer houghLineViewer;
        ImageViewer ContoursAndScanLineViewer;
        List<LineSegment2D> candidateZebraCrossingsByHoughLine;
        //候選的斑馬線(BoundingBox的寬有大於100且高/寬是0.15)
        List<Rectangle> candidateZebraCrossingsByContour;
        //紀錄斑馬線之間白色連結起來的線段
        List<LineSegment2DF> crossingConnectionlines;
        //取得線偵測並過濾角度後的線段方程式
        LinkedList<LineEquation> candidateHoughLineEquations;
        //取得線偵測並過濾角度後的線段方程式(播放用)
        List<LineEquation> candidateHoughLineEquationsForReplay;

        //顯示繪製ScanLine的圖
        Image<Bgr, byte> showScanlineImg;

        //黑白像素是否交叉呈現
        bool isBlackWhiteCrossing;
       

        private static Bgr[] drawLineColos;

       
        int candiateLineEquation_i, candiateLineEquation_j;

        //步驟用
        ImageViewer repairedHoughLineStepViewer; //顯示修復線好段的步驟
        ImageViewer searchRepairHoughLineStepViewer; //顯示尋找各線段是否可以修復的步驟
        Image<Bgr, byte> showFinishedRepairedHoughLineStepImg; //存放修復好的步驟圖
        Image<Bgr, byte> showSearchrepairedHoughLineStepImg; //存放尋找要修復線段的步驟圖

        //非步驟用        
        ImageViewer repairedHoughLineViwer; //顯示全部修復好的線段
        Image<Bgr, byte> showFinishedRepairedHoughLineImg;
        
        List<LineSegment2D> repairedHoughLine; //紀錄修復後的所有線段

        Dictionary<string, LinkedList<LineEquation>> linesHistogram; //統計不同角度的線段並歸類(過濾非主流限段)
        int mainDirectionLineGroupId = 0; //紀錄主要線段的群組ID
        int maxLineLength = 0;
        //統計每一條線的黑色與白色的pixel數量
        string checkBlackWhiteCrossingPoint;

        public Form1()
        {
            InitializeComponent();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
            houghLineViewer = new ImageViewer();
            houghLineViewer.FormClosing += houghLineViewer_FormClosing;

            ContoursAndScanLineViewer = new ImageViewer();
            ContoursAndScanLineViewer.FormClosing += ContoursAndScanLineViewer_FormClosing;

            candidateZebraCrossingsByContour = new List<Rectangle>();
            //ScanLine的各線段
            crossingConnectionlines = new List<LineSegment2DF>();
            candidateZebraCrossingsByHoughLine = new List<LineSegment2D>();

            //預設是假設都為True
            isBlackWhiteCrossing = true;

            drawLineColos = new Bgr[]{
                new Bgr(255,0,0),
                new Bgr(255,255,0),
                new Bgr(0,255,0),
                new Bgr(0,255,255),
                new Bgr(0,0,255),
                new Bgr(255,0,255),
                new Bgr(255,0,128),
                new Bgr(128,0,128),
                new Bgr(128,0,255),
                new Bgr(128,255,255),
                new Bgr(128,128,255),
                new Bgr(255,128,128),
                new Bgr(255,128,0),
                new Bgr(255,128,0),
            };

           
            candiateLineEquation_i = 0;
            candiateLineEquation_j = candiateLineEquation_i + 1;
            
            repairedHoughLine = new List<LineSegment2D>();

            //非步驟
            candidateHoughLineEquations = new LinkedList<LineEquation>();
            repairedHoughLineViwer = new ImageViewer();
            repairedHoughLineViwer.FormClosing += repairedHoughLineViwer_FormClosing;

            //步驟
            //初始化 修復線段步驟的修復圖
            searchRepairHoughLineStepViewer = new ImageViewer();
            searchRepairHoughLineStepViewer.FormClosing += searchRepairHoughLineStepViewer_FormClosing;
            //初始化 修復線段步驟的尋找可否修復圖
            repairedHoughLineStepViewer = new ImageViewer();
            repairedHoughLineStepViewer.FormClosing += repairHoughLineViewer_FormClosing;

            candidateHoughLineEquationsForReplay = new List<LineEquation>();

            linesHistogram = new Dictionary<string, LinkedList<LineEquation>>();

           
        }

        void searchRepairHoughLineStepViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            searchRepairHoughLineStepViewer.Hide(); //隱藏式窗,下次再show出
        }

        void repairedHoughLineViwer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            repairedHoughLineViwer.Hide(); //隱藏式窗,下次再show出
        }

        void repairHoughLineViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            repairedHoughLineStepViewer.Hide(); //隱藏式窗,下次再show出
        }

        void ContoursAndScanLineViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            ContoursAndScanLineViewer.Hide(); //隱藏式窗,下次再show出
        }

        void houghLineViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            houghLineViewer.Hide(); //隱藏式窗,下次再show出
        }


        #region 開檔
        private string OpenLearningImgFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.FullName + @"\Crossing";
            dlg.Title = "Open Image File";

            // Set filter for file extension and default file extension
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|All Files(*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method ->ShowDialog()
            // Get the selected file name and display in a TextBox
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlg.FileName != null)
            {
                // Open document
                string filename = dlg.FileName;
                return filename;
            }
            else
            {
                return null;
            }
        }
        #endregion

        private void loadImgButton_Click(object sender, EventArgs e)
        {
            string filename = OpenLearningImgFile();
            if (filename !=null)
            {
                oriImg = new Image<Bgr, byte>(filename);
                oriImg = oriImg.Resize(640, 480, INTER.CV_INTER_LINEAR);
                oriImageBox.Image = oriImg;

                //清空先前的資料
                candidateZebraCrossingsByHoughLine.Clear();
                //清空之前的資料
                candidateZebraCrossingsByContour.Clear();
                //清空原先上一張偵測的圖
                crossingConnectionlines.Clear();
               
                isBlackWhiteCrossing = false;

              
                repairedHoughLine.Clear();
                showFinishedRepairedHoughLineStepImg = oriImg.Copy();
                linesHistogram.Clear();
                //Auto測驗
                //ToCrop();
                //ToGray();
                //MaskWhite();
                //filterPepper();
                //DoHoughLine("灰階");
                //Smooth();
                //DoHoughLine("灰階模糊");
            }
        }

        private void MaskWhite() {

            //oriImg 與 grayImg 測試
            if (grayImg != null)
            {
                try
                {
                    maskWhiteImg = new Image<Gray, byte>(new Size(grayImg.Width, grayImg.Height));
                    CvInvoke.cvInRangeS(grayImg, new MCvScalar(160, 160, 160), new MCvScalar(255, 255, 255), grayImg);
                    maskImageBox.Image = grayImg;
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }

            }
            else
            {
                MessageBox.Show("取消此功能");
            }
        
        }
        private void maskImgButton_Click(object sender, EventArgs e)
        {
            MaskWhite();
        }

        private void Smooth() {

            if (oriImg != null)
            {
                grayImg = oriImg.Convert<Gray, byte>();
                CvInvoke.cvSmooth(grayImg, grayImg, SMOOTH_TYPE.CV_GAUSSIAN, 13, 13, 1.5, 1);
                grayImgBox.Image = grayImg;
            }
            
        }

        //先灰階再模糊 以去除Mask時的雜訊白點，增加足夠有利的線段
        private void smoothButton_Click(object sender, EventArgs e)
        {
            Smooth();
        }

        private void ToGray() {
            if (oriImg != null)
            {
                grayImg = oriImg.Convert<Gray, byte>();
                grayImgBox.Image = grayImg;
            }
            else
            {
                MessageBox.Show("尚未剪裁");
            }
        }

        private void toGrayButton_Click(object sender, EventArgs e)
        {
            ToGray();
        }

        private void ToCrop()
        {
            if (oriImg != null)
            {
                oriImg = oriImg.Copy();
                oriImg.ROI = new Rectangle(new Point(0, oriImg.Height / 2), new Size(oriImg.Width, oriImg.Height / 2));
                oriImageBox.Image = oriImg;

                //先先取得要做houghline步驟的圖片
                showFinishedRepairedHoughLineStepImg = oriImg.Copy();
            }
            else
            {
                MessageBox.Show("尚未載入圖片");
            }
        }

        private void cropBottomButton_Click(object sender, EventArgs e)
        {
            ToCrop();
        }

        #region 圖像處理 去雜訊 膨脹
        private void Dilate() {
            if (grayImg != null)
            {
                //膨脹
                grayImg = grayImg.Dilate(1);

                filterImageBox.Image = grayImg;
            }
            else
            {
                MessageBox.Show("尚未Mask");
            }
        }
        private void dilateButton_Click(object sender, EventArgs e)
        {
            Dilate();
        }

        private void filterPepper() {
            if (grayImg != null)
            {
                //用中值濾波去雜訊
                grayImg = grayImg.SmoothMedian(3);
                filterImageBox.Image = grayImg;
            }
            else
            {
                MessageBox.Show("尚未Mask");
            }
        }

        private void filterPepperButton_Click(object sender, EventArgs e)
        {
            filterPepper();
        }
        #endregion

      

        private void DoHoughLine(string title) {
            #region 偵測整張Mask White圖片
            if (grayImg != null)
            {
                Image<Bgr, byte> onlyLineImg = new Image<Bgr, byte>(oriImg.Width, oriImg.Height, new Bgr(0, 0, 0));
                using (Image<Bgr, byte> showLineImg = oriImg.Copy())
                {
                    //Hough transform for line detection
                    LineSegment2D[][] lines = grayImg.HoughLines(
                        new Gray(125),  //Canny algorithm low threshold
                        new Gray(260),  //Canny algorithm high threshold
                        1,              //rho parameter
                        Math.PI / 180.0,  //theta parameter 
                        100,            //threshold
                        1,             //min length for a line
                        20);            //max allowed gap along the line

                    int colorIndex = 0;
                    //draw lines on image
                    foreach (var line in lines[0])
                    {
                        //如何限制角度http://yy-programer.blogspot.tw/2013/02/emgucv-image-process-extracting-lines_28.html
                        //vector是向量，代表的是這個線的方向。HoughLine是採用亟座標的方式
                        //線的點是在LineSegment2D這個結構裡的：P1與P2才是。﻿
                        PointF vector = line.Direction;

                        double slope = (line.P2.Y - line.P1.Y) / Convert.ToDouble(line.P2.X - line.P1.X);
                        double angle = Math.Atan2(vector.Y, vector.X) * 180.0 / Math.PI;
                        showLineImg.Draw(line, new Bgr(0, 0, 0), 2);
                        onlyLineImg.Draw(line, new Bgr(255, 255, 255), 2);
                        if ((angle > 160 && angle < 190) || (angle > -190 && angle < -160))
                        {
                            showLineImg.Draw(line, drawLineColos[(colorIndex % drawLineColos.Length)], 2);
                            onlyLineImg.Draw(line, drawLineColos[(colorIndex % drawLineColos.Length)], 2);
                            //加入候選線
                            candidateZebraCrossingsByHoughLine.Add(line);

                            
                        }
                        //計算並取得線段的直線方程式
                        LineEquation eqation = GetLineEquation(line);
                        candidateHoughLineEquations.AddLast(eqation);
                        //步驟用
                        candidateHoughLineEquationsForReplay.Add(new LineEquation() { A = eqation.A, B = eqation.B, C = eqation.C, Slope = eqation.Slope, Angle = eqation.Angle,AdjustAngle = eqation.AdjustAngle, Direction = eqation.Direction ,Line = eqation.Line });

                        Console.WriteLine("index =" + colorIndex + "Angle = " + eqation.Angle + ",Adjusted Angle = " + eqation.AdjustAngle + ", slope = " + slope + ", P1 = " + line.P1 + ", P2 = " + line.P2 + ", length = " + line.Length);


                        colorIndex++;
                    }

                    Console.WriteLine("total detect lines = " + lines[0].Length + "\n");

                    houghLineViewer.Image = showLineImg;
                    houghLineViewer.Text = "HoughLine 偵測畫面";
                    houghLineViewer.Show();
                    //new ImageViewer(showLineImg, title).Show();
                    
                    new ImageViewer(onlyLineImg, title).Show();


                }

            }
            #endregion
        }

        private void houghLineButton_Click(object sender, EventArgs e)
        {
            candidateHoughLineEquations.Clear();
            candidateHoughLineEquationsForReplay.Clear();

            DoHoughLine("純線段方便觀看");
        }
        


        private LineEquation GetLineEquation(LineSegment2D line) {
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
            return new LineEquation() { A = a, B = b, C = c, Slope = m, Angle = angle, AdjustAngle = adjustAngle,Direction = direction ,Line = line };
        }

        //重建接近水平線段
        private LineSegment2D RepaiedHorizontalHoughLine(Point[] ps) {
            Point leftPoint = ps[0];
            Point rightPoint = ps[0];
            for (int i = 1; i < ps.Length; i++)
            {
                if (ps[i].X <= leftPoint.X)
                    leftPoint = ps[i];
                if (ps[i].X >= rightPoint.X)
                    rightPoint = ps[i];
            }
            return new LineSegment2D(leftPoint,rightPoint);
        }

        //重建接近垂直線段
        private LineSegment2D RepaiedVerticalHoughLine(Point[] ps)
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
        private bool CheckHorizontalIntersectionPoint(Point[] ps, int intersect_x, int intersect_y)
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
        private bool CheckVerticalIntersectionPoint(Point[] ps, int intersect_x, int intersect_y)
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
        private bool Intersect(LineEquation line1, LineEquation line2, out int x, out int y,ref LineSegment2D repaiedLine)
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
            float area1 = Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) ;
            float area2 = Math.Abs(x1 * (y2 - y4) + x2 * (y4 - y1) + x4 * (y1 - y2));
            if (area1 <= 1000 && area2 <=1000 && Math.Abs(y3 - y1) < 6  &&  Math.Abs(y4 - y1) < 6 )
            // Math.Abs(y3 - y1) < 8 => y3 - y1表示距離
            {
                Console.WriteLine("共線" + "\n");
                x = -1;
                y = -1;
                repaiedLine = RepaiedHorizontalHoughLine(points);
                return true;
            }
            else if (v != 0)
            {  //代表兩條線段方程式不是平行,y3 - y1表示距離
                Console.Write("相交");
                //Console.WriteLine("v = " + v);
                //兩條線相似
                Console.WriteLine("線段一原角度：" + line1.Angle + ",修正角度:" + line1.AdjustAngle + "\n線段原二角度：" + line2.Angle + ",修正角度" + line2.AdjustAngle);
                double angleDifference =  Math.Abs(line1.AdjustAngle - line2.AdjustAngle);
                Console.WriteLine("兩條線的角度差為：" + angleDifference);
                if (angleDifference< 15)
                {
                    Console.WriteLine("兩條線可能是可以銜接");
                    float delta_x = (line1.C * line2.B) - line2.C * line1.B;
                    float delta_y = (line1.A * line2.C) - line2.A * line1.C;

                    x = Convert.ToInt32(delta_x / v);
                    y = Convert.ToInt32(delta_y / v);


                    if ((x < 0 || x > oriImg.Width || y < 0 || y > oriImg.Height))
                    {
                        Console.WriteLine("所以超出畫面");
                        x = -1;
                        y = -1;
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
                            if (!CheckHorizontalIntersectionPoint(points, x, y))
                                return false;
                            Console.WriteLine("接近水平的線段,且交點有在兩線段內");
                        }
                        else if (line1_angle <= 150 && line1_angle >= 120 && line2_angle <= 150 && line2_angle >= 120 && line1.Direction == line2.Direction)
                        {
                            //斜45度
                            if (!CheckVerticalIntersectionPoint(points, x, y) && !CheckHorizontalIntersectionPoint(points, x, y))
                                return false;
                            Console.WriteLine("接近斜45度或135度的線段,且交點有在兩線段內");
                        }
                        else if (line1_angle < 120 && line2_angle < 120 && line1.Direction == line2.Direction) //接近垂直
                        {
                            if (!CheckVerticalIntersectionPoint(points, x, y))
                                return false;
                            Console.WriteLine("接近垂直的線段,且交點有在兩線段內");
                        }

                        Console.Write("\n");
                        repaiedLine = RepaiedHorizontalHoughLine(points);
                        return true;
                    }
                }
                else{
                    Console.WriteLine("但是角度差異過大，研判不是\n");
                    x = -1;
                    y = -1;
                    return false;
                }

                //Console.WriteLine("intersect x = " + x + ",intersect y = " + y + "\n");
              
            }
            else
            {
                x = -1;
                y = -1;
                Console.WriteLine("平行" + "\n");
                return false;
            }
            
        }

        private void restructLineButton_Click(object sender, EventArgs e)
        {
            showFinishedRepairedHoughLineImg = oriImg.Copy();
            int i = 0;
            //ToArray避開刪除後的List長度問題
            LinkedListNode<LineEquation> node = candidateHoughLineEquations.First;
            while (node != null) {
                LinkedListNode<LineEquation> matchedNode = node.Next;
                bool intersect = false;
                while (matchedNode != null) {
                    int x = 0, y = 0;
                    //是否共線或是相交的線段
                    LineSegment2D repairedLine = new LineSegment2D();
                    intersect = Intersect(node.Value, matchedNode.Value, out x, out y, ref repairedLine);
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
                //並把這次比過的線段畫上去
                showFinishedRepairedHoughLineImg.Draw(node.Value.Line, drawLineColos[(i % drawLineColos.Length)], 2);
                //下一個Node
                node = node.Next;
                i++;
            }

            repairedHoughLineViwer.Image = showFinishedRepairedHoughLineImg;
            repairedHoughLineViwer.Text = "HoughLine 修復完成";
            if (repairedHoughLineViwer.Visible)
            {
                repairedHoughLineViwer.Hide(); //隱藏式窗,下次再show出
            }
            repairedHoughLineViwer.Show();
        }


        private void replayRestrcutLinesButton_Click(object sender, EventArgs e)
        {
            showSearchrepairedHoughLineStepImg = new Image<Bgr, byte>(oriImg.Width, oriImg.Height, new Bgr(Color.Black));
            //透過點擊按鈕來實現一步一步的迴圈方式觀看
            if (candiateLineEquation_i < candidateHoughLineEquationsForReplay.Count)
            {
                bool interset = false;
                if (candiateLineEquation_j < candidateHoughLineEquationsForReplay.Count)
                {
                    
                    int x = 0, y = 0;
                    LineSegment2D repairedLine = new LineSegment2D();
                    //預設資料都是0
                    Console.WriteLine(repairedLine.Length + "," + repairedLine.P1 + "," + repairedLine.P2);

                    //繪製正在比較有無共線或是相交的線段
                    showSearchrepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line, drawLineColos[(candiateLineEquation_i % drawLineColos.Length)], 1);
                    showSearchrepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_j].Line, drawLineColos[(candiateLineEquation_j % drawLineColos.Length)], 1);
                    
                    //判斷是否共線或是相交的線段
                    interset = Intersect(candidateHoughLineEquationsForReplay[candiateLineEquation_i], candidateHoughLineEquationsForReplay[candiateLineEquation_j], out x, out y, ref repairedLine);
                    if (interset)
                    {
                        showSearchrepairedHoughLineStepImg.Draw(new CircleF(new PointF(x, y), 1), new Bgr(255, 255, 255), 1);

                        //改掉原先的線段
                        candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line = repairedLine;
                        //並刪除被比較的線段
                        candidateHoughLineEquationsForReplay.RemoveAt(candiateLineEquation_j);
                    }
                     candiateLineEquation_j++;

                  
                    repairedHoughLineStepViewer.Image = showFinishedRepairedHoughLineStepImg;
                    searchRepairHoughLineStepViewer.Image = showSearchrepairedHoughLineStepImg;
                }
                else
                {
                    //並把這次比過的線段畫上去
                    showFinishedRepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line, drawLineColos[(candiateLineEquation_i % drawLineColos.Length)], 2);
                    //換到下一條比對的線段
                    candiateLineEquation_i++;
                    //都從0開始比，並跳過自己
                    candiateLineEquation_j = candiateLineEquation_i +1;
                    
                }

            }
            else
            {
                MessageBox.Show("檢測完畢");
                candiateLineEquation_i = 0;
                candiateLineEquation_j = candiateLineEquation_i + 1;
                showSearchrepairedHoughLineStepImg = new Image<Bgr, byte>(oriImg.Width, oriImg.Height, new Bgr(Color.Black));
            }


            repairedHoughLineStepViewer.Text = "HoughLine 修復檢測完成";
            if (repairedHoughLineStepViewer.Visible)
            {
                repairedHoughLineStepViewer.Hide(); //隱藏式窗,下次再show出
            }
            repairedHoughLineStepViewer.Show();

            searchRepairHoughLineStepViewer.Text = "HoughLine 修復檢測";
            if (searchRepairHoughLineStepViewer.Visible)
            {
                searchRepairHoughLineStepViewer.Hide(); //隱藏式窗,下次再show出
            }
            searchRepairHoughLineStepViewer.Show();
            
        }

        #region 量化分類
        private void QuquantifyLinesByAngle()
        {
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


        }
        #endregion
     
        private void filterLineButton_Click(object sender, EventArgs e)
        {
            QuquantifyLinesByAngle();
           
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
                    Console.WriteLine("佔全部線段的比例 =>" + ratio );
                }
                else
                    Console.WriteLine("line[" + i + "]:" + 0);
            }
            Console.WriteLine("主方向群為:" + mainDirectionLineGroupId + "佔全部線段的比例 =>" + mainDirectionRatio );
              
            //過濾過短的線段
            //1.找最大
            var maxLengthLine = (from selectLine in linesHistogram[mainDirectionLineGroupId.ToString()]
                                 select selectLine).Max(line => line.Line.Length);
            Console.WriteLine("最長的線段為:" + maxLengthLine);
            //2.依照比例把過段的濾掉
            LinkedListNode<LineEquation> node = linesHistogram[mainDirectionLineGroupId.ToString()].First;
            while(node!= null){
                if (node.Value.Line.Length < (maxLengthLine / 3))
                {
                    linesHistogram[mainDirectionLineGroupId.ToString()].Remove(node);
                    Console.WriteLine("移除的線段為:\nLength:" + node.Value.Line.Length + ",p1:" + node.Value.Line.P1 +",p2:" + node.Value.Line.P2);
                }
                node = node.Next;
            }

            
        }

        /// <summary>
        /// 繪製黑白交錯的曲線(縱軸是Intensity,橫軸是排序的線段從最前面的線段到最下面的線段的座標)
        /// </summary>
        /// <param name="drawImg"></param>
        /// <param name="current">現在的座標像素</param>
        /// <param name="previous">前一個點的座標像素</param>
        /// <param name="x">x軸步近的值</param>
        private void DrawBlackWhiteCurve(Image<Bgr, byte> drawImg, IntensityPoint current, IntensityPoint previous,int x) 
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

        /// <summary>
        /// 判斷紋路是否黑白交錯
        /// </summary>
        /// <param name="checkPoints">記錄每一條線的黑白是否交錯的格式</param>
        /// <param name="intensity">當前像素亮度</param>
        /// <param name="lineIndex">目前的線段索引</param>
        /// <param name="pixelSum">黑或白的像素總和(有連續兩個)</param>
        /// <param name="previousPixelValue">前一個像素的值(來判斷當下與前一個像素的數值一致否)</param>
        private void CheckBlackWhiteTexture(string checkPoints, double intensity, ref int pixelSum, ref int previousPixelValue, ref int previousCheckIntentisty)
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
                        checkBlackWhiteCrossingPoint +="1";
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

        private void analyzeBlackWhiteButton_Click(object sender, EventArgs e)
        {

            Point prePoint = new Point();
            Point currentPoint = new Point();
            Image<Bgr, byte> scanLineImg = oriImg.Clone();
         
            IntensityPoint current, previous;
            current = new IntensityPoint();
            previous = new IntensityPoint();
           
            int index = 0;
            List<LineEquation> orderedLines=new List<LineEquation>();
            //角度幾乎呈垂直,所以排序用x軸
            if ((17 <= mainDirectionLineGroupId && mainDirectionLineGroupId <=18) || (7 <= mainDirectionLineGroupId && mainDirectionLineGroupId <= 9)){
                var orderedMainLines = from line in linesHistogram[mainDirectionLineGroupId.ToString()] orderby (line.Line.P1.X + line.Line.P2.X) / 2 select line;
                foreach (LineEquation line in orderedMainLines) {
                    orderedLines.Add(line);
                }
            }
            else {
                var orderedMainLines = from line in linesHistogram[mainDirectionLineGroupId.ToString()] orderby (line.Line.P1.Y + line.Line.P2.Y) / 2  select line;
                foreach (LineEquation line in orderedMainLines){
                    orderedLines.Add(line);
                }
            }

            foreach (LineEquation line in orderedLines){
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
                    scanLineImg.Draw(new LineSegment2DF(prePoint, currentPoint), drawLineColos[(index % drawLineColos.Length)], 2);
                }
                
                Console.WriteLine("-------------------------------------");
                index++;
            }
            ImageViewer showScanLine = new ImageViewer(scanLineImg, "Scan Line");
            showScanLine.Show();

            //統計黑白像素與判斷是否每條線段為白黑白的特徵
            DoBlackWhiteStatisticsByScanLine(crossingConnectionlines);

            if (isBlackWhiteCrossing && linesHistogram[mainDirectionLineGroupId.ToString()].Count > 3) {
                MessageBox.Show("找到斑馬線");
            }

        }
        private void DoBlackWhiteStatisticsByScanLine(List<LineSegment2DF> lines)
        {
            checkBlackWhiteCrossingPoint = "";
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
                    Gray pixel = grayImg[Convert.ToInt32(nextY), Convert.ToInt32(nextX)];
                    CheckBlackWhiteTexture(checkBlackWhiteCrossingPoint, pixel.Intensity, ref pixelSum, ref previousPixelValue, ref previousCheckIntentisty);

                    //取得目前掃描線步進的素值
                    current.SetData(new PointF(nextX, nextY), pixel.Intensity);

                    DrawBlackWhiteCurve(blackWhiteCurveImg, current, previous, x);
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
                isBlackWhiteCrossing = true;
            }

        }
        //計算直線方程式，並求x座標來取出圖片像素
        private float GetXPositionFromLineEquations(PointF p1, PointF p2, float y)
        {
            float m = (p2.Y - p1.Y) / (float)(p2.X - p1.X);
            // y - y0 = m(x - x0)
            float x = ((y - p2.Y) / m) + p2.X;
            //Console.WriteLine("y =" + y + "and find x=" + x);
            return x;
        }
        #region 尋找斑馬線的黑白特徵(輪廓法,舊方法)
        //////////////////////////////////////////////////////////////


        
        
        private void contourButton_Click(object sender, EventArgs e)
        {
            if (maskWhiteImg != null)
            {


                Contour<Point> contours = DoContours(maskWhiteImg);
                using (Image<Bgr, byte> showContoursImg = oriImg.Copy())
                {
                    //繪製所有輪廓
                    while (contours.HNext != null)
                    {
                        //繪製輪廓BoundingBox
                        showContoursImg.Draw(contours.BoundingRectangle, new Bgr(Color.Red), 2);
                        double ratio = Convert.ToDouble(contours.BoundingRectangle.Height) / contours.BoundingRectangle.Width;
                        //斑馬線的boundingBox寬要大於100,寬高比值 < 0.15
                        if (contours.BoundingRectangle.Width > 100 && ratio < 0.15)
                        {
                            showContoursImg.Draw(contours.BoundingRectangle, new Bgr(Color.Yellow), 1);
                            //加入候選斑馬線
                            candidateZebraCrossingsByContour.Add(contours.BoundingRectangle);

                        }

                        Console.WriteLine("Width = " + contours.BoundingRectangle.Width + ",Height = " + contours.BoundingRectangle.Height + ",h/w = " + ratio);
                        //繪製輪廓
                        //showContoursImg.Draw(contours, new Bgr(Color.Yellow), new Bgr(Color.GreenYellow), 1, 2);
                        contours = contours.HNext;
                    }
                    Console.WriteLine("Total Candidate Contours = " + candidateZebraCrossingsByContour.Count);
                    showScanlineImg = showContoursImg.Copy();
                    //contourImageBox.Image = showContoursImg;
                }
            }
            else
            {
                MessageBox.Show("尚未Mask");
            }

        }

        #region 取輪廓
        //////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 從圖像中取得所有的輪廓
        /// </summary>
        /// <param name="srcImg">來源圖像,這邊是二值化侵蝕膨脹後的圖像</param>
        /// <returns>回傳輪廓</returns>
        public Contour<Point> DoContours(Image<Gray, Byte> srcImg)
        {
            Contour<Point> objectContours = srcImg.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_LIST);
            //Contour<Point> objectContours = srcImg.FindContours();
            return objectContours;
        }
        /// <summary>
        /// 取得輪廓資料中的最大輪廓,若要取得最大輪廓的BoundingBox,使用contours.BoundingBox
        /// </summary>
        /// <param name="contours">輸入從圖像中取得的所有輪廓</param>
        /// <returns>回傳最大面積的輪廓</returns>
        public Contour<Point> GetMaxContours(Contour<Point> contours)
        {
            Contour<Point> MaxContour = contours;
            while (contours.HNext != null)
            {
                if (MaxContour.Area < contours.HNext.Area)
                {
                    MaxContour = contours.HNext;
                }
                contours = contours.HNext;
            }
            return MaxContour;
        }
        public Image<Bgr, Byte> GetBoundingBoxImage(Contour<Point> contours, Image<Bgr, Byte> sceneImg)
        {
            Image<Bgr, Byte> roiImage = sceneImg.GetSubRect(contours.BoundingRectangle);
            return roiImage;
        }
        /// <summary>
        /// 劃出所有輪廓到圖像上
        /// </summary>
        /// <param name="contours">取得的輪廓</param>
        /// <param name="drawImg">要畫到的圖像上</param>
        /// <returns>回傳畫上輪廓的圖像</returns>
        public Image<Bgr, Byte> DrawAllContoursOnImg(Contour<Point> contours, Image<Bgr, Byte> drawImg)
        {
            drawImg.Draw(contours, new Bgr(Color.Red), new Bgr(Color.Yellow), 1, 2);
            return drawImg;
        }
        /// <summary>
        /// 畫上最大的輪廓到圖像上
        /// </summary>
        /// <param name="maxContour">最大的輪廓</param>
        /// <param name="drawImg">要畫到的圖像上</param>
        /// <returns>回傳畫上輪廓的圖像</returns>
        public Image<Bgr, Byte> DrawMaxContoursOnImg(Contour<Point> maxContour, Image<Bgr, Byte> drawImg)
        {
            drawImg.Draw(maxContour, new Bgr(Color.Red), new Bgr(Color.Yellow), 1, 2);
            return drawImg;
        }
        /// <summary>
        /// 畫上最大輪廓的BoundimgBox
        /// </summary>
        /// <param name="maxContour">最大的輪廓</param>
        /// <param name="drawImg">要畫到的圖像上</param>
        /// <returns>回傳畫上最大輪廓的BoundingBox的圖像</returns>
        public Image<Bgr, Byte> DrawContoursMaxBoundingBoxOnImg(Contour<Point> maxContour, Image<Bgr, Byte> drawImg)
        {
            drawImg.Draw(maxContour.BoundingRectangle, new Bgr(Color.Red), 2);
            return drawImg;
        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        private void findScanLineButton_Click(object sender, EventArgs e)
        {

            Point prePoint = new Point();
            Point currentPoint = new Point();

            //依照y軸座標排序
            var zebras = from boundingBox in candidateZebraCrossingsByContour orderby boundingBox.Y select boundingBox;
            foreach (Rectangle rec in zebras)
            {
                if (!currentPoint.IsEmpty)
                    prePoint = currentPoint;
                currentPoint = new Point((rec.X + rec.Width / 2), (rec.Y + rec.Height / 2));

                //兩點 =>存放線條,並繪製
                if (!currentPoint.IsEmpty && !prePoint.IsEmpty)
                {
                    LineSegment2DF line = new LineSegment2DF(prePoint, currentPoint);
                    //記錄每一條線段
                    crossingConnectionlines.Add(line);
                    Console.WriteLine("draw Line:direction ,x = " + line.Direction.X + "y =" + line.Direction.Y + ",point p1.x =" + prePoint.X + ",p1.y = " + prePoint.Y + ", p2.x =" + currentPoint.X + ",p2.y = " + currentPoint.Y);
                    showScanlineImg.Draw(new LineSegment2DF(prePoint, currentPoint), new Bgr(Color.Azure), 2);
                }
                Console.WriteLine("center x =" + currentPoint.X + ",y = " + currentPoint.Y);
                showScanlineImg.Draw(new CircleF(currentPoint, 1), new Bgr(Color.Blue), 3);

            }
            //show center point
            //contourImageBox.Image = showScanlineImg;
            //ImageViewer
            ContoursAndScanLineViewer.Image = showScanlineImg;
            ContoursAndScanLineViewer.Show();

            //統計黑白像素與判斷是否每條線段為白黑白的特徵
            DoBlackWhiteStatisticsByScanLine(crossingConnectionlines);
        }

      
        #endregion
    }


  

    
   
}
