using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
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

      Assert.True(rec.FirstHeightTime == DateTime.MinValue.Ticks);
      Assert.True(rec.LastHeightTime == DateTime.MinValue.Ticks);
      Assert.True(rec.LowestHeightTime == DateTime.MinValue.Ticks);
      Assert.True(rec.HighestHeightTime == DateTime.MinValue.Ticks);
    }
  }
}
