using log4net;
using System;
using System.Reflection;
using VSS.Project.Processor.Interfaces;

namespace VSS.Project.Processor
{
  public class ServiceController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IProjectProcessor _projectProcessor;

    public ServiceController(IProjectProcessor ProjectProcessor)
    {
      _projectProcessor = ProjectProcessor;
    }

    public bool Start()
    {
      try
      {
        _projectProcessor.Process();
      }
      catch (Exception ex)
      {
        Log.Info(string.Format("Failed to start Project Processor.. \n {0} \n {1}", ex.Message, ex.StackTrace));
        return false;
      }
      Log.Info("Project Processor has been Started");
      return true;
    }

    public void Stop()
    {
      _projectProcessor.Stop();
      Log.InfoFormat("ProjectProcessor has been Stopped");
    }

    public void Error()
    {
      Log.InfoFormat("ProjectProcessor has thrown an error");
    }
  }
}
