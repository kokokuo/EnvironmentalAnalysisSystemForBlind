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
        Image<Gray, byte> processingImg;
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

        Dictionary<ZebraCrossingDetection.LineQuantification, LinkedList<LineEquation>> linesHistogram; //統計不同角度的線段並歸類(過濾非主流限段)
        int mainDirectionLineGroupId = 0; //紀錄主要線段的群組ID
        int maxLineLength = 0;
        //統計每一條線的黑色與白色的pixel數量
        string checkBlackWhiteCrossingPoint;

        ZebraCrossingDetection.ZebraCrossingDetection crossingDetection = new ZebraCrossingDetection.ZebraCrossingDetection();
        
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

            linesHistogram = new Dictionary<ZebraCrossingDetection.LineQuantification, LinkedList<LineEquation>>();

           
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
                oriImg = ZebraCrossingDetection.ZebraCrossingDetection.LoadImg(filename);
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
        private void cropBottomButton_Click(object sender, EventArgs e)
        {
            oriImg = ZebraCrossingDetection.ZebraCrossingDetection.ToCrop(oriImg);
            oriImageBox.Image = oriImg;
        }
        private void toGrayButton_Click(object sender, EventArgs e)
        {
            processingImg = ZebraCrossingDetection.ZebraCrossingDetection.ToGray(oriImg);
            grayImgBox.Image = processingImg;
        }
        //先灰階再模糊 以去除Mask時的雜訊白點，增加足夠有利的線段
        private void toGrayAndSmoothButton_Click(object sender, EventArgs e)
        {
            processingImg = ZebraCrossingDetection.ZebraCrossingDetection.ToGray(oriImg);
            processingImg = ZebraCrossingDetection.ZebraCrossingDetection.Smooth(processingImg);
            grayImgBox.Image = processingImg;
        }
        private void maskImgButton_Click(object sender, EventArgs e)
        {
            processingImg = ZebraCrossingDetection.ZebraCrossingDetection.MaskWhite(processingImg);
            maskImageBox.Image = processingImg;
        }

        private void filterPepperButton_Click(object sender, EventArgs e)
        {
            processingImg = ZebraCrossingDetection.ZebraCrossingDetection.PepperFilter(processingImg);
            filterImageBox.Image = processingImg;
        }
       
     

        private void houghLineButton_Click(object sender, EventArgs e)
        {
            candidateHoughLineEquations.Clear();
            candidateHoughLineEquationsForReplay.Clear();
            candidateHoughLineEquations = ZebraCrossingDetection.ZebraCrossingDetection.DetectHoughLine(processingImg);

            //步驟用
            candidateHoughLineEquationsForReplay = candidateHoughLineEquations.ToList<LineEquation>();

            //Show Image
            LinkedListNode<LineEquation> node = candidateHoughLineEquations.First;
            Image<Bgr, byte> houghlineImg = oriImg.Clone();
            int colorIndex = 0;
            while (node != null) {
                houghlineImg.Draw(node.Value.Line, drawLineColos[(colorIndex % drawLineColos.Length)], 2);
                colorIndex++;
            }

            houghLineViewer.Image = houghlineImg;
            houghLineViewer.Text = "HoughLine 偵測畫面";
            houghLineViewer.Show();
        }
        

        private void restructLineButton_Click(object sender, EventArgs e)
        {
            candidateHoughLineEquations = ZebraCrossingDetection.ZebraCrossingDetection.
                RepairedLines(candidateHoughLineEquations, oriImg);

            //Show repaired Image
            showFinishedRepairedHoughLineImg = oriImg.Copy();
            int i = 0;
            //ToArray避開刪除後的List長度問題
            LinkedListNode<LineEquation> node = candidateHoughLineEquations.First;
            while (node != null) {
                //把線段畫上去
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
            Point p;
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
                    interset = ZebraCrossingDetection.ZebraCrossingDetection.CheckIntersectOrNot(candidateHoughLineEquationsForReplay[candiateLineEquation_i], candidateHoughLineEquationsForReplay[candiateLineEquation_j], out p, ref repairedLine,oriImg);
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

        private void filterLineButton_Click(object sender, EventArgs e)
        {
            linesHistogram =  ZebraCrossingDetection.ZebraCrossingDetection.
                MainGroupLineFilter(candidateHoughLineEquations,ref mainDirectionLineGroupId);

        }


        private void analyzeBlackWhiteButton_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> stasticDst = new Image<Bgr, byte>(640, 480, new Bgr(Color.White));
            bool isZebra = ZebraCrossingDetection.ZebraCrossingDetection.AnalyzeZebraCrossingTexture(mainDirectionLineGroupId,linesHistogram, processingImg, oriImg, stasticDst);
            
            new ImageViewer(stasticDst, "統計圖表").Show();
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
            //DoBlackWhiteStatisticsByScanLine(crossingConnectionlines);
        }

      
        #endregion


        /// <summary>
        /// 繪製黑白交錯的曲線(縱軸是Intensity,橫軸是排序的線段從最前面的線段到最下面的線段的座標)
        /// </summary>
        /// <param name="drawImg"></param>
        /// <param name="current">現在的座標像素</param>
        /// <param name="previous">前一個點的座標像素</param>
        /// <param name="x">x軸步近的值</param>
        private void DrawBlackWhiteCurve(Image<Bgr, byte> drawImg, IntensityPoint current, IntensityPoint previous, int x)
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
