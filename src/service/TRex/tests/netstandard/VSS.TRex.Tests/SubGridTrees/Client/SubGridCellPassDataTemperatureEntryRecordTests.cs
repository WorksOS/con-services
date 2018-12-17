using VSS.TRex.Common.CellPasses;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataTemperatureEntryRecordTests
  {
    [Fact]
    public void Test_SubGridCellPassDataTemperatureEntryRecord_Creation()
    {
      SubGridCellPassDataTemperatureEntryRecord rec = new SubGridCellPassDataTemperatureEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredTemperature == CellPassConsts.NullMaterialTemperatureValue);
      Assert.True(rec.TemperatureLevels.Min == CellPassConsts.NullMaterialTemperatureValue);
      Assert.True(rec.TemperatureLevels.Max == CellPassConsts.NullMaterialTemperatureValue);
    }
  }
}
