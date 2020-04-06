using FluentAssertions;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class SubGridCellPassDataTemperatureEntryRecordTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      SubGridCellPassDataTemperatureEntryRecord rec = new SubGridCellPassDataTemperatureEntryRecord();

      rec.Clear();

      Assert.True(rec.MeasuredTemperature == CellPassConsts.NullMaterialTemperatureValue);
      Assert.True(rec.TemperatureLevels.Min == CellPassConsts.NullMaterialTemperatureValue);
      Assert.True(rec.TemperatureLevels.Max == CellPassConsts.NullMaterialTemperatureValue);
    }

    [Fact]
    public void Creation2()
    {
      var rec = new SubGridCellPassDataTemperatureEntryRecord(123, new TemperatureWarningLevelsRecord(100, 200));

      rec.MeasuredTemperature.Should().Be(123);
      rec.TemperatureLevels.Min.Should().Be(100);
      rec.TemperatureLevels.Max.Should().Be(200);
    }
  }
}
