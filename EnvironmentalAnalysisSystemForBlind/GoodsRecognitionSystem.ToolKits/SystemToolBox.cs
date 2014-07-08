﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
//Marshal類別
using System.Runtime.InteropServices;
using System.IO;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using RecognitionSys.ToolKits.SURFMethod;
using System.Windows.Forms;
namespace RecognitionSys.ToolKits
{
    /// <summary>
    /// 此專案額外工具
    /// </summary>
    public class SystemToolBox
    {
        /// <summary>
        /// 等化影像
        /// </summary>
        /// <param name="srcImg">要等化的影像</param>
        /// <returns>回傳等化過的影像</returns>
        public static Image<Bgr, Byte> EqualizationImage(Image<Bgr, Byte> srcImg)
        {
            IntPtr dstImg = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(srcImg), IPL_DEPTH.IPL_DEPTH_8U, 3);
            IntPtr redImage = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(srcImg), IPL_DEPTH.IPL_DEPTH_8U, 1);
            IntPtr greenImage = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(srcImg), IPL_DEPTH.IPL_DEPTH_8U, 1);
            IntPtr blueImage = CvInvoke.cvCreateImage(CvInvoke.cvGetSize(srcImg), IPL_DEPTH.IPL_DEPTH_8U, 1);
            CvInvoke.cvSplit(srcImg, blueImage, greenImage, redImage, IntPtr.Zero);
            CvInvoke.cvEqualizeHist(blueImage, blueImage);
            CvInvoke.cvEqualizeHist(greenImage, greenImage);
            CvInvoke.cvEqualizeHist(redImage, redImage);
            CvInvoke.cvMerge(blueImage, greenImage, redImage, IntPtr.Zero, dstImg);
            return EmguFormatConvetor.IplImagePointerToEmgucvImage<Bgr, Byte>(dstImg);
        }

        /// <summary>
        /// Consloe模式顯示1維(色相)值方圖資訊
        /// </summary>
        /// <param name="histDense">值方圖類別</param>
        public static void Show1DHistogramDataOnConsole(DenseHistogram histDense)
        {
            Console.WriteLine("Dimension is :" + histDense.Dimension);
            int hBins = histDense.BinDimension[0].Size;
            for (int h = 0; h < hBins; h++)
            {
                Console.Write(CvInvoke.cvQueryHistValue_1D(histDense, h).ToString() + " ,");
            }
            Console.WriteLine();
        }
        /// <summary>
        /// Consloe模式顯示2維(H-S)值方圖資訊
        /// </summary>
        /// <param name="histDense">值方圖類別</param>
        public static void Show2DHistogramDataOnConsole(DenseHistogram histDense)
        {
            Console.WriteLine("Dimension is :" + histDense.Dimension);
            int hBins = histDense.BinDimension[0].Size;
            int sBins = histDense.BinDimension[1].Size;
            for (int h = 0; h < hBins; h++)
            {
                for (int s = 0; s < sBins; s++)
                {
                    if (s == sBins - 1)
                    {
                        Console.WriteLine(CvInvoke.cvQueryHistValue_2D(histDense, h, s).ToString());
                    }
                    else
                    {
                        Console.Write(CvInvoke.cvQueryHistValue_2D(histDense, h, s).ToString() + " ,");
                    }
                }
            }
        }

      
        /// <summary>
        /// 畫出直方圖到圖上,會依據你選擇的計算維度畫出相對應維度的直方圖(只能提供一維與二維)
        /// </summary>
        /// <param name="hsvHist">值方圖資料</param>
        /// <returns>回傳畫上值方圖資料的影像</returns>
        public static Image<Bgr, Byte> DrawHsvHistogram(DenseHistogram hsvHist)
        {
            if (hsvHist.Dimension == 1)
                return HistogramOperation.Generate1DHistogramImgForDraw(hsvHist);
            else if(hsvHist.Dimension == 2)
                return HistogramOperation.Generate2DHistogramImgForDraw(hsvHist);
            else
                return null;
        }

        /// <summary>
        /// 畫上特徵點到圖上
        /// </summary>
        /// <param name="surf">SURF特徵類別</param>
        /// <returns>回傳畫好特徵點的影像</returns>
        public static Image<Bgr, byte> DrawSURFFeature(SURFFeatureData surf)
        {
            VectorOfKeyPoint keyPoints = surf.GetKeyPoints();
            //繪製特徵
            Image<Bgr, byte> result = Features2DToolbox.DrawKeypoints(surf.GetImg(), surf.GetKeyPoints(), new Bgr(255, 255, 255), Features2DToolbox.KeypointDrawType.DEFAULT);
            return result;
        }

        /// <summary>
        /// 畫上特徵點到圖上
        /// </summary>
        /// <param name="surf">SURF特徵類別</param>
        /// <param name="drawImg">要畫到的影像上</param>
        /// <returns>回傳畫好特徵點的影像</returns>
        public static Image<Bgr, byte> DrawSURFFeatureToWPF(SURFFeatureData surf, Image<Bgr, Byte> drawImg)
        {
            VectorOfKeyPoint keyPoints = surf.GetKeyPoints();
            Bitmap imgForDraw = drawImg.Copy().ToBitmap();
            //使用Graphics繪製
            using (Graphics g = Graphics.FromImage(imgForDraw))
            {
                for (int i = 0; i < keyPoints.Size; i++)
                {

                    g.DrawEllipse(new Pen(new SolidBrush(Color.White), 2), (int)keyPoints[i].Point.X, (int)keyPoints[i].Point.Y, 15, 15);
                }
                g.Dispose();
            }
            return new Image<Bgr, byte>(imgForDraw);
        }


        public static string GetMappingDescriptorDataFile(string histFileName, DirectoryInfo dir)
        {
            string path = dir.Parent.Parent.Parent.FullName + @"\SignBoardSURFFeatureData\";
            string filename;
            if (Directory.Exists(path) && File.Exists(path + histFileName))
            {
                filename = (path + histFileName);
                return filename;
            }
            else
            {
                MessageBox.Show("沒有對應的特徵檔案!");
                return null;
            }
        }
    }
}
