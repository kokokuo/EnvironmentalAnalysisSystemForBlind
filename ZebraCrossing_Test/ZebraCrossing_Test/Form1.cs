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
        List<LineEquation> candidateHoughLineEquations;

        //顯示繪製ScanLine的圖
        Image<Bgr, byte> showScanlineImg;

        //黑白像素是否交叉呈現
        bool isBlackWhiteCrossing;
        //黑色像素是否增加
        bool isBlackPixelIncreased;

        private static Bgr[] drawLineColos;
        

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
            };

            candidateHoughLineEquations = new List<LineEquation>();
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
            }
        }

        private void maskImgButton_Click(object sender, EventArgs e)
        {
            //oriImg 與 grayImg 測試
            if (grayImg != null)
            {
                try
                {
                    maskWhiteImg = new Image<Gray, byte>(new Size(grayImg.Width, grayImg.Height));
                    CvInvoke.cvInRangeS(grayImg, new MCvScalar(160, 160, 160), new MCvScalar(255, 255, 255), maskWhiteImg);
                    maskImageBox.Image = maskWhiteImg;
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
               
            }
            else
            {
                MessageBox.Show("尚未灰階");
            }
        }

        //先灰階再模糊 以去除Mask時的雜訊白點，增加足夠有利的線段
        private void smoothButton_Click(object sender, EventArgs e)
        {
            if (oriImg != null)
            {
                grayImg = oriImg.Convert<Gray, byte>();
                CvInvoke.cvSmooth(grayImg, grayImg, SMOOTH_TYPE.CV_GAUSSIAN, 13, 13, 1.5, 1);
                grayImgBox.Image = grayImg;
            }
        }

        private void toGrayButton_Click(object sender, EventArgs e)
        {
            if (oriImg != null)
            {
                grayImg = oriImg.Convert<Gray, byte>();
                grayImgBox.Image = grayImg;
            }
            else {
                MessageBox.Show("尚未剪裁");
            }
        }

        private void cropBottomButton_Click(object sender, EventArgs e)
        {
            if (oriImg != null)
            {
                oriImg = oriImg.Copy();
                oriImg.ROI = new Rectangle(new Point(0, 240), new Size(640, 320));
                oriImageBox.Image = oriImg;
            }
            else {
                MessageBox.Show("尚未載入圖片");
            }
        }


        private void houghLineButton_Click(object sender, EventArgs e)
        {

            #region 偵測整張Mask White圖片
            if (maskWhiteImg != null)
            {
                Image<Bgr, byte> onlyLineImg = new Image<Bgr,byte>(oriImg.Width,oriImg.Height,new Bgr(0,0,0));
                using (Image<Bgr, byte> showLineImg = oriImg.Copy())
                {
                    //Hough transform for line detection
                    LineSegment2D[][] lines = maskWhiteImg.HoughLines(
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
                        if ((angle > 160 && angle < 190) || (angle > -190 && angle < -160) )
                        {
                            showLineImg.Draw(line, drawLineColos[(colorIndex % drawLineColos.Length)], 2);
                            onlyLineImg.Draw(line, drawLineColos[(colorIndex % drawLineColos.Length)], 2);
                            //加入候選線
                            candidateZebraCrossingsByHoughLine.Add(line);
                            
                            //計算並取得線段的直線方程式
                            LineEquation eqation = GetLineEquation(line);
                            candidateHoughLineEquations.Add(eqation);
                        }
                        Console.WriteLine("index =" + colorIndex + "Angle = " + angle + ", slope = " + slope + ", P1 = " + line.P1 + ", P2 = " + line.P2 + ", length = " + line.Length);
                        showLineImg.Draw(line, new Bgr(0, 0, 0), 1);
                        onlyLineImg.Draw(line, new Bgr(0, 0, 0), 1);

                        colorIndex++;
                    }

                    Console.WriteLine("total detect lines = " + lines[0].Length);

                    houghLineViewer.Image = showLineImg;
                    houghLineViewer.Text = "HoughLine 偵測畫面";
                    houghLineViewer.Show();

                    new ImageViewer(onlyLineImg).Show();


                    //計算線段方程式有無相交
                    for (int i = 0; i < candidateHoughLineEquations.Count; i++) { 
                        bool interset = false;
                        for (int j = i + 1; j < candidateHoughLineEquations.Count; j++)
                        {
                            int x = 0, y = 0;
                            interset = Intersect(candidateHoughLineEquations[i], candidateHoughLineEquations[j],out x,out y);//= Intersect(candidateHoughLineEquations[i], candidateHoughLineEquations[j],out x, out y);
                            if (interset)
                            {
                                showLineImg.Draw(new CircleF(new PointF(x, y), 1), new Bgr(255, 255, 255), 3);
                                Console.WriteLine("Interset:" + interset + "\nLine1 P1 = " + candidateHoughLineEquations[i].Line.P1 + ",Line1 P2 = " + candidateHoughLineEquations[i].Line.P2 + ", length = " + candidateHoughLineEquations[i].Line.Length +
                                    "\nLine2 P1 = " + candidateHoughLineEquations[j].Line.P1 + ",Line2 P2 = " + candidateHoughLineEquations[j].Line.P2 + ", length = " + candidateHoughLineEquations[j].Line.Length);
                            }
                        }
                    }
                }

            }
            #endregion
            
        }
        


        private LineEquation GetLineEquation(LineSegment2D line) {
            float m = (line.P2.Y - line.P1.Y) / (float)(line.P2.X - line.P1.X);
            // y2 - y1 = m(x2 - x1)
            //ax + by = c 
            //a = m, b = -y2, c = mx1 - y1
            float a = m;
            float b = -line.P2.Y;
            float c = m * line.P1.X - line.P1.Y;
            //http://dufu.math.ncu.edu.tw/calculus/calculus_bus/node11.html
            Console.WriteLine("a =" + a + ",b = " + b + ",c = " + c);
            return new LineEquation() { A = a, B = b,C = c,Line = line};
        }

        //private bool Intersect(LineEquation line1, LineEquation line2, out int x, out int y)
        //{
        //    float v = (line1.A * line2.B) - line2.A * line1.B;
        //    Console.WriteLine("v = " + v);
        //    if (v != 0)
        //    {
        //        float delta_x = (line1.C * line2.B) - line2.C * line1.B;
        //        float delta_y = (line1.A * line2.C) - line2.A * line1.C;

        //        x = Convert.ToInt32(delta_x / v);
        //        y = Convert.ToInt32(delta_y / v);
        //        Console.WriteLine("intersect x = " + x + ",intersect y = " + y);
        //        return true;
        //    }
        //    else
        //    {
        //        x = -1;
        //        y = -1;
        //        return false;
        //    }

        //}

        private bool Intersect(LineSegment2D line1, LineSegment2D line2) {

            /*
                a1 = (y2-y1)/(x2-x1)
                b1 = y1 - a1*x1 
                a2 = (y4-y3)/(x4-x3)
                b2 = y3 - a2*x3
             */
            float a1 = (line1.P2.Y - line1.P1.Y) / (line1.P2.X - line1.P1.X);
            float b1 = line1.P1.Y - a1 * line1.P1.X;
            float a2 = (line2.P2.Y - line2.P1.Y) / (line2.P2.X - line2.P1.X);
            float b2 = line2.P1.Y - a2 * line2.P1.X;
            Console.WriteLine("line 1 slope = " + a1 + ", line 2 slope = " + a2);
            if ((line1.P2.Y - line1.P1.Y) * (line2.P2.X - line2.P1.X) == (line1.P2.X - line1.P1.X) *line2.P2.Y - line2.P1.Y) )
            {
                if (Math.Abs(b1 - b2) == 0) { 
                
                }    
            }
        }

        private void contourButton_Click(object sender, EventArgs e)
        {
            if (maskWhiteImg != null)
            {
                

                Contour<Point> contours = DoContours(maskWhiteImg);
                using (Image<Bgr, byte> showContoursImg = oriImg.Copy()) {
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
            else {
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



        private void dilateButton_Click(object sender, EventArgs e)
        {
            if (maskWhiteImg != null)
            {
                //膨脹
                maskWhiteImg = maskWhiteImg.Dilate(1);
                
                filterImageBox.Image = maskWhiteImg;
            }
            else {
                MessageBox.Show("尚未Mask");
            }
        }

        private void filterPepperButton_Click(object sender, EventArgs e)
        {
            if (maskWhiteImg != null)
            {
                //用中值濾波去雜訊
                maskWhiteImg = maskWhiteImg.SmoothMedian(3);
                filterImageBox.Image = maskWhiteImg;
            }
            else
            {
                MessageBox.Show("尚未Mask");
            }
        }

        private void findScanLineButton_Click(object sender, EventArgs e)
        {
       
            Point prePoint = new Point();
            Point currentPoint = new Point();
            
            //依照y軸座標排序
            var zebras = from boundingBox in candidateZebraCrossingsByContour orderby boundingBox.Y select boundingBox;
            foreach (Rectangle rec in zebras) {
                if(!currentPoint.IsEmpty)
                    prePoint = currentPoint;
                currentPoint = new Point((rec.X + rec.Width / 2), (rec.Y + rec.Height / 2));

                //兩點 =>存放線條,並繪製
                if (!currentPoint.IsEmpty && !prePoint.IsEmpty){
                    LineSegment2DF line = new LineSegment2DF(prePoint, currentPoint);
                    //記錄每一條線段
                    crossingConnectionlines.Add(line);
                    Console.WriteLine("draw Line:direction ,x = " + line.Direction.X + "y =" + line.Direction.Y + ",point p1.x =" + prePoint.X + ",p1.y = " + prePoint.Y + ", p2.x =" + currentPoint.X + ",p2.y = " + currentPoint.Y);
                    showScanlineImg.Draw(new LineSegment2DF(prePoint, currentPoint),new Bgr(Color.Azure),2);
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

        private void DoBlackWhiteStatistics(List<LineSegment2DF> lines) {
            //寬480是圖片高(等於垂直走訪的話,最多的pixel),高Intensity是255,但拉高到300好方便觀看
            Image<Bgr, byte> showBlackWhiteCurve = new Image<Bgr, byte>(480,300,new Bgr(Color.White));
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
                    if (peakValleyCheckPoint[0] == false) {
                        if (pixel.Intensity == 255) //White
                            peakValleyCheckPoint[0] = true;
                    }
                    else if (peakValleyCheckPoint[0] == true && peakValleyCheckPoint[1] == false) {
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
                    if (!current.IsEmpty() && !previous.IsEmpty()){
                        float prevPorjectY = Math.Abs((float)previous.GetIntensity() - 300);
                        showBlackWhiteCurve.Draw(new LineSegment2DF(new PointF(x - 2, projectY), new PointF(x, prevPorjectY )), new Bgr(Color.Red), 1);
                    }
                    else {
                        showBlackWhiteCurve.Draw(new LineSegment2DF(new PointF(0, 300), new PointF(x, projectY )), new Bgr(Color.Red), 1);
                    }
                    showBlackWhiteCurve.Draw(new CircleF(new PointF(x, projectY ), 1), new Bgr(Color.Blue), 1);
                    x+=2; //跳2,用來方便顯示圖形時可以比較清晰
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

            x =10;
            //顯示每條線段的統計量
            for (int i = 0; i < blackWhiteHistograms.Count;i++ ){
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
                if (previousBlackPixels != -1){
                    if (previousBlackPixels > blackWhiteHistograms[i][0]) {
                        isBlackPixelIncreased = false;
                    }
                    previousBlackPixels = blackWhiteHistograms[i][0];
                }
                else {
                    previousBlackPixels = blackWhiteHistograms[i][0];
                }
            
                
            }
            Console.WriteLine("Black pixel increased? =>" + isBlackWhiteCrossing);
            ImageViewer blackIncreasedCurve = new ImageViewer(showBlackIncreasedCurve,"Statistic of black pixels curve");
            blackIncreasedCurve.Show();

            ImageViewer blackWhiteScanCurve = new ImageViewer(showBlackWhiteCurve,"Scan Line Curve");
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

        private void detectZebraCrossingButton_Click(object sender, EventArgs e)
        {
            
        }
       
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
        public LineSegment2D Line{ get; set; }
    }
}
