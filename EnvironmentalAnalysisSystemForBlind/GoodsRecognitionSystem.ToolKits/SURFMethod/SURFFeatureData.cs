﻿using System;
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

//VectorOfKeyPoint
using Emgu.CV.Util;
namespace RecognitionSys.ToolKits.SURFMethod
{
    /// <summary>
    /// 紀錄SURF特徵點的類別
    /// </summary>
    
    //把SURFFeature類別變為可序列化,並採用二元序列化方式寫讀檔
    [Serializable] 
    public class SURFFeatureData
    {
        private VectorOfKeyPoint surfKeyPoints;
        private Matrix<float> surfDescriptors = null;
        private Image<Bgr, Byte> srcImage;
        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="src">圖片</param>
        /// <param name="keyPoints">特徵點</param>
        /// <param name="descriptors">特徵描述子</param>
        public SURFFeatureData(Image<Bgr, Byte> src, VectorOfKeyPoint keyPoints, Matrix<float> descriptors)
        {
            this.srcImage = src;
            surfKeyPoints = keyPoints;
            surfDescriptors = descriptors;

        }
        /// <summary>
        /// 取得圖片
        /// </summary>
        /// <returns></returns>
        public Image<Bgr, Byte> GetImg()
        {
            return srcImage;
        }
        /// <summary>
        /// 取得特徵點
        /// </summary>
        /// <returns></returns>
        public VectorOfKeyPoint GetKeyPoints()
        {
            return this.surfKeyPoints;
        }
        /// <summary>
        /// 取得描述子
        /// </summary>
        /// <returns></returns>
        public Matrix<float> GetDescriptors()
        {
            return this.surfDescriptors;
        }
    }
}
