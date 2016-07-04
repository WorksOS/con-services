using log4net;
using System;
using System.Reflection;

namespace VSS.MasterData.Common.Processor
{
  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IProcessor _processor;

    public ServiceController(IProcessor processor)
    {
      _processor = processor;
    }

    public bool Start()
    {
      try
      {
        _processor.Process();
      }
      catch (Exception ex)
      {
        Log.Info(string.Format("Failed to start Processor.. \n {0} \n {1}", ex.Message, ex.StackTrace));
        return false;
      }
      Log.Info("Processor has been Started");
      return true;
    }

    public void Stop()
    {
      _processor.Stop();
      Log.InfoFormat("Processor has been Stopped");
    }

    public void Error()
    {
      Log.InfoFormat("Processor has thrown an error");
    }
  }
}
