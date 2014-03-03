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

namespace SURFMethond
{
    public class SURFMatch
    {

        public static SURFFeatureData CalSURFFeature(Image<Bgr, Byte> srcImage,MCvSURFParams surfParam)
        {
            SURFDetector surfCPU = new SURFDetector(surfParam);
            VectorOfKeyPoint keyPoints;
            Matrix<float> descriptors = null;
           
            using (Image<Gray, Byte> grayImg = srcImage.Convert<Gray, Byte>())
            {
                keyPoints = surfCPU.DetectKeyPointsRaw(grayImg, null);
                descriptors = surfCPU.ComputeDescriptorsRaw(grayImg, null, keyPoints);

            }
            return new SURFFeatureData(srcImage.Copy(), keyPoints, descriptors);
        }
        public static SURFFeatureData CalSURFFeature(Image<Bgr, Byte> srcImage)
        {
            SURFDetector surfCPU = new SURFDetector(new MCvSURFParams(1200, false)); //越高,特徵點越少
            VectorOfKeyPoint keyPoints;
            Matrix<float> descriptors = null;
            using (Image<Gray, Byte> grayImg = srcImage.Convert<Gray, Byte>())
            {
                keyPoints = surfCPU.DetectKeyPointsRaw(grayImg, null);
                descriptors = surfCPU.ComputeDescriptorsRaw(grayImg, null, keyPoints);
            }
            return new SURFFeatureData(srcImage.Copy(), keyPoints, descriptors);
        }
        #region Features2DTracker use but have some problem
        
        public static int MatchSURFFeatureByFLANN2(SURFFeatureData template, SURFFeatureData observedScene, bool isDraw)
        {
            List<KeyValuePair<int, int>> ptPairs = new List<KeyValuePair<int, int>>();
            ImageFeature<float>[] modelFeature =  ImageFeature<float>.ConvertFromRaw(template.GetKeyPoints(), template.GetDescriptors());
            ImageFeature<float>[] observedFeature = ImageFeature<float>.ConvertFromRaw(observedScene.GetKeyPoints(), observedScene.GetDescriptors());
            int goodMatchCount = 0;
            Stopwatch watch;
            watch = Stopwatch.StartNew();
            goodMatchCount = FlannFindPairs(modelFeature, observedFeature,ref ptPairs);
            watch.Stop();
            Console.WriteLine("\nCal SURF Match time=======\n=> " + watch.ElapsedMilliseconds.ToString() + "ms\nCal SURF Match time=======");
            return goodMatchCount;
        }
        
        #endregion

        static int FlannFindPairs(ImageFeature<float>[] modelDescriptors, ImageFeature<float>[] imageDescriptors, ref List<KeyValuePair<int, int>> ptPairs)
        {
            //Check if we have some valid model descriptors
            if (modelDescriptors.Length == 0)
                return -1;

            int length = modelDescriptors[0].Descriptor.Length;

            //Create matrix object and matrix image
            var matrixModel = new Matrix<float>(modelDescriptors.Length, length);
            var matrixImage = new Matrix<float>(imageDescriptors.Length, length);
            
            //copy model descriptors into matrixModel
            int row = 0;
            foreach (var modelDescriptor in modelDescriptors)
            {
                for (int i = 0; i < modelDescriptor.Descriptor.Length; i++)
                {
                    matrixModel[row, i] = modelDescriptor.Descriptor[i];
                }

                row++;
            }

            //copy image descriptors into matrixImage
            row = 0;
            foreach (var imageDescriptor in imageDescriptors)
            {
                for (int i = 0; i < imageDescriptor.Descriptor.Length; i++)
                {
                    matrixImage[row, i] = imageDescriptor.Descriptor[i];
                }

                row++;
            }

            //create return matrices for KnnSearch
            var indices = new Matrix<int>(modelDescriptors.Length, 2);
            var dists = new Matrix<float>(modelDescriptors.Length, 2);

            //create our flannIndex
            var flannIndex = new Index(matrixImage);
            
            //do the search
            flannIndex.KnnSearch(matrixModel, indices, dists, 2, 2);

            //filter out all unnecessary pairs based on distance between pairs
            int pairCount = 0;
            for (int i = 0; i < indices.Rows; i++)
            {
                if (dists.Data[i, 0] < 0.6 * dists.Data[i, 1])
                {
                    ptPairs.Add(new KeyValuePair<int, int>(i, indices.Data[i, 0]));
                    pairCount++;
                }
            }
            //return the pair count
            return pairCount;
        }
        
