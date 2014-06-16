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
    /// GoodsRecognitionExperiment.xaml 的互動邏輯
    /// </summary>
    public partial class GoodsRecognitionExperiment : System.Windows.Controls.UserControl
    {
        GoodsRecognition goodsRecogSys;
        DirectoryInfo dir;
        Timer capTimer;
        Capture capture;
        int FPS = 30;
        bool isRunCamera;
        Image<Bgr, byte> observedImg;
        public GoodsRecognitionExperiment()
        {
            InitializeComponent();
            dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            capTimer = new Timer();
            try{
                capture = new Capture();
            }
            catch(Exception e) {
                System.Windows.MessageBox.Show(e.Message + "\n攝影機裝置有問題或是沒有安裝");
            }
            isRunCamera = false;
        }

        private void capTimer_Tick(object sender, EventArgs e){ 
             //如果有影片
            if (isRunCamera)
            {
                observedImg = capture.QueryFrame();
                if (observedImg != null) {
                    if (goodsRecogSys != null)
                        goodsRecogSys.SetupInputImage(observedImg);
                    else
                        goodsRecogSys = new GoodsRecognition(observedImg);

                    string goodData = goodsRecogSys.RunRecognition(true);
                    System.Windows.MessageBox.Show("商品資訊:" + goodData);
                }
            }
        
        }

        private void openCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                isRunCamera = true;
                //設定播放用的Timer
                capTimer.Tick += capTimer_Tick;
                capTimer.Interval = 1000 / FPS;
                capTimer.Start();
            }
            
           
        }

        private void openImgButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = OpenImgFile();
            if (filename != null) {
                try
                {
                    //此部分程式使用時要擺放在取得影像區塊
                    //-----------
                    Image<Bgr, byte> observedImg = new Image<Bgr, byte>(filename);

                    if (goodsRecogSys != null)
                        goodsRecogSys.SetupInputImage(observedImg);
                    else
                        goodsRecogSys = new GoodsRecognition(observedImg);

                    string goodData = goodsRecogSys.RunRecognition(true);
                    System.Windows.MessageBox.Show("商品資訊:" + goodData);
                    //-----------
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
              
            }

        }

        #region 開檔
        private string OpenImgFile()
        {
            string loadImgPath = dir.Parent.Parent.Parent.FullName + @"\TestGoods";
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

        #region 取出物體所在的ROI影像
        ////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 取出ROI物體圖像
        /// </summary>
        /// <param name="filteredSkinObserved">過濾膚色的圖像</param>
        /// <returns>回傳物體圖像</returns>
        private Image<Bgr, Byte> GetObjectBoundingBoxImg(Image<Bgr, Byte> filteredSkinObserved)
        {
            //get bounding box
            Image<Gray, Byte> gray = filteredSkinObserved.Convert<Gray, Byte>();
            gray = DetectObjects.DoBinaryThreshold(gray, 0);
            gray._Erode(5);
            gray._Dilate(5);
            new ImageViewer(gray).Show();
            Contour<System.Drawing.Point> sceneContours = DetectObjects.DoContours(gray);
            Contour<System.Drawing.Point> objectBox = DetectObjects.GetMaxContours(sceneContours);
            Image<Bgr, byte> boxImg = DetectObjects.GetBoundingBoxImage(objectBox, filteredSkinObserved);
            new ImageViewer(boxImg).Show();
            return boxImg;

        }
        /// <summary>
        /// 膚色過濾
        /// </summary>
        /// <param name="observed">要對得觀察景象</param>
        /// <returns>回傳過濾掉的膚色</returns>
        private Image<Bgr, Byte> SkinFilter(Image<Bgr, Byte> observed)
        {
            Image<Bgr, Byte> dst = observed.Copy();
            Image<Hsv, Byte> hsv = observed.Convert<Hsv, Byte>();
            //皮膚偵測並填為黑
            Image<Gray, Byte> skin = new Image<Gray, byte>(observed.Size);
            CvInvoke.cvInRangeS(hsv, new MCvScalar(0, 58, 89), new MCvScalar(25, 173, 229), skin);
            skin = FillHoles(skin); skin._Erode(2); skin._Dilate(2);
            Contour<System.Drawing.Point> skinContours = DetectObjects.DoContours(skin);
            Contour<System.Drawing.Point> max = DetectObjects.GetMaxContours(skinContours);
            //new ImageViewer(DetectObjects.DrawMaxContoursOnImg(max, dst)).Show();
            dst.Draw(max, new Bgr(0, 0, 0), -1);
            //new ImageViewer(dst).Show();
            return dst;

        }

        ////////////////////////////////////////////////////////////////////////////
        #endregion

        #region Flood Fill
        ////////////////////////////////////////////////////////////////////////////
        private Image<Gray, byte> FillHoles(Image<Gray, byte> image, int minArea, int maxArea)
        {
            var resultImage = image.CopyBlank();
            Gray gray = new Gray(255);

            using (var mem = new MemStorage())
            {
                for (var contour = image.FindContours(
                    CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                    RETR_TYPE.CV_RETR_CCOMP,
                    mem); contour != null; contour = contour.HNext)
                {
                    if ((contour.Area < maxArea) && (contour.Area > minArea))
                        resultImage.Draw(contour, gray, -1);
                }
            }

            return resultImage;
        }
        private Image<Gray, byte> FillHoles(Image<Gray, byte> image)
        {
            var resultImage = image.CopyBlank();
            Gray gray = new Gray(255);
            using (var mem = new MemStorage())
            {
                for (var contour = image.FindContours(
                    CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                    RETR_TYPE.CV_RETR_CCOMP,
                    mem); contour != null; contour = contour.HNext)
                {
                    resultImage.Draw(contour, gray, -1);
                }
            }

            return resultImage;
        }
        ////////////////////////////////////////////////////////////////////////////
        #endregion
    }
}
