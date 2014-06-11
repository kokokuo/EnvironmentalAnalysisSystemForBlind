namespace VideoEnvironmentObjLearningSys
{
    partial class Form1
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
            ((System.ComponentModel.ISupportInitialize)(this.videoFrameBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.candidateExtractImgBox)).BeginInit();
            this.SuspendLayout();
            // 
            // loadVideoButton
            // 
            this.loadVideoButton.Location = new System.Drawing.Point(9, 20);
            this.loadVideoButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.loadVideoButton.Name = "loadVideoButton";
            this.loadVideoButton.Size = new System.Drawing.Size(86, 38);
            this.loadVideoButton.TabIndex = 0;
            this.loadVideoButton.Text = "1.載入影片";
            this.loadVideoButton.UseVisualStyleBackColor = true;
            this.loadVideoButton.Click += new System.EventHandler(this.loadVideoButton_Click);
            // 
            // videoFrameBox
            // 
            this.videoFrameBox.Location = new System.Drawing.Point(9, 74);
            this.videoFrameBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.videoFrameBox.Name = "videoFrameBox";
            this.videoFrameBox.Size = new System.Drawing.Size(480, 384);
            this.videoFrameBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.videoFrameBox.TabIndex = 1;
            this.videoFrameBox.TabStop = false;
            this.videoFrameBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.videoFrameBox_MouseDown);
            this.videoFrameBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.videoFrameBox_MouseMove);
            this.videoFrameBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.videoFrameBox_MouseUp);
            // 
            // videoTrackBar
            // 
            this.videoTrackBar.Location = new System.Drawing.Point(9, 463);
            this.videoTrackBar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.videoTrackBar.Name = "videoTrackBar";
            this.videoTrackBar.Size = new System.Drawing.Size(480, 45);
            this.videoTrackBar.TabIndex = 2;
            this.videoTrackBar.ValueChanged += new System.EventHandler(this.videoTrackBar_ValueChanged);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(9, 513);
            this.playButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(86, 38);
            this.playButton.TabIndex = 3;
            this.playButton.Text = "播放";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // suspendButton
            // 
            this.suspendButton.Location = new System.Drawing.Point(106, 513);
            this.suspendButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.suspendButton.Name = "suspendButton";
            this.suspendButton.Size = new System.Drawing.Size(86, 38);
            this.suspendButton.TabIndex = 4;
            this.suspendButton.Text = "暫停";
            this.suspendButton.UseVisualStyleBackColor = true;
            this.suspendButton.Click += new System.EventHandler(this.suspendButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(202, 513);
            this.stopButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(86, 38);
            this.stopButton.TabIndex = 5;
            this.stopButton.Text = "停止";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // candidateExtractImgBox
            // 
            this.candidateExtractImgBox.Location = new System.Drawing.Point(520, 74);
            this.candidateExtractImgBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.candidateExtractImgBox.Name = "candidateExtractImgBox";
            this.candidateExtractImgBox.Size = new System.Drawing.Size(38, 40);
            this.candidateExtractImgBox.TabIndex = 2;
            this.candidateExtractImgBox.TabStop = false;
            // 
            // extractFeatureButton
            // 
            this.extractFeatureButton.Location = new System.Drawing.Point(520, 20);
            this.extractFeatureButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.extractFeatureButton.Name = "extractFeatureButton";
            this.extractFeatureButton.Size = new System.Drawing.Size(86, 38);
            this.extractFeatureButton.TabIndex = 6;
            this.extractFeatureButton.Text = "2.擷取特徵";
            this.extractFeatureButton.UseVisualStyleBackColor = true;
            this.extractFeatureButton.Click += new System.EventHandler(this.extractFeatureButton_Click);
            // 
            // saveFeatureButton
            // 
            this.saveFeatureButton.Location = new System.Drawing.Point(619, 20);
            this.saveFeatureButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.saveFeatureButton.Name = "saveFeatureButton";
            this.saveFeatureButton.Size = new System.Drawing.Size(86, 38);
            this.saveFeatureButton.TabIndex = 7;
            this.saveFeatureButton.Text = "3.純取特徵";
            this.saveFeatureButton.UseVisualStyleBackColor = true;
            this.saveFeatureButton.Click += new System.EventHandler(this.saveFeatureButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 592);
            this.Controls.Add(this.saveFeatureButton);
            this.Controls.Add(this.extractFeatureButton);
            this.Controls.Add(this.candidateExtractImgBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.suspendButton);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.videoTrackBar);
            this.Controls.Add(this.videoFrameBox);
            this.Controls.Add(this.loadVideoButton);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form1";
            this.Text = "影片環境物件學習系統";
            ((System.ComponentModel.ISupportInitialize)(this.videoFrameBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.candidateExtractImgBox)).EndInit();
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
    }
}

