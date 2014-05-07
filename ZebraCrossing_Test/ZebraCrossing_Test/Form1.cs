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
        //黑色像素是否增加
        bool isBlackPixelIncreased;

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
            isBlackWhiteCrossing = isBlackPixelIncreased = true;

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
                //預設是假設都為True
                isBlackWhiteCrossing = isBlackPixelIncreased = true;

              
                repairedHoughLine.Clear();
                
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
                        110,            //threshold
                        1,             //min length for a line
                        30);            //max allowed gap along the line

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
                        candidateHoughLineEquationsForReplay.Add(new LineEquation() { A = eqation.A, B = eqation.B, C = eqation.C, Slope = eqation.Slope, Angle = eqation.Angle, Line = eqation.Line });

                        Console.WriteLine("index =" + colorIndex + "Angle = " + angle + ", slope = " + slope + ", P1 = " + line.P1 + ", P2 = " + line.P2 + ", length = " + line.Length);


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
            //http://dufu.math.ncu.edu.tw/calculus/calculus_bus/node11.html
            //Console.WriteLine("a =" + a + ",b = " + b + ",c = " + c);
            return new LineEquation() { A = a, B = b, C = c, Slope = m, Angle = angle, Line = line };
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
            int x3 = line2.Line.P2.X;
            int y3 = line2.Line.P2.Y;
            
            float v = (line1.A * line2.B) - line2.A * line1.B;
            //Console.WriteLine("line1 slope = " + line1.Slope + ", line 2 slope = " + line2.Slope);
            //Console.WriteLine("line1 P1 = " + line1.Line.P1 + ",line1 P2 = " + line1.Line.P2 + ", length = " + line1.Line.Length);
            //Console.WriteLine("line2 P1 = " + line2.Line.P1 + ",line2 P2 = " + line2.Line.P2 + ", length = " + line2.Line.Length);
            //不太可能會有共線,需要給一個Range
            //面積公式來看 1/2 * x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2) 如果小於1000可以是
            //or 用A-B 與 A-C的斜率去看斜率誤差 約接近0表示共線高
            Console.WriteLine("面積公式 = " + Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) + " y3 -y1 = " + Math.Abs(y3 - y1));
            float p1p2Slope = (y2 - y1) / (float)(x2 - x1);
            float p1p3Slope = (y3 - y1) / (float)(x3 - x1);
            //Console.WriteLine("Slope p1 -> p2 = " +p1p2Slope+ ", Slope p1 -> p3 ="+ p1p3Slope + "差距值 = " + Math.Abs(Math.Abs(p1p2Slope) - Math.Abs(p1p3Slope)));

            //尋找兩端點
            Point[] points = new Point[] { line1.Line.P1, line1.Line.P2, line2.Line.P1, line2.Line.P2 };
            float area = Math.Abs(x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) ;
            if (area <= 1000 && Math.Abs(y3-y1) < 8 ) 
             //y3 - y1表示距離
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
                Console.WriteLine("線段一角度：" + Math.Abs(line1.Angle) + ",線段二角度：" + Math.Abs(line2.Angle));
                Console.WriteLine("兩條線的角度差為：" + Math.Abs(Math.Abs(line1.Angle) - Math.Abs(line2.Angle)));
                if (Math.Abs(Math.Abs(line1.Angle) - Math.Abs(line2.Angle)) < 15)
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
                        
                        double line1_angle = Math.Abs(line1.Angle);
                        double line2_angle = Math.Abs(line2.Angle);
                        
                        //接近平行線
                        if (line1_angle > 150 && line2_angle > 150 )
                        {
                            if (!CheckHorizontalIntersectionPoint(points, x, y))
                                return false;
                            Console.WriteLine("接近水平的線段,且交點有在兩線段內");
                        }
                        else if (line1_angle <= 150 && line1_angle >= 120 && line2_angle <= 150 && line2_angle >= 120)
                        {
                            //斜45度
                            if (!CheckVerticalIntersectionPoint(points, x, y) && !CheckHorizontalIntersectionPoint(points, x, y))
                                return false;
                            Console.WriteLine("接近斜45度或135度的線段,且交點有在兩線段內");
                        }
                        else if (line1_angle < 120 && line2_angle < 120) //接近垂直
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
                    ////如果是自己則跳過
                    //if (candiateLineEquation_j + 1 == candiateLineEquation_i)
                    //{
                    //    candiateLineEquation_j += 2;

                    //}
                    //else
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

        #region 尋找斑馬線的黑白特徵
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
                    contourImageBox.Image = showContoursImg;
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
            contourImageBox.Image = showScanlineImg;
            //ImageViewer
            ContoursAndScanLineViewer.Image = showScanlineImg;
            ContoursAndScanLineViewer.Show();

            //統計黑白像素與判斷是否每條線段為白黑白的特徵
            DoBlackWhiteStatistics(crossingConnectionlines);
        }

        private void DoBlackWhiteStatistics(List<LineSegment2DF> lines)
        {
            //寬480是圖片高(等於垂直走訪的話,最多的pixel),高Intensity是255,但拉高到300好方便觀看
            Image<Bgr, byte> showBlackWhiteCurve = new Image<Bgr, byte>(480, 300, new Bgr(Color.White));
            Image<Bgr, byte> showBlackIncreasedCurve = new Image<Bgr, byte>(480, 300, new Bgr(Color.White));
            int x = 0; // 要尋訪的起點
            IntensityPoint current, previous;
            current = new IntensityPoint();
            previous = new IntensityPoint();

            //統計每一條線的黑色與白色的pixel數量
            List<Dictionary<int, int>> blackWhiteHistograms = new List<Dictionary<int, int>>();
            //一條線段會是白黑白的經過
            bool[] peakValleyCheckPoint = new bool[] { false, false, false };

            //記錄每一條線段的像素統計用的索引
            int index = 0;

            //紀錄前一個線段的黑色像素統計值
            int previousBlackPixels = -1;

            //計算線段通過pixel
            foreach (LineSegment2DF line in lines)
            {
                float nextX;
                float nextY = line.P1.Y;

                //新增一條線
                blackWhiteHistograms.Add(new Dictionary<int, int>());
                blackWhiteHistograms[index][0] = 0;
                blackWhiteHistograms[index][255] = 0;

                //如果尋訪小於線段結束點的y軸，則不斷尋訪
                while (nextY < line.P2.Y)
                {

                    nextX = GetXPositionFromLineEquations(line.P1, line.P2, nextY);

                    //抓灰階 or 二值化做測試
                    Gray pixel = maskWhiteImg[Convert.ToInt32(nextY), Convert.ToInt32(nextX)];
                    //Console.WriteLine("next x =" + nextX + ",y = " + nextY + ",intensity = " + pixel.Intensity);

                    //取得目前掃描線步進的素值
                    current.SetData(new PointF(nextX, nextY), pixel.Intensity);

                    //判斷像素的變化是Peak-Valley的狀態
                    if (peakValleyCheckPoint[0] == false)
                    {
                        if (pixel.Intensity == 255) //White
                            peakValleyCheckPoint[0] = true;
                    }
                    else if (peakValleyCheckPoint[0] == true && peakValleyCheckPoint[1] == false)
                    {
                        if (pixel.Intensity == 0) //Black
                            peakValleyCheckPoint[1] = true;
                    }
                    else if (peakValleyCheckPoint[0] == true && peakValleyCheckPoint[1] == true && peakValleyCheckPoint[2] == false)
                    {
                        if (pixel.Intensity == 255) //White
                            peakValleyCheckPoint[2] = true;
                    }

                    //統計目前這條線的像素量
                    blackWhiteHistograms[index][(int)pixel.Intensity]++;

                    //繪製圖型======================================================================
                    //繪製呈現用，斑馬線黑白像素經過的圖形
                    int projectY = Math.Abs((int)current.GetIntensity() - 300);
                    if (!current.IsEmpty() && !previous.IsEmpty())
                    {
                        float prevPorjectY = Math.Abs((float)previous.GetIntensity() - 300);
                        showBlackWhiteCurve.Draw(new LineSegment2DF(new PointF(x - 2, projectY), new PointF(x, prevPorjectY)), new Bgr(Color.Red), 1);
                    }
                    else
                    {
                        showBlackWhiteCurve.Draw(new LineSegment2DF(new PointF(0, 300), new PointF(x, projectY)), new Bgr(Color.Red), 1);
                    }
                    showBlackWhiteCurve.Draw(new CircleF(new PointF(x, projectY), 1), new Bgr(Color.Blue), 1);
                    x += 2; //跳2,用來方便顯示圖形時可以比較清晰
                    //繪製圖型======================================================================

                    //設定前一筆
                    previous.SetData(current.GetLocation(), current.GetIntensity());

                    //步進Y
                    nextY++;

                }
                //如果有一個不是true,則代表不是peak valley的形狀
                if (peakValleyCheckPoint[0] == false || peakValleyCheckPoint[1] == false || peakValleyCheckPoint[2] == false)
                {
                    isBlackWhiteCrossing = false;

                }
                Console.WriteLine("Peak Valley State [0] =" + peakValleyCheckPoint[0] + ",[1] = " + peakValleyCheckPoint[1] + ",[2] = " + peakValleyCheckPoint[2]);
                //初始化回來再看新的線段
                peakValleyCheckPoint[0] = peakValleyCheckPoint[1] = peakValleyCheckPoint[2] = false;

                index++; //記錄下一條線

            }

            x = 10;
            //顯示每條線段的統計量
            for (int i = 0; i < blackWhiteHistograms.Count; i++)
            {
                Console.WriteLine("Line[" + i + "] ,statistic : black = " + blackWhiteHistograms[i][0] + ", white = " + blackWhiteHistograms[i][255] + ",ratio = " + (blackWhiteHistograms[i][0] / (float)blackWhiteHistograms[i][255]));

                //繪製圖型======================================================================
                int projectY = Math.Abs((int)blackWhiteHistograms[i][0] - 300);
                if (previousBlackPixels == -1)
                {
                    showBlackIncreasedCurve.Draw(new LineSegment2DF(new PointF(0, 300), new PointF(x, projectY)), new Bgr(Color.Red), 1);
                }
                else
                {
                    float prevPorjectY = Math.Abs((float)previousBlackPixels - 300);
                    showBlackIncreasedCurve.Draw(new LineSegment2DF(new PointF(x - 10, prevPorjectY), new PointF(x, projectY)), new Bgr(Color.Red), 1);
                }
                showBlackIncreasedCurve.Draw(new CircleF(new PointF(x, projectY), 1), new Bgr(Color.Blue), 1);
                //繪製圖型的X座標步進
                x += 10;
                //繪製圖型======================================================================

                //判斷Black像素是否越來越多
                if (previousBlackPixels != -1)
                {
                    if (previousBlackPixels > blackWhiteHistograms[i][0])
                    {
                        isBlackPixelIncreased = false;
                    }
                    previousBlackPixels = blackWhiteHistograms[i][0];
                }
                else
                {
                    previousBlackPixels = blackWhiteHistograms[i][0];
                }


            }
            Console.WriteLine("Black pixel increased? =>" + isBlackWhiteCrossing);
            ImageViewer blackIncreasedCurve = new ImageViewer(showBlackIncreasedCurve, "Statistic of black pixels curve");
            blackIncreasedCurve.Show();

            ImageViewer blackWhiteScanCurve = new ImageViewer(showBlackWhiteCurve, "Scan Line Curve");
            blackWhiteScanCurve.Show();
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

        #endregion
        

        

       

      
        

        
       
    }


    public class IntensityPoint {
        private PointF location;
        private double intensity;

        public IntensityPoint() {
            location = new PointF();
            intensity = -1;
        }

        public bool IsEmpty() {
            if (location.IsEmpty && intensity == -1)
                return true;
            return false;
        }
        public void SetData(PointF p,double value){
            location = p;
            intensity = value;
        }
        public PointF GetLocation(){  return location; }
        public double GetIntensity() { return intensity; }
    }

    
    public class LineEquation {
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float Slope { get; set; }
        public double Angle { get; set; }
        public LineSegment2D Line{ get; set; }
    }
}
