namespace VSS.Raptor.IgnitePOC.TestApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btnZoomAll = new System.Windows.Forms.Button();
            this.bntTranslateNorth = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.btmZoomIn = new System.Windows.Forms.Button();
            this.bntTranslateEast = new System.Windows.Forms.Button();
            this.bntTranslateWest = new System.Windows.Forms.Button();
            this.bntTranslateSouth = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(500, 500);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // btnZoomAll
            // 
            this.btnZoomAll.Location = new System.Drawing.Point(613, 12);
            this.btnZoomAll.Name = "btnZoomAll";
            this.btnZoomAll.Size = new System.Drawing.Size(75, 23);
            this.btnZoomAll.TabIndex = 1;
            this.btnZoomAll.Text = "Zoom All";
            this.btnZoomAll.UseVisualStyleBackColor = true;
            this.btnZoomAll.Click += new System.EventHandler(this.ZoomAll_Click);
            // 
            // bntTranslateNorth
            // 
            this.bntTranslateNorth.Location = new System.Drawing.Point(613, 123);
            this.bntTranslateNorth.Name = "bntTranslateNorth";
            this.bntTranslateNorth.Size = new System.Drawing.Size(75, 23);
            this.bntTranslateNorth.TabIndex = 2;
            this.bntTranslateNorth.Text = "North";
            this.bntTranslateNorth.UseVisualStyleBackColor = true;
            this.bntTranslateNorth.Click += new System.EventHandler(this.btnTranslateNorth_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.Location = new System.Drawing.Point(613, 70);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(75, 23);
            this.btnZoomOut.TabIndex = 3;
            this.btnZoomOut.Text = "Zoom Out";
            this.btnZoomOut.UseVisualStyleBackColor = true;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btmZoomIn
            // 
            this.btmZoomIn.Location = new System.Drawing.Point(613, 41);
            this.btmZoomIn.Name = "btmZoomIn";
            this.btmZoomIn.Size = new System.Drawing.Size(75, 23);
            this.btmZoomIn.TabIndex = 4;
            this.btmZoomIn.Text = "Zoom In";
            this.btmZoomIn.UseVisualStyleBackColor = true;
            this.btmZoomIn.Click += new System.EventHandler(this.btmZoomIn_Click);
            // 
            // bntTranslateEast
            // 
            this.bntTranslateEast.Location = new System.Drawing.Point(654, 152);
            this.bntTranslateEast.Name = "bntTranslateEast";
            this.bntTranslateEast.Size = new System.Drawing.Size(75, 23);
            this.bntTranslateEast.TabIndex = 5;
            this.bntTranslateEast.Text = "East";
            this.bntTranslateEast.UseVisualStyleBackColor = true;
            this.bntTranslateEast.Click += new System.EventHandler(this.bntTranslateEast_Click);
            // 
            // bntTranslateWest
            // 
            this.bntTranslateWest.Location = new System.Drawing.Point(574, 152);
            this.bntTranslateWest.Name = "bntTranslateWest";
            this.bntTranslateWest.Size = new System.Drawing.Size(74, 23);
            this.bntTranslateWest.TabIndex = 6;
            this.bntTranslateWest.Text = "West";
            this.bntTranslateWest.UseVisualStyleBackColor = true;
            this.bntTranslateWest.Click += new System.EventHandler(this.bntTranslateWest_Click);
            // 
            // bntTranslateSouth
            // 
            this.bntTranslateSouth.Location = new System.Drawing.Point(613, 181);
            this.bntTranslateSouth.Name = "bntTranslateSouth";
            this.bntTranslateSouth.Size = new System.Drawing.Size(75, 23);
            this.bntTranslateSouth.TabIndex = 7;
            this.bntTranslateSouth.Text = "South";
            this.bntTranslateSouth.UseVisualStyleBackColor = true;
            this.bntTranslateSouth.Click += new System.EventHandler(this.bntTranslateSouth_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1001, 643);
            this.Controls.Add(this.bntTranslateSouth);
            this.Controls.Add(this.bntTranslateWest);
            this.Controls.Add(this.bntTranslateEast);
            this.Controls.Add(this.btmZoomIn);
            this.Controls.Add(this.btnZoomOut);
            this.Controls.Add(this.bntTranslateNorth);
            this.Controls.Add(this.btnZoomAll);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnZoomAll;
        private System.Windows.Forms.Button bntTranslateNorth;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button btmZoomIn;
        private System.Windows.Forms.Button bntTranslateEast;
        private System.Windows.Forms.Button bntTranslateWest;
        private System.Windows.Forms.Button bntTranslateSouth;
    }
}