        public static int MatchSURFFeatureByFLANN(SURFFeatureData template, SURFFeatureData observedScene, bool isDraw)
        {
            Matrix<byte> mask;
            int k = 2;
            double uniquenessThreshold = 0.5; //default :0.8
            Matrix<int> indices;
            HomographyMatrix homography = null;
            Stopwatch watch;
            Matrix<float> dists;
            int pairCount = 0;
            Matrix<int> compareDists ;
            
            try
            {
                watch = Stopwatch.StartNew();
                #region Surf for CPU
                //match 
                Index flann = new Index(template.GetDescriptors(),4);
                
                indices = new Matrix<int>(observedScene.GetDescriptors().Rows, k);
                using (dists = new Matrix<float>(observedScene.GetDescriptors().Rows, k))
                {
                    flann.KnnSearch(observedScene.GetDescriptors(), indices, dists, k, 2);
                    mask = new Matrix<byte>(dists.Rows, 1);
                    mask.SetValue(255); //Mask is 拉式信號匹配
                    Features2DToolbox.VoteForUniqueness(dists, uniquenessThreshold, mask);
                    Console.WriteLine("\n==============\nVoteForUniqueness Mask:");
                    //SystemToolKits.ShowMaskDataOnConsole(mask);
                    Console.WriteLine("==============\n");
                }
                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                Console.WriteLine("\nVoteForUniqueness nonZeroCount=======\n=> " + nonZeroCount.ToString() + "\nVoteForUniqueness nonZeroCount=======");
                if (nonZeroCount >= 10) //原先是4
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 1.2, 50);
                    Console.WriteLine("\nVoteForSizeAndOrientation nonZeroCount=======\n=> " + nonZeroCount.ToString() + "\nVoteForSizeAndOrientation nonZeroCount=======");
                    if (nonZeroCount >=25) //原先是4
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 5);
                        
                }
                #endregion
                watch.Stop();
                Console.WriteLine("\nCal SURF Match time=======\n=> " + watch.ElapsedMilliseconds.ToString() + "ms\nCal SURF Match time=======");
                PointF[] matchPts = GetMatchBoundingBox(homography, template);
                if (isDraw)
                {
                    //Draw the matched keypoints
                    Image<Bgr, byte>  result = Features2DToolbox.DrawMatches(template.GetImg(), template.GetKeyPoints(), observedScene.GetImg(), observedScene.GetKeyPoints(),
                       indices, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.DEFAULT);
                    if (matchPts != null)
                    {
                        result.DrawPolyline(Array.ConvertAll<PointF, Point>(matchPts, Point.Round), true, new Bgr(Color.Red), 2);
                    }
                    new ImageViewer(result, "顯示匹配圖像").Show();
                }

