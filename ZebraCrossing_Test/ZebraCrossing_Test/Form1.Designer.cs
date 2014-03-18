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
            this.ori_imageBox = new Emgu.CV.UI.ImageBox();
            this.mask_imageBox = new Emgu.CV.UI.ImageBox();
            this.houghLine_imageBox = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.ori_imageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.mask_imageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.houghLine_imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ori_imageBox
            // 
            this.ori_imageBox.Location = new System.Drawing.Point(12, 126);
            this.ori_imageBox.Name = "ori_imageBox";
            this.ori_imageBox.Size = new System.Drawing.Size(320, 240);
            this.ori_imageBox.TabIndex = 2;
            this.ori_imageBox.TabStop = false;
            // 
            // mask_imageBox
            // 
            this.mask_imageBox.Location = new System.Drawing.Point(12, 435);
            this.mask_imageBox.Name = "mask_imageBox";
            this.mask_imageBox.Size = new System.Drawing.Size(320, 240);
            this.mask_imageBox.TabIndex = 3;
            this.mask_imageBox.TabStop = false;
            // 
            // houghLine_imageBox
            // 
            this.houghLine_imageBox.Location = new System.Drawing.Point(374, 126);
            this.houghLine_imageBox.Name = "houghLine_imageBox";
            this.houghLine_imageBox.Size = new System.Drawing.Size(320, 240);
            this.houghLine_imageBox.TabIndex = 4;
            this.houghLine_imageBox.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1020, 706);
            this.Controls.Add(this.houghLine_imageBox);
            this.Controls.Add(this.mask_imageBox);
            this.Controls.Add(this.ori_imageBox);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.ori_imageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.mask_imageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.houghLine_imageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Emgu.CV.UI.ImageBox ori_imageBox;
        private Emgu.CV.UI.ImageBox mask_imageBox;
        private Emgu.CV.UI.ImageBox houghLine_imageBox;
    }
}

