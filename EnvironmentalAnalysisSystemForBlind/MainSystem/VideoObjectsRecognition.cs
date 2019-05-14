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
using SpeechLib;
namespace MainSystem
{
    public class VideoObjectsRecognition
    {
        Image<Bgr, Byte> observedImg;
        ImageViewer viewer;
        List<string> surfFiles;
        Dictionary<string, SURFFeatureData> surfDatas;
        List<string> histFiles;
        Dictionary<string, DenseHistogram> histDatas;
        DirectoryInfo dir;
        Dictionary<string, string> objectsData = new Dictionary<string, string>();
        SURFFeatureData obervedSurfData;
        SpVoice voice;
        public VideoObjectsRecognition(Image<Bgr, Byte> observedSrcImg)
        {
            viewer = new ImageViewer();
            viewer.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            viewer.Location = new Point(860,200);
            SetUpSignBoardSURFFeatureData();
            observedImg = observedSrcImg.Copy();
            obervedSurfData = null;
            //要有深度過濾才行使用此code
            //Image<Bgr, Byte> dst = SkinFilter(observedImg);
            //objectImg = GetObjectBoundingBoxImg(dst);
            
            //取得專案執行檔所在的目錄=>WPF:AppDomain.CurrentDomain.BaseDirectory
            //使用DirectoryInfo移動至上層
            dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string projectPath = dir.Parent.Parent.Parent.FullName;

            LoadHistogramDataFiles(projectPath);

            LoadSurfDataFiles(projectPath);

            surfDatas = new Dictionary<string, SURFFeatureData>();
            histDatas = new Dictionary<string, DenseHistogram>();

            voice = new SpVoice();
            voice.Voice = voice.GetVoices(string.Empty, string.Empty).Item(0);//Item(0)中文女聲
        }

