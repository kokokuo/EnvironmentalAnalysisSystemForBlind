using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//斑馬線
using CrossingDetector;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using System.IO;
using System.Windows.Forms;

namespace MainSystem
{
    /// <summary>
    /// CrossingDetectorExperiment.xaml 的互動邏輯
    /// </summary>
    public partial class CrossingDetectorExperiment : System.Windows.Controls.UserControl
    {
        DirectoryInfo dir;
        Image<Bgr, byte> oriImg;
        Image<Gray, byte> processingImg;
        ImageViewer houghLineViewer;
        List<LineSegment2D> candidateZebraCrossingsByHoughLine;
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

        Dictionary<CrossingDetector.LineQuantification, LinkedList<LineEquation>> linesHistogram; //統計不同角度的線段並歸類(過濾非主流限段)
        int mainDirectionLineGroupId = 0; //紀錄主要線段的群組ID

        public CrossingDetectorExperiment()
        {
            InitializeComponent();

            dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            houghLineViewer = new ImageViewer();
            houghLineViewer.FormClosing += houghLineViewer_FormClosing;


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

            linesHistogram = new Dictionary<CrossingDetector.LineQuantification, LinkedList<LineEquation>>();

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


        void houghLineViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            houghLineViewer.Hide(); //隱藏式窗,下次再show出
        }

        private void loadImgButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = OpenImgFile();
            if (filename != null)
            {
                oriImg = ZebraCrossingDetector.LoadImg(filename);
                loadImgBox.Image = oriImg;

                //清空先前的資料
                candidateZebraCrossingsByHoughLine.Clear();

                //清空原先上一張偵測的圖
                crossingConnectionlines.Clear();

                repairedHoughLine.Clear();
                showFinishedRepairedHoughLineStepImg = oriImg.Copy();
                linesHistogram.Clear();

            }
        }

        private void cropImgButton_Click(object sender, RoutedEventArgs e)
        {
            oriImg = ZebraCrossingDetector.ToCrop(oriImg);
            loadImgBox.Image = oriImg;
        }

        private void toGrayButton_Click(object sender, RoutedEventArgs e)
        {
            processingImg = ZebraCrossingDetector.ToGray(oriImg);
            processingImgBox.Image = processingImg;
        }

