using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace ZebraCrossingDetection
{
    public class ZebraCrossingDetection
    {
        //原始影像
        Image<Bgr, byte> oriImg; 
        //灰階影像
        Image<Gray, byte> grayImg;


        public ZebraCrossingDetection() {
           
        }
        public Image<Bgr, byte> LoadImg(string filename)
        {
            if (filename != null)
            {
                oriImg = new Image<Bgr, byte>(filename);
                oriImg = oriImg.Resize(480, 360, INTER.CV_INTER_LINEAR);
                return oriImg;
            }
            else {
                return null;
            }
        }

        public Image<Gray, byte> ToGray(Image<Bgr, byte> source) {
            if (oriImg != null)
            {
                grayImg = oriImg.Convert<Gray, byte>();
                return grayImg;
            }
            else
            {
                return null;
            }
        }

        private Image<Bgr, byte> ToCrop(Image<Bgr, byte> source)
        {
            if (oriImg != null)
            {
                oriImg = oriImg.Copy();
                oriImg.ROI = new Rectangle(new Point(0, oriImg.Height / 2), new Size(oriImg.Width, oriImg.Height / 2));
                return
            }
            else
            {
                return null;
            }
        }
    }
}
