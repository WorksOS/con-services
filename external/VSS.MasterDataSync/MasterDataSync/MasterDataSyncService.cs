using System;
using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using TaskScheduler = VSS.Nighthawk.MasterDataSync.Implementation.TaskScheduler;

namespace VSS.Nighthawk.MasterDataSync
{
  internal class MasterDataSyncService 
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	  private readonly TaskScheduler _taskScheduler;
	  public MasterDataSyncService(TaskScheduler taskScheduler)
	  {
		  _taskScheduler = taskScheduler;
	  }

    public bool Start()
    {
      Log.IfInfo("MasterDataSync service is starting up..");
      try
      {
        AppDomain.CurrentDomain.UnhandledException += UnexpectedExceptionHandler;
        if (_taskScheduler.Start())
        {
          Log.IfInfo(string.Format("MasterDataSyncService started successfully.."));
          return true;
        }
        Log.IfInfo(string.Format("MasterDataSyncService not started.."));
        return false;
      }
      catch (Exception ex)
      {
        Log.IfInfo(string.Format("Failed to start MasterDataSyncService.. \n {0} \n {1}",ex.Message,ex.StackTrace));
        return false;
      }
    }

    private void UnexpectedExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
      Log.IfInfo(string.Format("Fatal error. MasterDataSyncService IsTerminating = {0}. Exception: {1}", e.IsTerminating, e.ExceptionObject));
    }

    public bool Stop()
    {
      Log.IfInfo("MasterDataSync service is stopping..");
      try
      {
        _taskScheduler.Stop();
        Log.IfInfo(string.Format("MasterDataSyncService stopped successfully.."));
        return true;
      }
      catch (Exception ex)
      {
        Log.IfInfo(string.Format("Failed to stop MasterDataSyncService.. \n {0} \n {1}",ex.Message, ex.StackTrace));
        throw;
      }
    }
  }
}