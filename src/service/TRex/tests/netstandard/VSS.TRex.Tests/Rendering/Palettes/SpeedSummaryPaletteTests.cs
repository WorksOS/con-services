using System.Drawing;
using FluentAssertions;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Rendering.Palettes;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class SpeedSummaryPaletteTests
  {
    private const ushort MEASURED_SPEED_MIN = 5;
    private const ushort MEASURED_SPEED_MAX = 7;
    private const ushort SPEED_INCREMENT = 5;
    
    [Fact]
    public void Test_SpeedSummaryPalette_Creation()
    {
      var palette = new SpeedSummaryPalette();

      palette.Should().NotBeNull();

      palette.MachineSpeedTarget.Min.Should().Be(10);
      palette.MachineSpeedTarget.Max.Should().Be(30);

      palette.OverSpeedRangeColour.Should().Be(Color.Red);
      palette.WithinSpeedRangeColour.Should().Be(Color.Lime);
      palette.LowerSpeedRangeColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().BeNull();
    }

    [Fact]
    public void Test_SpeedSummaryPalette_ChooseColour_NullTargetRange()
    {
      var palette = new SpeedSummaryPalette();

      palette.Should().NotBeNull();
      
      palette.MachineSpeedTarget = new MachineSpeedExtendedRecord(CellPassConsts.NullMachineSpeed, CellPassConsts.NullMachineSpeed);

      var data = new MachineSpeedExtendedRecord(MEASURED_SPEED_MIN, MEASURED_SPEED_MAX);
      
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      for (var i = 0; i < colors.Length; i++)
      {
        data.Min = (ushort)(MEASURED_SPEED_MIN + i * SPEED_INCREMENT);
        data.Max = (ushort)(MEASURED_SPEED_MAX + i * SPEED_INCREMENT);

        palette.ChooseColour(data).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_SpeedSummaryPalette_ChooseColour()
    {
      var palette = new SpeedSummaryPalette();

      palette.Should().NotBeNull();

      var data = new MachineSpeedExtendedRecord(MEASURED_SPEED_MIN, MEASURED_SPEED_MAX);
     
      var colors = new[] {
        palette.LowerSpeedRangeColour,
        palette.WithinSpeedRangeColour,
        palette.WithinSpeedRangeColour,
        palette.WithinSpeedRangeColour,
        palette.WithinSpeedRangeColour,
        palette.OverSpeedRangeColour,
        palette.OverSpeedRangeColour,
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.Min = (ushort)(MEASURED_SPEED_MIN + i * SPEED_INCREMENT);
        data.Max = (ushort)(MEASURED_SPEED_MAX + i * SPEED_INCREMENT);

        palette.ChooseColour(data).Should().Be(colors[i]);
      }
    }
  }
}
