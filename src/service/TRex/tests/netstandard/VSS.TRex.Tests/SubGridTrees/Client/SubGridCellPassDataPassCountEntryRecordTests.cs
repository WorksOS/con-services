using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataPassCountEntryRecordTests
  {
    [Fact]
    public void Test_SubGridCellPassDataPassCountEntryRecord_CreationDefault()
    {
      var rec = new SubGridCellPassDataPassCountEntryRecord();

      Assert.True(rec.MeasuredPassCount == 0);
      Assert.True(rec.TargetPassCount == 0);
    }

    [Fact]
    public void Test_SubGridCellPassDataPassCountEntryRecord_CreationWithArgs()
    {
      var rec = new SubGridCellPassDataPassCountEntryRecord(123, 456);

      Assert.True(rec.MeasuredPassCount == 123);
      Assert.True(rec.TargetPassCount == 456);
    }

    [Fact]
    public void Test_SubGridCellPassDataPassCountEntryRecord_Clear()
    {
      var rec = new SubGridCellPassDataPassCountEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredPassCount == CellPassConsts.NullPassCountValue);
      Assert.True(rec.TargetPassCount == CellPassConsts.NullPassCountValue);
    }

  }
}
