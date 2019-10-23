using System;
using System.Collections.Generic;

namespace VSS.Productivity3D.Scheduler.WebAPI.JobRunner
{
  public interface IJobRegistrationManager
  {
    string GetQueueName(Type t);
    string GetQueueName(Guid guid);
    void RegisterJob(Guid uid, Type type);
    Dictionary<Guid, Type> ResolveVssJobs();
    string GetJobName(Guid jobGuid);
  }
}
