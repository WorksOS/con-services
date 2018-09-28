using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace VSS.TRex.Gateway.Tests
{
  public class NoConcurrency
  {
    [Fact]
    public void DummyTestForForceNoTestConcurrency()
    {
      Assert.True(true);
    }
  }
}
