using VSS.TRex.Profiling;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_ProfileCell : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_ProfileCell_Simple()
    {
      SimpleBinarizableInstanceTester.TestClassEx<ProfileCell>("Empty ProfileCell not same after round trip serialisation");
    }
  }
}
