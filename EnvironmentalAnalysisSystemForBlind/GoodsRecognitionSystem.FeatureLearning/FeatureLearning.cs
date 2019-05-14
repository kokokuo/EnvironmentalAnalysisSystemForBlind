using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Drawing;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using RecognitionSys.ToolKits;
using RecognitionSys.ToolKits.SURFMethod;
using Emgu.CV.Features2D;
namespace RecognitionSys.FeatureLearning
{
    /// <summary>
    /// 學習用的類別
    /// </summary>
    public class FeatureLearning
    {
        Image<Bgr, Byte> templateImg;
        
        /// <summary>
        /// 初始化特徵學習系統,傳入圖片檔案路徑
        /// </summary>
        /// <param name="fileName"></param>
        public FeatureLearning(string fileName)
        {
            this.templateImg = new Image<Bgr, byte>(fileName);
        }
        /// <summary>
        /// 初始化特徵學習系統,傳入圖片
        /// </summary>
        /// <param name="wantExtractFeatureImg"></param>
        public FeatureLearning(Image<Bgr, byte> wantExtractFeatureImg)
        {
            this.templateImg = wantExtractFeatureImg.Copy();
        }
        /// <summary>
        /// 設定要學習的影像
        /// </summary>
        /// <param name="img">全彩圖像</param>
        public void SetLearningImage(Image<Bgr, Byte> img)
        {
            templateImg = img.Copy();
        }
        /// <summary>
        /// 設定要學習的影像
        /// </summary>
        /// <param name="fileName">'檔案的路徑'名稱</param>
        public void SetLearningImage(string fileName)
        {
            templateImg = new Image<Bgr, byte>(fileName);
        }
        /// <summary>
        /// 取得學習的圖像
        /// </summary>
        /// <returns></returns>
        public Image<Bgr, Byte> GetLearningImage()
        {
            return templateImg;
        }
        /// <summary>
        /// 計算特徵點(記得先把影像輸入到此類別中)
        /// </summary>
        /// <returns>回傳特徵資料</returns>
        public SURFFeatureData CalSURFFeature()
        {
            return SURFMatch.CalSURFFeature(templateImg);
        }
        /// <summary>
        /// 計算值方圖
        /// </summary>
        /// <param name="dim">選取維度計算,1 = 只取hue,2 = H-S,預設為1</param>
        /// <param name="HBins">H色調預設50</param>
        /// <param name="SBins">S飽和度預設0</param>
        /// /// <param name="VBins">V明暗度預設0</param>
        public DenseHistogram CalHist(int dim = 1, int HBins = 50, int SBins = 0, int VBins = 0)
        {
            return HistogramOperation.CalHsvHistogram(templateImg, dim, HBins, SBins, VBins);
        }
        /// <summary>
        /// 儲存值方圖資料
        /// </summary>
        /// <param name="fileName">檔案路徑名稱</param>
        /// <param name="hsvHist">值方圖資料</param>
        /// <returns>回傳是否儲存成功</returns>
        public bool SaveHistogram(string fileName, DenseHistogram hsvHist)
        {
            if (hsvHist != null)
            {
                string format = Path.GetExtension(fileName); //取得副檔名
                if (format == ".xml") FeatureDataFilesOperation.WriteHistogramDataToBinaryXml(hsvHist, fileName);
                //Console Output觀看數值
                Console.WriteLine("Save Histogram Data in " + format + "........\n ");
                if (hsvHist.Dimension == 1) SystemToolBox.Show1DHistogramDataOnConsole(hsvHist);
                else if (hsvHist.Dimension == 2) SystemToolBox.Show2DHistogramDataOnConsole(hsvHist);
                Console.WriteLine("\n");
                return true;
            }
            return false;
        }
        /// <summary>
        /// 儲存特徵點
        /// </summary>
        /// <param name="fileName">檔案路徑名稱</param>
        /// <param name="surf">特徵資料</param>
        /// <returns>回傳是否儲存成功</returns>
        public bool SaveSURFFeatureData(string fileName,SURFFeatureData surf)
        {
            if (surf.GetDescriptors() != null)
            {
                string format = Path.GetExtension(fileName);
                if (format == ".xml")
                {
                    FeatureDataFilesOperation.WriteSURFFeatureDataToBinaryXml(surf, fileName);
                    //Console Output觀看數值
                    Console.WriteLine("Save SURF Feature Data........\n");
                    Console.WriteLine("\n");
                    return true;
                }
                else
                    Console.WriteLine("Only support xml file........\n");
                    Console.WriteLine("\n");
                    return false;
                
            }
            return false;
        }
    }
}
