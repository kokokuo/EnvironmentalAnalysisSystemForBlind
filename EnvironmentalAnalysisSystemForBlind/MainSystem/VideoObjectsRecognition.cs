using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.Util;
using RecognitionSys;
using RecognitionSys.ToolKits;
using RecognitionSys.ToolKits.SURFMethod;
//stopWatch計算時間用
using System.Diagnostics;
namespace MainSystem
{
    public class VideoObjectsRecognition
    {
        Image<Bgr, Byte> observedImg;
        ImageViewer viewer;
        List<string> surfFiles;
        List<string> histFiles;
        DirectoryInfo dir;
        Dictionary<string, string> objectsData = new Dictionary<string, string>();
        public VideoObjectsRecognition(Image<Bgr, Byte> observedSrcImg)
        {
            viewer = new ImageViewer();
            SetUpSignBoardSURFFeatureData();
            observedImg = observedSrcImg.Copy();
            //要有深度過濾才行使用此code
            //Image<Bgr, Byte> dst = SkinFilter(observedImg);
            //objectImg = GetObjectBoundingBoxImg(dst);
            
            //取得專案執行檔所在的目錄=>WPF:AppDomain.CurrentDomain.BaseDirectory
            //使用DirectoryInfo移動至上層
            dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string projectPath = dir.Parent.Parent.Parent.FullName;

            LoadHistogramDataFiles(projectPath);

            LoadSurfDataFiles(projectPath);
            
        }

        private void LoadHistogramDataFiles(string projectPath)
        {
            //讀取Histogram檔案的目錄下所有檔案名稱
            Console.WriteLine("\nPath=>" + projectPath + "\n");
            //隨著此類別存放的位置不同,要重新設定檔案路徑
            if (Directory.Exists(projectPath + @"\SigbBoardHistData"))
            {
                histFiles = Directory.GetFiles(projectPath + @"\SigbBoardHistData").ToList();
            }
        }

        private void LoadSurfDataFiles(string projectPath)
        {
            //讀取surf檔案的目錄下所有檔案名稱
            Console.WriteLine("\nPath=>" + projectPath + "\n");
            //隨著此類別存放的位置不同,要重新設定檔案路徑
            if (Directory.Exists(projectPath + @"\SignBoardSURFFeatureData"))
            {
                surfFiles = Directory.GetFiles(projectPath + @"\SignBoardSURFFeatureData").ToList();
            }
        }

        /// <summary>
        /// 設置要作為辨識的輸入影像
        /// </summary>
        /// <param name="observedSrcImg">要比對觀察的影響</param>
        public void SetupInputImage(Image<Bgr, Byte> observedSrcImg) 
        {
            observedImg = observedSrcImg.Copy();
            //要有深度過濾才行使用此code
            //Image<Bgr, Byte> dst = SkinFilter(observedImg);
           // objectImg = GetObjectBoundingBoxImg(dst);
        }

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
            Contour<Point> sceneContours = DetectObjects.DoContours(gray);
            Contour<Point> objectBox = DetectObjects.GetMaxContours(sceneContours);
            Image<Bgr, Byte> boxImg = DetectObjects.GetBoundingBoxImage(objectBox, filteredSkinObserved);
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
            Contour<Point> skinContours = DetectObjects.DoContours(skin);
            Contour<Point> max = DetectObjects.GetMaxContours(skinContours);
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

        /// <summary>
        /// 執行辨識
        /// </summary>
        /// <param name="isDrawResultToShowOnDialog">是否要顯示出辨識的結果</param>
        /// <returns>回傳看板資訊, 格式=>"看板名稱" ;請記得做字串切割,若無比對到或有任何問題則會回傳null</returns>
        public void RunRecognition(bool isDrawResultToShowOnDialog)
        {
            SURFMatchedData mathedObjectsData = null;
            string matchedFileName = null;
            
            if (surfFiles.Count != 0 && histFiles.Count !=0 && surfFiles.Count == histFiles.Count)
            {
                Stopwatch watch = Stopwatch.StartNew();
               
                ////偵測物體
                foreach (string histFilename in histFiles)
                {
                    DenseHistogram hist = DetectObjects.ReadHistogram(histFilename, true);
                    //反投影
                    Image<Gray, byte> backProjectImg = DetectObjects.DoBackProject(hist, observedImg);
                    Image<Gray, byte> binaryImg = DetectObjects.DoBinaryThreshold(backProjectImg, 200);
                    Image<Gray, byte> morphologyImg = DetectObjects.DoErode(binaryImg, 2);
                    morphologyImg = DetectObjects.DoDilate(morphologyImg, 1);
                    List<Contour<System.Drawing.Point>> topContours = DetectObjects.GetOrderMaxContours(morphologyImg);
                    int i = 0;
                    double histMatchRate = 0;
                    int matchIndex = -1;
                    foreach (Contour<System.Drawing.Point> c in topContours)
                    {
                        if (i == 3)
                            break;
                        DenseHistogram observedRectHist;
                        Image<Bgr, byte> observedContourRectImg = DetectObjects.GetBoundingBoxImage(c, observedImg);
                        double compareRate = DetectObjects.CompareHistogram(hist, observedContourRectImg, out observedRectHist);
                        if (compareRate < histMatchRate)
                        {
                            histMatchRate = compareRate;
                            matchIndex = i;
                        }
                        i++;
                    }
                    if (histMatchRate < 0.5)
                    {
                        //顏色特徵相似,取出對應的特徵資料做辨識

                        string templateHistFileName = System.IO.Path.GetFileName(histFilename); //取得路徑的檔案名稱
                        string templateSURFPathFileName = SystemToolBox.GetMappingDescriptorDataFile(templateHistFileName, dir);
                       
                        //匹配特徵並取回匹配到的特徵
                        SURFMatchedData mathedCandidateData = MatchRecognition.MatchSURFFeatureForVideoObjs(templateSURFPathFileName, observedImg, null);
                        //招出最好的特徵
                        if (mathedCandidateData != null)
                        {
                            if (mathedObjectsData == null)
                            {
                                mathedObjectsData = mathedCandidateData;
                                matchedFileName = templateSURFPathFileName;
                            }
                            else if (mathedCandidateData.GetMatchedCount() > mathedObjectsData.GetMatchedCount())
                            {
                                mathedObjectsData = mathedCandidateData;
                                matchedFileName = templateSURFPathFileName;
                            }
                        }
                    } 
                }
                watch.Stop();
                Console.WriteLine("Video Analytics time = " + watch.ElapsedMilliseconds);
                System.Windows.MessageBox.Show(System.IO.Path.GetFileName(matchedFileName));
                SURFMatch.ShowSURFMatchForm(mathedObjectsData, SURFMatch.CalSURFFeature(observedImg), viewer);
            }
            
                
                
        }
        //目前寫死,要加入的看板資訊(切記,要有對應到的特徵檔案)
        private void SetUpSignBoardSURFFeatureData() 
        {
            this.objectsData.Add("TW000000","喜樂牙醫");
            
        }
    }
}
