using System.Drawing;
using FluentAssertions;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class TemperatureSummaryPaletteTests
  {
    private const ushort MEASURED_TEMPERATURE = 5;
    private const ushort TEMPERATURE_INCREMENT = 30;
    private const ushort TEMPERATURE_LEVEL_MIN = 20;
    private const ushort TEMPERATURE_LEVEL_MAX = 110;

    [Fact]
    public void Test_TemperatureSummaryPalette_Creation()
    {
      var palette = new TemperatureSummaryPalette();

      palette.Should().NotBeNull();

      palette.TemperatureLevels.Min.Should().Be(10);
      palette.TemperatureLevels.Max.Should().Be(150);
      palette.UseMachineTempWarningLevels.Should().Be(false);

      palette.AboveMaxLevelColour.Should().Be(Color.Red);
      palette.WithinLevelsColour.Should().Be(Color.Lime);
      palette.BelowMinLevelColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().BeNull();
    }

    [Fact]
    public void Test_TemperatureSummaryPalette_ChooseColour_NullMachineLevels()
    {
      var palette = new TemperatureSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTempWarningLevels = true;
      palette.UseMachineTempWarningLevels.Should().Be(true);

      var temperatureLevels = new TemperatureWarningLevelsRecord(CellPassConsts.NullMaterialTemperatureValue, CellPassConsts.NullMaterialTemperatureValue);

      var data = new SubGridCellPassDataTemperatureEntryRecord(MEASURED_TEMPERATURE, temperatureLevels);

      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };
      
      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredTemperature = (ushort)(MEASURED_TEMPERATURE + i * TEMPERATURE_INCREMENT);
        palette.ChooseColour(data.MeasuredTemperature, data.TemperatureLevels.Min, data.TemperatureLevels.Max).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_TemperatureSummaryPalette_ChooseColour_NullOverrideLevels()
    {
      var palette = new TemperatureSummaryPalette();

      palette.Should().NotBeNull();

      palette.TemperatureLevels = new TemperatureWarningLevelsRecord(CellPassConsts.NullMaterialTemperatureValue, CellPassConsts.NullMaterialTemperatureValue);
      palette.TemperatureLevels.Min.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      palette.TemperatureLevels.Max.Should().Be(CellPassConsts.NullMaterialTemperatureValue);

      var temperatureLevels = new TemperatureWarningLevelsRecord(TEMPERATURE_LEVEL_MIN, TEMPERATURE_LEVEL_MAX);

      var data = new SubGridCellPassDataTemperatureEntryRecord(MEASURED_TEMPERATURE, temperatureLevels);

      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredTemperature = (ushort)(MEASURED_TEMPERATURE + i * TEMPERATURE_INCREMENT);
        palette.ChooseColour(data.MeasuredTemperature, data.TemperatureLevels.Min, data.TemperatureLevels.Max).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_TemperatureSummaryPalette_ChooseColour_MachineLevels()
    {
      var palette = new TemperatureSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTempWarningLevels = true;
      palette.UseMachineTempWarningLevels.Should().Be(true);

      var temperatureLevels = new TemperatureWarningLevelsRecord(TEMPERATURE_LEVEL_MIN, TEMPERATURE_LEVEL_MAX);

      var data = new SubGridCellPassDataTemperatureEntryRecord(MEASURED_TEMPERATURE, temperatureLevels);

      var colors = new[]
      {
        palette.BelowMinLevelColour,
        palette.WithinLevelsColour,
        palette.WithinLevelsColour,
        palette.WithinLevelsColour,
        palette.AboveMaxLevelColour,
        palette.AboveMaxLevelColour,
        palette.AboveMaxLevelColour
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredTemperature = (ushort)(MEASURED_TEMPERATURE + i * TEMPERATURE_INCREMENT);
        palette.ChooseColour(data.MeasuredTemperature, data.TemperatureLevels.Min, data.TemperatureLevels.Max).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_TemperatureSummaryPalette_ChooseColour_OverrideLevels()
    {
      var palette = new TemperatureSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTempWarningLevels.Should().Be(false);
      palette.TemperatureLevels.Min.Should().Be(10);
      palette.TemperatureLevels.Max.Should().Be(150);

      var temperatureLevels = new TemperatureWarningLevelsRecord(TEMPERATURE_LEVEL_MIN, TEMPERATURE_LEVEL_MAX);

      var data = new SubGridCellPassDataTemperatureEntryRecord(MEASURED_TEMPERATURE, temperatureLevels);

      var colors = new[]
      {
        palette.BelowMinLevelColour,
        palette.WithinLevelsColour,
        palette.WithinLevelsColour,
        palette.WithinLevelsColour,
        palette.WithinLevelsColour,
        palette.AboveMaxLevelColour,
        palette.AboveMaxLevelColour
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredTemperature = (ushort)(MEASURED_TEMPERATURE + i * TEMPERATURE_INCREMENT);
        palette.ChooseColour(data.MeasuredTemperature, data.TemperatureLevels.Min, data.TemperatureLevels.Max).Should().Be(colors[i]);
      }
    }
  }
}
