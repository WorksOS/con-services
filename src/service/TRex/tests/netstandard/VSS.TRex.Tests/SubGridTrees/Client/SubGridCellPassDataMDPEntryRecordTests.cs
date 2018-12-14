using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataMDPEntryRecordTests
  {
    [Fact]
    public void Test_SubGridCellPassDataMDPEntryRecord_Creation()
    {
      SubGridCellPassDataMDPEntryRecord rec = new SubGridCellPassDataMDPEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredMDP == CellPassConsts.NullMDP);
      Assert.True(rec.TargetMDP == CellPassConsts.NullMDP);
    }
  }
}
