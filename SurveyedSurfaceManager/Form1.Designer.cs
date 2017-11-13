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
            this.btnRegisterServices = new System.Windows.Forms.Button();
            this.bntListSurveyedSurfaces = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAddNewSurveyedSurface
            // 
            this.btnAddNewSurveyedSurface.Location = new System.Drawing.Point(30, 190);
            this.btnAddNewSurveyedSurface.Name = "btnAddNewSurveyedSurface";
            this.btnAddNewSurveyedSurface.Size = new System.Drawing.Size(166, 23);
            this.btnAddNewSurveyedSurface.TabIndex = 0;
            this.btnAddNewSurveyedSurface.Text = "Add New Surveyed Surface";
            this.btnAddNewSurveyedSurface.UseVisualStyleBackColor = true;
            this.btnAddNewSurveyedSurface.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(27, 91);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(369, 20);
            this.txtFilePath.TabIndex = 1;
            this.txtFilePath.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(27, 128);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(369, 20);
            this.txtFileName.TabIndex = 2;
            this.txtFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "File Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "File Path:";
            // 
            // txtSiteModelID
            // 
            this.txtSiteModelID.Location = new System.Drawing.Point(27, 49);
            this.txtSiteModelID.Name = "txtSiteModelID";
            this.txtSiteModelID.Size = new System.Drawing.Size(74, 20);
            this.txtSiteModelID.TabIndex = 5;
            this.txtSiteModelID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Site Model ID:";
            // 
            // btnRegisterServices
            // 
            this.btnRegisterServices.Location = new System.Drawing.Point(475, 161);
            this.btnRegisterServices.Name = "btnRegisterServices";
            this.btnRegisterServices.Size = new System.Drawing.Size(166, 23);
            this.btnRegisterServices.TabIndex = 7;
            this.btnRegisterServices.Text = "Register Services";
            this.btnRegisterServices.UseVisualStyleBackColor = true;
            this.btnRegisterServices.Click += new System.EventHandler(this.button2_Click);
            // 
            // bntListSurveyedSurfaces
            // 
            this.bntListSurveyedSurfaces.Location = new System.Drawing.Point(251, 190);
            this.bntListSurveyedSurfaces.Name = "bntListSurveyedSurfaces";
            this.bntListSurveyedSurfaces.Size = new System.Drawing.Size(166, 23);
            this.bntListSurveyedSurfaces.TabIndex = 8;
            this.bntListSurveyedSurfaces.Text = "List Surveyed Surfaces";
            this.bntListSurveyedSurfaces.UseVisualStyleBackColor = true;
            this.bntListSurveyedSurfaces.Click += new System.EventHandler(this.btnListSurveyedSurfacesClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(475, 190);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(166, 23);
            this.button1.TabIndex = 9;
            this.button1.Text = "Create Direct Access";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 244);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.bntListSurveyedSurfaces);
            this.Controls.Add(this.btnRegisterServices);
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
        private System.Windows.Forms.Button btnRegisterServices;
        private System.Windows.Forms.Button bntListSurveyedSurfaces;
        private System.Windows.Forms.Button button1;
    }
}

