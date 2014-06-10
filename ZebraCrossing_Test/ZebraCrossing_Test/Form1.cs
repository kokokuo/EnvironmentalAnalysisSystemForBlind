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

                repairedHoughLine.Clear();
                showFinishedRepairedHoughLineStepImg = oriImg.Copy();
                linesHistogram.Clear();

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
                houghlineImg.Draw(node.Value.Line, DrawColorLines.LineColors[(colorIndex % DrawColorLines.LineColors.Length)], 2);
               
                node = node.Next;
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
                showFinishedRepairedHoughLineImg.Draw(node.Value.Line, DrawColorLines.LineColors[(i % DrawColorLines.LineColors.Length)], 2);
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
                    showSearchrepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line, DrawColorLines.LineColors[(candiateLineEquation_i % DrawColorLines.LineColors.Length)], 1);
                    showSearchrepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_j].Line, DrawColorLines.LineColors[(candiateLineEquation_j % DrawColorLines.LineColors.Length)], 1);
                    
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
                    showFinishedRepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line, DrawColorLines.LineColors[(candiateLineEquation_i % DrawColorLines.LineColors.Length)], 2);
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
            Image<Bgr, byte> drawScanLineImg = oriImg.Clone();
            bool isZebra = ZebraCrossingDetection.ZebraCrossingDetection.AnalyzeZebraCrossingTexture(mainDirectionLineGroupId, linesHistogram, processingImg, oriImg, stasticDst, drawScanLineImg);
            
            new ImageViewer(stasticDst, "統計圖表").Show();

            //Show Scan Line
            new ImageViewer(drawScanLineImg, "繪製掃描線路徑").Show();
        }

        private void runZebraDetectionButton_Click(object sender, EventArgs e)
        {
            if (oriImg != null) {
                bool isZebra = ZebraCrossingDetection.ZebraCrossingDetection.StartToDetect(oriImg);
                if (isZebra)
                    MessageBox.Show("有斑馬線");
                else
                    MessageBox.Show("不是斑馬線");
            }
        }
       
     
    }

    public static class DrawColorLines
    {
        public static Bgr[] LineColors = new Bgr[]{
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
    }


    
   
}
