using System;
using System.Threading.Tasks;
using VSS.Productivity3D.Scheduler.Abstractions;

namespace VSS.Productivity3D.Scheduler.Tests
{
  public class MockTileGenerationJob : IJob
  {
    public event EventHandler SetupInvoked;
    public event EventHandler RunInvoked;
    public event EventHandler TearDownInvoked;

    public Guid VSSJobUid => Guid.Parse("5b4b0f10-ec2c-4308-a282-7a538dc087f0");

    public Task Setup(object o, object context)
    {
      SetupInvoked?.Invoke(this, null);
      return Task.FromResult(true);
    }

    public Task Run(object o, object context)
    {
      RunInvoked?.Invoke(this, null);
      return Task.FromResult(true);
    }

    public Task TearDown(object o, object context)
    {
      TearDownInvoked?.Invoke(this, null);
      return Task.FromResult(true);
    }
  }
}
