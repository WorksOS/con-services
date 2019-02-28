using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace VSS.Trex.ConnectedSiteGateway.Tests
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
