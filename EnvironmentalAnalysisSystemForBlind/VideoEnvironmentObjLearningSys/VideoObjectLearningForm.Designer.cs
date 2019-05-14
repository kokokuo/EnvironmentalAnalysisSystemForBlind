namespace VideoObjectLearningApp
{
    partial class VideoObjectLearningForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.loadVideoButton = new System.Windows.Forms.Button();
            this.videoFrameBox = new System.Windows.Forms.PictureBox();
            this.videoTrackBar = new System.Windows.Forms.TrackBar();
            this.playButton = new System.Windows.Forms.Button();
            this.suspendButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.candidateExtractImgBox = new Emgu.CV.UI.ImageBox();
            this.extractFeatureButton = new System.Windows.Forms.Button();
            this.saveFeatureButton = new System.Windows.Forms.Button();
            this.HsvHistButton = new System.Windows.Forms.Button();
            this.saveHistogramButton = new System.Windows.Forms.Button();
            this.DimsChoiceGroupBox = new System.Windows.Forms.GroupBox();
            this.HSRadioButton = new System.Windows.Forms.RadioButton();
            this.SBinTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.HRadioButton = new System.Windows.Forms.RadioButton();
            this.HBinTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.videoFrameBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.candidateExtractImgBox)).BeginInit();
            this.DimsChoiceGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // loadVideoButton
            // 
            this.loadVideoButton.Location = new System.Drawing.Point(12, 25);
            this.loadVideoButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.loadVideoButton.Name = "loadVideoButton";
            this.loadVideoButton.Size = new System.Drawing.Size(115, 48);
            this.loadVideoButton.TabIndex = 0;
            this.loadVideoButton.Text = "1.載入影片";
            this.loadVideoButton.UseVisualStyleBackColor = true;
            this.loadVideoButton.Click += new System.EventHandler(this.loadVideoButton_Click);
            // 
            // videoFrameBox
            // 
            this.videoFrameBox.Location = new System.Drawing.Point(12, 92);
            this.videoFrameBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.videoFrameBox.Name = "videoFrameBox";
            this.videoFrameBox.Size = new System.Drawing.Size(640, 480);
            this.videoFrameBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.videoFrameBox.TabIndex = 1;
            this.videoFrameBox.TabStop = false;
            this.videoFrameBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.videoFrameBox_MouseDown);
            this.videoFrameBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.videoFrameBox_MouseMove);
            this.videoFrameBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.videoFrameBox_MouseUp);
            // 
            // videoTrackBar
            // 
            this.videoTrackBar.Location = new System.Drawing.Point(12, 579);
            this.videoTrackBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.videoTrackBar.Name = "videoTrackBar";
            this.videoTrackBar.Size = new System.Drawing.Size(640, 56);
            this.videoTrackBar.TabIndex = 2;
            this.videoTrackBar.ValueChanged += new System.EventHandler(this.videoTrackBar_ValueChanged);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(12, 641);
            this.playButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(115, 48);
            this.playButton.TabIndex = 3;
            this.playButton.Text = "播放";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // suspendButton
            // 
            this.suspendButton.Location = new System.Drawing.Point(141, 641);
            this.suspendButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.suspendButton.Name = "suspendButton";
            this.suspendButton.Size = new System.Drawing.Size(115, 48);
            this.suspendButton.TabIndex = 4;
            this.suspendButton.Text = "暫停";
            this.suspendButton.UseVisualStyleBackColor = true;
            this.suspendButton.Click += new System.EventHandler(this.suspendButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(269, 641);
            this.stopButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(115, 48);
            this.stopButton.TabIndex = 5;
            this.stopButton.Text = "停止";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // candidateExtractImgBox
            // 
            this.candidateExtractImgBox.Location = new System.Drawing.Point(693, 92);
            this.candidateExtractImgBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.candidateExtractImgBox.Name = "candidateExtractImgBox";
            this.candidateExtractImgBox.Size = new System.Drawing.Size(51, 50);
            this.candidateExtractImgBox.TabIndex = 2;
            this.candidateExtractImgBox.TabStop = false;
            // 
            // extractFeatureButton
            // 
            this.extractFeatureButton.Location = new System.Drawing.Point(693, 25);
            this.extractFeatureButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.extractFeatureButton.Name = "extractFeatureButton";
            this.extractFeatureButton.Size = new System.Drawing.Size(115, 48);
            this.extractFeatureButton.TabIndex = 6;
            this.extractFeatureButton.Text = "2.擷取特徵";
            this.extractFeatureButton.UseVisualStyleBackColor = true;
            this.extractFeatureButton.Click += new System.EventHandler(this.extractFeatureButton_Click);
            // 
            // saveFeatureButton
            // 
            this.saveFeatureButton.Location = new System.Drawing.Point(837, 25);
            this.saveFeatureButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.saveFeatureButton.Name = "saveFeatureButton";
            this.saveFeatureButton.Size = new System.Drawing.Size(115, 48);
            this.saveFeatureButton.TabIndex = 7;
            this.saveFeatureButton.Text = "3.保存特徵";
            this.saveFeatureButton.UseVisualStyleBackColor = true;
            this.saveFeatureButton.Click += new System.EventHandler(this.saveFeatureButton_Click);
            // 
            // HsvHistButton
            // 
            this.HsvHistButton.Location = new System.Drawing.Point(693, 415);
            this.HsvHistButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.HsvHistButton.Name = "HsvHistButton";
            this.HsvHistButton.Size = new System.Drawing.Size(115, 48);
            this.HsvHistButton.TabIndex = 8;
            this.HsvHistButton.Text = "4.計算值方圖";
            this.HsvHistButton.UseVisualStyleBackColor = true;
            this.HsvHistButton.Click += new System.EventHandler(this.HsvHistButton_Click);
            // 
            // saveHistogramButton
            // 
            this.saveHistogramButton.Location = new System.Drawing.Point(837, 415);
            this.saveHistogramButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.saveHistogramButton.Name = "saveHistogramButton";
            this.saveHistogramButton.Size = new System.Drawing.Size(115, 48);
            this.saveHistogramButton.TabIndex = 9;
            this.saveHistogramButton.Text = "5.保存值方圖";
            this.saveHistogramButton.UseVisualStyleBackColor = true;
            this.saveHistogramButton.Click += new System.EventHandler(this.saveHistogramButton_Click);
            // 
            // DimsChoiceGroupBox
            // 
            this.DimsChoiceGroupBox.Controls.Add(this.HSRadioButton);
            this.DimsChoiceGroupBox.Controls.Add(this.SBinTextBox);
            this.DimsChoiceGroupBox.Controls.Add(this.label5);
            this.DimsChoiceGroupBox.Controls.Add(this.HRadioButton);
            this.DimsChoiceGroupBox.Controls.Add(this.HBinTextBox);
            this.DimsChoiceGroupBox.Controls.Add(this.label4);
            this.DimsChoiceGroupBox.Location = new System.Drawing.Point(693, 469);
            this.DimsChoiceGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.DimsChoiceGroupBox.Name = "DimsChoiceGroupBox";
            this.DimsChoiceGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.DimsChoiceGroupBox.Size = new System.Drawing.Size(352, 120);
            this.DimsChoiceGroupBox.TabIndex = 17;
            this.DimsChoiceGroupBox.TabStop = false;
            this.DimsChoiceGroupBox.Text = "直方圖維度選擇";
            // 
            // HSRadioButton
            // 
            this.HSRadioButton.AutoSize = true;
            this.HSRadioButton.Location = new System.Drawing.Point(8, 80);
            this.HSRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.HSRadioButton.Name = "HSRadioButton";
            this.HSRadioButton.Size = new System.Drawing.Size(76, 19);
            this.HSRadioButton.TabIndex = 13;
            this.HSRadioButton.TabStop = true;
            this.HSRadioButton.Text = "HS維度";
            this.HSRadioButton.UseVisualStyleBackColor = true;
            this.HSRadioButton.CheckedChanged += new System.EventHandler(this.HSRadioButton_CheckedChanged);
            // 
            // SBinTextBox
            // 
            this.SBinTextBox.Enabled = false;
            this.SBinTextBox.Location = new System.Drawing.Point(263, 71);
            this.SBinTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.SBinTextBox.Name = "SBinTextBox";
            this.SBinTextBox.Size = new System.Drawing.Size(63, 25);
            this.SBinTextBox.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(180, 82);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 15);
            this.label5.TabIndex = 14;
            this.label5.Text = "S Bins";
            // 
            // HRadioButton
            // 
            this.HRadioButton.AutoSize = true;
            this.HRadioButton.Checked = true;
            this.HRadioButton.Location = new System.Drawing.Point(8, 35);
            this.HRadioButton.Margin = new System.Windows.Forms.Padding(4);
            this.HRadioButton.Name = "HRadioButton";
            this.HRadioButton.Size = new System.Drawing.Size(68, 19);
            this.HRadioButton.TabIndex = 12;
            this.HRadioButton.TabStop = true;
            this.HRadioButton.Text = "H維度";
            this.HRadioButton.UseVisualStyleBackColor = true;
            this.HRadioButton.CheckedChanged += new System.EventHandler(this.HRadioButton_CheckedChanged);
            // 
            // HBinTextBox
            // 
            this.HBinTextBox.Location = new System.Drawing.Point(263, 29);
            this.HBinTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.HBinTextBox.Name = "HBinTextBox";
            this.HBinTextBox.Size = new System.Drawing.Size(63, 25);
            this.HBinTextBox.TabIndex = 11;
            this.HBinTextBox.Text = "50";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(180, 40);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 15);
            this.label4.TabIndex = 10;
            this.label4.Text = "H Bins";
            // 
            // VideoObjectLearningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1188, 740);
            this.Controls.Add(this.DimsChoiceGroupBox);
            this.Controls.Add(this.saveHistogramButton);
            this.Controls.Add(this.HsvHistButton);
            this.Controls.Add(this.saveFeatureButton);
            this.Controls.Add(this.extractFeatureButton);
            this.Controls.Add(this.candidateExtractImgBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.suspendButton);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.videoTrackBar);
            this.Controls.Add(this.videoFrameBox);
            this.Controls.Add(this.loadVideoButton);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "VideoObjectLearningForm";
            this.Text = "影片環境物件學習系統";
            ((System.ComponentModel.ISupportInitialize)(this.videoFrameBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.candidateExtractImgBox)).EndInit();
            this.DimsChoiceGroupBox.ResumeLayout(false);
            this.DimsChoiceGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button loadVideoButton;
        private System.Windows.Forms.PictureBox videoFrameBox;
        private System.Windows.Forms.TrackBar videoTrackBar;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button suspendButton;
        private System.Windows.Forms.Button stopButton;
        private Emgu.CV.UI.ImageBox candidateExtractImgBox;
        private System.Windows.Forms.Button extractFeatureButton;
        private System.Windows.Forms.Button saveFeatureButton;
        private System.Windows.Forms.Button HsvHistButton;
        private System.Windows.Forms.Button saveHistogramButton;
        private System.Windows.Forms.GroupBox DimsChoiceGroupBox;
        private System.Windows.Forms.RadioButton HSRadioButton;
        private System.Windows.Forms.TextBox SBinTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton HRadioButton;
        private System.Windows.Forms.TextBox HBinTextBox;
        private System.Windows.Forms.Label label4;
    }
}

