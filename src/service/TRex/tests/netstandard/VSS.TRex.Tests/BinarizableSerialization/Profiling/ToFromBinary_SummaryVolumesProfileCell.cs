using FluentAssertions;
using VSS.TRex.Profiling;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Profiling
{
  public class ToFromBinary_SummaryVolumesProfileCell
  {
    [Fact]
    public void Test_SummaryVolumesProfileCell_Simple()
    {
      SimpleBinarizableInstanceTester.TestClassEx<SummaryVolumeProfileCell>("Empty SummaryVolumes ProfileCell not same after round trip serialisation");
    }

    [Fact]
    public void Test_SummaryVolumesProfileCell_NotSimple()
    {
      var pc = new SummaryVolumeProfileCell
      {
        LastCellPassElevation1 = (float)1.0,
        LastCellPassElevation2 = (float)2.0
      }; 
      var result = SimpleBinarizableInstanceTester.TestClassEx(pc,"SummaryVolumes ProfileCell not same after round trip serialisation");
      result.member.LastCellPassElevation1.Should().Be((float)1.0);
      result.member.LastCellPassElevation2.Should().Be((float)2.0);
    }

  }
}
