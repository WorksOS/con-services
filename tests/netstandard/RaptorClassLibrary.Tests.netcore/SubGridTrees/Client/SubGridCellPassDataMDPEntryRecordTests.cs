
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Types;
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

      Assert.True(rec.MeasuredMDP == CellPass.NullMDP);
      Assert.True(rec.TargetMDP == CellPass.NullMDP);
    }
  }
}
