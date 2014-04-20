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
        List<Rectangle> candidateZebraCrossings;
        Image<Bgr, byte> showScanlineImg;

        //黑白像素是否交叉呈現
        bool isBlackWhiteCrossing;
        //黑色像素是否增加
        bool isBlackPixelIncreased;
        public Form1()
        {
            InitializeComponent();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
            houghLineViewer = new ImageViewer();
            houghLineViewer.FormClosing += houghLineViewer_FormClosing;
            candidateZebraCrossings = new List<Rectangle>();

            //預設是假設都為True
            isBlackWhiteCrossing = isBlackPixelIncreased = true;
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
                    CvInvoke.cvInRangeS(grayImg, new MCvScalar(180, 180, 180), new MCvScalar(255, 255, 255), maskWhiteImg);
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
            if (maskWhiteImg != null)
            {
                using (Image<Bgr, byte> showLineImg = oriImg.Copy()) {
                    //Hough transform for line detection
                    LineSegment2D[][] lines = maskWhiteImg.HoughLines(
                        new Gray(125),  //Canny algorithm low threshold
                        new Gray(260),  //Canny algorithm high threshold
                        1,              //rho parameter
                        Math.PI / 180.0,  //theta parameter 
                        80,            //threshold
                        1,             //min length for a line
                        50);            //max allowed gap along the line
                    

                    //draw lines on image
                    foreach (var line in lines[0])
                    {
                        //如何限制角度http://yy-programer.blogspot.tw/2013/02/emgucv-image-process-extracting-lines_28.html
                        //vector是向量，代表的是這個線的方向。HoughLine是採用亟座標的方式
                        //線的點是在LineSegment2D這個結構裡的：P1與P2才是。﻿
                        PointF vector = line.Direction;

                        double slope = (line.P2.Y - line.P1.Y) / Convert.ToDouble(line.P2.X - line.P1.X);
                        double angle = Math.Atan2(vector.Y, vector.X) * 180.0 / Math.PI;
                        if ((angle > 160 && angle < 190) || (angle > -190 && angle < -160))
                        {
                            showLineImg.Draw(line, new Bgr(0, 0, 255), 2);
                        }
                        Console.WriteLine("Angle = " + angle + ", slope = " + slope + ", P1 = " + line.P1 + ", P2 = " + line.P2 + ", length = " + line.Length);
                        showLineImg.Draw(line, new Bgr(255, 0, 0), 1);
                        
                    }

                    Console.WriteLine("total detect lines = " + lines[0].Length);

                    houghLineViewer.Image = showLineImg;
                    houghLineViewer.Text = "HoughLine 偵測畫面";
                    houghLineViewer.Show();
                }
                
            }
        }

        private void contourButton_Click(object sender, EventArgs e)
        {
            if (maskWhiteImg != null)
            {
                //清空之前的資料
                candidateZebraCrossings.Clear();

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
                            candidateZebraCrossings.Add(contours.BoundingRectangle);
                            
                        }
                        
                        Console.WriteLine("Width = " + contours.BoundingRectangle.Width + ",Height = " + contours.BoundingRectangle.Height + ",h/w = " + ratio);
                        //繪製輪廓
                        //showContoursImg.Draw(contours, new Bgr(Color.Yellow), new Bgr(Color.GreenYellow), 1, 2);
                        contours = contours.HNext;
                    }
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
            List<LineSegment2DF> lines = new List<LineSegment2DF>();
            //依照y軸座標排序
            var zebras = from boundingBox in candidateZebraCrossings orderby boundingBox.Y select boundingBox;
            foreach (Rectangle rec in zebras) {
                if(!currentPoint.IsEmpty)
                    prePoint = currentPoint;
                currentPoint = new Point((rec.X + rec.Width / 2), (rec.Y + rec.Height / 2));

                //兩點 =>存放線條,並繪製
                if (!currentPoint.IsEmpty && !prePoint.IsEmpty){
                    LineSegment2DF line = new LineSegment2DF(prePoint, currentPoint);
                    lines.Add(line);
                    Console.WriteLine("draw Line:direction ,x = " + line.Direction.X + "y =" + line.Direction.Y + ",point p1.x =" + prePoint.X + ",p1.y = " + prePoint.Y + ", p2.x =" + currentPoint.X + ",p2.y = " + currentPoint.Y);
                    showScanlineImg.Draw(new LineSegment2DF(prePoint, currentPoint),new Bgr(Color.Azure),2);
                }
                Console.WriteLine("center x =" + currentPoint.X + ",y = " + currentPoint.Y);
                showScanlineImg.Draw(new CircleF(currentPoint, 1), new Bgr(Color.Blue), 3);

            }
            //show center point
            contourImageBox.Image = showScanlineImg;

            DoBlackWhiteStatistics(lines);
        }

        private void DoBlackWhiteStatistics(List<LineSegment2DF> lines) {
            //寬480是圖片高(等於垂直走訪的話,最多的pixel),高Intensity是255,但拉高到300好方便觀看
            Image<Bgr, byte> showBlackWhiteCurve = new Image<Bgr, byte>(480,300,new Bgr(Color.White));
            int x = 0; // 要尋訪的起點
            IntensityPoint current, previous;
            current = new IntensityPoint();
            previous = new IntensityPoint();

            //統計每一條線的黑色與白色的pixel數量
            List<Dictionary<int,int>> blackWhiteHistograms = new List<Dictionary<int,int>>();

            //一條線段會是白黑白的經過
            bool[] peakValleyCheckPoint = new bool[] { false, false, false };

            int index = 0;
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

                    if (peakValleyCheckPoint[0] == false) {
                        if (pixel.Intensity == 255)
                            peakValleyCheckPoint[0] = true;
                    }
                    else if (peakValleyCheckPoint[0] == true && peakValleyCheckPoint[1] == false) {
                        if (pixel.Intensity == 0)
                            peakValleyCheckPoint[1] = true;
                    }
                    else if (peakValleyCheckPoint[0] == true && peakValleyCheckPoint[1] == true && peakValleyCheckPoint[2] == false)
                    {
                        if (pixel.Intensity == 255)
                            peakValleyCheckPoint[2] = true;
                    }

                    //統計目前這條線的像素量
                    blackWhiteHistograms[index][(int)pixel.Intensity]++;


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

            //顯示每條線段的統計量
            for (int i = 0; i < blackWhiteHistograms.Count;i++ ){
                Console.WriteLine("Line[" + i + "] ,statistic : black = " + blackWhiteHistograms[i][0] + ", white = " + blackWhiteHistograms[i][255] + ",ratio = " + (blackWhiteHistograms[i][0] / (float)blackWhiteHistograms[i][255]));

            }
           
            ImageViewer curve = new ImageViewer(showBlackWhiteCurve);
            curve.Show();
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
}
