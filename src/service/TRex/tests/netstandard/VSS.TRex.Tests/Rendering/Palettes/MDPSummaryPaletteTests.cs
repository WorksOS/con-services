using System.Drawing;
using FluentAssertions;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Types;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class MDPSummaryPaletteTests
  {
    private const short MEASURED_MDP = 5;
    private const short TARGET_MDP = 90;
    private const short MDP_INCREMENT = 20;

    [Fact]
    public void Test_MDPSummaryPalette_Creation()
    {
      var palette = new MDPSummaryPalette();

      palette.Should().NotBeNull();

      palette.MDPPercentageRange.Min.Should().Be(75);
      palette.MDPPercentageRange.Max.Should().Be(110);
      palette.AbsoluteTargetMDP.Should().Be(50);
      palette.UseMachineTargetMDP.Should().Be(false);

      palette.AboveMDPTargetRangeColour.Should().Be(Color.Red);
      palette.WithinMDPTargetRangeColour.Should().Be(Color.Lime);
      palette.BelowMDPTargetRangeColour.Should().Be(Color.Blue);

      palette.PaletteTransitions.Should().BeNull();
    }

    [Fact]
    public void Test_MDPSummaryPalette_ChooseColour_NullMachineTarget()
    {
      var palette = new MDPSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetMDP = true;
      palette.UseMachineTargetMDP.Should().Be(true);

      var data = new SubGridCellPassDataMDPEntryRecord(MEASURED_MDP, TARGET_MDP);
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredMDP = (short)(MEASURED_MDP + i * MDP_INCREMENT);
        palette.ChooseColour(data.MeasuredMDP, CellPassConsts.NullMDP).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_MDPSummaryPalette_ChooseColour_NullOverrideTarget()
    {
      var palette = new MDPSummaryPalette();

      palette.Should().NotBeNull();

      palette.AbsoluteTargetMDP = CellPassConsts.NullMDP;
      palette.AbsoluteTargetMDP.Should().Be(CellPassConsts.NullMDP);

      var data = new SubGridCellPassDataMDPEntryRecord(MEASURED_MDP, TARGET_MDP);
      var colors = new[] { Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty, Color.Empty };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredMDP = (short)(MEASURED_MDP + i * MDP_INCREMENT);
        palette.ChooseColour(data.MeasuredMDP, TARGET_MDP).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_MDPSummaryPalette_ChooseColour_MachineTarget()
    {
      var palette = new MDPSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetMDP = true;
      palette.UseMachineTargetMDP.Should().Be(true);

      var data = new SubGridCellPassDataMDPEntryRecord(MEASURED_MDP, TARGET_MDP);
      var colors = new[]
      {
        palette.BelowMDPTargetRangeColour,
        palette.BelowMDPTargetRangeColour,
        palette.BelowMDPTargetRangeColour,
        palette.BelowMDPTargetRangeColour,
        palette.WithinMDPTargetRangeColour,
        palette.AboveMDPTargetRangeColour,
        palette.AboveMDPTargetRangeColour
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredMDP = (short)(MEASURED_MDP + i * MDP_INCREMENT);
        palette.ChooseColour(data.MeasuredMDP, data.TargetMDP).Should().Be(colors[i]);
      }
    }

    [Fact]
    public void Test_MDPSummaryPalette_ChooseColour_OverrideTarget()
    {
      var palette = new MDPSummaryPalette();

      palette.Should().NotBeNull();

      palette.UseMachineTargetMDP.Should().Be(false);
      palette.AbsoluteTargetMDP.Should().Be(50);

      var data = new SubGridCellPassDataMDPEntryRecord(MEASURED_MDP, TARGET_MDP);
      var colors = new[]
      {
        palette.BelowMDPTargetRangeColour,
        palette.BelowMDPTargetRangeColour,
        palette.WithinMDPTargetRangeColour,
        palette.AboveMDPTargetRangeColour,
        palette.AboveMDPTargetRangeColour,
        palette.AboveMDPTargetRangeColour,
        palette.AboveMDPTargetRangeColour
      };

      for (var i = 0; i < colors.Length; i++)
      {
        data.MeasuredMDP = (short)(MEASURED_MDP + i * MDP_INCREMENT);
        palette.ChooseColour(data.MeasuredMDP, data.TargetMDP).Should().Be(colors[i]);
      }
    }
  }
}
