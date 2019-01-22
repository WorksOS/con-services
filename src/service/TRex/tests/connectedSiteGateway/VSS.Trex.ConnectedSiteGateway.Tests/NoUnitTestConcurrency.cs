using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace TAGFiles.Tests
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
