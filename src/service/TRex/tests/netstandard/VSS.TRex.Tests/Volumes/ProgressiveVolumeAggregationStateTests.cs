using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Volumes;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class ProgressiveVolumeAggregationStateTests
  {
    [Fact]
    public void Creation()
    {
      var state = new ProgressiveVolumeAggregationState(SubGridTreeConsts.DefaultCellSize);
      state.Should().NotBeNull();
    }

    [Fact]
    public void Finalise()
    {
      const double cellSize = 2.0;

      var state = new ProgressiveVolumeAggregationState(cellSize)
      {
        CellsUsed = 3,
        CellsDiscarded = 1,
        CellsScanned = 3,
        CellsUsedCut = 1,
        CellsUsedFill = 1
      };

      state.CoverageMap[SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset] = true;
      state.CoverageMap[SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset + 1] = true;

      state.Finalise();

      state.CoverageArea.Should().Be(3.0 * cellSize * cellSize);
      state.CutArea.Should().Be(1.0 * cellSize * cellSize);
      state.FillArea.Should().Be(1.0 * cellSize * cellSize);
      state.TotalArea.Should().Be(3.0 * cellSize * cellSize);
      state.BoundingExtents.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, cellSize, 2 * cellSize));
    }

    private float[,] NullHeights()
    {
      var heights = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => heights[x, y] = Consts.NullHeight);
      return heights;
    }

    [Fact]
    public void ProcessElevationInformationForSubGrid_NullBaseAndTopHeights()
    {
      const double cellSize = SubGridTreeConsts.DefaultCellSize;
      const double cellArea = cellSize * cellSize;

      var state = new ProgressiveVolumeAggregationState(cellSize);

      state.ProcessElevationInformationForSubGrid(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        NullHeights(), NullHeights());

      state.Finalise();

      state.CutFillVolume.CutVolume.Should().Be(0.0);
      state.CutFillVolume.FillVolume.Should().Be(0.0);

      state.CoverageArea.Should().Be(0.0 * cellArea);
      state.CutArea.Should().Be(0.0 * cellArea);
      state.FillArea.Should().Be(0.0 * cellArea);
      state.TotalArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state.BoundingExtents.Should().BeEquivalentTo(BoundingWorldExtent3D.Inverted());
    }

    [Fact]
    public void ProcessElevationInformationForSubGrid_DefinedBaseWithNullTopHeights()
    {
      const double cellSize = SubGridTreeConsts.DefaultCellSize;
      const double cellArea = cellSize * cellSize;

      var state = new ProgressiveVolumeAggregationState(cellSize);
      var baseHeights = NullHeights();
      var topHeights = NullHeights();

      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => baseHeights[x, y] = 0.0f);

      state.ProcessElevationInformationForSubGrid(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        baseHeights, topHeights);

      state.Finalise();

      state.CutFillVolume.CutVolume.Should().Be(0.0);
      state.CutFillVolume.FillVolume.Should().Be(0.0);

      state.CoverageArea.Should().Be(0.0 * cellArea);
      state.CutArea.Should().Be(0.0 * cellArea);
      state.FillArea.Should().Be(0.0 * cellArea);
      state.TotalArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state.BoundingExtents.Should().BeEquivalentTo(BoundingWorldExtent3D.Inverted());
    }

    [Fact]
    public void ProcessElevationInformationForSubGrid_DefinedTopWithNullBaseHeights()
    {
      const double cellSize = SubGridTreeConsts.DefaultCellSize;
      const double cellArea = cellSize * cellSize;

      var state = new ProgressiveVolumeAggregationState(cellSize);
      var baseHeights = NullHeights();
      var topHeights = NullHeights();

      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => topHeights[x, y] = 0.0f);

      state.ProcessElevationInformationForSubGrid(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        baseHeights, topHeights);

      state.Finalise();

      state.CutFillVolume.CutVolume.Should().Be(0.0);
      state.CutFillVolume.FillVolume.Should().Be(0.0);

      state.CoverageArea.Should().Be(0.0 * cellArea);
      state.CutArea.Should().Be(0.0 * cellArea);
      state.FillArea.Should().Be(0.0 * cellArea);
      state.TotalArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state.BoundingExtents.Should().BeEquivalentTo(BoundingWorldExtent3D.Inverted());
    }

    [Fact]
    public void ProcessElevationInformationForSubGrid_DefinedTopAndBaseHeights()
    {
      const double cellSize = SubGridTreeConsts.DefaultCellSize;
      const double cellArea = cellSize * cellSize;

      var state = new ProgressiveVolumeAggregationState(cellSize);
      var baseHeights = NullHeights();
      var topHeights = NullHeights();

      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => baseHeights[x, y] = 0.0f);
      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => topHeights[x, y] = (x % 2 == 0 ? 1.0f : -1.0f));

      state.ProcessElevationInformationForSubGrid(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        baseHeights, topHeights);

      state.Finalise();

      state.CutFillVolume.CutVolume.Should().BeApproximately((SubGridTreeConsts.CellsPerSubGrid / 2) * cellArea, 0.0001);
      state.CutFillVolume.FillVolume.Should().BeApproximately((SubGridTreeConsts.CellsPerSubGrid / 2) * cellArea, 0.0001);

      state.CoverageArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state.CutArea.Should().Be((SubGridTreeConsts.CellsPerSubGrid / 2) * cellArea);
      state.FillArea.Should().Be((SubGridTreeConsts.CellsPerSubGrid / 2) * cellArea);
      state.TotalArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state.BoundingExtents.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, SubGridTreeConsts.SubGridTreeDimension * cellSize, SubGridTreeConsts.SubGridTreeDimension * cellSize));
    }

    [Fact]
    public void AggregateWith()
    {
      const double cellSize = SubGridTreeConsts.DefaultCellSize;
      const double cellArea = cellSize * cellSize;

      var state1 = new ProgressiveVolumeAggregationState(cellSize);
      var state2 = new ProgressiveVolumeAggregationState(cellSize);
      var baseHeights = NullHeights();
      var topHeights = NullHeights();

      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => baseHeights[x, y] = 0.0f);
      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => topHeights[x, y] = (x % 2 == 0 ? 1.0f : -1.0f));

      state1.ProcessElevationInformationForSubGrid(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, baseHeights, topHeights);
      state2.ProcessElevationInformationForSubGrid(SubGridTreeConsts.DefaultIndexOriginOffset + SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.DefaultIndexOriginOffset, baseHeights, topHeights);

      state1.AggregateWith(state2);
      state1.Finalise();

      state1.CoverageArea.Should().Be(2 * SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state1.CutArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state1.FillArea.Should().Be(SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state1.TotalArea.Should().Be(2 * SubGridTreeConsts.CellsPerSubGrid * cellArea);
      state1.BoundingExtents.Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, SubGridTreeConsts.SubGridTreeDimension * cellSize, SubGridTreeConsts.SubGridTreeDimension * cellSize));
    }

    [Fact]
    public void TestToString()
    {
      var state = new ProgressiveVolumeAggregationState(SubGridTreeConsts.DefaultCellSize);
      state.ToString().Should().StartWith("CellSize:");
    }
  }
}
