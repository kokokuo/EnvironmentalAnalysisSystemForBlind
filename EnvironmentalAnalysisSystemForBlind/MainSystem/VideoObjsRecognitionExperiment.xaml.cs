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
        //Video
        Timer testVideoTimer;
        Capture testVideoCapture;
        DirectoryInfo dir;
        int FPS = 30;
        int videoTotalFrame;
        bool isPlay, isSuspend, isStop, isScroll;
        Image<Bgr, byte> queryFrame;

        int videoScrollValue;
        VideoObjectsRecognition videoObjRecogSys;

        //Image
        Image<Bgr, byte> loadTestImg;
        DenseHistogram templateHist;
       
        Image<Gray, byte> backProjectImg;
        Image<Gray, byte> binaryImg;
        Image<Gray, byte> morphologyImg;
        Image<Bgr, byte> contoursImg;
        List<Contour<System.Drawing.Point>> topContours;
        ImageViewer templateHistImgBox;
        ImageViewer observedHistImgBox;
        Image<Bgr, byte> showTemplateHistImg;
        Image<Bgr, byte> showObservedHistImg;
        string templateHistFilePathName;
        string templateSURFPathFileName;
        SURFFeatureData templateSurfFeature;
        SURFFeatureData observedSurfFeature;

        public VideoObjsRecognitionExperiment()
        {
            InitializeComponent();
            testVideoTimer = new Timer();
            dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            videoTotalFrame = 0;
            isScroll = isPlay = isSuspend = isStop = false;

            //Image
            templateHistImgBox = new ImageViewer();
            observedHistImgBox = new ImageViewer();
            templateHistImgBox.FormClosing += histImgBox_FormClosing;
            observedHistImgBox.FormClosing += observedHistImgBox_FormClosing;
            templateSURFPathFileName = templateHistFilePathName = null;

           
        }

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
        private void observedHistImgBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            observedHistImgBox.Hide(); //隱藏式窗,下次再show出
        }
        private void histImgBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            //如果close掉視窗,資源會被釋放而無法開啟
            e.Cancel = true; //關閉視窗時取消
            templateHistImgBox.Hide(); //隱藏式窗,下次再show出

        }
        //////////////////////////////////////////////////////////////////////////////////////////////
        #endregion

        #region 載入Video的部分－實驗
        //////////////////////////////////////////////////////////////////////////////////////////////
        //顯示Frame
        private Image<Bgr, byte> QueryFrameAndShow()
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
            if (filename != null)
            {
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
                                //處理辨識===========================================================
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
        //////////////////////////////////////////////////////////////////////////////////////////////
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

        private string OpenImgFile()
        {
            string loadImgPath = dir.Parent.Parent.Parent.FullName + @"\SignBoardTestData";
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
        #region 直方圖與特徵點
        private string OpenLearnedDescriptorDataFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.Parent.FullName + @"\SignBoardSURFFeatureData";
            // Set filter for file extension and default file extension
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.Title = "Open DescriptorData File";
            // Display OpenFileDialog by calling ShowDialog method ->ShowDialog()
            Nullable<bool> result = dlg.ShowDialog();
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
        private string OpenLearnedHistogramDataFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.Parent.FullName + @"\SigbBoardHistData";
            // Set filter for file extension and default file extension
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.Title = "Open HistogramData File";
            // Display OpenFileDialog by calling ShowDialog method ->ShowDialog()
            Nullable<bool> result = dlg.ShowDialog();
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

        #endregion

        #region 載入圖片的部分－實驗

        private void loadTestImgButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = OpenImgFile();
            if (filename != null) {
                loadTestImg = new Image<Bgr, byte>(filename);
                loadImgBox.Image = loadTestImg.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
            }
        }

        private void loadHistFileButton_Click(object sender, RoutedEventArgs e)
        {
            templateHistFilePathName = OpenLearnedHistogramDataFile();
            if (templateHistFilePathName != null)
            {
                templateHist = DetectObjects.ReadHistogram(templateHistFilePathName, true);
               
                if (templateHist.Dimension < 3)
                {
                    showTemplateHistImg = SystemToolBox.DrawHsvHistogram(templateHist);
                    ShowHistViewer(templateHistImgBox, showTemplateHistImg, "樣板影像");
                }
                else
                {
                    System.Windows.MessageBox.Show("Dim = " + templateHist.Dimension.ToString() + ",can;t draw");
                }

               
            }
        }

        private void backProjectButton_Click(object sender, RoutedEventArgs e)
        {
            backProjectImg = DetectObjects.DoBackProject(templateHist, loadTestImg);
            backProjectImgBox.Image = backProjectImg.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void binaryThresButton_Click(object sender, RoutedEventArgs e)
        {
            binaryImg = DetectObjects.DoBinaryThreshold(backProjectImg, 200);
            morphologyImgBox.Image = binaryImg.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void erodeButton_Click(object sender, RoutedEventArgs e)
        {
            morphologyImg = DetectObjects.DoErode(binaryImg,Convert.ToInt32(erodeTextBox.Text));
            morphologyImgBox.Image = morphologyImg.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void dialteButton_Click(object sender, RoutedEventArgs e)
        {
            morphologyImg = DetectObjects.DoDilate(morphologyImg,Convert.ToInt32(dialteTextBox.Text));
            morphologyImgBox.Image = morphologyImg.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void drawContoursButton_Click(object sender, RoutedEventArgs e)
        {
            contoursImg = morphologyImg.Convert<Bgr, byte>();
            contoursImgBox.Image = DetectObjects.DrawAllContoursOnImg(DetectObjects.DoContours(morphologyImg), contoursImg).Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void findTopThreeContourButton_Click(object sender, RoutedEventArgs e)
        {
            contoursImg = morphologyImg.Convert<Bgr, byte>();
            topContours = DetectObjects.GetOrderMaxContours(morphologyImg);
            contoursImgBox.Image = DetectObjects.DrawContoursTopThreeBoundingBoxOnImg(topContours, contoursImg).Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        }

        private void compareHistButton_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            ShowHistViewer(templateHistImgBox, showTemplateHistImg, "樣板影像");
            foreach (Contour<System.Drawing.Point> c in topContours)
            {
                if (i == 3)
                    break;
                DenseHistogram observedRectHist;
                Image<Bgr, byte> observedContourRectImg = DetectObjects.GetBoundingBoxImage(c, loadTestImg);
                double compareRate = DetectObjects.CompareHistogram(templateHist, observedContourRectImg, out observedRectHist);
                showObservedHistImg = SystemToolBox.DrawHsvHistogram(observedRectHist);
                ShowHistViewer(new ImageViewer(), showObservedHistImg, "觀察影像" + i);
                System.Windows.MessageBox.Show("compareRate is =" + compareRate.ToString());
                i++;
               
            }
        }
        private string GetMappingDescriptorDataFile(string histogramFileId)
        {
            string path = dir.Parent.Parent.Parent.FullName + @"\SigbBoardHistData";
            string filename;
            if (Directory.Exists(path))
            {
                filename = path;
                return filename;
            }
            else
            {
                System.Windows.MessageBox.Show("沒有對應的特徵檔案!");
                return null;
            }
        }
        private void getMappingFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (templateHistFilePathName != null)
            {
                string templateHistFileName = System.IO.Path.GetFileName(templateHistFilePathName); //取得路徑的檔案名稱
                templateSURFPathFileName = GetMappingDescriptorDataFile(templateHistFileName);
                if (templateSURFPathFileName != null)
                {
                    templateSurfFeature = MatchRecognition.ReadSURFFeature(templateSURFPathFileName);

                    SystemToolBox.DrawSURFFeature(templateSurfFeature);
                }

            }
        }

        private void matchFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (templateSurfFeature != null)
            {
                foreach (Contour<System.Drawing.Point> c in topContours)
                {
                    DenseHistogram observedRectHist;
                    Image<Bgr, byte> observedContourRectImg = DetectObjects.GetBoundingBoxImage(c, loadTestImg);
                    double compareRate = DetectObjects.CompareHistogram(templateHist, observedContourRectImg,out observedRectHist);
                    MatchRecognition.MatchSURFFeature(templateSurfFeature, observedContourRectImg,true); //先使用原影像
                }
            }
        }

        #endregion

       

       
    }
}
