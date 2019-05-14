using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RecognitionSys.FeatureLearning;
using RecognitionSys.ToolKits;
using RecognitionSys.ToolKits.SURFMethod;
using Emgu.Util;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
//File directory operation
using System.IO;
using Emgu.CV.UI;

namespace GoodsFeatureLearningApp
{
    public partial class GoodsFeatureLearningForm : Form
    {
        DirectoryInfo dir;
        FeatureLearning learningSys;
        Image<Bgr, byte> loadImg;
        SURFFeatureData surfData;

        public GoodsFeatureLearningForm()
        {
            InitializeComponent();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
        }

        private void loadImgButton_Click(object sender, EventArgs e)
        {
            string fileName = OpenLearningImgFile();
            if (fileName != null)
            {
                loadImg = new Image<Bgr, byte>(fileName);
                if (learningSys != null)
                    learningSys.SetLearningImage(fileName);
                else
                    learningSys = new FeatureLearning(fileName);
                loadImgBox.Image = loadImg.Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

            }
        }

        private void extractFeatureButton_Click(object sender, EventArgs e)
        {
            if (learningSys != null)
            {
                surfData = learningSys.CalSURFFeature();
                Image<Bgr, byte> drawKeyPointImg = SystemToolBox.DrawSURFFeature(surfData);
                new ImageViewer(SystemToolBox.DrawSURFFeatureToWPF(surfData, surfData.GetImg())).Show();
                extractFeatureImgBox.Image = drawKeyPointImg.Resize(320, 240, INTER.CV_INTER_LINEAR);
            }
        }

        private void saveFeatureButton_Click(object sender, EventArgs e)
        {
            SaveSURFFeatureFile(surfData);
        }

        #region 開檔存檔
        private string OpenLearningImgFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.Parent.FullName + @"\GoodsModelImages";
            dlg.Title = "Open Image File";
            // Set filter for file extension and default file extension
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method ->ShowDialog()
            // Get the selected file name and display in a TextBox
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlg.FileName != null)
            {
                // Open document
                string filename = dlg.FileName;
                return filename;
            }
            else
            {
                return null;
            }
        }

        private void SaveSURFFeatureFile(SURFFeatureData surf)
        {
            string saveSURFDataPath = dir.Parent.Parent.Parent.FullName + @"\GoodsSURFFeatureData";
            if (File.Exists(saveSURFDataPath))
                MessageBox.Show("路徑錯誤");
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.Title = "Save Descriptor to File";
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = saveSURFDataPath;
            // If the file name is not an empty string open it for saving.
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK == true && dlg.FileName != "" && learningSys != null)
            {
                bool isOk = learningSys.SaveSURFFeatureData(dlg.FileName, surf);
                if (isOk) MessageBox.Show("Save SURF Feature Data Ok");
                else MessageBox.Show("Save SURF Feature Data Faild");
            }
        }
      
        #endregion
    }
}
