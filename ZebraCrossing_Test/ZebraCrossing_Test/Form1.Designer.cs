namespace ZebraCrossing_Test
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
            this.contourButton = new System.Windows.Forms.Button();
            this.contourImageBox = new Emgu.CV.UI.ImageBox();
            this.smoothButton = new System.Windows.Forms.Button();
            this.dilateButton = new System.Windows.Forms.Button();
            this.filterImageBox = new Emgu.CV.UI.ImageBox();
            this.filterPepperButton = new System.Windows.Forms.Button();
            this.findScanLineButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.oriImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maskImageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grayImgBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.contourImageBox)).BeginInit();
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
            // contourButton
            // 
            this.contourButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.contourButton.Location = new System.Drawing.Point(744, 393);
            this.contourButton.Name = "contourButton";
            this.contourButton.Size = new System.Drawing.Size(126, 35);
            this.contourButton.TabIndex = 11;
            this.contourButton.Text = "5.連通";
            this.contourButton.UseVisualStyleBackColor = true;
            this.contourButton.Click += new System.EventHandler(this.contourButton_Click);
            // 
            // contourImageBox
            // 
            this.contourImageBox.Location = new System.Drawing.Point(744, 445);
            this.contourImageBox.Name = "contourImageBox";
            this.contourImageBox.Size = new System.Drawing.Size(320, 240);
            this.contourImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.contourImageBox.TabIndex = 12;
            this.contourImageBox.TabStop = false;
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
            this.smoothButton.Click += new System.EventHandler(this.smoothButton_Click);
            // 
            // dilateButton
            // 
            this.dilateButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.dilateButton.Location = new System.Drawing.Point(557, 393);
            this.dilateButton.Name = "dilateButton";
            this.dilateButton.Size = new System.Drawing.Size(137, 35);
            this.dilateButton.TabIndex = 14;
            this.dilateButton.Text = "4.opt-膨脹";
            this.dilateButton.UseVisualStyleBackColor = true;
            this.dilateButton.Click += new System.EventHandler(this.dilateButton_Click);
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
            // findScanLineButton
            // 
            this.findScanLineButton.Font = new System.Drawing.Font("微軟正黑體", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.findScanLineButton.Location = new System.Drawing.Point(876, 393);
            this.findScanLineButton.Name = "findScanLineButton";
            this.findScanLineButton.Size = new System.Drawing.Size(188, 35);
            this.findScanLineButton.TabIndex = 17;
            this.findScanLineButton.Text = "6.連出Scan Line";
            this.findScanLineButton.UseVisualStyleBackColor = true;
            this.findScanLineButton.Click += new System.EventHandler(this.findScanLineButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1649, 706);
            this.Controls.Add(this.findScanLineButton);
            this.Controls.Add(this.filterPepperButton);
            this.Controls.Add(this.filterImageBox);
            this.Controls.Add(this.dilateButton);
            this.Controls.Add(this.smoothButton);
            this.Controls.Add(this.contourImageBox);
            this.Controls.Add(this.contourButton);
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
            ((System.ComponentModel.ISupportInitialize)(this.contourImageBox)).EndInit();
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
        private System.Windows.Forms.Button contourButton;
        private Emgu.CV.UI.ImageBox contourImageBox;
        private System.Windows.Forms.Button smoothButton;
        private System.Windows.Forms.Button dilateButton;
        private Emgu.CV.UI.ImageBox filterImageBox;
        private System.Windows.Forms.Button filterPepperButton;
        private System.Windows.Forms.Button findScanLineButton;
    }
}

