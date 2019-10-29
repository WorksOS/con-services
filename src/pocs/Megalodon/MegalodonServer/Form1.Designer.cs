namespace MegalodonServer
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
      this.btnProcessTrack = new System.Windows.Forms.Button();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.btnClear = new System.Windows.Forms.Button();
      this.btnMove = new System.Windows.Forms.Button();
      this.btnTagfile = new System.Windows.Forms.Button();
      this.btnWrite = new System.Windows.Forms.Button();
      this.btnOpenPort = new System.Windows.Forms.Button();
      this.txtPort = new System.Windows.Forms.TextBox();
      this.btnCloseTagFile = new System.Windows.Forms.Button();
      this.btnCloseListener = new System.Windows.Forms.Button();
      this.button1 = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.btnNewTagfile = new System.Windows.Forms.Button();
      this.chkDump = new System.Windows.Forms.CheckBox();
      this.chkDumpContent = new System.Windows.Forms.CheckBox();
      this.label21 = new System.Windows.Forms.Label();
      this.txtTCIP = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.btnDump = new System.Windows.Forms.Button();
      this.btnThread = new System.Windows.Forms.Button();
      this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnInterface = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // btnProcessTrack
      // 
      this.btnProcessTrack.Location = new System.Drawing.Point(774, 12);
      this.btnProcessTrack.Name = "btnProcessTrack";
      this.btnProcessTrack.Size = new System.Drawing.Size(112, 23);
      this.btnProcessTrack.TabIndex = 0;
      this.btnProcessTrack.Text = "Process Track";
      this.btnProcessTrack.UseVisualStyleBackColor = true;
      this.btnProcessTrack.Visible = false;
      this.btnProcessTrack.Click += new System.EventHandler(this.BtnProcessTrack_Click);
      // 
      // listBox1
      // 
      this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(19, 122);
      this.listBox1.Name = "listBox1";
      this.listBox1.ScrollAlwaysVisible = true;
      this.listBox1.Size = new System.Drawing.Size(991, 381);
      this.listBox1.TabIndex = 1;
      // 
      // btnClear
      // 
      this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClear.Location = new System.Drawing.Point(935, 67);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(75, 23);
      this.btnClear.TabIndex = 2;
      this.btnClear.Text = "Clear";
      this.btnClear.UseVisualStyleBackColor = true;
      this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
      // 
      // btnMove
      // 
      this.btnMove.Location = new System.Drawing.Point(892, 12);
      this.btnMove.Name = "btnMove";
      this.btnMove.Size = new System.Drawing.Size(90, 23);
      this.btnMove.TabIndex = 3;
      this.btnMove.Text = "Move Archive";
      this.btnMove.UseVisualStyleBackColor = true;
      this.btnMove.Visible = false;
      this.btnMove.Click += new System.EventHandler(this.BtnMove_Click);
      // 
      // btnTagfile
      // 
      this.btnTagfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTagfile.Location = new System.Drawing.Point(910, 537);
      this.btnTagfile.Name = "btnTagfile";
      this.btnTagfile.Size = new System.Drawing.Size(100, 23);
      this.btnTagfile.TabIndex = 4;
      this.btnTagfile.Text = "TagFile Read Test";
      this.btnTagfile.UseVisualStyleBackColor = true;
      this.btnTagfile.Visible = false;
      this.btnTagfile.Click += new System.EventHandler(this.BtnTagfile_Click);
      // 
      // btnWrite
      // 
      this.btnWrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnWrite.Location = new System.Drawing.Point(806, 537);
      this.btnWrite.Name = "btnWrite";
      this.btnWrite.Size = new System.Drawing.Size(98, 23);
      this.btnWrite.TabIndex = 5;
      this.btnWrite.Text = "Create Taqgfile Data";
      this.btnWrite.UseVisualStyleBackColor = true;
      this.btnWrite.Visible = false;
      this.btnWrite.Click += new System.EventHandler(this.BtnWrite_Click);
      // 
      // btnOpenPort
      // 
      this.btnOpenPort.Location = new System.Drawing.Point(19, 13);
      this.btnOpenPort.Name = "btnOpenPort";
      this.btnOpenPort.Size = new System.Drawing.Size(171, 23);
      this.btnOpenPort.TabIndex = 6;
      this.btnOpenPort.Text = "Start Port Listener";
      this.btnOpenPort.UseVisualStyleBackColor = true;
      this.btnOpenPort.Click += new System.EventHandler(this.BtnOpenPort_Click);
      // 
      // txtPort
      // 
      this.txtPort.Location = new System.Drawing.Point(402, 16);
      this.txtPort.Name = "txtPort";
      this.txtPort.Size = new System.Drawing.Size(68, 20);
      this.txtPort.TabIndex = 7;
      this.txtPort.Text = "1500";
      // 
      // btnCloseTagFile
      // 
      this.btnCloseTagFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCloseTagFile.Location = new System.Drawing.Point(702, 537);
      this.btnCloseTagFile.Name = "btnCloseTagFile";
      this.btnCloseTagFile.Size = new System.Drawing.Size(98, 23);
      this.btnCloseTagFile.TabIndex = 8;
      this.btnCloseTagFile.Text = "Close Tagfile";
      this.btnCloseTagFile.UseVisualStyleBackColor = true;
      this.btnCloseTagFile.Visible = false;
      this.btnCloseTagFile.Click += new System.EventHandler(this.BtnCloseTagFile_Click);
      // 
      // btnCloseListener
      // 
      this.btnCloseListener.Location = new System.Drawing.Point(19, 42);
      this.btnCloseListener.Name = "btnCloseListener";
      this.btnCloseListener.Size = new System.Drawing.Size(171, 23);
      this.btnCloseListener.TabIndex = 9;
      this.btnCloseListener.Text = "Close Listener";
      this.btnCloseListener.UseVisualStyleBackColor = true;
      this.btnCloseListener.Click += new System.EventHandler(this.BtnCloseListener_Click);
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(196, 42);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 10;
      this.button1.Text = "Status";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Visible = false;
      this.button1.Click += new System.EventHandler(this.Button1_Click);
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(16, 506);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(35, 13);
      this.label1.TabIndex = 11;
      this.label1.Text = "label1";
      // 
      // btnNewTagfile
      // 
      this.btnNewTagfile.Location = new System.Drawing.Point(554, 38);
      this.btnNewTagfile.Name = "btnNewTagfile";
      this.btnNewTagfile.Size = new System.Drawing.Size(130, 23);
      this.btnNewTagfile.TabIndex = 12;
      this.btnNewTagfile.Text = "New Tagfile";
      this.btnNewTagfile.UseVisualStyleBackColor = true;
      this.btnNewTagfile.Click += new System.EventHandler(this.BtnNewTagfile_Click);
      // 
      // chkDump
      // 
      this.chkDump.AutoSize = true;
      this.chkDump.Location = new System.Drawing.Point(284, 73);
      this.chkDump.Name = "chkDump";
      this.chkDump.Size = new System.Drawing.Size(141, 17);
      this.chkDump.TabIndex = 13;
      this.chkDump.Text = "Dump Datapacket to file";
      this.chkDump.UseVisualStyleBackColor = true;
      // 
      // chkDumpContent
      // 
      this.chkDumpContent.AutoSize = true;
      this.chkDumpContent.Location = new System.Drawing.Point(431, 71);
      this.chkDumpContent.Name = "chkDumpContent";
      this.chkDumpContent.Size = new System.Drawing.Size(94, 17);
      this.chkDumpContent.TabIndex = 14;
      this.chkDumpContent.Text = "Dump Content";
      this.chkDumpContent.UseVisualStyleBackColor = true;
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(214, 19);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(31, 13);
      this.label21.TabIndex = 51;
      this.label21.Text = "TCIP";
      // 
      // txtTCIP
      // 
      this.txtTCIP.Location = new System.Drawing.Point(251, 16);
      this.txtTCIP.Name = "txtTCIP";
      this.txtTCIP.Size = new System.Drawing.Size(100, 20);
      this.txtTCIP.TabIndex = 50;
      this.txtTCIP.Text = "127.0.0.1";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(370, 18);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(26, 13);
      this.label2.TabIndex = 52;
      this.label2.Text = "Port";
      // 
      // btnDump
      // 
      this.btnDump.Location = new System.Drawing.Point(554, 67);
      this.btnDump.Name = "btnDump";
      this.btnDump.Size = new System.Drawing.Size(130, 23);
      this.btnDump.TabIndex = 53;
      this.btnDump.Text = "Dump to Log";
      this.btnDump.UseVisualStyleBackColor = true;
      this.btnDump.Click += new System.EventHandler(this.BtnDump_Click);
      // 
      // btnThread
      // 
      this.btnThread.Location = new System.Drawing.Point(19, 67);
      this.btnThread.Name = "btnThread";
      this.btnThread.Size = new System.Drawing.Size(75, 23);
      this.btnThread.TabIndex = 54;
      this.btnThread.Text = "Thread";
      this.btnThread.UseVisualStyleBackColor = true;
      this.btnThread.Click += new System.EventHandler(this.BtnThread_Click);
      // 
      // backgroundWorker1
      // 
      this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
      this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker1_ProgressChanged);
      this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1_RunWorkerCompleted);
      // 
      // btnCancel
      // 
      this.btnCancel.Location = new System.Drawing.Point(100, 67);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.TabIndex = 55;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // btnInterface
      // 
      this.btnInterface.Location = new System.Drawing.Point(19, 93);
      this.btnInterface.Name = "btnInterface";
      this.btnInterface.Size = new System.Drawing.Size(106, 23);
      this.btnInterface.TabIndex = 56;
      this.btnInterface.Text = "Start Interface";
      this.btnInterface.UseVisualStyleBackColor = true;
      this.btnInterface.Click += new System.EventHandler(this.BtnInterface_Click);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1022, 572);
      this.Controls.Add(this.btnInterface);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnThread);
      this.Controls.Add(this.btnDump);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label21);
      this.Controls.Add(this.txtTCIP);
      this.Controls.Add(this.chkDumpContent);
      this.Controls.Add(this.chkDump);
      this.Controls.Add(this.btnNewTagfile);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.btnCloseListener);
      this.Controls.Add(this.btnCloseTagFile);
      this.Controls.Add(this.txtPort);
      this.Controls.Add(this.btnOpenPort);
      this.Controls.Add(this.btnWrite);
      this.Controls.Add(this.btnTagfile);
      this.Controls.Add(this.btnMove);
      this.Controls.Add(this.btnClear);
      this.Controls.Add(this.listBox1);
      this.Controls.Add(this.btnProcessTrack);
      this.Name = "Form1";
      this.Text = "Megalodon Server";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button btnProcessTrack;
    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.Button btnMove;
    private System.Windows.Forms.Button btnTagfile;
    private System.Windows.Forms.Button btnWrite;
    private System.Windows.Forms.Button btnOpenPort;
    private System.Windows.Forms.TextBox txtPort;
    private System.Windows.Forms.Button btnCloseTagFile;
    private System.Windows.Forms.Button btnCloseListener;
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnNewTagfile;
    private System.Windows.Forms.CheckBox chkDump;
    private System.Windows.Forms.CheckBox chkDumpContent;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.TextBox txtTCIP;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnDump;
    private System.Windows.Forms.Button btnThread;
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnInterface;
  }
}

