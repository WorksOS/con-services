namespace VSS.Nighthawk.NHOPSvc
{
  partial class ProjectInstaller
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

    #region Component Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
      this.ReportEventServiceInstaller = new System.ServiceProcess.ServiceInstaller();
      // 
      // serviceProcessInstaller1
      // 
      this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
      this.serviceProcessInstaller1.Password = null;
      this.serviceProcessInstaller1.Username = null;
      // 
      // ReportEventServiceInstaller
      // 
      this.ReportEventServiceInstaller.Description = "Processes events for the NH_OP database";
      this.ReportEventServiceInstaller.ServiceName = "_NHOPSvc";
      this.ReportEventServiceInstaller.ServicesDependedOn = new string[] {
        "MSMQ"};
      this.ReportEventServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
      // 
      // ProjectInstaller
      // 
      this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.ReportEventServiceInstaller});

    }

    #endregion

    private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
    private System.ServiceProcess.ServiceInstaller ReportEventServiceInstaller;
  }
}