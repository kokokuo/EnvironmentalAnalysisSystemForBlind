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
using FeatureRecognitionSystem.FeatureLearning;
using FeatureRecognitionSystem.ToolKits.SURFMethod;
namespace VideoEnvironmentObjLearningSys
{
    public partial class Form1 : Form
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
        Image<Bgr, byte> senceImage;
        SURFFeatureData trainingExtractSurfData;
        int oneSecFrameIndex;

        FeatureLearning learningSys;
        Image<Bgr, byte> loadImg;

        public Form1()
        {
            InitializeComponent();
            trainingVideoTimer = new Timer();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
            trainingVideoTotalFrame = 0;
            isScroll = isPlay = isSuspend = isStop = false;
            isPressed = false;
        }

        private void loadVideoButton_Click(object sender, EventArgs e)
        {
            string videoFilename = OpenVideo();
            if (videoFilename != string.Empty)
            {
                videoCapture = new Capture(videoFilename);
                trainingVideoTotalFrame = (int)videoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT); //Get total frame number

                //第一張做影片的封面
                queryFrame = videoCapture.QueryFrame();
                queryFrame = queryFrame.Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                videoFrameBox.Image = queryFrame.Copy().ToBitmap();
                //設定刻度
                videoTrackBar.TickStyle = TickStyle.Both;
                videoTrackBar.Minimum = 0;
                videoTrackBar.TickFrequency = 1;
                videoTrackBar.Maximum = trainingVideoTotalFrame;

                //設定播放用的Timer
                trainingVideoTimer.Tick += trainingVideoTimer_Tick;
                trainingVideoTimer.Interval = 1000 / FPS;
                trainingVideoTimer.Start();
            }
        }

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

        private void extractFeatureButton_Click(object sender, EventArgs e)
        {
            if (wantExtractFeatureImage != null)
            {
                if (learningSys != null)
                    learningSys.SetLearningImage(wantExtractFeatureImage);
                else
                    learningSys = new FeatureLearning(wantExtractFeatureImage);

                trainingExtractSurfData = learningSys.CalSURFFeature();
                //Draw Feature
                Image<Bgr, Byte> drawKeyPointImg = learningSys.DrawSURFFeature(trainingExtractSurfData, loadImg);
                candidateExtractImgBox.Image = drawKeyPointImg;


            }
        }

        private void saveFeatureButton_Click(object sender, EventArgs e)
        {
            SaveSURFFeatureFile(trainingExtractSurfData);
        }

        private void videoTrackBar_ValueChanged(object sender, EventArgs e)
        {
            trainingScrollValue = videoTrackBar.Value;
            isScroll = true;
        }

        void trainingVideoTimer_Tick(object sender, EventArgs e)
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
                            //顯示
                            queryFrame = videoCapture.QueryFrame();
                            queryFrame = queryFrame.Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
                            videoFrameBox.Image = queryFrame.ToBitmap();
                        }
                    }

                }
                else if (isSuspend)
                {
                    //擷取想要的區塊 做SURF
                    g = videoFrameBox.CreateGraphics();

                }
                else if (isStop)
                {
                    //關閉，回到一開始畫面
                    videoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_AVI_RATIO, 0);
                }
            }
        }
        #region 開檔讀檔
        private string OpenVideo()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.FullName + @"\TrainingVideo";
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


        private void SaveSURFFeatureFile(SURFFeatureData surf)
        {
            string saveSURFDataPath = dir.Parent.Parent.Parent.FullName + @"\SURFFeatureData";
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
    }
}
