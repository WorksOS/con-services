using System;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IJobFactory
  {
    IJob GetJob(Guid jobUid);

    void RegisterJob(Guid uid, Type type);
  }
}
