using System;
using System.Threading.Tasks;
using VSS.Productivity3D.Scheduler.Abstractions;

namespace VSS.Productivity3D.Scheduler.Tests
{
  public class MockDxfTileGenerationJob : IVSSJob
  {
    public event EventHandler SetupInvoked;
    public event EventHandler RunInvoked;
    public event EventHandler TearDownInvoked;

    public Task Setup(object o)
    {
      SetupInvoked?.Invoke(this, null);
      return Task.FromResult(true);
    }

    public Task Run(object o)
    {
      RunInvoked?.Invoke(this, null);
      return Task.FromResult(true);
    }

    public Task TearDown(object o)
    {
      TearDownInvoked?.Invoke(this, null);
      return Task.FromResult(true);
    }
  }
}
