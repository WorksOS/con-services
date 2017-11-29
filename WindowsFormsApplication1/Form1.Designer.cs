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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.edtNumRuns = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.edtNumImages = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnMultiThreadTest = new System.Windows.Forms.Button();
            this.editProjectID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.displayMode = new System.Windows.Forms.ComboBox();
            this.btnRedraw = new System.Windows.Forms.Button();
            this.chkSelectEarliestPass = new System.Windows.Forms.CheckBox();
            this.lblCellsPerPixel = new System.Windows.Forms.Label();
            this.lblViewHeight = new System.Windows.Forms.Label();
            this.lblViewWidth = new System.Windows.Forms.Label();
            this.bntTranslateSouth = new System.Windows.Forms.Button();
            this.bntTranslateWest = new System.Windows.Forms.Button();
            this.bntTranslateEast = new System.Windows.Forms.Button();
            this.btmZoomIn = new System.Windows.Forms.Button();
            this.btnZoomOut = new System.Windows.Forms.Button();
            this.bntTranslateNorth = new System.Windows.Forms.Button();
            this.btnZoomAll = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkIncludeSurveyedSurfaces = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.chkIncludeSurveyedSurfaces);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.edtNumRuns);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.edtNumImages);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnMultiThreadTest);
            this.panel1.Controls.Add(this.editProjectID);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.displayMode);
            this.panel1.Controls.Add(this.btnRedraw);
            this.panel1.Controls.Add(this.chkSelectEarliestPass);
            this.panel1.Controls.Add(this.lblCellsPerPixel);
            this.panel1.Controls.Add(this.lblViewHeight);
            this.panel1.Controls.Add(this.lblViewWidth);
            this.panel1.Controls.Add(this.bntTranslateSouth);
            this.panel1.Controls.Add(this.bntTranslateWest);
            this.panel1.Controls.Add(this.bntTranslateEast);
            this.panel1.Controls.Add(this.btmZoomIn);
            this.panel1.Controls.Add(this.btnZoomOut);
            this.panel1.Controls.Add(this.bntTranslateNorth);
            this.panel1.Controls.Add(this.btnZoomAll);
            this.panel1.Location = new System.Drawing.Point(883, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(187, 539);
            this.panel1.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(143, 494);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 36;
            this.label4.Text = "Runs";
            // 
            // edtNumRuns
            // 
            this.edtNumRuns.Location = new System.Drawing.Point(100, 491);
            this.edtNumRuns.Name = "edtNumRuns";
            this.edtNumRuns.Size = new System.Drawing.Size(34, 20);
            this.edtNumRuns.TabIndex = 35;
            this.edtNumRuns.Text = "10";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(17, 520);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(152, 19);
            this.button1.TabIndex = 34;
            this.button1.Text = "Scan all keys...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // edtNumImages
            // 
            this.edtNumImages.Location = new System.Drawing.Point(17, 491);
            this.edtNumImages.Name = "edtNumImages";
            this.edtNumImages.Size = new System.Drawing.Size(34, 20);
            this.edtNumImages.TabIndex = 33;
            this.edtNumImages.Text = "10";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(57, 494);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "Images";
            // 
            // btnMultiThreadTest
            // 
            this.btnMultiThreadTest.Location = new System.Drawing.Point(17, 462);
            this.btnMultiThreadTest.Name = "btnMultiThreadTest";
            this.btnMultiThreadTest.Size = new System.Drawing.Size(152, 23);
            this.btnMultiThreadTest.TabIndex = 31;
            this.btnMultiThreadTest.Text = "Multithread Test";
            this.btnMultiThreadTest.UseVisualStyleBackColor = true;
            this.btnMultiThreadTest.Click += new System.EventHandler(this.btnMultiThreadTest_Click);
            // 
            // editProjectID
            // 
            this.editProjectID.Location = new System.Drawing.Point(57, 6);
            this.editProjectID.Name = "editProjectID";
            this.editProjectID.Size = new System.Drawing.Size(100, 20);
            this.editProjectID.TabIndex = 30;
            this.editProjectID.Text = "3";
            this.editProjectID.TextChanged += new System.EventHandler(this.editProjectID_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Project";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 303);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Display mode";
            // 
            // displayMode
            // 
            this.displayMode.FormattingEnabled = true;
            this.displayMode.Location = new System.Drawing.Point(14, 322);
            this.displayMode.Name = "displayMode";
            this.displayMode.Size = new System.Drawing.Size(121, 21);
            this.displayMode.TabIndex = 27;
            // 
            // btnRedraw
            // 
            this.btnRedraw.Location = new System.Drawing.Point(53, 162);
            this.btnRedraw.Name = "btnRedraw";
            this.btnRedraw.Size = new System.Drawing.Size(75, 23);
            this.btnRedraw.TabIndex = 26;
            this.btnRedraw.Text = "Redraw";
            this.btnRedraw.UseVisualStyleBackColor = true;
            this.btnRedraw.Click += new System.EventHandler(this.btnRedraw_Click);
            // 
            // chkSelectEarliestPass
            // 
            this.chkSelectEarliestPass.AutoSize = true;
            this.chkSelectEarliestPass.Location = new System.Drawing.Point(17, 439);
            this.chkSelectEarliestPass.Name = "chkSelectEarliestPass";
            this.chkSelectEarliestPass.Size = new System.Drawing.Size(117, 17);
            this.chkSelectEarliestPass.TabIndex = 25;
            this.chkSelectEarliestPass.Text = "Select earliest pass";
            this.chkSelectEarliestPass.UseVisualStyleBackColor = true;
            // 
            // lblCellsPerPixel
            // 
            this.lblCellsPerPixel.AutoSize = true;
            this.lblCellsPerPixel.Location = new System.Drawing.Point(14, 397);
            this.lblCellsPerPixel.Name = "lblCellsPerPixel";
            this.lblCellsPerPixel.Size = new System.Drawing.Size(73, 13);
            this.lblCellsPerPixel.TabIndex = 24;
            this.lblCellsPerPixel.Text = "Cells Per Pixel";
            // 
            // lblViewHeight
            // 
            this.lblViewHeight.AutoSize = true;
            this.lblViewHeight.Location = new System.Drawing.Point(14, 378);
            this.lblViewHeight.Name = "lblViewHeight";
            this.lblViewHeight.Size = new System.Drawing.Size(64, 13);
            this.lblViewHeight.TabIndex = 23;
            this.lblViewHeight.Text = "View Height";
            // 
            // lblViewWidth
            // 
            this.lblViewWidth.AutoSize = true;
            this.lblViewWidth.Location = new System.Drawing.Point(14, 359);
            this.lblViewWidth.Name = "lblViewWidth";
            this.lblViewWidth.Size = new System.Drawing.Size(61, 13);
            this.lblViewWidth.TabIndex = 22;
            this.lblViewWidth.Text = "View Width";
            // 
            // bntTranslateSouth
            // 
            this.bntTranslateSouth.Location = new System.Drawing.Point(53, 264);
            this.bntTranslateSouth.Name = "bntTranslateSouth";
            this.bntTranslateSouth.Size = new System.Drawing.Size(75, 23);
            this.bntTranslateSouth.TabIndex = 21;
            this.bntTranslateSouth.Text = "South";
            this.bntTranslateSouth.UseVisualStyleBackColor = true;
            this.bntTranslateSouth.Click += new System.EventHandler(this.bntTranslateSouth_Click);
            // 
            // bntTranslateWest
            // 
            this.bntTranslateWest.Location = new System.Drawing.Point(14, 235);
            this.bntTranslateWest.Name = "bntTranslateWest";
            this.bntTranslateWest.Size = new System.Drawing.Size(74, 23);
            this.bntTranslateWest.TabIndex = 20;
            this.bntTranslateWest.Text = "West";
            this.bntTranslateWest.UseVisualStyleBackColor = true;
            this.bntTranslateWest.Click += new System.EventHandler(this.bntTranslateWest_Click);
            // 
            // bntTranslateEast
            // 
            this.bntTranslateEast.Location = new System.Drawing.Point(94, 235);
            this.bntTranslateEast.Name = "bntTranslateEast";
            this.bntTranslateEast.Size = new System.Drawing.Size(75, 23);
            this.bntTranslateEast.TabIndex = 19;
            this.bntTranslateEast.Text = "East";
            this.bntTranslateEast.UseVisualStyleBackColor = true;
            this.bntTranslateEast.Click += new System.EventHandler(this.bntTranslateEast_Click);
            // 
            // btmZoomIn
            // 
            this.btmZoomIn.Location = new System.Drawing.Point(53, 104);
            this.btmZoomIn.Name = "btmZoomIn";
            this.btmZoomIn.Size = new System.Drawing.Size(75, 23);
            this.btmZoomIn.TabIndex = 18;
            this.btmZoomIn.Text = "Zoom In";
            this.btmZoomIn.UseVisualStyleBackColor = true;
            this.btmZoomIn.Click += new System.EventHandler(this.btmZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.Location = new System.Drawing.Point(53, 133);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(75, 23);
            this.btnZoomOut.TabIndex = 17;
            this.btnZoomOut.Text = "Zoom Out";
            this.btnZoomOut.UseVisualStyleBackColor = true;
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // bntTranslateNorth
            // 
            this.bntTranslateNorth.Location = new System.Drawing.Point(53, 206);
            this.bntTranslateNorth.Name = "bntTranslateNorth";
            this.bntTranslateNorth.Size = new System.Drawing.Size(75, 23);
            this.bntTranslateNorth.TabIndex = 16;
            this.bntTranslateNorth.Text = "North";
            this.bntTranslateNorth.UseVisualStyleBackColor = true;
            this.bntTranslateNorth.Click += new System.EventHandler(this.btnTranslateNorth_Click);
            // 
            // btnZoomAll
            // 
            this.btnZoomAll.Location = new System.Drawing.Point(53, 75);
            this.btnZoomAll.Name = "btnZoomAll";
            this.btnZoomAll.Size = new System.Drawing.Size(75, 23);
            this.btnZoomAll.TabIndex = 15;
            this.btnZoomAll.Text = "Zoom All";
            this.btnZoomAll.UseVisualStyleBackColor = true;
            this.btnZoomAll.Click += new System.EventHandler(this.ZoomAll_Click);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.pictureBox1);
            this.panel2.Location = new System.Drawing.Point(2, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(875, 539);
            this.panel2.TabIndex = 16;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(10, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(862, 527);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // chkIncludeSurveyedSurfaces
            // 
            this.chkIncludeSurveyedSurfaces.AutoSize = true;
            this.chkIncludeSurveyedSurfaces.Location = new System.Drawing.Point(17, 416);
            this.chkIncludeSurveyedSurfaces.Name = "chkIncludeSurveyedSurfaces";
            this.chkIncludeSurveyedSurfaces.Size = new System.Drawing.Size(150, 17);
            this.chkIncludeSurveyedSurfaces.TabIndex = 37;
            this.chkIncludeSurveyedSurfaces.Text = "Include surveyed surfaces";
            this.chkIncludeSurveyedSurfaces.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1072, 546);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Raptor.Net/Ignite Test Application";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox displayMode;
        private System.Windows.Forms.Button btnRedraw;
        private System.Windows.Forms.CheckBox chkSelectEarliestPass;
        private System.Windows.Forms.Label lblCellsPerPixel;
        private System.Windows.Forms.Label lblViewHeight;
        private System.Windows.Forms.Label lblViewWidth;
        private System.Windows.Forms.Button bntTranslateSouth;
        private System.Windows.Forms.Button bntTranslateWest;
        private System.Windows.Forms.Button bntTranslateEast;
        private System.Windows.Forms.Button btmZoomIn;
        private System.Windows.Forms.Button btnZoomOut;
        private System.Windows.Forms.Button bntTranslateNorth;
        private System.Windows.Forms.Button btnZoomAll;
        private System.Windows.Forms.TextBox editProjectID;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnMultiThreadTest;
        private System.Windows.Forms.TextBox edtNumImages;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox edtNumRuns;
        private System.Windows.Forms.CheckBox chkIncludeSurveyedSurfaces;
    }
}

