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
using RecognitionSys.FeatureLearning;
using RecognitionSys.ToolKits.SURFMethod;
namespace VideoObjectLearningApp
{
    public partial class VideoObjectLearningForm : Form
    {
        //取得專案執行黨所在的目錄=>System.Windows.Forms.Application.StartupPath
        //使用DirectoryInfo移動至上層
        DirectoryInfo dir;
        Capture videoCapture;
        Timer trainingVideoTimer;
        int FPS = 30;
        int trainingVideoTotalFrame;
        bool isPlay, isSuspend, isStop, isScroll;
        Image<Bgr, byte> queryFrame;

        int trainingScrollValue;
        Graphics g; //draw rectangle
        Point pressedToDrawPoint;
        bool isPressed;
        Rectangle extractFeatureMaskROI;
        Image<Bgr, byte> wantExtractFeatureImage;
        SURFFeatureData surfData;
        DenseHistogram templateHist;
        ImageViewer templateHistImgBox;
        Image<Bgr, byte> showTemplateHistImg;
        FeatureLearning learningSys;
        int HistDim;
        public VideoObjectLearningForm()
        {
            InitializeComponent();
            trainingVideoTimer = new Timer();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
            trainingVideoTotalFrame = 0;
            isScroll = isPlay = isSuspend = isStop = false;
            isPressed = false;
            HistDim = 1;
            templateHistImgBox = new ImageViewer();
            templateHistImgBox.FormClosing += histImgBox_FormClosing;
        }

        private void loadVideoButton_Click(object sender, EventArgs e)
        {
            string videoFilename = OpenVideo();
            if (videoFilename != null)
            {
                videoCapture = new Capture(videoFilename);
                trainingVideoTotalFrame = (int)videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT); //Get total frame number

                //第一張做影片的封面
                QueryFrameAndShow();
                //設定刻度
                videoTrackBar.TickStyle = TickStyle.Both;
                videoTrackBar.Minimum = 0;
                videoTrackBar.Maximum = trainingVideoTotalFrame;

                //設定播放用的Timer
                trainingVideoTimer.Tick += trainingVideoTimer_Tick;
                trainingVideoTimer.Interval = 1000 / FPS;
                trainingVideoTimer.Start();
            }
        }

        #region 影片播放狀態按鈕
        private void playButton_Click(object sender, EventArgs e)
        {
            isPlay = true;
            isSuspend = isStop = false;
        }

        private void suspendButton_Click(object sender, EventArgs e)
        {
            isPlay = isStop = false;
            isSuspend = true;

        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            isStop = true;
            isPlay = isSuspend = false;
        }
        #endregion

        #region 擷取與存放特徵
        private void extractFeatureButton_Click(object sender, EventArgs e)
        {
            if (wantExtractFeatureImage != null)
            {
                if (learningSys != null)
                    learningSys.SetLearningImage(wantExtractFeatureImage);
                else
                    learningSys = new FeatureLearning(wantExtractFeatureImage);

                surfData = learningSys.CalSURFFeature();
                //Draw Feature
                Image<Bgr, byte> drawKeyPointImg = learningSys.DrawSURFFeature(surfData);
                new ImageViewer(drawKeyPointImg, "擷取特徵點結果").Show();
            }
        }

        private void saveFeatureButton_Click(object sender, EventArgs e)
        {
            if (surfData != null)
            {
                SaveSURFFeatureFile(surfData);
            }
        }
        #endregion


        private void videoTrackBar_ValueChanged(object sender, EventArgs e)
        {
            trainingScrollValue = videoTrackBar.Value;
            isScroll = true;
        }

        //顯示Frame
        private void QueryFrameAndShow()
        {
            //顯示
            queryFrame = videoCapture.QueryFrame();
            queryFrame = queryFrame.Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            videoFrameBox.Image = queryFrame.ToBitmap();
        }

