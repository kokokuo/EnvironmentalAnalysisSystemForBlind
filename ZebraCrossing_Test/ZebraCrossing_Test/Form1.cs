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
        public Form1()
        {
            InitializeComponent();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
            houghLineViewer = new ImageViewer();
            houghLineViewer.FormClosing += houghLineViewer_FormClosing;
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
                Contour<Point> contours = DoContours(maskWhiteImg);
                using (Image<Bgr, byte> showContoursImg = oriImg.Copy()) {
                    //繪製所有輪廓
                    while (contours.HNext != null)
                    {
                        //繪製輪廓BoundingBox
                        showContoursImg.Draw(contours.BoundingRectangle, new Bgr(Color.Red), 2);
                        //繪製輪廓
                        //showContoursImg.Draw(contours, new Bgr(Color.Yellow), new Bgr(Color.GreenYellow), 1, 2);
                        contours = contours.HNext;
                    }

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

    }
}
