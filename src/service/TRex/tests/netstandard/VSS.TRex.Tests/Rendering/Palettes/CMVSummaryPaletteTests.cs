using System.Drawing;
using FluentAssertions;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CMVSummaryPaletteTests
  {
    private const short MEASURED_CMV = 5;
    private const short TARGET_CMV = 90;
    private const short CMV_INCREMENT = 20;

    [Fact]
    public void Test_CMVSummaryPalette_Creation()
    {
      var palette = new CMVSummaryPalette();

      palette.Should().NotBeNull();

      palette.CMVPercentageRange.Min.Should().Be(80);
      palette.CMVPercentageRange.Max.Should().Be(120);
      palette.AbsoluteTargetCMV.Should().Be(70);
      palette.UseMachineTargetCMV.Should().Be(false);

      palette.AboveCMVTargetRangeColour.Should().Be(Color.Red);
      palette.WithinCMVTargetRangeColour.Should().Be(Color.Lime);
      palette.BelowCMVTargetRangeColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().BeNull();
    }

    [Fact]
    public void Test_CMVSummaryPalette_ChooseColour_NullMachineTarget()
    {
      var palette = new CMVSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetCMV = true;
      palette.UseMachineTargetCMV.Should().Be(true);

      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data.MeasuredCMV, CellPassConsts.NullCCV).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_CMVSummaryPalette_ChooseColour_NullOverrideTarget()
    {
      var palette = new CMVSummaryPalette();

      palette.Should().NotBeNull();

      palette.AbsoluteTargetCMV = CellPassConsts.NullCCV;
      palette.AbsoluteTargetCMV.Should().Be(CellPassConsts.NullCCV);
      
      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data.MeasuredCMV, TARGET_CMV).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_CMVSummaryPalette_ChooseColour_MachineTarget()
    {
      var palette = new CMVSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetCMV = true;
      palette.UseMachineTargetCMV.Should().Be(true);

      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new[]
      {
        palette.BelowCMVTargetRangeColour,
        palette.BelowCMVTargetRangeColour,
        palette.BelowCMVTargetRangeColour,
        palette.BelowCMVTargetRangeColour,
        palette.WithinCMVTargetRangeColour,
        palette.WithinCMVTargetRangeColour,
        palette.AboveCMVTargetRangeColour
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data.MeasuredCMV, data.TargetCMV).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_CMVSummaryPalette_ChooseColour_OverrideTarget()
    {
      var palette = new CMVSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetCMV.Should().Be(false);
      palette.AbsoluteTargetCMV.Should().Be(70);

      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      var colors = new[]
      {
        palette.BelowCMVTargetRangeColour,
        palette.BelowCMVTargetRangeColour,
        palette.BelowCMVTargetRangeColour,
        palette.WithinCMVTargetRangeColour,
        palette.AboveCMVTargetRangeColour,
        palette.AboveCMVTargetRangeColour,
        palette.AboveCMVTargetRangeColour
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredCMV = (short)(MEASURED_CMV + i * CMV_INCREMENT);
        palette.ChooseColour(data.MeasuredCMV, data.TargetCMV).Should().Be(colors[i]);
      }
    }
  }
}
