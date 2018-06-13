using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Types;
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

      Assert.True(rec.MeasuredTemperature == CellPass.NullMaterialTemperatureValue);
      Assert.True(rec.TemperatureLevels.Min == CellPass.NullMaterialTemperatureValue);
      Assert.True(rec.TemperatureLevels.Max == CellPass.NullMaterialTemperatureValue);
    }
  }
}
