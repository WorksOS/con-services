using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataCMVEntryRecordTests
  {
    [Fact]
    public void Test_SubGridCellPassDataCMVEntryRecord_Creation()
    {
      SubGridCellPassDataCMVEntryRecord rec = new SubGridCellPassDataCMVEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredCMV == CellPassConsts.NullCCV);
      Assert.True(rec.TargetCMV == CellPassConsts.NullCCV);
      Assert.True(rec.PreviousMeasuredCMV == CellPassConsts.NullCCV);
      Assert.True(rec.PreviousTargetCMV == CellPassConsts.NullCCV);
    }
  }
}
