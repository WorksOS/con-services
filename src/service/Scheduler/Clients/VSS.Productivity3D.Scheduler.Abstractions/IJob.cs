using System;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IJob
  {
    Guid VSSJobUid { get; }
    Task Setup(object o, object context = null);
    Task Run(object o, object context = null);
    Task TearDown(object o, object context = null);
  }
}
