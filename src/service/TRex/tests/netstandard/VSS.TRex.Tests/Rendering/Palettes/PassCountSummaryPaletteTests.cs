using System.Drawing;
using FluentAssertions;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Records;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class PassCountSummaryPaletteTests
  {
    private const ushort MEASURED_PASS_COUNT = 1;
    private const ushort TARGET_PASS_COUNT = 7;
    private const ushort PASS_COUNT_INCREMENT = 1;

    [Fact]
    public void Test_PassCountSummaryPalette_Creation()
    {
      var palette = new PassCountSummaryPalette();

      palette.Should().NotBeNull();

      palette.TargetPassCountRange.Min.Should().Be(3);
      palette.TargetPassCountRange.Max.Should().Be(5);
      palette.UseMachineTargetPass.Should().Be(false);

      palette.AbovePassTargetRangeColour.Should().Be(Color.Red);
      palette.WithinPassTargetRangeColour.Should().Be(Color.Lime);
      palette.BelowPassTargetRangeColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().BeNull();
    }

    [Fact]
    public void Test_PassCountSummaryPalette_ChooseColour_NullMachineTargetRange()
    {
      var palette = new PassCountSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetPass = true;
      palette.UseMachineTargetPass.Should().Be(true);

      var data = new SubGridCellPassDataPassCountEntryRecord(MEASURED_PASS_COUNT, CellPassConsts.NullPassCountValue);
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      var targetPassCountRange = new PassCountRangeRecord(data.TargetPassCount, data.TargetPassCount);

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredPassCount = (ushort)(MEASURED_PASS_COUNT + i * PASS_COUNT_INCREMENT);
        palette.ChooseColour(data.MeasuredPassCount, targetPassCountRange).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_PassCountSummaryPalette_ChooseColour_NullOverrideTargetRange()
    {
      var palette = new PassCountSummaryPalette();

      palette.Should().NotBeNull();

      palette.TargetPassCountRange = new PassCountRangeRecord(CellPassConsts.NullPassCountValue, CellPassConsts.NullPassCountValue);
      palette.TargetPassCountRange.Min.Should().Be(CellPassConsts.NullPassCountValue);
      palette.TargetPassCountRange.Max.Should().Be(CellPassConsts.NullPassCountValue);

      var data = new SubGridCellPassDataPassCountEntryRecord(MEASURED_PASS_COUNT, TARGET_PASS_COUNT);
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      var targetPassCountRange = new PassCountRangeRecord(data.TargetPassCount, data.TargetPassCount);

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredPassCount = (ushort)(MEASURED_PASS_COUNT + i * PASS_COUNT_INCREMENT);
        palette.ChooseColour(data.MeasuredPassCount, targetPassCountRange).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_PassCountSummaryPalette_ChooseColour_MachineTargetRange()
    {
      var palette = new PassCountSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetPass = true;
      palette.UseMachineTargetPass.Should().Be(true);

      var data = new SubGridCellPassDataPassCountEntryRecord(MEASURED_PASS_COUNT, TARGET_PASS_COUNT);
      var colors = new[]
      {
        palette.BelowPassTargetRangeColour,
        palette.BelowPassTargetRangeColour,
        palette.BelowPassTargetRangeColour,
        palette.BelowPassTargetRangeColour,
        palette.BelowPassTargetRangeColour,
        palette.BelowPassTargetRangeColour,
        palette.WithinPassTargetRangeColour,
        palette.AbovePassTargetRangeColour,
        palette.AbovePassTargetRangeColour
      };

      var targetPassCountRange = new PassCountRangeRecord(data.TargetPassCount, data.TargetPassCount);

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredPassCount = (ushort)(MEASURED_PASS_COUNT + i * PASS_COUNT_INCREMENT);
        palette.ChooseColour(data.MeasuredPassCount, targetPassCountRange).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_PassCountSummaryPalette_ChooseColour_OverrideTargetRange()
    {
      var palette = new PassCountSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetPass.Should().Be(false);
      palette.TargetPassCountRange.Min.Should().Be(3);
      palette.TargetPassCountRange.Max.Should().Be(5);


      var data = new SubGridCellPassDataPassCountEntryRecord(MEASURED_PASS_COUNT, TARGET_PASS_COUNT);
      var colors = new[]
      {
        palette.BelowPassTargetRangeColour,
        palette.BelowPassTargetRangeColour,
        palette.WithinPassTargetRangeColour,
        palette.WithinPassTargetRangeColour,
        palette.WithinPassTargetRangeColour,
        palette.AbovePassTargetRangeColour,
        palette.AbovePassTargetRangeColour,
        palette.AbovePassTargetRangeColour,
        palette.AbovePassTargetRangeColour
      };

      var targetPassCountRange = new PassCountRangeRecord(data.TargetPassCount, data.TargetPassCount);

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredPassCount = (ushort)(MEASURED_PASS_COUNT + i * PASS_COUNT_INCREMENT);
        palette.ChooseColour(data.MeasuredPassCount, targetPassCountRange).Should().Be(colors[i]);
      }
    }
  }
}
