using log4net;
using System;
using System.Reflection;
using VSS.Geofence.Processor.Interfaces;


namespace VSS.Geofence.Processor
{
  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IGeofenceProcessor _geofenceProcessor;

    public ServiceController(IGeofenceProcessor GeofenceProcessor)
    {
      _geofenceProcessor = GeofenceProcessor;
    }

    public bool Start()
    {
      try
      {
        _geofenceProcessor.Process();
      }
      catch (Exception ex)
      {
        Log.Info(string.Format("Failed to start Geofence Processor.. \n {0} \n {1}", ex.Message, ex.StackTrace));
        return false;
      }
      Log.Info("Geofence Processor has been Started");
      return true;
    }

    public void Stop()
    {
      _geofenceProcessor.Stop();
      Log.InfoFormat("GeofenceProcessor has been Stopped");
    }

    public void Error()
    {
      Log.InfoFormat("GeofenceProcessor has thrown an error");
    }
  }
}
