using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IJobCallback
  {
    JobRequest ExecuteCallback(object parameter, object context);
  }
}
