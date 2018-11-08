using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataPassCountEntryRecordTests
  {
    [Fact]
    public void Test_SubGridCellPassDataPassCountEntryRecord_Creation()
    {
      SubGridCellPassDataPassCountEntryRecord rec = new SubGridCellPassDataPassCountEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredPassCount == CellPassConsts.NullPassCountValue);
      Assert.True(rec.TargetPassCount == CellPassConsts.NullPassCountValue);
    }
  }
}
