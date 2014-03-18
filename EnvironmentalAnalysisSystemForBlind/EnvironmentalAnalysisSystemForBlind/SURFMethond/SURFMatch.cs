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
            Console.WriteLine("keypoint size" + keyPoints.Size); 
            return new SURFFeatureData(srcImage.Copy(), keyPoints, descriptors);
        }
        public static SURFFeatureData CalSURFFeature(Image<Bgr, Byte> srcImage)
        {
            SURFDetector surfCPU = new SURFDetector(new MCvSURFParams(1200, false)); //越高,特徵點越少
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
            Console.WriteLine("keypoint size" + keyPoints.Size); 
            return new SURFFeatureData(srcImage.Copy(), keyPoints, descriptors);
        }

        public static Image<Bgr, byte> MatchSURFFeatureByBF(SURFFeatureData template, SURFFeatureData observedScene,out long processingTime,out int pairCount)
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
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(template.GetKeyPoints(), observedScene.GetKeyPoints(), trainIdx, mask, 1.2, 30);  //default 1.5,10
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
                //Console.WriteLine("\nCal SURF Match time=======\n=> " + watch.ElapsedTicks.ToString() + "ms\nCal SURF Match time=======");
                processingTime = watch.ElapsedMilliseconds;
                pairCount = nonZeroCount;

                return result;
            }
            catch (CvException ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ErrorMessage);
               processingTime = -1L;
               pairCount = -1;
                return null;
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
