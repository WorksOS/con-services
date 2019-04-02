using System;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IJob
  {
    Guid VSSJobUid { get; }
    Task Setup(object o);
    Task Run(object o);
    Task TearDown(object o);
  }
}
