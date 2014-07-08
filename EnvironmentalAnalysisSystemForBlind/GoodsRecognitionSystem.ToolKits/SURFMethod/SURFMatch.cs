using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//stopWatch計算時間用
using System.Diagnostics;
//PointF
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
//VectorOfKeyPoint
using Emgu.CV.Util;
namespace RecognitionSys.ToolKits.SURFMethod
{
    /// <summary>
    /// SURF運算相關類別,可用來計算特徵或匹配特徵
    /// </summary>
    public class SURFMatch
    {
        #region 擷取特徵

        /// <summary>
        /// 計算特徵點
        /// </summary>
        /// <param name="srcImage">來源影像</param>
        /// <param name="surfParam">SURF的參數</param>
        /// <returns>回傳特徵類別</returns>
        public static SURFFeatureData CalSURFFeature(Image<Bgr, Byte> srcImage, MCvSURFParams surfParam)
        {
            SURFDetector surfCPU = new SURFDetector(surfParam);
            VectorOfKeyPoint keyPoints;
            Matrix<float> descriptors = null;
            Stopwatch watch;
            watch = Stopwatch.StartNew();
            using (Image<Gray, Byte> grayImg = srcImage.Convert<Gray, Byte>())
            {
                keyPoints = surfCPU.DetectKeyPointsRaw(grayImg, null);
                descriptors = surfCPU.ComputeDescriptorsRaw(grayImg, null, keyPoints);
            }
            watch.Stop();
            Console.WriteLine("\nExtract SURF time=> " + watch.ElapsedMilliseconds.ToString() + "ms");

            //抽取出的特徵點數量
            Console.WriteLine("keypoint size:" + keyPoints.Size);
            return new SURFFeatureData(srcImage.Copy(), keyPoints, descriptors);
        }
        /// <summary>
        /// 計算特徵點
        /// </summary>
        /// <param name="srcImage">來源影像</param>
        /// <returns>回傳特徵類別</returns>
        public static SURFFeatureData CalSURFFeature(Image<Bgr, Byte> srcImage)
        {
            SURFDetector surfCPU = new SURFDetector(new MCvSURFParams(1200, false)); //預設500
            VectorOfKeyPoint keyPoints;
            Matrix<float> descriptors = null;
            Stopwatch watch;
            watch = Stopwatch.StartNew();
            using (Image<Gray, Byte> grayImg = srcImage.Convert<Gray, Byte>())
            {
                keyPoints = surfCPU.DetectKeyPointsRaw(grayImg, null);
                descriptors = surfCPU.ComputeDescriptorsRaw(grayImg, null, keyPoints);
            }
            watch.Stop();
            Console.WriteLine("\nExtract SURF time=> " + watch.ElapsedMilliseconds.ToString() + "ms");

            //抽取出的特徵點數量
            Console.WriteLine("keypoint size:" + keyPoints.Size);
            return new SURFFeatureData(srcImage.Copy(), keyPoints, descriptors);
        }
        #endregion

