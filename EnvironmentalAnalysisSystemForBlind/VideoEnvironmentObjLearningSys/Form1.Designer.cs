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
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.extractFeatureButton = new System.Windows.Forms.Button();
            this.saveFeatureButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.videoFrameBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // loadVideoButton
            // 
            this.loadVideoButton.Location = new System.Drawing.Point(12, 25);
            this.loadVideoButton.Name = "loadVideoButton";
            this.loadVideoButton.Size = new System.Drawing.Size(114, 48);
            this.loadVideoButton.TabIndex = 0;
            this.loadVideoButton.Text = "1.載入影片";
            this.loadVideoButton.UseVisualStyleBackColor = true;
            this.loadVideoButton.Click += new System.EventHandler(this.loadVideoButton_Click);
            // 
            // videoFrameBox
            // 
            this.videoFrameBox.Location = new System.Drawing.Point(12, 93);
            this.videoFrameBox.Name = "videoFrameBox";
            this.videoFrameBox.Size = new System.Drawing.Size(640, 480);
            this.videoFrameBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.videoFrameBox.TabIndex = 1;
            this.videoFrameBox.TabStop = false;
            // 
            // videoTrackBar
            // 
            this.videoTrackBar.Location = new System.Drawing.Point(12, 579);
            this.videoTrackBar.Name = "videoTrackBar";
            this.videoTrackBar.Size = new System.Drawing.Size(640, 56);
            this.videoTrackBar.TabIndex = 2;
            this.videoTrackBar.ValueChanged += new System.EventHandler(this.videoTrackBar_ValueChanged);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(12, 641);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(114, 48);
            this.playButton.TabIndex = 3;
            this.playButton.Text = "播放";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // suspendButton
            // 
            this.suspendButton.Location = new System.Drawing.Point(141, 641);
            this.suspendButton.Name = "suspendButton";
            this.suspendButton.Size = new System.Drawing.Size(114, 48);
            this.suspendButton.TabIndex = 4;
            this.suspendButton.Text = "暫停";
            this.suspendButton.UseVisualStyleBackColor = true;
            this.suspendButton.Click += new System.EventHandler(this.suspendButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(270, 641);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(114, 48);
            this.stopButton.TabIndex = 5;
            this.stopButton.Text = "停止";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // imageBox1
            // 
            this.imageBox1.Location = new System.Drawing.Point(693, 93);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(50, 50);
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            // 
            // extractFeatureButton
            // 
            this.extractFeatureButton.Location = new System.Drawing.Point(693, 25);
            this.extractFeatureButton.Name = "extractFeatureButton";
            this.extractFeatureButton.Size = new System.Drawing.Size(114, 48);
            this.extractFeatureButton.TabIndex = 6;
            this.extractFeatureButton.Text = "2.擷取特徵";
            this.extractFeatureButton.UseVisualStyleBackColor = true;
            this.extractFeatureButton.Click += new System.EventHandler(this.extractFeatureButton_Click);
            // 
            // saveFeatureButton
            // 
            this.saveFeatureButton.Location = new System.Drawing.Point(825, 25);
            this.saveFeatureButton.Name = "saveFeatureButton";
            this.saveFeatureButton.Size = new System.Drawing.Size(114, 48);
            this.saveFeatureButton.TabIndex = 7;
            this.saveFeatureButton.Text = "3.純取特徵";
            this.saveFeatureButton.UseVisualStyleBackColor = true;
            this.saveFeatureButton.Click += new System.EventHandler(this.saveFeatureButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1188, 740);
            this.Controls.Add(this.saveFeatureButton);
            this.Controls.Add(this.extractFeatureButton);
            this.Controls.Add(this.imageBox1);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.suspendButton);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.videoTrackBar);
            this.Controls.Add(this.videoFrameBox);
            this.Controls.Add(this.loadVideoButton);
            this.Name = "Form1";
            this.Text = "影片環境物件學習系統";
            ((System.ComponentModel.ISupportInitialize)(this.videoFrameBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.videoTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
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
        private Emgu.CV.UI.ImageBox imageBox1;
        private System.Windows.Forms.Button extractFeatureButton;
        private System.Windows.Forms.Button saveFeatureButton;
    }
}

