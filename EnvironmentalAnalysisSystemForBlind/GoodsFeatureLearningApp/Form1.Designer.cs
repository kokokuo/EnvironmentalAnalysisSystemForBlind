namespace GoodsFeatureLearningApp
{
    partial class GoodsFeatureLearningForm
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
            this.loadImgButton = new System.Windows.Forms.Button();
            this.loadImgBox = new Emgu.CV.UI.ImageBox();
            this.extractFeatureImgBox = new Emgu.CV.UI.ImageBox();
            this.extractFeatureButton = new System.Windows.Forms.Button();
            this.saveFeatureButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.loadImgBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.extractFeatureImgBox)).BeginInit();
            this.SuspendLayout();
            // 
            // loadImgButton
            // 
            this.loadImgButton.Location = new System.Drawing.Point(28, 43);
            this.loadImgButton.Name = "loadImgButton";
            this.loadImgButton.Size = new System.Drawing.Size(133, 39);
            this.loadImgButton.TabIndex = 0;
            this.loadImgButton.Text = "1.載入學習影像";
            this.loadImgButton.UseVisualStyleBackColor = true;
            this.loadImgButton.Click += new System.EventHandler(this.loadImgButton_Click);
            // 
            // loadImgBox
            // 
            this.loadImgBox.Location = new System.Drawing.Point(28, 88);
            this.loadImgBox.Name = "loadImgBox";
            this.loadImgBox.Size = new System.Drawing.Size(320, 240);
            this.loadImgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.loadImgBox.TabIndex = 2;
            this.loadImgBox.TabStop = false;
            // 
            // extractFeatureImgBox
            // 
            this.extractFeatureImgBox.Location = new System.Drawing.Point(28, 427);
            this.extractFeatureImgBox.Name = "extractFeatureImgBox";
            this.extractFeatureImgBox.Size = new System.Drawing.Size(320, 240);
            this.extractFeatureImgBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.extractFeatureImgBox.TabIndex = 4;
            this.extractFeatureImgBox.TabStop = false;
            // 
            // extractFeatureButton
            // 
            this.extractFeatureButton.Location = new System.Drawing.Point(28, 382);
            this.extractFeatureButton.Name = "extractFeatureButton";
            this.extractFeatureButton.Size = new System.Drawing.Size(133, 39);
            this.extractFeatureButton.TabIndex = 3;
            this.extractFeatureButton.Text = "2.擷取特徵";
            this.extractFeatureButton.UseVisualStyleBackColor = true;
            this.extractFeatureButton.Click += new System.EventHandler(this.extractFeatureButton_Click);
            // 
            // saveFeatureButton
            // 
            this.saveFeatureButton.Location = new System.Drawing.Point(215, 382);
            this.saveFeatureButton.Name = "saveFeatureButton";
            this.saveFeatureButton.Size = new System.Drawing.Size(133, 39);
            this.saveFeatureButton.TabIndex = 5;
            this.saveFeatureButton.Text = "3.保存特徵檔案";
            this.saveFeatureButton.UseVisualStyleBackColor = true;
            this.saveFeatureButton.Click += new System.EventHandler(this.saveFeatureButton_Click);
            // 
            // GoodsFeatureLearningForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 690);
            this.Controls.Add(this.saveFeatureButton);
            this.Controls.Add(this.extractFeatureImgBox);
            this.Controls.Add(this.extractFeatureButton);
            this.Controls.Add(this.loadImgBox);
            this.Controls.Add(this.loadImgButton);
            this.Name = "GoodsFeatureLearningForm";
            this.Text = "商品特徵學習系統";
            ((System.ComponentModel.ISupportInitialize)(this.loadImgBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.extractFeatureImgBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button loadImgButton;
        private Emgu.CV.UI.ImageBox loadImgBox;
        private Emgu.CV.UI.ImageBox extractFeatureImgBox;
        private System.Windows.Forms.Button extractFeatureButton;
        private System.Windows.Forms.Button saveFeatureButton;
    }
}

