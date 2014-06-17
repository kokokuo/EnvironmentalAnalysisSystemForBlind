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
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using System.IO;
using System.Windows.Forms;
using RecognitionSys;
using RecognitionSys.ToolKits;
using RecognitionSys.ToolKits.SURFMethod;
namespace MainSystem
{
    /// <summary>
    /// VideoObjsRecognitionExperiment.xaml 的互動邏輯
    /// </summary>
    public partial class VideoObjsRecognitionExperiment : System.Windows.Controls.UserControl
    {
        Timer testVideoTimer;
        Capture testVideoCapture;
        DirectoryInfo dir;
        int FPS = 30;
        int videoTotalFrame;
        bool isPlay, isSuspend, isStop, isScroll;
        Image<Bgr, byte> queryFrame;

        int videoScrollValue;
        VideoObjectsRecognition videoObjRecogSys;

        public VideoObjsRecognitionExperiment()
        {
            InitializeComponent();
            testVideoTimer = new Timer();
            dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            videoTotalFrame = 0;
            isScroll = isPlay = isSuspend = isStop = false;
        }

        //顯示Frame
        private Image<Bgr,byte> QueryFrameAndShow()
        {
            //顯示
            queryFrame = testVideoCapture.QueryFrame();
            queryFrame = queryFrame.Resize(640, 480, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            videoFrameBox.Image = queryFrame;
            return queryFrame;
        }

        //重置影片
        private void ResetVideo()
        {
            //重置，回到一開始畫面
            testVideoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_AVI_RATIO, 0);
            videoScrollValue = 0;
            videoTrackBar.Value = 0;
            isScroll = false;
            QueryFrameAndShow();
        }

        private void loadTestVideoButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = OpenVideo();
            if (filename != null) {
                testVideoCapture = new Capture(filename);
                videoTotalFrame = (int)testVideoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT); //Get total frame number

                //第一張做影片的封面
                QueryFrameAndShow();
                //設定刻度
                videoTrackBar.Minimum = 0;
                videoTrackBar.Maximum = videoTotalFrame;

                //設定播放用的Timer
                testVideoTimer.Tick += testVideoTimer_Tick;
                testVideoTimer.Interval = 1000 / FPS;
                testVideoTimer.Start();
            }
        }

        private void testVideoTimer_Tick(object sender, EventArgs e)
        {
            //如果有影片
            if (testVideoCapture != null)
            {
                if (isPlay)
                {
                    lock (this)
                    {
                        
                        //如果Frame的index沒有差過影片的最大index
                        if (testVideoCapture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES) < videoTotalFrame)
                        {
                            Image<Bgr, byte> currentFrame = QueryFrameAndShow();
                            //如果正在捲動
                            if (isScroll)
                            {
                                //設定要移動到的frame
                                //http://stackoverflow.com/questions/20902323/get-specific-frames-using-emgucv
                                testVideoCapture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, videoScrollValue);
                                isScroll = false;
                            }
                            else
                            {
                                //處理辨識
                                if (currentFrame != null)
                                {
                                    if (videoObjRecogSys != null)
                                        videoObjRecogSys.SetupInputImage(currentFrame);
                                    else
                                        videoObjRecogSys = new VideoObjectsRecognition(currentFrame);

                                    string objData = videoObjRecogSys.RunRecognition(true);
                                }
                            }
                            
                        }
                        else
                        {
                            ResetVideo();
                        }
                    }

                }
                else if (isSuspend)
                {
                   
                }
                else if (isStop)
                {
                    //關閉，回到一開始畫面
                    ResetVideo();
                }
            }
        }





        private void videoTrackBar_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            videoScrollValue = (int)videoTrackBar.Value;
            isScroll = true;
        }

        #region 影片狀態切換
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            isPlay = true;
            isSuspend = isStop = false;
        }

        private void suspendButton_Click(object sender, RoutedEventArgs e)
        {
            isPlay = isStop = false;
            isSuspend = true;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            isStop = true;
            isPlay = isSuspend = false;
        }
        #endregion

        #region 開檔
        private string OpenVideo()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //dlg.InitialDirectory = dir.Parent.Parent.FullName + @"\TrainingVideo";
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.Title = "Open Video File";
            // Set filter for file extension and default file extension
            dlg.Filter = "AVI Video(*.avi)|*.avi|MP4(*.mp4)|*.mp4|All Files(*.*)|*.*";
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