        #region 商品辨識用
        /// <summary>
        /// 商品辨識使用BruteForce匹配(較精確但較慢)
        /// </summary>
        /// <param name="template">樣板的特徵點類別</param>
        /// <param name="observedScene">被觀察的場景匹配的特徵點</param>
        /// <returns>回傳匹配的資料類別</returns>
        public static SURFMatchedData MatchSURFFeatureByBruteForceForGoods(SURFFeatureData template, SURFFeatureData observedScene)
        {
            //This matrix indicates which row is valid for the matches.
            Matrix<byte> mask;
            //Number of nearest neighbors to search for
            int k = 2;
            //The distance different ratio which a match is consider unique, a good number will be 0.8 , NNDR match
            double uniquenessThreshold = 0.8; //default:0.8

            //The resulting n*k matrix of descriptor index from the training descriptors
            Matrix<int> indices;
            HomographyMatrix homography = null;
            Stopwatch watch;
            try
            {
                watch = Stopwatch.StartNew();
                #region bruteForce match for CPU
                //match 
                BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2Sqr); //default:L2
                matcher.Add(template.GetDescriptors());
                
                indices = new Matrix<int>(observedScene.GetDescriptors().Rows, k);
                //The resulting n*k matrix of distance value from the training descriptors
                using (Matrix<float> dist = new Matrix<float>(observedScene.GetDescriptors().Rows, k))
                {
                    matcher.KnnMatch(observedScene.GetDescriptors(), indices, dist, k, null);
                    #region Test Output
                    //for (int i = 0; i < indices.Rows; i++)
                    //{
                    //    for (int j = 0; j < indices.Cols; j++)
                    //    {
                    //        Console.Write(indices[i, j] + " ");
                    //    }
                    //    Console.Write("\n");
                    //}
                    //Console.WriteLine("\n distance");
                    //for (int i = 0; i < dist.Rows; i++)
                    //{
                    //    for (int j = 0; j < dist.Cols; j++)
                    //    {
                    //        Console.Write(dist[i, j] + " ");
                    //    }
                    //    Console.Write("\n");
                    //}
                    //Console.WriteLine("\n");  
                    #endregion
                 
                    mask = new Matrix<byte>(dist.Rows, 1);
                    mask.SetValue(255); //mask is 拉式信號
                    //http://stackoverflow.com/questions/21932861/how-does-features2dtoolbox-voteforuniqueness-work
                    //how the VoteForUniqueness work...
                    Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
                   
                }

                int nonZeroCount = CvInvoke.cvCountNonZero(mask); //means good match
                Console.WriteLine("-----------------\nVoteForUniqueness pairCount => " + nonZeroCount.ToString() + "\n-----------------");
                if (nonZeroCount >= 4)
                {
                    //50 is model and mathing image rotation similarity ex: m1 = 60 m2 = 50 => 60 - 50 <=50 so is similar
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 1.5, 50); //default:1.5 , 10
                    Console.WriteLine("VoteForSizeAndOrientation pairCount => " + nonZeroCount.ToString() + "\n-----------------");
                    if (nonZeroCount >= 15) //defalut :4 ,set 15
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 5);
                }
                #endregion
                watch.Stop();
                Console.WriteLine("Cal SURF Match time => " + watch.ElapsedMilliseconds.ToString() + "\n-----------------");

