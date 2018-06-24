using VSS.TRex.Cells;
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

      Assert.True(rec.MeasuredCMV == CellPass.NullCCV);
      Assert.True(rec.TargetCMV == CellPass.NullCCV);
      Assert.True(rec.PreviousMeasuredCMV == CellPass.NullCCV);
      Assert.True(rec.PreviousTargetCMV == CellPass.NullCCV);
    }
  }
}
