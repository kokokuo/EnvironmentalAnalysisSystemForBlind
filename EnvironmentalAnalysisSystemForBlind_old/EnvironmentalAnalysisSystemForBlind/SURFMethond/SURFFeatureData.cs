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
namespace SURFMethond
{
    [Serializable] //把SURFFeature類別變為可序列化,並採用二元序列化方式寫讀檔

    public class SURFFeatureData
    {
        private VectorOfKeyPoint surfKeyPoints;
        private Matrix<float> surfDescriptors = null;
        private Image<Bgr, Byte> srcImage;
        public SURFFeatureData(Image<Bgr, Byte> src,VectorOfKeyPoint keyPoints,Matrix<float> descriptors)
        {
            this.srcImage = src;
            surfKeyPoints = keyPoints;
            surfDescriptors = descriptors;
            
        }
        public Image<Bgr, Byte> GetImg() 
        {
            return srcImage;
        }
        public VectorOfKeyPoint GetKeyPoints() 
        {
            return this.surfKeyPoints;
        }
        public Matrix<float> GetDescriptors() 
        {
            return this.surfDescriptors;
        }
    }
}
