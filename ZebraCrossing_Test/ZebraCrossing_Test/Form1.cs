using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
//EmguCV
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.CvEnum;

namespace ZebraCrossing_Test
{
    public partial class Form1 : Form
    {
        DirectoryInfo dir;
        Image<Bgr, byte> oriImg;
        Image<Bgr, byte> bottomSideImg;
        Image<Gray, byte> grayImg;
        Image<Gray, byte> maskWhiteImg;
        ImageViewer houghLineViewer;
        public Form1()
        {
            InitializeComponent();
            dir = new DirectoryInfo(System.Windows.Forms.Application.StartupPath);
            houghLineViewer = new ImageViewer();
            houghLineViewer.FormClosing += houghLineViewer_FormClosing;
        }

        void houghLineViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //關閉視窗時取消
            houghLineViewer.Hide(); //隱藏式窗,下次再show出
        }

       

        #region 開檔
        private string OpenLearningImgFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.FullName + @"\Crossing";
            dlg.Title = "Open Image File";

            // Set filter for file extension and default file extension
            dlg.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|All Files(*.*)|*.*";

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
        #endregion

        private void loadImgButton_Click(object sender, EventArgs e)
        {
            string filename = OpenLearningImgFile();
            if (filename !=null)
            {
                oriImg = new Image<Bgr, byte>(filename);
                oriImg = oriImg.Resize(640, 480, INTER.CV_INTER_LINEAR);
                oriImageBox.Image = oriImg;
            }
        }

        private void maskImgButton_Click(object sender, EventArgs e)
        {
            //oriImg 與 grayImg 測試
            if (grayImg != null)
            {
                try
                {
                    maskWhiteImg = new Image<Gray, byte>(new Size(grayImg.Width, grayImg.Height));
                    CvInvoke.cvInRangeS(grayImg, new MCvScalar(200, 200, 200), new MCvScalar(255, 255, 255), maskWhiteImg);
                    maskImageBox.Image = maskWhiteImg;
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
               
            }
            else
            {
                MessageBox.Show("尚未灰階");
            }
        }


        private void toGrayButton_Click(object sender, EventArgs e)
        {
            if (bottomSideImg != null)
            {
                grayImg = bottomSideImg.Convert<Gray, byte>();
                grayImgBox.Image = grayImg;
            }
            else {
                MessageBox.Show("尚未剪裁");
            }
        }

        private void cropBottomButton_Click(object sender, EventArgs e)
        {
            if (oriImg != null)
            {
                bottomSideImg = oriImg.Copy();
                bottomSideImg.ROI = new Rectangle(new Point(0, 240), new Size(640, 320));
                oriImageBox.Image = bottomSideImg;
            }
            else {
                MessageBox.Show("尚未載入圖片");
            }
        }


        private void houghLineButton_Click(object sender, EventArgs e)
        {
            if (maskWhiteImg != null)
            {

                //Hough transform for line detection
                LineSegment2D[][] lines = maskWhiteImg.HoughLines(
                    new Gray(125),  //Canny algorithm low threshold
                    new Gray(260),  //Canny algorithm high threshold
                    1,              //rho parameter
                    Math.PI / 180.0,  //theta parameter 
                    100,            //threshold
                    1,             //min length for a line
                    30);            //max allowed gap along the line
                //draw lines on image
                foreach (var line in lines[0])
                {

                    bottomSideImg.Draw(line, new Bgr(0, 0, 255), 1);
                }

                houghLineViewer.Image = bottomSideImg;
                houghLineViewer.Text = "HoughLine 偵測畫面";
                houghLineViewer.Show();
            }
        }
    }
}
