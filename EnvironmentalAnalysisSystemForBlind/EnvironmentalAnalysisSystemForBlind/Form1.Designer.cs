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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.playMatchingVideoButton = new System.Windows.Forms.Button();
            this.stopMatchingVideoButton = new System.Windows.Forms.Button();
            this.loadMatchingVideoButton = new System.Windows.Forms.Button();
            this.mathcingVideoPictureBox = new System.Windows.Forms.PictureBox();
            this.matchingSuspendButton = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.trainingVideoTrackBar = new System.Windows.Forms.TrackBar();
            this.playVideoButton = new System.Windows.Forms.Button();
            this.suspendButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.wantExtractFeatureImageBox = new Emgu.CV.UI.ImageBox();
            this.extractFeatureButton = new System.Windows.Forms.Button();
            this.LoadtrainingVideoButton = new System.Windows.Forms.Button();
            this.trainingVideoPictureBox = new System.Windows.Forms.PictureBox();
            this.saveFeatureDataButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.surfOpenCheckBox = new System.Windows.Forms.CheckBox();
            this.pedestrianCheckBox = new System.Windows.Forms.CheckBox();
            this.runAnalysisButton = new System.Windows.Forms.Button();
            this.stopAnalysisButton = new System.Windows.Forms.Button();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mathcingVideoPictureBox)).BeginInit();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trainingVideoTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wantExtractFeatureImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingVideoPictureBox)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.stopAnalysisButton);
            this.tabPage2.Controls.Add(this.runAnalysisButton);
            this.tabPage2.Controls.Add(this.pedestrianCheckBox);
            this.tabPage2.Controls.Add(this.surfOpenCheckBox);
            this.tabPage2.Controls.Add(this.matchingSuspendButton);
            this.tabPage2.Controls.Add(this.mathcingVideoPictureBox);
            this.tabPage2.Controls.Add(this.loadMatchingVideoButton);
            this.tabPage2.Controls.Add(this.stopMatchingVideoButton);
            this.tabPage2.Controls.Add(this.playMatchingVideoButton);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1283, 666);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // playMatchingVideoButton
            // 
            this.playMatchingVideoButton.Location = new System.Drawing.Point(17, 557);
            this.playMatchingVideoButton.Name = "playMatchingVideoButton";
            this.playMatchingVideoButton.Size = new System.Drawing.Size(108, 45);
            this.playMatchingVideoButton.TabIndex = 10;
            this.playMatchingVideoButton.Text = "Play and Match";
            this.playMatchingVideoButton.UseVisualStyleBackColor = true;
            this.playMatchingVideoButton.Click += new System.EventHandler(this.playMatchingVideoButton_Click);
            // 
            // stopMatchingVideoButton
            // 
            this.stopMatchingVideoButton.Location = new System.Drawing.Point(231, 557);
            this.stopMatchingVideoButton.Name = "stopMatchingVideoButton";
            this.stopMatchingVideoButton.Size = new System.Drawing.Size(112, 45);
            this.stopMatchingVideoButton.TabIndex = 11;
            this.stopMatchingVideoButton.Text = "Stop";
            this.stopMatchingVideoButton.UseVisualStyleBackColor = true;
            this.stopMatchingVideoButton.Click += new System.EventHandler(this.stopMatchingVideoButton_Click);
            // 
            // loadMatchingVideoButton
            // 
            this.loadMatchingVideoButton.Location = new System.Drawing.Point(18, 12);
            this.loadMatchingVideoButton.Name = "loadMatchingVideoButton";
            this.loadMatchingVideoButton.Size = new System.Drawing.Size(107, 41);
            this.loadMatchingVideoButton.TabIndex = 12;
            this.loadMatchingVideoButton.Text = "Load Video";
            this.loadMatchingVideoButton.UseVisualStyleBackColor = true;
            this.loadMatchingVideoButton.Click += new System.EventHandler(this.loadMatchingVideoButton_Click);
            // 
            // mathcingVideoPictureBox
            // 
            this.mathcingVideoPictureBox.Location = new System.Drawing.Point(18, 59);
            this.mathcingVideoPictureBox.Name = "mathcingVideoPictureBox";
            this.mathcingVideoPictureBox.Size = new System.Drawing.Size(640, 480);
            this.mathcingVideoPictureBox.TabIndex = 13;
            this.mathcingVideoPictureBox.TabStop = false;
            // 
            // matchingSuspendButton
            // 
            this.matchingSuspendButton.Location = new System.Drawing.Point(131, 557);
            this.matchingSuspendButton.Name = "matchingSuspendButton";
            this.matchingSuspendButton.Size = new System.Drawing.Size(94, 45);
            this.matchingSuspendButton.TabIndex = 14;
            this.matchingSuspendButton.Text = "Suspend";
            this.matchingSuspendButton.UseVisualStyleBackColor = true;
            this.matchingSuspendButton.Click += new System.EventHandler(this.matchingSuspendButton_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.saveFeatureDataButton);
            this.tabPage1.Controls.Add(this.trainingVideoPictureBox);
            this.tabPage1.Controls.Add(this.LoadtrainingVideoButton);
            this.tabPage1.Controls.Add(this.extractFeatureButton);
            this.tabPage1.Controls.Add(this.wantExtractFeatureImageBox);
            this.tabPage1.Controls.Add(this.stopButton);
            this.tabPage1.Controls.Add(this.suspendButton);
            this.tabPage1.Controls.Add(this.playVideoButton);
            this.tabPage1.Controls.Add(this.trainingVideoTrackBar);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1283, 666);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // trainingVideoTrackBar
            // 
            this.trainingVideoTrackBar.Location = new System.Drawing.Point(3, 544);
            this.trainingVideoTrackBar.Name = "trainingVideoTrackBar";
            this.trainingVideoTrackBar.Size = new System.Drawing.Size(640, 56);
            this.trainingVideoTrackBar.TabIndex = 3;
            this.trainingVideoTrackBar.ValueChanged += new System.EventHandler(this.trainingVideoTrackBar_ValueChanged);
            // 
            // playVideoButton
            // 
            this.playVideoButton.Location = new System.Drawing.Point(6, 606);
            this.playVideoButton.Name = "playVideoButton";
            this.playVideoButton.Size = new System.Drawing.Size(108, 45);
            this.playVideoButton.TabIndex = 4;
            this.playVideoButton.Text = "Play";
            this.playVideoButton.UseVisualStyleBackColor = true;
            this.playVideoButton.Click += new System.EventHandler(this.playVideoButton_Click);
            // 
            // suspendButton
            // 
            this.suspendButton.Location = new System.Drawing.Point(121, 606);
            this.suspendButton.Name = "suspendButton";
            this.suspendButton.Size = new System.Drawing.Size(94, 45);
            this.suspendButton.TabIndex = 5;
            this.suspendButton.Text = "Suspend";
            this.suspendButton.UseVisualStyleBackColor = true;
            this.suspendButton.Click += new System.EventHandler(this.suspendButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(221, 606);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(112, 45);
            this.stopButton.TabIndex = 6;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // wantExtractFeatureImageBox
            // 
            this.wantExtractFeatureImageBox.Location = new System.Drawing.Point(671, 67);
            this.wantExtractFeatureImageBox.Name = "wantExtractFeatureImageBox";
            this.wantExtractFeatureImageBox.Size = new System.Drawing.Size(64, 64);
            this.wantExtractFeatureImageBox.TabIndex = 2;
            this.wantExtractFeatureImageBox.TabStop = false;
            // 
            // extractFeatureButton
            // 
            this.extractFeatureButton.Location = new System.Drawing.Point(671, 11);
            this.extractFeatureButton.Name = "extractFeatureButton";
            this.extractFeatureButton.Size = new System.Drawing.Size(108, 41);
            this.extractFeatureButton.TabIndex = 7;
            this.extractFeatureButton.Text = "ExtractFeature";
            this.extractFeatureButton.UseVisualStyleBackColor = true;
            this.extractFeatureButton.Click += new System.EventHandler(this.extractFeatureButton_Click);
            // 
            // LoadtrainingVideoButton
            // 
            this.LoadtrainingVideoButton.Location = new System.Drawing.Point(7, 11);
            this.LoadtrainingVideoButton.Name = "LoadtrainingVideoButton";
            this.LoadtrainingVideoButton.Size = new System.Drawing.Size(107, 41);
            this.LoadtrainingVideoButton.TabIndex = 8;
            this.LoadtrainingVideoButton.Text = "Load Video";
            this.LoadtrainingVideoButton.UseVisualStyleBackColor = true;
            this.LoadtrainingVideoButton.Click += new System.EventHandler(this.trainingLoadVideoButton_Click);
            // 
            // trainingVideoPictureBox
            // 
            this.trainingVideoPictureBox.Location = new System.Drawing.Point(7, 58);
            this.trainingVideoPictureBox.Name = "trainingVideoPictureBox";
            this.trainingVideoPictureBox.Size = new System.Drawing.Size(640, 480);
            this.trainingVideoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.trainingVideoPictureBox.TabIndex = 9;
            this.trainingVideoPictureBox.TabStop = false;
            this.trainingVideoPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trainingVideoPictureBox_MouseDown);
            this.trainingVideoPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.trainingVideoPictureBox_MouseMove);
            this.trainingVideoPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trainingVideoPictureBox_MouseUp);
            // 
            // saveFeatureDataButton
            // 
            this.saveFeatureDataButton.Location = new System.Drawing.Point(799, 11);
            this.saveFeatureDataButton.Name = "saveFeatureDataButton";
            this.saveFeatureDataButton.Size = new System.Drawing.Size(158, 41);
            this.saveFeatureDataButton.TabIndex = 10;
            this.saveFeatureDataButton.Text = "SaveFeatureData";
            this.saveFeatureDataButton.UseVisualStyleBackColor = true;
            this.saveFeatureDataButton.Click += new System.EventHandler(this.saveFeatureDataButton_Click);
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
            // surfOpenCheckBox
            // 
            this.surfOpenCheckBox.AutoSize = true;
            this.surfOpenCheckBox.Location = new System.Drawing.Point(701, 24);
            this.surfOpenCheckBox.Name = "surfOpenCheckBox";
            this.surfOpenCheckBox.Size = new System.Drawing.Size(185, 19);
            this.surfOpenCheckBox.TabIndex = 15;
            this.surfOpenCheckBox.Text = "Open SignBoard CheckBox";
            this.surfOpenCheckBox.UseVisualStyleBackColor = true;
            // 
            // pedestrianCheckBox
            // 
            this.pedestrianCheckBox.AutoSize = true;
            this.pedestrianCheckBox.Location = new System.Drawing.Point(701, 59);
            this.pedestrianCheckBox.Name = "pedestrianCheckBox";
            this.pedestrianCheckBox.Size = new System.Drawing.Size(241, 19);
            this.pedestrianCheckBox.TabIndex = 16;
            this.pedestrianCheckBox.Text = "Open Pedestrian Detection CheckBox";
            this.pedestrianCheckBox.UseVisualStyleBackColor = true;
            // 
            // runAnalysisButton
            // 
            this.runAnalysisButton.Location = new System.Drawing.Point(701, 93);
            this.runAnalysisButton.Name = "runAnalysisButton";
            this.runAnalysisButton.Size = new System.Drawing.Size(110, 42);
            this.runAnalysisButton.TabIndex = 17;
            this.runAnalysisButton.Text = "Start Analysis";
            this.runAnalysisButton.UseVisualStyleBackColor = true;
            this.runAnalysisButton.Click += new System.EventHandler(this.runAnalysisButton_Click);
            // 
            // stopAnalysisButton
            // 
            this.stopAnalysisButton.Location = new System.Drawing.Point(830, 90);
            this.stopAnalysisButton.Name = "stopAnalysisButton";
            this.stopAnalysisButton.Size = new System.Drawing.Size(112, 45);
            this.stopAnalysisButton.TabIndex = 18;
            this.stopAnalysisButton.Text = "Stop Analysis";
            this.stopAnalysisButton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 757);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mathcingVideoPictureBox)).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trainingVideoTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wantExtractFeatureImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trainingVideoPictureBox)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button matchingSuspendButton;
        private System.Windows.Forms.PictureBox mathcingVideoPictureBox;
        private System.Windows.Forms.Button loadMatchingVideoButton;
        private System.Windows.Forms.Button stopMatchingVideoButton;
        private System.Windows.Forms.Button playMatchingVideoButton;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button saveFeatureDataButton;
        private System.Windows.Forms.PictureBox trainingVideoPictureBox;
        private System.Windows.Forms.Button LoadtrainingVideoButton;
        private System.Windows.Forms.Button extractFeatureButton;
        private Emgu.CV.UI.ImageBox wantExtractFeatureImageBox;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button suspendButton;
        private System.Windows.Forms.Button playVideoButton;
        private System.Windows.Forms.TrackBar trainingVideoTrackBar;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.CheckBox pedestrianCheckBox;
        private System.Windows.Forms.CheckBox surfOpenCheckBox;
        private System.Windows.Forms.Button stopAnalysisButton;
        private System.Windows.Forms.Button runAnalysisButton;

    }
}