        private void LoadHistogramDataFiles(string projectPath)
        {
            //讀取Histogram檔案的目錄下所有檔案名稱
            Console.WriteLine("\nPath=>" + projectPath + "\n");
            //隨著此類別存放的位置不同,要重新設定檔案路徑
            if (Directory.Exists(projectPath + @"\SigbBoardHistData-bin50"))
            {
                histFiles = Directory.GetFiles(projectPath + @"\SigbBoardHistData-bin50").ToList();
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
            obervedSurfData = null;
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
            Image<Bgr, byte> observedContourRectImg = null;
            if (surfFiles.Count != 0 && histFiles.Count !=0)
            {
                Stopwatch watch = Stopwatch.StartNew();
               
                ////偵測物體
                foreach (string histFilePath in histFiles)
                {
                    //1.取出直方圖資料
                    DenseHistogram hist;
                    string histFilename = System.IO.Path.GetFileName(histFilePath);
                    if (!histDatas.ContainsKey(histFilename))
                    {
                        hist = DetectObjects.ReadHistogram(histFilePath, false);
                        histDatas.Add(histFilename, hist);
                    }
                    else {
                        hist = histDatas[histFilename];
                    }
                    
                    //2.取出SURF資料
                    string templateHistFileName = System.IO.Path.GetFileName(histFilePath); //取得路徑的檔案名稱
                    string templateSURFPathFileName = SystemToolBox.GetMappingDescriptorDataFile(templateHistFileName, dir);

                    SURFFeatureData templateSurf;
                    string surfFilename = System.IO.Path.GetFileName(templateSURFPathFileName);
                    if (!surfDatas.ContainsKey(surfFilename))
                    {
                        templateSurf = MatchRecognition.ReadSURFFeature(templateSURFPathFileName);
                        surfDatas.Add(surfFilename, templateSurf);
                    }
                    else
                    {
                        templateSurf = surfDatas[surfFilename];
                    }

                    //3.做偵測
                    using(Image<Gray, byte> backProjectImg = DetectObjects.DoBackProject(hist, observedImg))
                    using (Image<Gray, byte> binaryImg = DetectObjects.DoBinaryThreshold(backProjectImg, 200)) { 
                        Image<Gray, byte> morphologyImg = DetectObjects.DoErode(binaryImg, 2);
                        morphologyImg = DetectObjects.DoDilate(morphologyImg, 1);
                        List<Contour<System.Drawing.Point>> topContours = DetectObjects.GetOrderMaxContours(morphologyImg);

                         //new ImageViewer(DetectObjects.DrawContoursTopThreeBoundingBoxOnImg(topContours, observedImg.Copy())).Show();
                         int i = 0;
                         double histMatchRate = 1;
                         int matchIndex = -1;
                         foreach (Contour<System.Drawing.Point> c in topContours)
                         {
                             if (i == 3)
                                 break;
                             //判斷待偵測的輪廓面積是否過小，如果太小就省略
                             if (c.Area >= (templateSurf.GetImg().Width * templateSurf.GetImg().Height) * 0.4)
                             {
                                 DenseHistogram observedRectHist;
                                 observedContourRectImg = DetectObjects.GetBoundingBoxImage(c, observedImg.Copy());
                                 double compareRate = DetectObjects.CompareHistogram(hist, observedContourRectImg, out observedRectHist);
                                 if (compareRate < histMatchRate)
                                 {
                                     histMatchRate = compareRate;
                                     matchIndex = i;
                                 }
                                 observedRectHist.Dispose();
                             }
                             i++;
                         }

                         if (histMatchRate < 0.5)
                         {
                             //影像正規化(如果觀察影像過大的話)
                            // if (observedContourRectImg != null && observedContourRectImg.Height * observedContourRectImg.Width > templateSurf.GetImg().Width * templateSurf.GetImg().Height)
                                 //observedContourRectImg = observedContourRectImg.Resize(templateSurf.GetImg().Width, templateSurf.GetImg().Height, INTER.CV_INTER_LINEAR);
                             //取出特徵
                             if (obervedSurfData == null && observedContourRectImg != null)
                             {
                                 obervedSurfData = SURFMatch.CalSURFFeature(observedContourRectImg);
                                 observedContourRectImg.Dispose();
                             }
                             //匹配特徵並取回匹配到的特徵
                             SURFMatchedData mathedCandidateData = MatchRecognition.MatchSURFFeatureForVideoObjs(templateSurf, obervedSurfData, null);
                             //招出最好的特徵
                             if (mathedCandidateData != null && mathedCandidateData.GetHomography() != null)
                             {
                                 if (mathedObjectsData == null )
                                 {
                                     mathedObjectsData = mathedCandidateData;
                                     matchedFileName = templateSURFPathFileName;
                                 }
                                 else if (mathedCandidateData.GetMatchedCount() > mathedObjectsData.GetMatchedCount() && mathedCandidateData.GetHomography() !=null)
                                 {
                                     mathedObjectsData = mathedCandidateData;
                                     matchedFileName = templateSURFPathFileName;
                                 }
                             }
                         }
                         
                         morphologyImg.Dispose();

                         topContours.Clear();
                         if (mathedObjectsData != null && obervedSurfData != null)
                             SURFMatch.ShowSURFMatchForm(mathedObjectsData, obervedSurfData, viewer);
                    }
                }
                watch.Stop();
                Console.WriteLine("File = " + System.IO.Path.GetFileName(matchedFileName) + " Video Analytics time = " + watch.ElapsedMilliseconds);
                //if (matchedFileName != null)
                //{
                //    string[] split = System.IO.Path.GetFileName(matchedFileName).Split('b');
                //    //voice.Speak("前方有" + split[0], SpeechVoiceSpeakFlags.SVSFlagsAsync);
                //}
            }
            
                
                
        }
        //目前寫死,要加入的看板資訊(切記,要有對應到的特徵檔案)
        private void SetUpSignBoardSURFFeatureData() 
        {
            this.objectsData.Add("TW000000","喜樂牙醫");
            
        }
    }
}
