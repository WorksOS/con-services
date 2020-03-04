namespace MegalodonClient
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
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
      this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
      this.label20 = new System.Windows.Forms.Label();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label21 = new System.Windows.Forms.Label();
      this.txtTCIP = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.txtPort = new System.Windows.Forms.TextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.chkDontMove = new System.Windows.Forms.CheckBox();
      this.chkVaryBOG = new System.Windows.Forms.CheckBox();
      this.txtEpochLimit = new System.Windows.Forms.TextBox();
      this.btnDefDimensions = new System.Windows.Forms.Button();
      this.label16 = new System.Windows.Forms.Label();
      this.txtESteps = new System.Windows.Forms.TextBox();
      this.label15 = new System.Windows.Forms.Label();
      this.txtSleep = new System.Windows.Forms.TextBox();
      this.txtSerial = new System.Windows.Forms.TextBox();
      this.label14 = new System.Windows.Forms.Label();
      this.txtDesign = new System.Windows.Forms.TextBox();
      this.label10 = new System.Windows.Forms.Label();
      this.txtLong = new System.Windows.Forms.TextBox();
      this.label7 = new System.Windows.Forms.Label();
      this.txtLat = new System.Windows.Forms.TextBox();
      this.label9 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.txtNSteps = new System.Windows.Forms.TextBox();
      this.listBox1 = new System.Windows.Forms.ListBox();
      this.chkUseCustomDate = new System.Windows.Forms.CheckBox();
      this.label6 = new System.Windows.Forms.Label();
      this.dtpStart = new System.Windows.Forms.DateTimePicker();
      this.chkTwoEpochsOnly = new System.Windows.Forms.CheckBox();
      this.chkSOH = new System.Windows.Forms.CheckBox();
      this.cboType = new System.Windows.Forms.ComboBox();
      this.chkVaryHgt = new System.Windows.Forms.CheckBox();
      this.btnStop = new System.Windows.Forms.Button();
      this.btnRunSim = new System.Windows.Forms.Button();
      this.txtVName = new System.Windows.Forms.TextBox();
      this.label18 = new System.Windows.Forms.Label();
      this.txtStartHgt = new System.Windows.Forms.TextBox();
      this.label19 = new System.Windows.Forms.Label();
      this.txtRN = new System.Windows.Forms.TextBox();
      this.label11 = new System.Windows.Forms.Label();
      this.txtRE = new System.Windows.Forms.TextBox();
      this.label12 = new System.Windows.Forms.Label();
      this.txtNorth = new System.Windows.Forms.TextBox();
      this.label13 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.txtLN = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.txtLE = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.txtEast = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.statusStrip1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // statusStrip1
      // 
      this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lblStatus});
      this.statusStrip1.Location = new System.Drawing.Point(0, 590);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(813, 22);
      this.statusStrip1.TabIndex = 1;
      this.statusStrip1.Text = "LastAction:";
      // 
      // toolStripStatusLabel1
      // 
      this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
      this.toolStripStatusLabel1.Size = new System.Drawing.Size(66, 17);
      this.toolStripStatusLabel1.Text = "LastAction:";
      // 
      // lblStatus
      // 
      this.lblStatus.Name = "lblStatus";
      this.lblStatus.Size = new System.Drawing.Size(695, 17);
      this.lblStatus.Text = "Sim works by moving left to right, north, then right to left.  Pattern then repea" +
    "ts. Finally after x steps north, the whole cycle repeats.";
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(633, 126);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(56, 13);
      this.label20.TabIndex = 42;
      this.label20.Text = "Responce";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label21);
      this.groupBox1.Controls.Add(this.txtTCIP);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.txtPort);
      this.groupBox1.Location = new System.Drawing.Point(16, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(783, 66);
      this.groupBox1.TabIndex = 60;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Server Details";
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(168, 31);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(31, 13);
      this.label21.TabIndex = 51;
      this.label21.Text = "TCIP";
      // 
      // txtTCIP
      // 
      this.txtTCIP.Location = new System.Drawing.Point(205, 28);
      this.txtTCIP.Name = "txtTCIP";
      this.txtTCIP.Size = new System.Drawing.Size(100, 20);
      this.txtTCIP.TabIndex = 50;
      this.txtTCIP.Text = "127.0.0.1";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 31);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(26, 13);
      this.label1.TabIndex = 5;
      this.label1.Text = "Port";
      // 
      // txtPort
      // 
      this.txtPort.Location = new System.Drawing.Point(45, 28);
      this.txtPort.Name = "txtPort";
      this.txtPort.Size = new System.Drawing.Size(100, 20);
      this.txtPort.TabIndex = 4;
      this.txtPort.Text = "1500";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.chkDontMove);
      this.groupBox2.Controls.Add(this.chkVaryBOG);
      this.groupBox2.Controls.Add(this.txtEpochLimit);
      this.groupBox2.Controls.Add(this.btnDefDimensions);
      this.groupBox2.Controls.Add(this.label16);
      this.groupBox2.Controls.Add(this.txtESteps);
      this.groupBox2.Controls.Add(this.label15);
      this.groupBox2.Controls.Add(this.txtSleep);
      this.groupBox2.Controls.Add(this.txtSerial);
      this.groupBox2.Controls.Add(this.label14);
      this.groupBox2.Controls.Add(this.txtDesign);
      this.groupBox2.Controls.Add(this.label10);
      this.groupBox2.Controls.Add(this.txtLong);
      this.groupBox2.Controls.Add(this.label7);
      this.groupBox2.Controls.Add(this.txtLat);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.txtNSteps);
      this.groupBox2.Controls.Add(this.listBox1);
      this.groupBox2.Controls.Add(this.chkUseCustomDate);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.dtpStart);
      this.groupBox2.Controls.Add(this.chkTwoEpochsOnly);
      this.groupBox2.Controls.Add(this.chkSOH);
      this.groupBox2.Controls.Add(this.cboType);
      this.groupBox2.Controls.Add(this.chkVaryHgt);
      this.groupBox2.Controls.Add(this.btnStop);
      this.groupBox2.Controls.Add(this.btnRunSim);
      this.groupBox2.Controls.Add(this.txtVName);
      this.groupBox2.Controls.Add(this.label18);
      this.groupBox2.Controls.Add(this.txtStartHgt);
      this.groupBox2.Controls.Add(this.label19);
      this.groupBox2.Controls.Add(this.txtRN);
      this.groupBox2.Controls.Add(this.label11);
      this.groupBox2.Controls.Add(this.txtRE);
      this.groupBox2.Controls.Add(this.label12);
      this.groupBox2.Controls.Add(this.txtNorth);
      this.groupBox2.Controls.Add(this.label13);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.txtLN);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.txtLE);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.txtEast);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Location = new System.Drawing.Point(16, 84);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(783, 492);
      this.groupBox2.TabIndex = 61;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Hex Simulator (Default Location Dimensions)";
      // 
      // chkDontMove
      // 
      this.chkDontMove.AutoSize = true;
      this.chkDontMove.Location = new System.Drawing.Point(122, 399);
      this.chkDontMove.Name = "chkDontMove";
      this.chkDontMove.Size = new System.Drawing.Size(85, 17);
      this.chkDontMove.TabIndex = 106;
      this.chkDontMove.Text = "Stop moving";
      this.chkDontMove.UseVisualStyleBackColor = true;
      // 
      // chkVaryBOG
      // 
      this.chkVaryBOG.AutoSize = true;
      this.chkVaryBOG.Location = new System.Drawing.Point(122, 376);
      this.chkVaryBOG.Name = "chkVaryBOG";
      this.chkVaryBOG.Size = new System.Drawing.Size(73, 17);
      this.chkVaryBOG.TabIndex = 105;
      this.chkVaryBOG.Text = "Vary BOG";
      this.chkVaryBOG.UseVisualStyleBackColor = true;
      // 
      // txtEpochLimit
      // 
      this.txtEpochLimit.Location = new System.Drawing.Point(122, 422);
      this.txtEpochLimit.Name = "txtEpochLimit";
      this.txtEpochLimit.Size = new System.Drawing.Size(37, 20);
      this.txtEpochLimit.TabIndex = 104;
      this.txtEpochLimit.Text = "2";
      // 
      // btnDefDimensions
      // 
      this.btnDefDimensions.Location = new System.Drawing.Point(249, 31);
      this.btnDefDimensions.Name = "btnDefDimensions";
      this.btnDefDimensions.Size = new System.Drawing.Size(109, 23);
      this.btnDefDimensions.TabIndex = 102;
      this.btnDefDimensions.Text = "Reset Defaults";
      this.btnDefDimensions.UseVisualStyleBackColor = true;
      this.btnDefDimensions.Click += new System.EventHandler(this.BtnDefDimensions_Click);
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(36, 300);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(81, 13);
      this.label16.TabIndex = 101;
      this.label16.Text = "Max East Steps";
      // 
      // txtESteps
      // 
      this.txtESteps.Location = new System.Drawing.Point(123, 297);
      this.txtESteps.Name = "txtESteps";
      this.txtESteps.Size = new System.Drawing.Size(46, 20);
      this.txtESteps.TabIndex = 100;
      this.txtESteps.Text = "50";
      // 
      // label15
      // 
      this.label15.AutoSize = true;
      this.label15.Location = new System.Drawing.Point(34, 242);
      this.label15.Name = "label15";
      this.label15.Size = new System.Drawing.Size(87, 13);
      this.label15.TabIndex = 99;
      this.label15.Text = "Epoch Sleep(ms)";
      // 
      // txtSleep
      // 
      this.txtSleep.Location = new System.Drawing.Point(122, 239);
      this.txtSleep.Name = "txtSleep";
      this.txtSleep.Size = new System.Drawing.Size(65, 20);
      this.txtSleep.TabIndex = 98;
      this.txtSleep.Text = "200";
      // 
      // txtSerial
      // 
      this.txtSerial.Location = new System.Drawing.Point(123, 323);
      this.txtSerial.Name = "txtSerial";
      this.txtSerial.Size = new System.Drawing.Size(235, 20);
      this.txtSerial.TabIndex = 97;
      this.txtSerial.Text = "7g2b0b13-5b31-4ac7-b67a-4f16d76c9b30";
      // 
      // label14
      // 
      this.label14.AutoSize = true;
      this.label14.Location = new System.Drawing.Point(30, 326);
      this.label14.Name = "label14";
      this.label14.Size = new System.Drawing.Size(87, 13);
      this.label14.TabIndex = 96;
      this.label14.Text = "Machine Serial #";
      // 
      // txtDesign
      // 
      this.txtDesign.Location = new System.Drawing.Point(286, 269);
      this.txtDesign.Name = "txtDesign";
      this.txtDesign.Size = new System.Drawing.Size(104, 20);
      this.txtDesign.TabIndex = 95;
      this.txtDesign.Text = "DesignA";
      // 
      // label10
      // 
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(287, 253);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(40, 13);
      this.label10.TabIndex = 94;
      this.label10.Text = "Design";
      // 
      // txtLong
      // 
      this.txtLong.Location = new System.Drawing.Point(406, 215);
      this.txtLong.Name = "txtLong";
      this.txtLong.Size = new System.Drawing.Size(97, 20);
      this.txtLong.TabIndex = 93;
      this.txtLong.Text = "-115.020131";
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(407, 199);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(59, 13);
      this.label7.TabIndex = 92;
      this.label7.Text = "Seed Long";
      // 
      // txtLat
      // 
      this.txtLat.Location = new System.Drawing.Point(285, 215);
      this.txtLat.Name = "txtLat";
      this.txtLat.Size = new System.Drawing.Size(104, 20);
      this.txtLat.TabIndex = 91;
      this.txtLat.Text = "36.206979";
      // 
      // label9
      // 
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(286, 199);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(50, 13);
      this.label9.TabIndex = 90;
      this.label9.Text = "Seed Lat";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(33, 272);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(86, 13);
      this.label8.TabIndex = 89;
      this.label8.Text = "Max North Steps";
      // 
      // txtNSteps
      // 
      this.txtNSteps.Location = new System.Drawing.Point(122, 269);
      this.txtNSteps.Name = "txtNSteps";
      this.txtNSteps.Size = new System.Drawing.Size(46, 20);
      this.txtNSteps.TabIndex = 88;
      this.txtNSteps.Text = "10";
      // 
      // listBox1
      // 
      this.listBox1.FormattingEnabled = true;
      this.listBox1.Location = new System.Drawing.Point(535, 48);
      this.listBox1.Name = "listBox1";
      this.listBox1.ScrollAlwaysVisible = true;
      this.listBox1.Size = new System.Drawing.Size(231, 433);
      this.listBox1.TabIndex = 87;
      // 
      // chkUseCustomDate
      // 
      this.chkUseCustomDate.AutoSize = true;
      this.chkUseCustomDate.Location = new System.Drawing.Point(289, 379);
      this.chkUseCustomDate.Name = "chkUseCustomDate";
      this.chkUseCustomDate.Size = new System.Drawing.Size(134, 17);
      this.chkUseCustomDate.TabIndex = 85;
      this.chkUseCustomDate.Text = "Use Custom Start Date";
      this.chkUseCustomDate.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(287, 399);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(90, 13);
      this.label6.TabIndex = 84;
      this.label6.Text = "Tagfile Start Time";
      // 
      // dtpStart
      // 
      this.dtpStart.CustomFormat = "MM/dd/yyyy hh:mm:ss";
      this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
      this.dtpStart.Location = new System.Drawing.Point(287, 415);
      this.dtpStart.Name = "dtpStart";
      this.dtpStart.Size = new System.Drawing.Size(200, 20);
      this.dtpStart.TabIndex = 83;
      this.dtpStart.Value = new System.DateTime(2019, 11, 1, 10, 0, 0, 0);
      // 
      // chkTwoEpochsOnly
      // 
      this.chkTwoEpochsOnly.AutoSize = true;
      this.chkTwoEpochsOnly.Location = new System.Drawing.Point(165, 424);
      this.chkTwoEpochsOnly.Name = "chkTwoEpochsOnly";
      this.chkTwoEpochsOnly.Size = new System.Drawing.Size(84, 17);
      this.chkTwoEpochsOnly.TabIndex = 82;
      this.chkTwoEpochsOnly.Text = "Epochs only";
      this.chkTwoEpochsOnly.UseVisualStyleBackColor = true;
      // 
      // chkSOH
      // 
      this.chkSOH.AutoSize = true;
      this.chkSOH.Checked = true;
      this.chkSOH.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkSOH.Location = new System.Drawing.Point(535, 22);
      this.chkSOH.Name = "chkSOH";
      this.chkSOH.Size = new System.Drawing.Size(134, 17);
      this.chkSOH.TabIndex = 81;
      this.chkSOH.Text = "Show Header Request";
      this.chkSOH.UseVisualStyleBackColor = true;
      // 
      // cboType
      // 
      this.cboType.FormattingEnabled = true;
      this.cboType.Items.AddRange(new object[] {
            "HEX"});
      this.cboType.Location = new System.Drawing.Point(122, 212);
      this.cboType.Name = "cboType";
      this.cboType.Size = new System.Drawing.Size(133, 21);
      this.cboType.TabIndex = 80;
      this.cboType.Text = "HEX";
      // 
      // chkVaryHgt
      // 
      this.chkVaryHgt.AutoSize = true;
      this.chkVaryHgt.Location = new System.Drawing.Point(122, 349);
      this.chkVaryHgt.Name = "chkVaryHgt";
      this.chkVaryHgt.Size = new System.Drawing.Size(184, 17);
      this.chkVaryHgt.TabIndex = 79;
      this.chkVaryHgt.Text = "Vary Height. 10cm over 4m range";
      this.chkVaryHgt.UseVisualStyleBackColor = true;
      // 
      // btnStop
      // 
      this.btnStop.Location = new System.Drawing.Point(103, 31);
      this.btnStop.Name = "btnStop";
      this.btnStop.Size = new System.Drawing.Size(75, 23);
      this.btnStop.TabIndex = 78;
      this.btnStop.Text = "Stop Sim";
      this.btnStop.UseVisualStyleBackColor = true;
      this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
      // 
      // btnRunSim
      // 
      this.btnRunSim.Location = new System.Drawing.Point(22, 31);
      this.btnRunSim.Name = "btnRunSim";
      this.btnRunSim.Size = new System.Drawing.Size(75, 23);
      this.btnRunSim.TabIndex = 77;
      this.btnRunSim.Text = "Run SIM";
      this.btnRunSim.UseVisualStyleBackColor = true;
      this.btnRunSim.Click += new System.EventHandler(this.BtnRunSim_Click);
      // 
      // txtVName
      // 
      this.txtVName.Location = new System.Drawing.Point(123, 182);
      this.txtVName.Name = "txtVName";
      this.txtVName.Size = new System.Drawing.Size(132, 20);
      this.txtVName.TabIndex = 76;
      this.txtVName.Text = "Machine1";
      // 
      // label18
      // 
      this.label18.AutoSize = true;
      this.label18.Location = new System.Drawing.Point(16, 185);
      this.label18.Name = "label18";
      this.label18.Size = new System.Drawing.Size(108, 13);
      this.label18.TabIndex = 75;
      this.label18.Text = "Machine Name (MID)";
      // 
      // txtStartHgt
      // 
      this.txtStartHgt.Location = new System.Drawing.Point(123, 102);
      this.txtStartHgt.Name = "txtStartHgt";
      this.txtStartHgt.Size = new System.Drawing.Size(95, 20);
      this.txtStartHgt.TabIndex = 74;
      this.txtStartHgt.Text = "542.0";
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(41, 105);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(77, 13);
      this.label19.TabIndex = 73;
      this.label19.Text = "Start Height(m)";
      // 
      // txtRN
      // 
      this.txtRN.Location = new System.Drawing.Point(406, 157);
      this.txtRN.Name = "txtRN";
      this.txtRN.Size = new System.Drawing.Size(97, 20);
      this.txtRN.TabIndex = 72;
      this.txtRN.Text = "1163.0";
      // 
      // label11
      // 
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(407, 141);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(105, 13);
      this.label11.TabIndex = 71;
      this.label11.Text = "Right Northing Blade";
      // 
      // txtRE
      // 
      this.txtRE.Location = new System.Drawing.Point(286, 157);
      this.txtRE.Name = "txtRE";
      this.txtRE.Size = new System.Drawing.Size(104, 20);
      this.txtRE.TabIndex = 70;
      this.txtRE.Text = "2744.0";
      // 
      // label12
      // 
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(287, 141);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(100, 13);
      this.label12.TabIndex = 69;
      this.label12.Text = "Right Easting Blade";
      // 
      // txtNorth
      // 
      this.txtNorth.Location = new System.Drawing.Point(123, 154);
      this.txtNorth.Name = "txtNorth";
      this.txtNorth.Size = new System.Drawing.Size(64, 20);
      this.txtNorth.TabIndex = 68;
      this.txtNorth.Text = "3.0";
      // 
      // label13
      // 
      this.label13.AutoSize = true;
      this.label13.Location = new System.Drawing.Point(42, 157);
      this.label13.Name = "label13";
      this.label13.Size = new System.Drawing.Size(72, 13);
      this.label13.TabIndex = 67;
      this.label13.Text = "Step North(m)";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(41, 215);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(75, 13);
      this.label5.TabIndex = 66;
      this.label5.Text = "Machine Type";
      // 
      // txtLN
      // 
      this.txtLN.Location = new System.Drawing.Point(406, 103);
      this.txtLN.Name = "txtLN";
      this.txtLN.Size = new System.Drawing.Size(99, 20);
      this.txtLN.TabIndex = 65;
      this.txtLN.Text = "1166.0";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(407, 87);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(98, 13);
      this.label4.TabIndex = 64;
      this.label4.Text = "Left Northing Blade";
      // 
      // txtLE
      // 
      this.txtLE.Location = new System.Drawing.Point(286, 102);
      this.txtLE.Name = "txtLE";
      this.txtLE.Size = new System.Drawing.Size(104, 20);
      this.txtLE.TabIndex = 63;
      this.txtLE.Text = "2744.0";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(287, 86);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(93, 13);
      this.label3.TabIndex = 62;
      this.label3.Text = "Left Easting Blade";
      // 
      // txtEast
      // 
      this.txtEast.Location = new System.Drawing.Point(123, 128);
      this.txtEast.Name = "txtEast";
      this.txtEast.Size = new System.Drawing.Size(64, 20);
      this.txtEast.TabIndex = 61;
      this.txtEast.Text = "0.34";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(41, 131);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 13);
      this.label2.TabIndex = 60;
      this.label2.Text = "Epoch Step(m)";
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(813, 612);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.label20);
      this.Controls.Add(this.statusStrip1);
      this.Name = "Form1";
      this.Text = "Hex Tagfile Simulator";
      this.statusStrip1.ResumeLayout(false);
      this.statusStrip1.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ListBox listBox1;
    private System.Windows.Forms.CheckBox chkUseCustomDate;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.DateTimePicker dtpStart;
    private System.Windows.Forms.CheckBox chkTwoEpochsOnly;
    private System.Windows.Forms.CheckBox chkSOH;
    private System.Windows.Forms.ComboBox cboType;
    private System.Windows.Forms.CheckBox chkVaryHgt;
    private System.Windows.Forms.Button btnStop;
    private System.Windows.Forms.Button btnRunSim;
    private System.Windows.Forms.TextBox txtVName;
    private System.Windows.Forms.Label label18;
    private System.Windows.Forms.TextBox txtStartHgt;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.TextBox txtRN;
    private System.Windows.Forms.Label label11;
    private System.Windows.Forms.TextBox txtRE;
    private System.Windows.Forms.Label label12;
    private System.Windows.Forms.TextBox txtNorth;
    private System.Windows.Forms.Label label13;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox txtLN;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox txtLE;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txtEast;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox txtNSteps;
    private System.Windows.Forms.TextBox txtLong;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox txtLat;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox txtDesign;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.TextBox txtSerial;
    private System.Windows.Forms.Label label14;
    private System.Windows.Forms.Label label15;
    private System.Windows.Forms.TextBox txtSleep;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.TextBox txtESteps;
    private System.Windows.Forms.Button btnDefDimensions;
    private System.Windows.Forms.TextBox txtEpochLimit;
    private System.Windows.Forms.CheckBox chkVaryBOG;
    private System.Windows.Forms.CheckBox chkDontMove;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox txtTCIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPort;
    }
}

