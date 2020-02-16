namespace VSS.Nighthawk.NHDataSvc
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
      this.NHDataServiceInstaller = new System.ServiceProcess.ServiceInstaller();
      this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
      // 
      // NHDataServiceInstaller
      // 
      this.NHDataServiceInstaller.Description = "Processes events for the NH_Data database";
      this.NHDataServiceInstaller.ServiceName = "_NHDataSvc";
      this.NHDataServiceInstaller.ServicesDependedOn = new string[] {
        "MSMQ"};
      this.NHDataServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
      // 
      // serviceProcessInstaller1
      // 
      this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
      this.serviceProcessInstaller1.Password = null;
      this.serviceProcessInstaller1.Username = null;
      // 
      // ProjectInstaller
      // 
      this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.NHDataServiceInstaller,
            this.serviceProcessInstaller1});

    }

    #endregion

    private System.ServiceProcess.ServiceInstaller NHDataServiceInstaller;
    private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
  }
}