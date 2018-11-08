namespace BookMark
{
    partial class BookmarkFrm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookmarkFrm));
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.lbSuccess = new System.Windows.Forms.Label();
            this.dgvBookmarks = new System.Windows.Forms.DataGridView();
            this.ColSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Organisation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BookMark = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastUpdated = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastFilesProcessed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LastFilesErrorneous = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TotalFilesProcessed = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.monthCalendar = new System.Windows.Forms.MonthCalendar();
            this.grpSave = new System.Windows.Forms.GroupBox();
            this.rbtAll = new System.Windows.Forms.RadioButton();
            this.rbtSelected = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBookmarks)).BeginInit();
            this.grpSave.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.btnBrowse.BackColor = System.Drawing.Color.Blue;
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(9, 37);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(2);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(202, 52);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "Browse for input XML file";
            this.btnBrowse.UseVisualStyleBackColor = false;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this.btnUpdate.ForeColor = System.Drawing.Color.White;
            this.btnUpdate.Location = new System.Drawing.Point(9, 460);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(202, 52);
            this.btnUpdate.TabIndex = 2;
            this.btnUpdate.Text = "Update and save as XML file\r\n";
            this.btnUpdate.UseVisualStyleBackColor = false;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // lbSuccess
            // 
            this.lbSuccess.AutoSize = true;
            this.lbSuccess.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbSuccess.Location = new System.Drawing.Point(14, 330);
            this.lbSuccess.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbSuccess.Name = "lbSuccess";
            this.lbSuccess.Size = new System.Drawing.Size(47, 13);
            this.lbSuccess.TabIndex = 4;
            this.lbSuccess.Text = "Status:";
            // 
            // dgvBookmarks
            // 
            this.dgvBookmarks.AllowUserToAddRows = false;
            this.dgvBookmarks.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgvBookmarks.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvBookmarks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvBookmarks.CausesValidation = false;
            this.dgvBookmarks.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dgvBookmarks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBookmarks.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColSelect,
            this.Organisation,
            this.BookMark,
            this.LastUpdated,
            this.LastFilesProcessed,
            this.LastFilesErrorneous,
            this.TotalFilesProcessed});
            this.dgvBookmarks.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.dgvBookmarks.Location = new System.Drawing.Point(9, 27);
            this.dgvBookmarks.Margin = new System.Windows.Forms.Padding(2);
            this.dgvBookmarks.Name = "dgvBookmarks";
            this.dgvBookmarks.RowTemplate.Height = 24;
            this.dgvBookmarks.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBookmarks.Size = new System.Drawing.Size(1039, 552);
            this.dgvBookmarks.TabIndex = 5;
            //this.dgvBookmarks.RowStateChanged += new System.Windows.Forms.DataGridViewRowStateChangedEventHandler(this.dgvBookmarks_RowStateChanged);
            this.dgvBookmarks.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dgvBookmarks_KeyPress);
            // 
            // ColSelect
            // 
            this.ColSelect.FalseValue = "";
            this.ColSelect.HeaderText = "Select";
            this.ColSelect.IndeterminateValue = "";
            this.ColSelect.Name = "ColSelect";
            this.ColSelect.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ColSelect.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ColSelect.TrueValue = "";
            this.ColSelect.Width = 60;
            // 
            // Organisation
            // 
            this.Organisation.HeaderText = "Organisation";
            this.Organisation.Name = "Organisation";
            this.Organisation.Width = 150;
            // 
            // BookMark
            // 
            this.BookMark.HeaderText = "Book Mark";
            this.BookMark.Name = "BookMark";
            this.BookMark.Width = 180;
            // 
            // LastUpdated
            // 
            this.LastUpdated.HeaderText = "Last Updated";
            this.LastUpdated.Name = "LastUpdated";
            this.LastUpdated.Width = 150;
            // 
            // LastFilesProcessed
            // 
            this.LastFilesProcessed.HeaderText = "Last Files Processed";
            this.LastFilesProcessed.Name = "LastFilesProcessed";
            this.LastFilesProcessed.Width = 150;
            // 
            // LastFilesErrorneous
            // 
            this.LastFilesErrorneous.HeaderText = "Last Files Errorneous";
            this.LastFilesErrorneous.Name = "LastFilesErrorneous";
            this.LastFilesErrorneous.Width = 150;
            // 
            // TotalFilesProcessed
            // 
            this.TotalFilesProcessed.HeaderText = "Total Files Processed";
            this.TotalFilesProcessed.Name = "TotalFilesProcessed";
            this.TotalFilesProcessed.Width = 150;
            // 
            // monthCalendar
            // 
            this.monthCalendar.Location = new System.Drawing.Point(9, 120);
            this.monthCalendar.Margin = new System.Windows.Forms.Padding(7);
            this.monthCalendar.MaxDate = new System.DateTime(2020, 12, 31, 0, 0, 0, 0);
            this.monthCalendar.MaxSelectionCount = 1;
            this.monthCalendar.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.monthCalendar.Name = "monthCalendar";
            this.monthCalendar.TabIndex = 6;
            this.monthCalendar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.monthCalendar_MouseDown);
            // 
            // grpSave
            // 
            this.grpSave.Controls.Add(this.rbtAll);
            this.grpSave.Controls.Add(this.rbtSelected);
            this.grpSave.Location = new System.Drawing.Point(9, 365);
            this.grpSave.Margin = new System.Windows.Forms.Padding(2);
            this.grpSave.Name = "grpSave";
            this.grpSave.Padding = new System.Windows.Forms.Padding(2);
            this.grpSave.Size = new System.Drawing.Size(182, 80);
            this.grpSave.TabIndex = 7;
            this.grpSave.TabStop = false;
            this.grpSave.Text = "Update bookmarks";
            // 
            // rbtAll
            // 
            this.rbtAll.AutoSize = true;
            this.rbtAll.Location = new System.Drawing.Point(10, 48);
            this.rbtAll.Margin = new System.Windows.Forms.Padding(2);
            this.rbtAll.Name = "rbtAll";
            this.rbtAll.Size = new System.Drawing.Size(118, 17);
            this.rbtAll.TabIndex = 1;
            this.rbtAll.Text = "All bookmarks in file";
            this.rbtAll.UseVisualStyleBackColor = true;
            // 
            // rbtSelected
            // 
            this.rbtSelected.AutoSize = true;
            this.rbtSelected.Checked = true;
            this.rbtSelected.Location = new System.Drawing.Point(10, 17);
            this.rbtSelected.Margin = new System.Windows.Forms.Padding(2);
            this.rbtSelected.Name = "rbtSelected";
            this.rbtSelected.Size = new System.Drawing.Size(122, 17);
            this.rbtSelected.TabIndex = 0;
            this.rbtSelected.TabStop = true;
            this.rbtSelected.Text = "Selected bookmarks";
            this.rbtSelected.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Controls.Add(this.grpSave);
            this.groupBox1.Controls.Add(this.monthCalendar);
            this.groupBox1.Controls.Add(this.btnUpdate);
            this.groupBox1.Controls.Add(this.lbSuccess);
            this.groupBox1.Location = new System.Drawing.Point(1052, 11);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(242, 568);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // BookmarkFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1305, 637);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dgvBookmarks);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BookmarkFrm";
            this.Text = "Bookmark ";
            ((System.ComponentModel.ISupportInitialize)(this.dgvBookmarks)).EndInit();
            this.grpSave.ResumeLayout(false);
            this.grpSave.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Label lbSuccess;
        private System.Windows.Forms.DataGridView dgvBookmarks;
        private System.Windows.Forms.MonthCalendar monthCalendar;
        private System.Windows.Forms.GroupBox grpSave;
        private System.Windows.Forms.RadioButton rbtAll;
        private System.Windows.Forms.RadioButton rbtSelected;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn Organisation;
        private System.Windows.Forms.DataGridViewTextBoxColumn BookMark;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastUpdated;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastFilesProcessed;
        private System.Windows.Forms.DataGridViewTextBoxColumn LastFilesErrorneous;
        private System.Windows.Forms.DataGridViewTextBoxColumn TotalFilesProcessed;
    }
}