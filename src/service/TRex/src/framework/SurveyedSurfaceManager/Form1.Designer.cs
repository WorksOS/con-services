namespace SurveyedSurfaceManager
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
      this.btnAddNewSurveyedSurface = new System.Windows.Forms.Button();
      this.txtFilePath = new System.Windows.Forms.TextBox();
      this.txtFileName = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.txtSiteModelID = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.bntListSurveyedSurfaces = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.txtSurveyedSurfaceID = new System.Windows.Forms.TextBox();
      this.btnRemoveSurveyedSurface = new System.Windows.Forms.Button();
      this.btnAddAsNewDesign = new System.Windows.Forms.Button();
      this.label5 = new System.Windows.Forms.Label();
      this.txtOffset = new System.Windows.Forms.TextBox();
      this.label6 = new System.Windows.Forms.Label();
      this.dateTimePicker = new System.Windows.Forms.DateTimePicker();
      this.label7 = new System.Windows.Forms.Label();
      this.txtDesignID = new System.Windows.Forms.TextBox();
      this.btnRemoveDesign = new System.Windows.Forms.Button();
      this.btnListDesigns = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnAddNewSurveyedSurface
      // 
      this.btnAddNewSurveyedSurface.Location = new System.Drawing.Point(27, 193);
      this.btnAddNewSurveyedSurface.Name = "btnAddNewSurveyedSurface";
      this.btnAddNewSurveyedSurface.Size = new System.Drawing.Size(166, 23);
      this.btnAddNewSurveyedSurface.TabIndex = 0;
      this.btnAddNewSurveyedSurface.Text = "Add As New Surveyed Surface";
      this.btnAddNewSurveyedSurface.UseVisualStyleBackColor = true;
      this.btnAddNewSurveyedSurface.Click += new System.EventHandler(this.button1_Click);
      // 
      // txtFilePath
      // 
      this.txtFilePath.Location = new System.Drawing.Point(28, 75);
      this.txtFilePath.Name = "txtFilePath";
      this.txtFilePath.Size = new System.Drawing.Size(369, 20);
      this.txtFilePath.TabIndex = 1;
      this.txtFilePath.Text = "C:\\Dev\\VSS.TRex\\tests\\TestData\\Dimensions 2012\\BC Data\\Sites\\BootCamp 2012\\Design" +
    "s(linework in USFt)\\Original Ground Survey";
      this.txtFilePath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.txtFilePath.TextChanged += new System.EventHandler(this.txtFilePath_TextChanged);
      // 
      // txtFileName
      // 
      this.txtFileName.Location = new System.Drawing.Point(28, 112);
      this.txtFileName.Name = "txtFileName";
      this.txtFileName.Size = new System.Drawing.Size(369, 20);
      this.txtFileName.TabIndex = 2;
      this.txtFileName.Text = "Original Ground Survey - Dimensions 2012.ttm";
      this.txtFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(28, 98);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(57, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "File Name:";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(28, 56);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(51, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "File Path:";
      // 
      // txtSiteModelID
      // 
      this.txtSiteModelID.Location = new System.Drawing.Point(28, 33);
      this.txtSiteModelID.Name = "txtSiteModelID";
      this.txtSiteModelID.Size = new System.Drawing.Size(74, 20);
      this.txtSiteModelID.TabIndex = 5;
      this.txtSiteModelID.Text = "5";
      this.txtSiteModelID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(28, 17);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(74, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "Site Model ID:";
      // 
      // bntListSurveyedSurfaces
      // 
      this.bntListSurveyedSurfaces.Location = new System.Drawing.Point(647, 109);
      this.bntListSurveyedSurfaces.Name = "bntListSurveyedSurfaces";
      this.bntListSurveyedSurfaces.Size = new System.Drawing.Size(166, 23);
      this.bntListSurveyedSurfaces.TabIndex = 8;
      this.bntListSurveyedSurfaces.Text = "List Surveyed Surfaces";
      this.bntListSurveyedSurfaces.UseVisualStyleBackColor = true;
      this.bntListSurveyedSurfaces.Click += new System.EventHandler(this.btnListSurveyedSurfacesClick);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(710, 17);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(103, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "Surveyd Surface ID:";
      // 
      // txtSurveyedSurfaceID
      // 
      this.txtSurveyedSurfaceID.Location = new System.Drawing.Point(713, 33);
      this.txtSurveyedSurfaceID.Name = "txtSurveyedSurfaceID";
      this.txtSurveyedSurfaceID.Size = new System.Drawing.Size(100, 20);
      this.txtSurveyedSurfaceID.TabIndex = 10;
      this.txtSurveyedSurfaceID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // btnRemoveSurveyedSurface
      // 
      this.btnRemoveSurveyedSurface.Location = new System.Drawing.Point(647, 62);
      this.btnRemoveSurveyedSurface.Name = "btnRemoveSurveyedSurface";
      this.btnRemoveSurveyedSurface.Size = new System.Drawing.Size(166, 23);
      this.btnRemoveSurveyedSurface.TabIndex = 12;
      this.btnRemoveSurveyedSurface.Text = "Remove Surveyed Surface";
      this.btnRemoveSurveyedSurface.UseVisualStyleBackColor = true;
      this.btnRemoveSurveyedSurface.Click += new System.EventHandler(this.btnRemoveSurveyedSurface_Click);
      // 
      // btnAddAsNewDesign
      // 
      this.btnAddAsNewDesign.Location = new System.Drawing.Point(199, 193);
      this.btnAddAsNewDesign.Name = "btnAddAsNewDesign";
      this.btnAddAsNewDesign.Size = new System.Drawing.Size(166, 23);
      this.btnAddAsNewDesign.TabIndex = 13;
      this.btnAddAsNewDesign.Text = "Add As New Design";
      this.btnAddAsNewDesign.UseVisualStyleBackColor = true;
      this.btnAddAsNewDesign.Click += new System.EventHandler(this.btnAddAsNewDesign_Click);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(28, 141);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(78, 13);
      this.label5.TabIndex = 15;
      this.label5.Text = "Offset (meters):";
      // 
      // txtOffset
      // 
      this.txtOffset.Location = new System.Drawing.Point(28, 157);
      this.txtOffset.Name = "txtOffset";
      this.txtOffset.Size = new System.Drawing.Size(74, 20);
      this.txtOffset.TabIndex = 14;
      this.txtOffset.Text = "5.5";
      this.txtOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(132, 141);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(89, 13);
      this.label6.TabIndex = 17;
      this.label6.Text = "As At Date/Time:";
      // 
      // dateTimePicker
      // 
      this.dateTimePicker.Location = new System.Drawing.Point(133, 157);
      this.dateTimePicker.Name = "dateTimePicker";
      this.dateTimePicker.Size = new System.Drawing.Size(200, 20);
      this.dateTimePicker.TabIndex = 18;
      // 
      // label7
      // 
      this.label7.AllowDrop = true;
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(442, 17);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(57, 13);
      this.label7.TabIndex = 20;
      this.label7.Text = "Design ID:";
      // 
      // txtDesignID
      // 
      this.txtDesignID.AllowDrop = true;
      this.txtDesignID.Location = new System.Drawing.Point(445, 33);
      this.txtDesignID.Name = "txtDesignID";
      this.txtDesignID.Size = new System.Drawing.Size(100, 20);
      this.txtDesignID.TabIndex = 19;
      this.txtDesignID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      // 
      // btnRemoveDesign
      // 
      this.btnRemoveDesign.Location = new System.Drawing.Point(445, 62);
      this.btnRemoveDesign.Name = "btnRemoveDesign";
      this.btnRemoveDesign.Size = new System.Drawing.Size(166, 23);
      this.btnRemoveDesign.TabIndex = 21;
      this.btnRemoveDesign.Text = "Remove Design";
      this.btnRemoveDesign.UseVisualStyleBackColor = true;
      this.btnRemoveDesign.Click += new System.EventHandler(this.btnRemoveDesign_Click);
      // 
      // btnListDesigns
      // 
      this.btnListDesigns.Location = new System.Drawing.Point(445, 109);
      this.btnListDesigns.Name = "btnListDesigns";
      this.btnListDesigns.Size = new System.Drawing.Size(166, 23);
      this.btnListDesigns.TabIndex = 22;
      this.btnListDesigns.Text = "List Designs";
      this.btnListDesigns.UseVisualStyleBackColor = true;
      this.btnListDesigns.Click += new System.EventHandler(this.btnListDesigns_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(836, 231);
      this.Controls.Add(this.btnListDesigns);
      this.Controls.Add(this.btnRemoveDesign);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.txtDesignID);
      this.Controls.Add(this.dateTimePicker);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.txtOffset);
      this.Controls.Add(this.btnAddAsNewDesign);
      this.Controls.Add(this.btnRemoveSurveyedSurface);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.txtSurveyedSurfaceID);
      this.Controls.Add(this.bntListSurveyedSurfaces);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.txtSiteModelID);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtFileName);
      this.Controls.Add(this.txtFilePath);
      this.Controls.Add(this.btnAddNewSurveyedSurface);
      this.Name = "Form1";
      this.Text = "Surveyed Surface Manager";
      this.ResumeLayout(false);
      this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAddNewSurveyedSurface;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSiteModelID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button bntListSurveyedSurfaces;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSurveyedSurfaceID;
        private System.Windows.Forms.Button btnRemoveSurveyedSurface;
        private System.Windows.Forms.Button btnAddAsNewDesign;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dateTimePicker;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDesignID;
        private System.Windows.Forms.Button btnRemoveDesign;
        private System.Windows.Forms.Button btnListDesigns;
    }
}

