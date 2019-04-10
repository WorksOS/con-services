using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSubgridTests
  /// </summary>
  public class SubGridCellCompositeHeightsRecordTests
  {
    [Fact]
    public void Test_SubGridCellCompositeHeightsRecord_Creation()
    {
      SubGridCellCompositeHeightsRecord rec = new SubGridCellCompositeHeightsRecord();

      rec.Clear();

      Assert.True(rec.FirstHeight == Consts.NullHeight);
      Assert.True(rec.LastHeight == Consts.NullHeight);
      Assert.True(rec.LowestHeight == Consts.NullHeight);
      Assert.True(rec.HighestHeight == Consts.NullHeight);

      var minTicks = Consts.MIN_DATETIME_AS_UTC.Ticks;
      Assert.True(rec.FirstHeightTime == minTicks);
      Assert.True(rec.LastHeightTime == minTicks);
      Assert.True(rec.LowestHeightTime == minTicks);
      Assert.True(rec.HighestHeightTime == minTicks);
    }
  }
}
