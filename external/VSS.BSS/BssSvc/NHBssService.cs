using System;
using System.Security.Principal;
using System.ServiceProcess;
using log4net;
using VSS.Nighthawk.NHBssSvc.BSSEndPoints;
using VSS.Hosted.VLCommon;

namespace VSS.Nighthawk.NHBssSvc
{
  /// <summary>
  /// The Windows Service service implementation, for start/stop event handling.
  /// 
  /// Starts and stops the BSS Service <see cref="NHBssMessageProcessor"/>.
  /// </summary>
  public partial class NHBssService : ServiceBase
  {
    #region Variables
    
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly bool StartAppServices = Environment.MachineName.ToLower().Contains(BssSvcSettings.Default.AppServerContains.ToLower());
    private static readonly bool StartWebServices = Environment.MachineName.ToLower().Contains(BssSvcSettings.Default.WebServerContains.ToLower());

    #endregion

    #region Windows Service Start/Stop

    protected override void OnStart(string[] args)
    {
      Log.IfInfo("_NHBssSvc starting up...");

      AppDomain.CurrentDomain.UnhandledException += UnexpectedExceptionHandler;

      try
      {
        Start();
      }
      catch (Exception ex)
      {
        Log.IfFatal("An essential NHBss hosted service failed to start up. The hosting service is terminating.",ex);
        throw ex;
      }
    }

    protected override void OnStop()
    {
      Log.IfInfo("_NHBssSvc stopping...");

      try
      {
        End();
      }
      catch (Exception ex)
      {
        Log.IfError("A hosted NHBss svc failed to stop successfully.", ex);
      }
    }
    #endregion

    #region Implementation

    private static void UnexpectedExceptionHandler(object obj, UnhandledExceptionEventArgs e)
    {
      Log.FatalFormat("Fatal error. NHBssSvc IsTerminating = {0}. Exception: {1}", e.IsTerminating, e.ExceptionObject);
    }

    private void Start()
    {
      AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

	    bool matchNeither = !StartWebServices && !StartAppServices;
			if (matchNeither)
	    {
				Log.WarnFormat("Machine name {0} does not match either web or app. Starting both web and app services.", Environment.MachineName.ToLower());
	    }

      if (StartWebServices || matchNeither)
      {
				Log.IfInfo("BSS Endpoint Svc Started");
        NHBssEndPointSvc.Start();
      }

      if (StartAppServices || matchNeither)
      {
        NHBssMessageProcessor.Start();
        Log.IfInfo("BSS MessageProcessor Started");

        NHBSSResponseProcessor.Start();
        Log.IfInfo("BSS ResponseProcessor Started");
      }
    }

    private void End()
    {
      if (StartAppServices || (!StartWebServices && !StartAppServices))
      {
        NHBssMessageProcessor.Stop();
        Log.IfInfo("BSS MessageProcessor Stopped");

        NHBSSResponseProcessor.Stop();
        Log.IfInfo("BSS ResponseProcessor Stopped");
      }

      if (StartWebServices || (!StartWebServices && !StartAppServices))
      {
        NHBssEndPointSvc.Stop();
      }
    }

    #endregion
  }
}