                return nonZeroCount;
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
                return 0;
            }
        }

        public static Image<Bgr, byte> MatchSURFFeatureByBF(SURFFeatureData template, SURFFeatureData observedScene)
        {
            //This matrix indicates which row is valid for the matches.
            Matrix<byte> mask;
            //Number of nearest neighbors to search for
            int k = 8;
            //The distance different ratio which a match is consider unique, a good number will be 0.8
            double uniquenessThreshold = 0.3;  //default 0.8

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
                    mask.SetValue(255);
                    Features2DToolbox.VoteForUniqueness(distance, uniquenessThreshold, mask);
                    
                }

                Image<Bgr, byte> result = null;

                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                Console.WriteLine("\nVoteForUniqueness nonZeroCount=======\n=> " + nonZeroCount.ToString() + "\nVoteForUniqueness nonZeroCount=======");
                if (nonZeroCount >= 10)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), trainIdx, mask, 1.2, 50);  //default 1.5,10
                    Console.WriteLine("\nVoteForSizeAndOrientation nonZeroCount=======\n=> " + nonZeroCount.ToString() + "\nVoteForSizeAndOrientation nonZeroCount=======");
                    if (nonZeroCount >= 15) //default 4
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), trainIdx, mask, 5);
                   
                    PointF[] matchPts = GetMatchBoundingBox(homography, template);

                    //Draw the matched keypoints
                    result = Features2DToolbox.DrawMatches(template.GetImg(), template.GetKeyPoints(), observedScene.GetImg(), observedScene.GetKeyPoints(),
                        trainIdx, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.DEFAULT);
                    if (matchPts != null)
                    {
                        result.DrawPolyline(Array.ConvertAll<PointF, Point>(matchPts, Point.Round), true, new Bgr(Color.Red), 2);
                    }
                }
                #endregion
                watch.Stop();
                Console.WriteLine("\nCal SURF Match time=======\n=> " + watch.ElapsedMilliseconds.ToString() + "\nCal SURF Match time=======");
                
                return result;
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
                return null;
            }
        }



        public static int MatchSURFFeatureByBF(SURFFeatureData template, SURFFeatureData observedScene,bool isDraw)
        {
            Matrix<byte> mask;
            int k = 2;
            double uniquenessThreshold = 0.5;  //default 0.8
            Matrix<int> indices;
            HomographyMatrix homography = null;
            Stopwatch watch;
            try
            {
                watch = Stopwatch.StartNew();
                #region Surf for CPU
                //match 
                BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2Sqr);
                matcher.Add(template.GetDescriptors());
                
                indices = new Matrix<int>(observedScene.GetDescriptors().Rows, k);
                using (Matrix<float> dist = new Matrix<float>(observedScene.GetDescriptors().Rows, k))
                {
                    matcher.KnnMatch(observedScene.GetDescriptors(), indices, dist, k, null);
                    mask = new Matrix<byte>(dist.Rows, 1);
                    mask.SetValue(255);
                    Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
                }

                int nonZeroCount = CvInvoke.cvCountNonZero(mask);
                Console.WriteLine("\nVoteForUniqueness nonZeroCount=======\n=> " + nonZeroCount.ToString() + "\nVoteForUniqueness nonZeroCount=======");
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 1.2, 50);  //default 1.5,10
                    Console.WriteLine("\nVoteForSizeAndOrientation nonZeroCount=======\n=> " + nonZeroCount.ToString() + "\nVoteForSizeAndOrientation nonZeroCount=======");
                    if (nonZeroCount >= 15) //default 4
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(template.GetKeyPoints(), observedScene.GetKeyPoints(), indices, mask, 5);
                }
                #endregion
                watch.Stop();
                Console.WriteLine("\nCal SURF Match time=======\n=> " + watch.ElapsedMilliseconds.ToString() + "\nCal SURF Match time=======");
                PointF[] matchPts = GetMatchBoundingBox(homography, template);
                if (isDraw)
                {
                    //Draw the matched keypoints
                    Image<Bgr, byte>  result = Features2DToolbox.DrawMatches(template.GetImg(), template.GetKeyPoints(), observedScene.GetImg(), observedScene.GetKeyPoints(),
                       indices, new Bgr(255, 255, 255), new Bgr(255, 255, 255), mask, Features2DToolbox.KeypointDrawType.DEFAULT);
                    if (matchPts != null)
                    {
                        result.DrawPolyline(Array.ConvertAll<PointF, Point>(matchPts, Point.Round), true, new Bgr(Color.Red), 2);
                    }
                    new ImageViewer(result, "顯示匹配圖像");
                }
                    
                return nonZeroCount;
            }
            catch (CvException ex)
            {
               System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
               return 0;
            }
        }

        public static PointF[] GetMatchBoundingBox(HomographyMatrix homography,SURFFeatureData template) 
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
    }
}
