using System;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IVSSJobFactory
  {
    IVSSJob GetJob(Guid jobUid);

    void RegisterJob(Guid uid, Type type);
  }
}