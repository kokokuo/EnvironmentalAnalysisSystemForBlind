using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// 包裝了匹配到的相關訊息類別
    /// </summary>
    public class SURFMatchedData
    {
        Matrix<int> indices;
        HomographyMatrix homography = null;
        Matrix<byte> mask;
        int matchedCount;
        SURFFeatureData templateSURFData;
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="indices"></param>
        /// <param name="homography">用來繪製ROI的矩陣</param>
        /// <param name="mask"></param>
        /// <param name="matchedCount">匹配點</param>
        /// <param name="template">樣板特徵類別</param>
        public SURFMatchedData(Matrix<int> indices, HomographyMatrix homography, Matrix<byte> mask, int matchedCount,SURFFeatureData template) 
        {
            this.indices = indices;
            this.homography = homography;
            this.mask = mask;
            this.matchedCount = matchedCount;
            this.templateSURFData = template;
        }
        /// <summary>
        /// The resulting n*k matrix of descriptor index from the training descriptors (取得訓練的後的n*k matrix結果)
        /// </summary>
        /// <returns></returns>
        public Matrix<int>  GetIndices()
        {
            return this.indices;
        }
        /// <summary>
        /// 取得比對到的Homography矩陣
        /// </summary>
        /// <returns></returns>
        public HomographyMatrix GetHomography() 
        {
            return this.homography;
        }
        /// <summary>
        /// This matrix indicates which row is valid for the matches. (取得有效的比對結果)
        /// </summary>
        /// <returns></returns>
        public Matrix<byte> GetMask() 
        {
            return this.mask;
        }
        /// <summary>
        /// 取得比對到的數量
        /// </summary>
        /// <returns></returns>
        public int GetMatchedCount() 
        {
            return this.matchedCount;
        }
        /// <summary>
        /// 取得樣板圖片(被訓練的)的資料
        /// </summary>
        /// <returns></returns>
        public SURFFeatureData GetTemplateSURFData()
        {
            return this.templateSURFData;
        }
    }
}
