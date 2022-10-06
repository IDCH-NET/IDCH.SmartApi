namespace TestApi
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Pic1 = new System.Windows.Forms.PictureBox();
            this.BtnPick = new System.Windows.Forms.Button();
            this.Pic2 = new System.Windows.Forms.PictureBox();
            this.BtnDetect = new System.Windows.Forms.Button();
            this.TxtInfo = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Pic1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pic2)).BeginInit();
            this.SuspendLayout();
            // 
            // Pic1
            // 
            this.Pic1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pic1.Location = new System.Drawing.Point(12, 12);
            this.Pic1.Name = "Pic1";
            this.Pic1.Size = new System.Drawing.Size(335, 274);
            this.Pic1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Pic1.TabIndex = 0;
            this.Pic1.TabStop = false;
            // 
            // BtnPick
            // 
            this.BtnPick.Location = new System.Drawing.Point(12, 292);
            this.BtnPick.Name = "BtnPick";
            this.BtnPick.Size = new System.Drawing.Size(75, 23);
            this.BtnPick.TabIndex = 1;
            this.BtnPick.Text = "&Pick Image";
            this.BtnPick.UseVisualStyleBackColor = true;
            // 
            // Pic2
            // 
            this.Pic2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Pic2.Location = new System.Drawing.Point(353, 12);
            this.Pic2.Name = "Pic2";
            this.Pic2.Size = new System.Drawing.Size(335, 274);
            this.Pic2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Pic2.TabIndex = 2;
            this.Pic2.TabStop = false;
            // 
            // BtnDetect
            // 
            this.BtnDetect.Location = new System.Drawing.Point(93, 292);
            this.BtnDetect.Name = "BtnDetect";
            this.BtnDetect.Size = new System.Drawing.Size(124, 23);
            this.BtnDetect.TabIndex = 3;
            this.BtnDetect.Text = "&Detect Objects";
            this.BtnDetect.UseVisualStyleBackColor = true;
            // 
            // TxtInfo
            // 
            this.TxtInfo.Location = new System.Drawing.Point(353, 292);
            this.TxtInfo.Name = "TxtInfo";
            this.TxtInfo.Size = new System.Drawing.Size(335, 96);
            this.TxtInfo.TabIndex = 4;
            this.TxtInfo.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TxtInfo);
            this.Controls.Add(this.BtnDetect);
            this.Controls.Add(this.Pic2);
            this.Controls.Add(this.BtnPick);
            this.Controls.Add(this.Pic1);
            this.Name = "Form1";
            this.Text = "Smart Api Test App";
            ((System.ComponentModel.ISupportInitialize)(this.Pic1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Pic2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox Pic1;
        private Button BtnPick;
        private PictureBox Pic2;
        private Button BtnDetect;
        private RichTextBox TxtInfo;
    }
}