                return new SURFMatchedData(indices, homography, mask, nonZeroCount, template);
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
                return null;
            }
        }

        #region FLANN
        /// <summary>
        /// 匹配較快速但精確度較低
        /// </summary>
        /// <param name="template">樣板的特徵點類別</param>
        /// <param name="observedScene">被觀察的場景匹配的特徵點</param>
        /// <returns>回傳匹配的資料類別</returns>
        public static SURFMatchedData MatchSURFFeatureByFLANNForGoods(SURFFeatureData template, SURFFeatureData observedScene)
        {
            Matrix<byte> mask;
            int k = 1;
            double uniquenessThreshold = 0.8;//NNDR
            //The resulting n*k matrix of descriptor index from the training descriptors,存放找到的NN索引
            Matrix<int> indices;
            HomographyMatrix homography = null;
            Stopwatch watch;
            //distance
            Matrix<float> dists;

            try
            {
                watch = Stopwatch.StartNew();
                #region FLANN Match CPU
                //match 
                Index flann = new Index(template.GetDescriptors(), 12);

                indices = new Matrix<int>(observedScene.GetDescriptors().Rows, k);
                //dists是對應indices索引的距離值
                using (dists = new Matrix<float>(observedScene.GetDescriptors().Rows, k))
                {
                    //找出最好的NN
                    flann.KnnSearch(observedScene.GetDescriptors(), indices, dists, k, 12);
                    #region Test Output
                    //for (int i = 0; i < indices.Rows; i++)
                    //{
                    //    for (int j = 0; j < indices.Cols; j++)
                    //    {
                    //        Console.Write(indices[i, j] + " ");
                    //    }
                    //    Console.Write("\n");
                    //}
                    //Console.WriteLine("\n distance");
                    //for (int i = 0; i < dists.Rows; i++)
                    //{
                    //    for (int j = 0; j < dists.Cols; j++)
                    //    {
                    //        Console.Write(dists[i, j] + " ");
                    //    }
                    //    Console.Write("\n");
                    //}
                    //Console.WriteLine("\n");
                    #endregion

                    mask = new Matrix<byte>(dists.Rows, 1);
                    mask.SetValue(0);

                    //此emgucv是NNDR,已實驗過,如果要使用VoteForUniqueness 請把mask改回255,mask存放的是樣板與觀察對應的特徵點是否相似0表不是,255表一樣
                    // Features2DToolbox.VoteForUniqueness(dists, uniquenessThreshold, mask);
                    //如下,數值會一樣
                    //for (int i = 0; i < indices.Rows; i++)
                    //{
                    //    Console.WriteLine("距離比：" + (dists.Data[i, 0] / dists.Data[i, 1]));
                    //    if ((dists.Data[i, 0] / dists.Data[i, 1]) < 0.8)
                    //    {
                    //        mask.Data[i, 0] = 255;
                    //    }
                    //}

                    double min_dist = 100;
                    double max_dist = 0;

                    //FLANN 取自http://docs.opencv.org/doc/tutorials/features2d/feature_flann_matcher/feature_flann_matcher.html#feature-flann-matcher
                    //很微妙的是這個方法匹配效果更加...
                    for (int i = 0; i < indices.Rows; i++)
                    {
                        if (dists.Data[i, 0] < min_dist) min_dist = dists.Data[i, 0];
                        if (dists.Data[i, 0] > max_dist) max_dist = dists.Data[i, 0];
                    }
                    for (int i = 0; i < indices.Rows; i++)
                    {

                        if (dists.Data[i, 0] <= Math.Max(2 * min_dist, 0.02))
                        {
                            mask.Data[i, 0] = 255;
                        }
                    }

                    //for (int i = 0; i < mask.Rows; i++)
                    //{
                    //    Console.WriteLine(mask.Data[i, 0]);
                    //}
                }
                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                Console.WriteLine("good Match number:" + nonZeroCount);
                //Console.WriteLine("-----------------\nVoteForUniqueness pairCount => " + nonZeroCount.ToString() + "\n-----------------");
                if (nonZeroCount >= 4) //原先是4
                {
                    //nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 1.2, 50);
                    //Console.WriteLine("VoteForSizeAndOrientation pairCount => " + nonZeroCount.ToString() + "\n-----------------");
                    //filter out all unnecessary pairs based on distance between pairs

                    //if (nonZeroCount >= 30) //原先是4
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 5); //原先是5

                }
                #endregion
                watch.Stop();
                Console.WriteLine("Cal SURF Match time => " + watch.ElapsedMilliseconds.ToString() + "\n-----------------");


                return new SURFMatchedData(indices, homography, mask, nonZeroCount, template);
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
                return null;
            }
        }
        #endregion


        #endregion

        #region 商家看板辨識

        #region FLANN
        /// <summary>
        /// 匹配較快速但精確度較低
        /// </summary>
        /// <param name="template">樣板的特徵點類別</param>
        /// <param name="observedScene">被觀察的場景匹配的特徵點</param>
        /// <returns>回傳匹配的資料類別</returns>
        public static SURFMatchedData MatchSURFFeatureByFLANNForObjs(SURFFeatureData template, SURFFeatureData observedScene)
        {

            Matrix<byte> mask;
            int k = 1;
            double uniquenessThreshold = 0.5;//NNDR
            //The resulting n*k matrix of descriptor index from the training descriptors,存放找到的NN索引
            Matrix<int> indices; 
            HomographyMatrix homography = null;
            Stopwatch watch;
            //distance
            Matrix<float> dists;

            try
            {
                watch = Stopwatch.StartNew();
                #region FLANN Match CPU
                //match 
                Index flann = new Index(template.GetDescriptors(), 12);

                indices = new Matrix<int>(observedScene.GetDescriptors().Rows, k);
                //dists是對應indices索引的距離值
                using (dists = new Matrix<float>(observedScene.GetDescriptors().Rows, k))
                {
                    //找出最好的NN
                    flann.KnnSearch(observedScene.GetDescriptors(), indices, dists, k, 12);
                    #region Test Output
                    //for (int i = 0; i < indices.Rows; i++)
                    //{
                    //    for (int j = 0; j < indices.Cols; j++)
                    //    {
                    //        Console.Write(indices[i, j] + " ");
                    //    }
                    //    Console.Write("\n");
                    //}
                    //Console.WriteLine("\n distance");
                    //for (int i = 0; i < dists.Rows; i++)
                    //{
                    //    for (int j = 0; j < dists.Cols; j++)
                    //    {
                    //        Console.Write(dists[i, j] + " ");
                    //    }
                    //    Console.Write("\n");
                    //}
                    //Console.WriteLine("\n");
                    #endregion
                  
                    mask = new Matrix<byte>(dists.Rows, 1);
                    mask.SetValue(0);
                    
                    //此emgucv是NNDR,已實驗過,如果要使用VoteForUniqueness 請把mask改回255,mask存放的是樣板與觀察對應的特徵點是否相似0表不是,255表一樣
                     //Features2DToolbox.VoteForUniqueness(dists, uniquenessThreshold, mask);
                    //如下,數值會一樣
                    //for (int i = 0; i < indices.Rows; i++)
                    //{
                    //    Console.WriteLine("距離比：" + (dists.Data[i, 0] / dists.Data[i, 1]));
                    //    if ((dists.Data[i, 0] / dists.Data[i, 1]) < 0.8)
                    //    {
                    //        mask.Data[i, 0] = 255;
                    //    }
                    //}
                    
                    double min_dist = 100;
                    double max_dist = 0;
                   
                    //FLANN 取自http://docs.opencv.org/doc/tutorials/features2d/feature_flann_matcher/feature_flann_matcher.html#feature-flann-matcher
                    //很微妙的是這個方法匹配效果更加...
                    for (int i = 0; i < indices.Rows; i++)
                    {
                        if (dists.Data[i, 0] < min_dist) min_dist = dists.Data[i, 0];
                        if (dists.Data[i, 0] > max_dist) max_dist = dists.Data[i, 0];
                    }
                    for (int i = 0; i < indices.Rows; i++)
                    {

                        if (dists.Data[i, 0] <= Math.Max(2 * min_dist, 0.02))
                        {
                            mask.Data[i, 0] = 255;
                        }
                    }

                    //for (int i = 0; i < mask.Rows; i++)
                    //{
                    //    Console.WriteLine(mask.Data[i, 0]);
                    //}
                }
                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                Console.WriteLine("good Match number:" + nonZeroCount+",template keypoint number = "+ template.GetKeyPoints().Size);
                //Console.WriteLine("-----------------\nVoteForUniqueness pairCount => " + nonZeroCount.ToString() + "\n-----------------");
                //因為是小圖比大圖，可能有特徵點重複比對到觀察影像導致駔後留下的特徵做於樣板特徵，因為多數情境下，一個畫面中只需要比對出一個即可，應該不需要超過樣板特特徵...
                if (template.GetKeyPoints().Size > nonZeroCount && nonZeroCount >= template.GetKeyPoints().Size * 0.6) //原先是4
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 1.2, 50);
                    Console.WriteLine("VoteForSizeAndOrientation pairCount => " + nonZeroCount.ToString() + "\n-----------------");
                    //filter out all unnecessary pairs based on distance between pairs
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 5); //原先是5
                    watch.Stop();
                    Console.WriteLine("Cal SURF Match time => " + watch.ElapsedMilliseconds.ToString() + "\n-----------------");

                    if (template.GetKeyPoints().Size > nonZeroCount && nonZeroCount >= template.GetKeyPoints().Size * 0.5)
                    {
                        return new SURFMatchedData(indices, homography, mask, nonZeroCount, template);
                    }
                    else {
                        return null;
                    }
                    //if (nonZeroCount >= template.GetKeyPoints().Size * 0.5) //原先是4
                       
                   
                    
                }
                else {
                    watch.Stop();
                    Console.WriteLine("Cal SURF Match time => " + watch.ElapsedMilliseconds.ToString() + "\n-----------------");
                    return null;
                }
                #endregion
                


                
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
                return null;
            }
        }
        #endregion
    
        /// <summary>
        /// 環境看板辨識使用BruteForce匹配(較精確但較慢)
        /// </summary>
        /// <param name="template">樣板的特徵點類別</param>
        /// <param name="observedScene">被觀察的場景匹配的特徵點</param>
        /// <returns>回傳匹配的資料類別</returns>
        public static SURFMatchedData MatchSURFFeatureByBruteForceForObjs(SURFFeatureData template, SURFFeatureData observedScene)
        {
            //This matrix indicates which row is valid for the matches.
            Matrix<byte> mask;
            //Number of nearest neighbors to search for
            int k = 5;
            //The distance different ratio which a match is consider unique, a good number will be 0.8 , NNDR match
            double uniquenessThreshold = 0.5;  //default 0.8

            //The resulting n*k matrix of descriptor index from the training descriptors
            Matrix<int> trainIdx;
            HomographyMatrix homography = null;
            Stopwatch watch;

            try
            {
                watch = Stopwatch.StartNew();
                #region Surf for CPU
                //match 
                BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2Sqr);
                matcher.Add(template.GetDescriptors());

                trainIdx = new Matrix<int>(observedScene.GetDescriptors().Rows, k);
                //The resulting n*k matrix of distance value from the training descriptors
                using (Matrix<float> distance = new Matrix<float>(observedScene.GetDescriptors().Rows, k))
                {
                    matcher.KnnMatch(observedScene.GetDescriptors(), trainIdx, distance, k, null);
                    mask = new Matrix<byte>(distance.Rows, 1);
                    mask.SetValue(255); //Mask is 拉式信號匹配 
                    //http://stackoverflow.com/questions/21932861/how-does-features2dtoolbox-voteforuniqueness-work
                    //how the VoteForUniqueness work...
                    Features2DToolbox.VoteForUniqueness(distance, uniquenessThreshold, mask);

                }

                Image<Bgr, byte> result = null;
                int nonZeroCount = CvInvoke.cvCountNonZero(mask); //means good match
                Console.WriteLine("VoteForUniqueness nonZeroCount=> " + nonZeroCount.ToString());
                if (nonZeroCount >= (template.GetKeyPoints().Size * 0.2)) //set 10
                {
                    //50 is model and mathing image rotation similarity ex: m1 = 60 m2 = 50 => 60 - 50 <=50 so is similar
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), trainIdx, mask, 1.2, 50);  //default 1.5,10
                    Console.WriteLine("VoteForSizeAndOrientation nonZeroCount=> " + nonZeroCount.ToString());
                    if (nonZeroCount >= (template.GetKeyPoints().Size * 0.5)) //default 4 ,set 15
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), trainIdx, mask, 5);

                    PointF[] matchPts = GetMatchBoundingBox(homography, template);

                    //Draw the matched keypoints
                    result = Features2DToolbox.DrawMatches(template.GetImg(), template.GetKeyPoints(), observedScene.GetImg(), observedScene.GetKeyPoints(),
                        trainIdx, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.NOT_DRAW_SINGLE_POINTS);
                    if (matchPts != null)
                    {
                        result.DrawPolyline(Array.ConvertAll<PointF, Point>(matchPts, Point.Round), true, new Bgr(Color.Red), 2);
                    }
                }
                #endregion
                watch.Stop();
                Console.WriteLine("\nCal SURF Match time=======\n=> " + watch.ElapsedTicks.ToString() + "ms\nCal SURF Match time=======");


                return new SURFMatchedData(trainIdx, homography, mask, nonZeroCount, template);
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
                return null;
            }
        }
        #endregion
      

        /// <summary>
        /// 取得對應出物體的ROI座標點
        /// </summary>
        /// <param name="homography">保存了相關匹配後的資訊(用來投影至商品上的匹配位置)</param>
        /// <param name="template">樣板特徵類別</param>
        /// <returns>回傳座標點</returns>
        public static PointF[] GetMatchBoundingBox(HomographyMatrix homography, SURFFeatureData template)
        {
            if (homography != null) //Get RoI box
            {
                //draw a rectangle along the projected model    
                PointF[] pts = new PointF[] { 
                        new PointF(template.GetImg().ROI.Left, template.GetImg().ROI.Bottom),
                        new PointF(template.GetImg().ROI.Right, template.GetImg().ROI.Bottom),
                        new PointF(template.GetImg().ROI.Right, template.GetImg().ROI.Top),
                        new PointF(template.GetImg().ROI.Left, template.GetImg().ROI.Top)
                };
                homography.ProjectPoints(pts); //project points
                return pts;
            }
            else
                return null;
        }
        /// <summary>
        /// 顯示畫出匹配的視窗
        /// </summary>
        /// <param name="matchData">匹配後回傳的資料類別</param>
        /// <param name="observedScene">觀察景象特徵資料</param>
        public static void ShowSURFMatchForm(SURFMatchedData matchData, SURFFeatureData observedScene,ImageViewer viewer) 
        {
           
                //Draw the matched keypoints
                Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(matchData.GetTemplateSURFData().GetImg(), matchData.GetTemplateSURFData().GetKeyPoints(), observedScene.GetImg(), observedScene.GetKeyPoints(),
                    matchData.GetIndices(), new Bgr(255, 255, 255), new Bgr(255, 255, 255), matchData.GetMask(), Features2DToolbox.KeypointDrawType.DEFAULT);

                if (matchData.GetHomography() != null) //Get RoI box
                {
                    PointF[] matchPts = GetMatchBoundingBox(matchData.GetHomography(), matchData.GetTemplateSURFData());
                    if (matchPts != null)
                    {
                        result.DrawPolyline(Array.ConvertAll<PointF, Point>(matchPts, Point.Round), true, new Bgr(Color.Red), 2);
                    }
                }
            
                viewer.Image = result;
                viewer.Show();
            
        }
    }
}
