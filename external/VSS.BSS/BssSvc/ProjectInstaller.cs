using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using log4net;
using VSS.Hosted.VLCommon;


namespace VSS.Nighthawk.NHBssSvc
{
  /// <summary>
  /// Standard installaer class, to facilitate installation of the Windows Service.
  /// </summary>
  [RunInstaller(true)]
  public partial class ProjectInstaller : Installer
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    public ProjectInstaller()
    {
      InitializeComponent();
    }

    public override void Install(IDictionary stateSaver)
    {
      try
      {
        base.Install(stateSaver);
      }
      catch (Exception exception1)
      {
        Console.Out.WriteLine(exception1.Message);
      }

      string description = "Processes Info For BSS";

      AppServiceInstaller.RecoveryAction[] action = GetRecoveryAction();

      if (!AppServiceInstaller.ServiceDescription("_NHBssSvc", description, action))
      {
        throw new ApplicationException("Unable to set the service's description");
      }
    }

    private AppServiceInstaller.RecoveryAction[] GetRecoveryAction()
    {
      if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("Service.RecoveryActions")))
      {
        string val = ConfigurationManager.AppSettings["Service.RecoveryActions"];
        char[] actionSeparators = new char[] { '/', ';' };

        if (val != null)
        {
          // Format of this string is {action/delay}+ so 'restart/30000/restart/30000/reboot/60000'
          // would set the first and second actions to restart the service after 30 seconds and
          // the third recovery action to reboot the machine after 60 seconds.  Note that we allow
          // ; as well so you could do 'restart/30000;restart/30000;reboot/60000'.

          string[] opts = val.Split(actionSeparators, 6);

          if ((opts.Length & 1) != 0)
          {
            throw new ApplicationException(String.Format("'Service.RecoveryActions' is of the wrong format: {0}",
               val));
          }

          if (opts.Length > 0)
          {
            AppServiceInstaller.RecoveryAction[] actions = new AppServiceInstaller.RecoveryAction[opts.Length / 2];
            int i = 0;

            while (i < actions.Length)
            {
              actions[i].Action = (AppServiceInstaller.ActionType)Enum.Parse(typeof(AppServiceInstaller.ActionType), opts[2 * i], true);
              actions[i].DelayMs = Convert.ToInt32(opts[2 * i + 1]);

              i++;
            }

            return actions;
          }
        }
      }

      return null;
    }
  }
}