        private void maskWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            processingImg = ZebraCrossingDetector.MaskWhite(processingImg);
            processingImgBox.Image = processingImg;
        }

        private void pepperfilter_Click(object sender, RoutedEventArgs e)
        {
            processingImg = ZebraCrossingDetector.PepperFilter(processingImg);
            processingImgBox.Image = processingImg;
        }

        private void detectHoughLineButton_Click(object sender, RoutedEventArgs e)
        {
            candidateHoughLineEquations.Clear();
            candidateHoughLineEquationsForReplay.Clear();
            candidateHoughLineEquations = ZebraCrossingDetector.DetectHoughLine(processingImg);

            //步驟用
            candidateHoughLineEquationsForReplay = candidateHoughLineEquations.ToList<LineEquation>();

            //Show Image
            LinkedListNode<LineEquation> node = candidateHoughLineEquations.First;
            Image<Bgr, byte> houghlineImg = oriImg.Clone();
            int colorIndex = 0;
            while (node != null)
            {
                houghlineImg.Draw(node.Value.Line, Utilities.LineColors[(colorIndex % Utilities.LineColors.Length)], 2);

                node = node.Next;
                colorIndex++;
            }

            houghLineViewer.Image = houghlineImg;
            houghLineViewer.Text = "HoughLine 偵測畫面";
            houghLineViewer.Show();
        }

        private void repairLinesButton_Click(object sender, RoutedEventArgs e)
        {
            candidateHoughLineEquations = ZebraCrossingDetector.RepairedLines(candidateHoughLineEquations, oriImg);

            //Show repaired Image
            showFinishedRepairedHoughLineImg = oriImg.Copy();
            int i = 0;
            //ToArray避開刪除後的List長度問題
            LinkedListNode<LineEquation> node = candidateHoughLineEquations.First;
            while (node != null)
            {
                //把線段畫上去
                showFinishedRepairedHoughLineImg.Draw(node.Value.Line, Utilities.LineColors[(i % Utilities.LineColors.Length)], 2);
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

        private void stepRepairLinesButton_Click(object sender, RoutedEventArgs e)
        {
            showSearchrepairedHoughLineStepImg = new Image<Bgr, byte>(oriImg.Width, oriImg.Height, new Bgr(System.Drawing.Color.Black));
            System.Drawing.Point p;
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
                    showSearchrepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line, Utilities.LineColors[(candiateLineEquation_i % Utilities.LineColors.Length)], 1);
                    showSearchrepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_j].Line, Utilities.LineColors[(candiateLineEquation_j % Utilities.LineColors.Length)], 1);

                    //判斷是否共線或是相交的線段
                    interset = ZebraCrossingDetector.CheckIntersectOrNot(candidateHoughLineEquationsForReplay[candiateLineEquation_i], candidateHoughLineEquationsForReplay[candiateLineEquation_j], out p, ref repairedLine, oriImg);
                    if (interset)
                    {
                        showSearchrepairedHoughLineStepImg.Draw(new CircleF(new System.Drawing.PointF(x, y), 1), new Bgr(255, 255, 255), 1);

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
                    showFinishedRepairedHoughLineStepImg.Draw(candidateHoughLineEquationsForReplay[candiateLineEquation_i].Line, Utilities.LineColors[(candiateLineEquation_i % Utilities.LineColors.Length)], 2);
                    //換到下一條比對的線段
                    candiateLineEquation_i++;
                    //都從0開始比，並跳過自己
                    candiateLineEquation_j = candiateLineEquation_i + 1;

                }

            }
            else
            {
                System.Windows.MessageBox.Show("檢測完畢");
                candiateLineEquation_i = 0;
                candiateLineEquation_j = candiateLineEquation_i + 1;
                showSearchrepairedHoughLineStepImg = new Image<Bgr, byte>(oriImg.Width, oriImg.Height, new Bgr(System.Drawing.Color.Black));
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

        private void filterLinesButton_Click(object sender, RoutedEventArgs e)
        {
            linesHistogram = ZebraCrossingDetector.MainGroupLineFilter(candidateHoughLineEquations, ref mainDirectionLineGroupId);
        }

        private void AnalyzeBlackWhiteButton_Click(object sender, RoutedEventArgs e)
        {

            Image<Bgr, byte> stasticDst = new Image<Bgr, byte>(640, 480, new Bgr(System.Drawing.Color.White));
            Image<Bgr, byte> drawScanLineImg = oriImg.Clone();
            bool isZebra = ZebraCrossingDetector.AnalyzeZebraCrossingTexture(mainDirectionLineGroupId, linesHistogram, processingImg, oriImg, stasticDst, drawScanLineImg);

            new ImageViewer(stasticDst, "統計圖表").Show();

            //Show Scan Line
            new ImageViewer(drawScanLineImg, "繪製掃描線路徑").Show();
            if(isZebra)
                System.Windows.MessageBox.Show("前方有斑馬線");
        }

        #region 開檔
         private string OpenImgFile()
        {
            string loadImgPath = dir.Parent.Parent.Parent.FullName + @"\CrossingTemplateImg";
            if (File.Exists(loadImgPath))
                System.Windows.MessageBox.Show("路徑錯誤");
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = loadImgPath;
            dlg.Title = "Open Image File";
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|All Files (*.*)|*.*";
            Nullable<bool> result = dlg.ShowDialog();
            // Display OpenFileDialog by calling ShowDialog method ->ShowDialog()
            // Get the selected file name and display in a TextBox
            if (result == true && dlg.FileName != "")
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

      
    }
}
