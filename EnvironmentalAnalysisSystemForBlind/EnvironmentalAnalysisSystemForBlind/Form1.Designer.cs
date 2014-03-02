namespace EnvironmentalAnalysisSystemForBlind
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.trainingVideoTrackBar = new System.Windows.Forms.TrackBar();
            this.playVideoButton = new System.Windows.Forms.Button();
            this.suspendButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.imageBox2 = new Emgu.CV.UI.ImageBox();
            this.extractFeatureButton = new System.Windows.Forms.Button();
            this.trainingLoadVideoButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingVideoTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 50);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1291, 695);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.trainingLoadVideoButton);
            this.tabPage1.Controls.Add(this.extractFeatureButton);
            this.tabPage1.Controls.Add(this.imageBox2);
            this.tabPage1.Controls.Add(this.stopButton);
            this.tabPage1.Controls.Add(this.suspendButton);
            this.tabPage1.Controls.Add(this.playVideoButton);
            this.tabPage1.Controls.Add(this.trainingVideoTrackBar);
            this.tabPage1.Controls.Add(this.imageBox1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1283, 666);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 71);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // imageBox1
            // 
            this.imageBox1.Location = new System.Drawing.Point(6, 58);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(640, 480);
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            // 
            // trainingVideoTrackBar
            // 
            this.trainingVideoTrackBar.Location = new System.Drawing.Point(3, 544);
            this.trainingVideoTrackBar.Name = "trainingVideoTrackBar";
            this.trainingVideoTrackBar.Size = new System.Drawing.Size(640, 56);
            this.trainingVideoTrackBar.TabIndex = 3;
            // 
            // playVideoButton
            // 
            this.playVideoButton.Location = new System.Drawing.Point(6, 606);
            this.playVideoButton.Name = "playVideoButton";
            this.playVideoButton.Size = new System.Drawing.Size(108, 45);
            this.playVideoButton.TabIndex = 4;
            this.playVideoButton.Text = "Play";
            this.playVideoButton.UseVisualStyleBackColor = true;
            // 
            // suspendButton
            // 
            this.suspendButton.Location = new System.Drawing.Point(121, 606);
            this.suspendButton.Name = "suspendButton";
            this.suspendButton.Size = new System.Drawing.Size(94, 45);
            this.suspendButton.TabIndex = 5;
            this.suspendButton.Text = "Suspend";
            this.suspendButton.UseVisualStyleBackColor = true;
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(221, 606);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(112, 45);
            this.stopButton.TabIndex = 6;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            // 
            // imageBox2
            // 
            this.imageBox2.Location = new System.Drawing.Point(671, 67);
            this.imageBox2.Name = "imageBox2";
            this.imageBox2.Size = new System.Drawing.Size(64, 64);
            this.imageBox2.TabIndex = 2;
            this.imageBox2.TabStop = false;
            // 
            // extractFeatureButton
            // 
            this.extractFeatureButton.Location = new System.Drawing.Point(671, 11);
            this.extractFeatureButton.Name = "extractFeatureButton";
            this.extractFeatureButton.Size = new System.Drawing.Size(108, 41);
            this.extractFeatureButton.TabIndex = 7;
            this.extractFeatureButton.Text = "ExtractFeature";
            this.extractFeatureButton.UseVisualStyleBackColor = true;
            // 
            // trainingLoadVideoButton
            // 
            this.trainingLoadVideoButton.Location = new System.Drawing.Point(7, 11);
            this.trainingLoadVideoButton.Name = "trainingLoadVideoButton";
            this.trainingLoadVideoButton.Size = new System.Drawing.Size(107, 41);
            this.trainingLoadVideoButton.TabIndex = 8;
            this.trainingLoadVideoButton.Text = "Load Video";
            this.trainingLoadVideoButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 757);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingVideoTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button suspendButton;
        private System.Windows.Forms.Button playVideoButton;
        private System.Windows.Forms.TrackBar trainingVideoTrackBar;
        private Emgu.CV.UI.ImageBox imageBox1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button trainingLoadVideoButton;
        private System.Windows.Forms.Button extractFeatureButton;
        private Emgu.CV.UI.ImageBox imageBox2;
    }
}

