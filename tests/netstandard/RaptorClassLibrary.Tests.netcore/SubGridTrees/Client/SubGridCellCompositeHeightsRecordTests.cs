using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using Xunit;

namespace RaptorClassLibrary.Tests.netcore.SubGridTrees.Client
{
  /// <summary>
  /// Includes tests not covered in GenericClientLeafSibgriTests
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

      Assert.True(rec.FirstHeightTime == DateTime.MinValue.ToBinary());
      Assert.True(rec.LastHeightTime == DateTime.MinValue.ToBinary());
      Assert.True(rec.LowestHeightTime == DateTime.MinValue.ToBinary());
      Assert.True(rec.HighestHeightTime == DateTime.MinValue.ToBinary());
    }
  }
}
