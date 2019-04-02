using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace VSS.Tpaas.Client.UnitTests
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
