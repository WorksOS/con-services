using System;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IVSSJobRunner
  {
    void RegisterJob(Guid uid, Type type);
    Task<ContractExecutionResult> RunJob(JobRequest request);
  }
}
