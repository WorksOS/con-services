namespace TagFileUtility
{
  partial class UndoCheckout
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
      this.txtUserName = new System.Windows.Forms.TextBox();
      this.txtOrg = new System.Windows.Forms.TextBox();
      this.txtPassword = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.txtOutput = new System.Windows.Forms.TextBox();
      this.btnGo = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // txtUserName
      // 
      this.txtUserName.Location = new System.Drawing.Point(117, 19);
      this.txtUserName.Name = "txtUserName";
      this.txtUserName.Size = new System.Drawing.Size(203, 20);
      this.txtUserName.TabIndex = 0;
      // 
      // txtOrg
      // 
      this.txtOrg.Location = new System.Drawing.Point(117, 45);
      this.txtOrg.Name = "txtOrg";
      this.txtOrg.Size = new System.Drawing.Size(203, 20);
      this.txtOrg.TabIndex = 1;
      // 
      // txtPassword
      // 
      this.txtPassword.Location = new System.Drawing.Point(117, 71);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.Size = new System.Drawing.Size(203, 20);
      this.txtPassword.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(28, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(60, 13);
      this.label1.TabIndex = 3;
      this.label1.Text = "User Name";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(28, 52);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(66, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "Organization";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(28, 78);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(53, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "Password";
      // 
      // txtOutput
      // 
      this.txtOutput.Location = new System.Drawing.Point(12, 107);
      this.txtOutput.Multiline = true;
      this.txtOutput.Name = "txtOutput";
      this.txtOutput.ReadOnly = true;
      this.txtOutput.Size = new System.Drawing.Size(724, 391);
      this.txtOutput.TabIndex = 6;
      // 
      // btnGo
      // 
      this.btnGo.Location = new System.Drawing.Point(376, 69);
      this.btnGo.Name = "btnGo";
      this.btnGo.Size = new System.Drawing.Size(75, 23);
      this.btnGo.TabIndex = 7;
      this.btnGo.Text = "Go";
      this.btnGo.UseVisualStyleBackColor = true;
      this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
      // 
      // UndoCheckout
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScroll = true;
      this.ClientSize = new System.Drawing.Size(747, 510);
      this.Controls.Add(this.btnGo);
      this.Controls.Add(this.txtOutput);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.txtPassword);
      this.Controls.Add(this.txtOrg);
      this.Controls.Add(this.txtUserName);
      this.Name = "UndoCheckout";
      this.Text = "Cancel Checkout";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtUserName;
    private System.Windows.Forms.TextBox txtOrg;
    private System.Windows.Forms.TextBox txtPassword;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox txtOutput;
    private System.Windows.Forms.Button btnGo;
  }
}

