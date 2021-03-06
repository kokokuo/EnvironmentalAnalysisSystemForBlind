﻿namespace ZebraCrossing_Test
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
            this.oriImageBox = new Emgu.CV.UI.ImageBox();
            this.maskImageBox = new Emgu.CV.UI.ImageBox();
            this.loadImgButton = new System.Windows.Forms.Button();
            this.maskImgButton = new System.Windows.Forms.Button();
            this.houghLineButton = new System.Windows.Forms.Button();
            this.toGrayButton = new System.Windows.Forms.Button();
            this.cropBottomButton = new System.Windows.Forms.Button();
            this.grayImgBox = new Emgu.CV.UI.ImageBox();
            this.smoothButton = new System.Windows.Forms.Button();
            this.runZebraDetectionButton = new System.Windows.Forms.Button();
            this.filterImageBox = new Emgu.CV.UI.ImageBox();
            this.filterPepperButton = new System.Windows.Forms.Button();
            this.restructLineButton = new System.Windows.Forms.Button();
            this.replayRestrcutLinesButton = new System.Windows.Forms.Button();
            this.filterLineButton = new System.Windows.Forms.Button();
            this.analyzeBlackWhiteButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.oriImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maskImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grayImgBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // oriImageBox
            // 
            this.oriImageBox.Location = new System.Drawing.Point(12, 122);
            this.oriImageBox.Name = "oriImageBox";
            this.oriImageBox.Size = new System.Drawing.Size(320, 240);
            this.oriImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.oriImageBox.TabIndex = 2;
            this.oriImageBox.TabStop = false;
            // 
            // maskImageBox
            // 
            this.maskImageBox.Location = new System.Drawing.Point(12, 445);
            this.maskImageBox.Name = "maskImageBox";
            this.maskImageBox.Size = new System.Drawing.Size(320, 240);
            this.maskImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.maskImageBox.TabIndex = 3;
            this.maskImageBox.TabStop = false;
            // 
            // loadImgButton
            // 
            this.loadImgButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.loadImgButton.Location = new System.Drawing.Point(12, 26);
            this.loadImgButton.Name = "loadImgButton";
            this.loadImgButton.Size = new System.Drawing.Size(126, 35);
            this.loadImgButton.TabIndex = 5;
            this.loadImgButton.Text = "1.載入圖片";
            this.loadImgButton.UseVisualStyleBackColor = true;
            this.loadImgButton.Click += new System.EventHandler(this.loadImgButton_Click);
            // 
            // maskImgButton
            // 
            this.maskImgButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.maskImgButton.Location = new System.Drawing.Point(12, 393);
            this.maskImgButton.Name = "maskImgButton";
            this.maskImgButton.Size = new System.Drawing.Size(141, 35);
            this.maskImgButton.TabIndex = 6;
            this.maskImgButton.Text = "3.Mask白色";
            this.maskImgButton.UseVisualStyleBackColor = true;
            this.maskImgButton.Click += new System.EventHandler(this.maskImgButton_Click);
            // 
            // houghLineButton
            // 
            this.houghLineButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.houghLineButton.Location = new System.Drawing.Point(723, 26);
            this.houghLineButton.Name = "houghLineButton";
            this.houghLineButton.Size = new System.Drawing.Size(220, 35);
            this.houghLineButton.TabIndex = 7;
            this.houghLineButton.Text = "5.顯示HoughLine";
            this.houghLineButton.UseVisualStyleBackColor = true;
            this.houghLineButton.Click += new System.EventHandler(this.houghLineButton_Click);
            // 
            // toGrayButton
            // 
            this.toGrayButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toGrayButton.Location = new System.Drawing.Point(374, 26);
            this.toGrayButton.Name = "toGrayButton";
            this.toGrayButton.Size = new System.Drawing.Size(126, 35);
            this.toGrayButton.TabIndex = 8;
            this.toGrayButton.Text = "2.灰階";
            this.toGrayButton.UseVisualStyleBackColor = true;
            this.toGrayButton.Click += new System.EventHandler(this.toGrayButton_Click);
            // 
            // cropBottomButton
            // 
            this.cropBottomButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cropBottomButton.Location = new System.Drawing.Point(12, 67);
            this.cropBottomButton.Name = "cropBottomButton";
            this.cropBottomButton.Size = new System.Drawing.Size(168, 35);
            this.cropBottomButton.TabIndex = 9;
            this.cropBottomButton.Text = "1.opt-剪裁下半部";
            this.cropBottomButton.UseVisualStyleBackColor = true;
            this.cropBottomButton.Click += new System.EventHandler(this.cropBottomButton_Click);
            // 
            // grayImgBox
            // 
            this.grayImgBox.Location = new System.Drawing.Point(374, 122);
            this.grayImgBox.Name = "grayImgBox";
            this.grayImgBox.Size = new System.Drawing.Size(320, 240);
            this.grayImgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.grayImgBox.TabIndex = 10;
            this.grayImgBox.TabStop = false;
            // 
            // smoothButton
            // 
            this.smoothButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.smoothButton.Location = new System.Drawing.Point(374, 67);
            this.smoothButton.Name = "smoothButton";
            this.smoothButton.Size = new System.Drawing.Size(211, 35);
            this.smoothButton.TabIndex = 13;
            this.smoothButton.Text = "2.opt-灰階與模糊";
            this.smoothButton.UseVisualStyleBackColor = true;
            this.smoothButton.Click += new System.EventHandler(this.toGrayAndSmoothButton_Click);
            // 
            // runZebraDetectionButton
            // 
            this.runZebraDetectionButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.runZebraDetectionButton.Location = new System.Drawing.Point(723, 269);
            this.runZebraDetectionButton.Name = "runZebraDetectionButton";
            this.runZebraDetectionButton.Size = new System.Drawing.Size(220, 35);
            this.runZebraDetectionButton.TabIndex = 14;
            this.runZebraDetectionButton.Text = "直接全部處理";
            this.runZebraDetectionButton.UseVisualStyleBackColor = true;
            this.runZebraDetectionButton.Click += new System.EventHandler(this.runZebraDetectionButton_Click);
            // 
            // filterImageBox
            // 
            this.filterImageBox.Location = new System.Drawing.Point(374, 445);
            this.filterImageBox.Name = "filterImageBox";
            this.filterImageBox.Size = new System.Drawing.Size(320, 240);
            this.filterImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.filterImageBox.TabIndex = 15;
            this.filterImageBox.TabStop = false;
            // 
            // filterPepperButton
            // 
            this.filterPepperButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.filterPepperButton.Location = new System.Drawing.Point(374, 393);
            this.filterPepperButton.Name = "filterPepperButton";
            this.filterPepperButton.Size = new System.Drawing.Size(143, 35);
            this.filterPepperButton.TabIndex = 16;
            this.filterPepperButton.Text = "4.opt-去雜訊";
            this.filterPepperButton.UseVisualStyleBackColor = true;
            this.filterPepperButton.Click += new System.EventHandler(this.filterPepperButton_Click);
            // 
            // restructLineButton
            // 
            this.restructLineButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.restructLineButton.Location = new System.Drawing.Point(723, 71);
            this.restructLineButton.Name = "restructLineButton";
            this.restructLineButton.Size = new System.Drawing.Size(220, 31);
            this.restructLineButton.TabIndex = 18;
            this.restructLineButton.Text = "破碎線段連線";
            this.restructLineButton.UseVisualStyleBackColor = true;
            this.restructLineButton.Click += new System.EventHandler(this.restructLineButton_Click);
            // 
            // replayRestrcutLinesButton
            // 
            this.replayRestrcutLinesButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.replayRestrcutLinesButton.Location = new System.Drawing.Point(949, 71);
            this.replayRestrcutLinesButton.Name = "replayRestrcutLinesButton";
            this.replayRestrcutLinesButton.Size = new System.Drawing.Size(220, 31);
            this.replayRestrcutLinesButton.TabIndex = 19;
            this.replayRestrcutLinesButton.Text = "步驟播放破碎線段連線";
            this.replayRestrcutLinesButton.UseVisualStyleBackColor = true;
            this.replayRestrcutLinesButton.Click += new System.EventHandler(this.replayRestrcutLinesButton_Click);
            // 
            // filterLineButton
            // 
            this.filterLineButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.filterLineButton.Location = new System.Drawing.Point(723, 122);
            this.filterLineButton.Name = "filterLineButton";
            this.filterLineButton.Size = new System.Drawing.Size(220, 31);
            this.filterLineButton.TabIndex = 20;
            this.filterLineButton.Text = "過濾線段";
            this.filterLineButton.UseVisualStyleBackColor = true;
            this.filterLineButton.Click += new System.EventHandler(this.filterLineButton_Click);
            // 
            // analyzeBlackWhiteButton
            // 
            this.analyzeBlackWhiteButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.analyzeBlackWhiteButton.Location = new System.Drawing.Point(723, 183);
            this.analyzeBlackWhiteButton.Name = "analyzeBlackWhiteButton";
            this.analyzeBlackWhiteButton.Size = new System.Drawing.Size(220, 31);
            this.analyzeBlackWhiteButton.TabIndex = 21;
            this.analyzeBlackWhiteButton.Text = "分析黑白變化";
            this.analyzeBlackWhiteButton.UseVisualStyleBackColor = true;
            this.analyzeBlackWhiteButton.Click += new System.EventHandler(this.analyzeBlackWhiteButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1649, 706);
            this.Controls.Add(this.analyzeBlackWhiteButton);
            this.Controls.Add(this.filterLineButton);
            this.Controls.Add(this.replayRestrcutLinesButton);
            this.Controls.Add(this.restructLineButton);
            this.Controls.Add(this.filterPepperButton);
            this.Controls.Add(this.filterImageBox);
            this.Controls.Add(this.runZebraDetectionButton);
            this.Controls.Add(this.smoothButton);
            this.Controls.Add(this.grayImgBox);
            this.Controls.Add(this.cropBottomButton);
            this.Controls.Add(this.toGrayButton);
            this.Controls.Add(this.houghLineButton);
            this.Controls.Add(this.maskImgButton);
            this.Controls.Add(this.loadImgButton);
            this.Controls.Add(this.maskImageBox);
            this.Controls.Add(this.oriImageBox);
            this.Name = "Form1";
            this.Text = "斑馬線偵測實驗專案";
            ((System.ComponentModel.ISupportInitialize)(this.oriImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maskImageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grayImgBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.filterImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Emgu.CV.UI.ImageBox oriImageBox;
        private Emgu.CV.UI.ImageBox maskImageBox;
        private System.Windows.Forms.Button loadImgButton;
        private System.Windows.Forms.Button maskImgButton;
        private System.Windows.Forms.Button houghLineButton;
        private System.Windows.Forms.Button toGrayButton;
        private System.Windows.Forms.Button cropBottomButton;
        private Emgu.CV.UI.ImageBox grayImgBox;
        private System.Windows.Forms.Button smoothButton;
        private System.Windows.Forms.Button runZebraDetectionButton;
        private Emgu.CV.UI.ImageBox filterImageBox;
        private System.Windows.Forms.Button filterPepperButton;
        private System.Windows.Forms.Button restructLineButton;
        private System.Windows.Forms.Button replayRestrcutLinesButton;
        private System.Windows.Forms.Button filterLineButton;
        private System.Windows.Forms.Button analyzeBlackWhiteButton;
    }
}

