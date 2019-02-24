using VSS.TRex.Profiling;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_ProfileCellBase
  {
    [Fact]
    public void Test_ProfileCell_Simple()
    {
      SimpleBinarizableInstanceTester.TestClassEx<ProfileCellBase>("Empty ProfileCellBase not same after round trip serialisation");
    }
  }
}
