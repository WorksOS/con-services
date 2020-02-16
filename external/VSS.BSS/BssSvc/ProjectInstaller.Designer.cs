namespace VSS.Nighthawk.NHBssSvc
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
      this.NHBssServiceInstaller = new System.ServiceProcess.ServiceInstaller();
      this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
      // 
      // NHBssServiceInstaller
      // 
      this.NHBssServiceInstaller.Description = "Processes events for the NH_Data database";
      this.NHBssServiceInstaller.ServiceName = "_NHBssSvc";
      this.NHBssServiceInstaller.ServicesDependedOn = new string[] {
        "MSMQ"};
      this.NHBssServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
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
            this.NHBssServiceInstaller,
            this.serviceProcessInstaller1});

    }

    #endregion

    private System.ServiceProcess.ServiceInstaller NHBssServiceInstaller;
    private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
  }
}