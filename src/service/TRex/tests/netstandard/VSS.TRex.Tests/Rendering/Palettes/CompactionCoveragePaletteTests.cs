using System.Drawing;
using FluentAssertions;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.Rendering.Palettes
{
  public class CompactionCoveragePaletteTests
  {
    private const short MEASURED_CMV = 5;
    private const short TARGET_CMV = 90;
    private const short CMV_INCREMENT = 20;

    [Fact]
    public void Test_CompactionCoveragePalette_Creation()
    {
      var palette = new CompactionCoveragePalette();

      palette.Should().NotBeNull();

      palette.HasCMVData.Should().Be(Color.Green);
      palette.HasNoCMVData.Should().Be(Color.Cyan);
    }

    [Fact]
    public void Test_CompactionCoveragePalette_ChooseColour_HasCmvData()
    {
      var palette = new CompactionCoveragePalette();

      palette.Should().NotBeNull();

      var data = new SubGridCellPassDataCMVEntryRecord(MEASURED_CMV, TARGET_CMV, 0, 0);
      palette.ChooseColour(data.MeasuredCMV).Should().Be(palette.HasCMVData);
    }

    [Fact]
    public void Test_CompactionCoveragePalette_ChooseColour_HasNoCmvData()
    {
      var palette = new CompactionCoveragePalette();

      palette.Should().NotBeNull();

      var data = new SubGridCellPassDataCMVEntryRecord(CellPassConsts.NullCCV, TARGET_CMV, 0, 0);
      palette.ChooseColour(data.MeasuredCMV).Should().Be(palette.HasNoCMVData);
    }
  }
}
