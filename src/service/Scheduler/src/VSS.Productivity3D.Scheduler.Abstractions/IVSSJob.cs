using System;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Scheduler.Abstractions
{
  public interface IVSSJob
  {
    Task Setup(object o);
    Task Run(object o);
    Task TearDown(object o);
  }
}