        //重置影片
        private void ResetVideo()
        {
            //重置，回到一開始畫面
            videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_AVI_RATIO, 0);
            trainingScrollValue = videoTrackBar.Value = 0;
            isScroll = false;
            QueryFrameAndShow();
        }

        //影片的Timer
        private void trainingVideoTimer_Tick(object sender, EventArgs e)
        {
            //如果有影片
            if (videoCapture != null)
            {
                if (isPlay)
                {
                    lock (this)
                    {
                        if (isScroll)
                        {
                            //設定要移動到的frame
                            //http://stackoverflow.com/questions/20902323/get-specific-frames-using-emgucv
                            videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, trainingScrollValue);
                            isScroll = false;
                        }
                        //如果Frame的index沒有差過影片的最大index
                        if (videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES) < trainingVideoTotalFrame)
                        {
                            QueryFrameAndShow();
                        }
                        else
                        {
                            ResetVideo();
                        }
                    }

                }
                else if (isSuspend)
                {
                    g = videoFrameBox.CreateGraphics();
                }
                else if (isStop)
                {
                    //關閉，回到一開始畫面
                    ResetVideo();
                }
            }
        }

        #region 開檔讀檔
        private string OpenVideo()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            //dlg.InitialDirectory = dir.Parent.Parent.FullName + @"\TrainingVideo";
            dlg.Title = "Open Video File";

            // Set filter for file extension and default file extension
            dlg.Filter = "AVI Video(*.avi)|*.avi|MP4(*.mp4)|*.mp4|All Files(*.*)|*.*";

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
        private void SaveHistogramFile()
        {
            // Displays a SaveFileDialog so the user can save the Image

            //wpf->Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog(); //WPF
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.Title = "Save Histogram to File";

            //dir.Parent.Parent.FullName取得對路徑目錄
            //InitialDirectory要設置對路徑目錄
            dlg.InitialDirectory = dir.Parent.Parent.Parent.FullName + @"\SigbBoardHistData";
            dlg.RestoreDirectory = true;

            // If the file name is not an empty string open it for saving.
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlg.FileName != "")
            {
                bool isOk = learningSys.SaveHistogram(dlg.FileName,templateHist);
                if (isOk) MessageBox.Show("SaveHistogramFile Ok");
                else MessageBox.Show("SaveHistogramFile Faild");
            }
        }

        private void SaveSURFFeatureFile(SURFFeatureData surf)
        {
            string saveSURFDataPath = dir.Parent.Parent.Parent.FullName + @"\SignBoardSURFFeatureData";
            if (File.Exists(saveSURFDataPath))
                MessageBox.Show("路徑錯誤");
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.Title = "Save Descriptor to File";
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = saveSURFDataPath;
            // If the file name is not an empty string open it for saving.
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK == true && dlg.FileName != "" && learningSys != null)
            {
                bool isOk = learningSys.SaveSURFFeatureData(dlg.FileName, surf);
                if (isOk) MessageBox.Show("Save SURF Feature Data Ok");
                else MessageBox.Show("Save SURF Feature Data Faild");
            }
        }
        #endregion

        #region 暫停影片時的圈選事件
        private void videoFrameBox_MouseUp(object sender, MouseEventArgs e)
        {
            isPressed = false;
        }
        private void videoFrameBox_MouseMove(object sender, MouseEventArgs e)
        {
            //繪製要擷取的ROI
            if (videoCapture != null && isSuspend)
            {
                if (pressedToDrawPoint != null && isPressed)
                {
                    videoFrameBox.Image = queryFrame.Copy().ToBitmap();
                    using (Graphics g = Graphics.FromImage(videoFrameBox.Image))
                    {
                        //取得ROI座標
                        extractFeatureMaskROI = new Rectangle(pressedToDrawPoint.X, pressedToDrawPoint.Y, Math.Abs(e.X - pressedToDrawPoint.X), Math.Abs(e.Y - pressedToDrawPoint.Y));
                        g.DrawRectangle(new Pen(Brushes.Red, 5), extractFeatureMaskROI);
                    }
                    try
                    {
                        //指定要在畫面上顯示的ROI大小
                        candidateExtractImgBox.Width = extractFeatureMaskROI.Width;
                        candidateExtractImgBox.Height = extractFeatureMaskROI.Height;
                        //取得影像中的ROI影像
                        Image<Bgr, byte> roi = queryFrame.Copy();
                        roi.ROI = extractFeatureMaskROI; //如此指定影像變繪製取得ROI
                        //取得要處理的影像
                        wantExtractFeatureImage = roi.Copy();
                        //顯示出ROI
                        candidateExtractImgBox.Image = roi;
                        //重繪
                        candidateExtractImgBox.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error:" + ex.Message);
                    }

                }
            }
        }

        private void videoFrameBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (queryFrame != null)
            {
                pressedToDrawPoint = e.Location;
                isPressed = true;
                //如果畫完又再次點及其他位置，會再重新拿取乾淨畫面，重新繪製別的ROI
                videoFrameBox.Image = queryFrame.Copy().ToBitmap();
            }
        }
        #endregion


        #region 直方圖視窗 方法
        //////////////////////////////////////////////////////////////////////////////////////////////
        private void ShowHistViewer(ImageViewer Box, Image<Bgr, Byte> img, string message)
        {
            Box.Width = img.Width + 50;
            Box.Height = img.Height + 50;
            Box.Image = img;
            Box.Text = "直方圖顏色分布區域 : " + message;
            Box.Show();
            Box.Focus();
        }

        private void histImgBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            //如果close掉視窗,資源會被釋放而無法開啟
            e.Cancel = true; //關閉視窗時取消
            templateHistImgBox.Hide(); //隱藏式窗,下次再show出

        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        #region 學習步驟二 : HSV直方圖計算與儲存
        //////////////////////////////////////////////////////////////////////////////////////////////
        private void HRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SBinTextBox.Text = String.Empty;
            SBinTextBox.Enabled = false;
            HistDim = 1;
            HBinTextBox.Text = Convert.ToString(50);
        }

        private void HSRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SBinTextBox.Enabled = true;

            HBinTextBox.Text = Convert.ToString(30);
            SBinTextBox.Text = Convert.ToString(32);
            HistDim = 2;
        }
        private void HsvHistButton_Click(object sender, EventArgs e)
        {
            int HBins = 50;
            int SBins = 0;
            int VBins = 0;
            bool isCorrectValue = false;
            if (HistDim == 1 && int.TryParse(HBinTextBox.Text, out HBins))
            {
                isCorrectValue = true;
            }
            else if (HistDim == 2 && int.TryParse(HBinTextBox.Text, out HBins) && int.TryParse(SBinTextBox.Text, out SBins))
            {
                isCorrectValue = true;
            }
            else
            {
                MessageBox.Show("數值不對");
            }
            if (wantExtractFeatureImage != null && isCorrectValue)
            {
                if (learningSys != null)
                    learningSys.SetLearningImage(wantExtractFeatureImage);
                else
                    learningSys = new FeatureLearning(wantExtractFeatureImage);

                templateHist = learningSys.CalHist(HistDim, HBins, SBins, VBins);
                if (HistDim <= 2)
                {
                    showTemplateHistImg = learningSys.DrawHsvHistogram(templateHist);
                    ShowHistViewer(templateHistImgBox, showTemplateHistImg, "樣板影像");
                }
                else
                    MessageBox.Show("Dim = " + HistDim + ",can;t draw");
            }
            else
            {
                MessageBox.Show("圖片未載入");
            }
        }
        private void saveHistogramButton_Click(object sender, EventArgs e)
        {
            if (showTemplateHistImg != null) SaveHistogramFile();
            else MessageBox.Show("沒有產生值方圖!");
        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
