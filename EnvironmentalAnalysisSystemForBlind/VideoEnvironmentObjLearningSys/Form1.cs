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
using FeatureRecognitionSystem.FeatureLearning;
using FeatureRecognitionSystem.ToolKits.SURFMethod;
namespace VideoEnvironmentObjLearningSys
{
    public partial class Form1 : Form
    {
        //取得專案執行黨所在的目錄=>System.Windows.Forms.Application.StartupPath
        //使用DirectoryInfo移動至上層
        DirectoryInfo dir;
        Capture videoCapture;
        Timer trainingVideoTimer;
        Timer matchingVideoTimer;
        int FPS = 30;
        int trainingVideoTotalFrame, matchingVideoTotalFrame;
        bool isPlay, isSuspend, isStop, isScroll;
        Image<Bgr, byte> queryFrame;

        int trainingScrollValue;
        Graphics g; //draw rectangle
        Point pressedToDrawPoint;
        bool isPressed;
        Rectangle extractFeatureMaskROI;
        Image<Bgr, byte> extractFeatureImage;
        Image<Bgr, byte> senceImage;
        SURFFeatureData trainingExtractSurfData;
        SURFFeatureData matchingModelSurfData;
        ImageViewer matchViewer, pedestrianViewer;
        int oneSecFrameIndex;

        public Form1()
        {
            InitializeComponent();
        }

        private void loadVideoButton_Click(object sender, EventArgs e)
        {

        }

        private void playButton_Click(object sender, EventArgs e)
        {

        }

        private void suspendButton_Click(object sender, EventArgs e)
        {

        }

        private void stopButton_Click(object sender, EventArgs e)
        {

        }

        private void extractFeatureButton_Click(object sender, EventArgs e)
        {

        }

        private void saveFeatureButton_Click(object sender, EventArgs e)
        {

        }

        private void videoTrackBar_ValueChanged(object sender, EventArgs e)
        {

        }

        private string OpenVideo()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //移動上層在指定下層路徑
            dlg.RestoreDirectory = true;
            dlg.InitialDirectory = dir.Parent.Parent.FullName + @"\TrainingVideo";
            dlg.Title = "Open Video File";

            // Set filter for file extension and default file extension
            dlg.Filter = "AVI Video(*.avi)|*.avi|MP4(*.mp4)|*.mp4|All Files(*.*)|*.*";

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
    }
}
