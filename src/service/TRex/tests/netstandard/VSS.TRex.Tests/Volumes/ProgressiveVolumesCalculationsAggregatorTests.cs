using System;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class ProgressiveVolumesCalculationsAggregatorTests
  {
    [Fact]
    public void Creation()
    {
      var aggr = new ProgressiveVolumesCalculationsAggregator();
      aggr.Should().NotBeNull();

      aggr.AggregationStates.Should().BeNull();
      aggr.VolumeType.Should().Be(VolumeComputationType.None);
      aggr.CutTolerance.Should().Be(VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE);
      aggr.FillTolerance.Should().Be(VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE);
    }

    [Fact]
    public void TestToString()
    {
      var aggr = new ProgressiveVolumesCalculationsAggregator();
      aggr.ToString().Should().StartWith("VolumeType");
    }

    [Fact]
    public void ProcessSubGridResult_FailWothNoDefinedVolumeType()
    {
      var subGrids = new[] { new[] { ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.ProgressiveVolumes) } };

      var aggr = new ProgressiveVolumesCalculationsAggregator();
      aggr.AggregationStates = new ProgressiveVolumeAggregationState[1];

      Action act = () => aggr.ProcessSubGridResult(subGrids);
      act.Should().Throw<ArgumentException>().WithMessage("Unsupported volume type*");
    }

    [Fact]
    public void ProcessSubGridResult_EmptySubGrid_NoHeightLayers()
    {
      var subGrids = new[]
      {
        new[] {ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.ProgressiveVolumes)}
      };

      var aggr = new ProgressiveVolumesCalculationsAggregator
      {
        VolumeType = VolumeComputationType.Between2Filters,
        AggregationStates = new ProgressiveVolumeAggregationState[1]
      };

      aggr.ProcessSubGridResult(subGrids);
    }

    [Fact]
    public void ProcessSubGridResult_EmptySubGrid_NoHeightsInLayers()
    {
      var subGrids = new[]
      {
        new[] {ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.ProgressiveVolumes)}
      };

      if (!(subGrids[0][0] is ClientProgressiveHeightsLeafSubGrid progressiveSubGrid))
        throw new ArgumentException("Sub grid not a ClientProgressiveHeightsLeafSubGrid");

      progressiveSubGrid.NumberOfHeightLayers = 2;

      var aggr = new ProgressiveVolumesCalculationsAggregator
      {
        VolumeType = VolumeComputationType.Between2Filters,
        AggregationStates = new [] {new ProgressiveVolumeAggregationState(SubGridTreeConsts.DefaultCellSize)}
      };

      aggr.ProcessSubGridResult(subGrids);
    }

    [Fact]
    public void Finalise_NoFailWithNoAggregators()
    {
      var aggr = new ProgressiveVolumesCalculationsAggregator();
      aggr.Finalise();
    }

    private ProgressiveVolumesCalculationsAggregator BuildFilterToFilterAggregatorWithOneAggregation(float baseLevel, float topLevel)
    {
      var subGrids = new[]
      {
        new[] {ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.ProgressiveVolumes)}
      };

      if (!(subGrids[0][0] is ClientProgressiveHeightsLeafSubGrid progressiveSubGrid))
        throw new ArgumentException("Sub grid not a ClientProgressiveHeightsLeafSubGrid");

      progressiveSubGrid.NumberOfHeightLayers = 2;

      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => progressiveSubGrid.Heights[0][x, y] = baseLevel);
      TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) => progressiveSubGrid.Heights[1][x, y] = topLevel);

      var aggr = new ProgressiveVolumesCalculationsAggregator
      {
        VolumeType = VolumeComputationType.Between2Filters,
        AggregationStates = new[] { new ProgressiveVolumeAggregationState(SubGridTreeConsts.DefaultCellSize) }
      };

      aggr.ProcessSubGridResult(subGrids);

      return aggr;
    }

    private void CheckAggregationstate(ProgressiveVolumeAggregationState aggr,
      double expCoverageArea, CutFillVolume expCutFillVolume, 
      int expCellDiscarded, int expCellsScanned, int expCellsUsed, int expCellsUsedFill, int expCellsUsedCut, 
      double expCutArea, double expFillArea)
    {
      aggr.CoverageArea.Should().Be(expCoverageArea);
      aggr.CutFillVolume.FillVolume.Should().BeApproximately(expCutFillVolume.FillVolume, 0.0001);
      aggr.CutFillVolume.CutVolume.Should().BeApproximately(expCutFillVolume.CutVolume, 0.0001);
      aggr.CellsDiscarded.Should().Be(expCellDiscarded);
      aggr.CellsScanned.Should().Be(expCellsScanned);
      aggr.CellsUsed.Should().Be(expCellsUsed);
      aggr.CellsUsedFill.Should().Be(expCellsUsedFill);
      aggr.CellsUsedCut.Should().Be(expCellsUsedCut);
      aggr.CutArea.Should().BeApproximately(expCutArea, 0.0001);
      aggr.FillArea.Should().BeApproximately(expFillArea, 0.0001);
    }

    [Fact]
    public void Finalise_Fill()
    {
      var cellArea = SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize;
      var aggr = BuildFilterToFilterAggregatorWithOneAggregation(0.0f, 1.0f);

      aggr.Finalise();

      CheckAggregationstate(aggr.AggregationStates[0], SubGridTreeConsts.CellsPerSubGrid * cellArea,
        new CutFillVolume(0, SubGridTreeConsts.CellsPerSubGrid * cellArea),
        0, SubGridTreeConsts.CellsPerSubGrid, SubGridTreeConsts.CellsPerSubGrid, SubGridTreeConsts.CellsPerSubGrid, 0,
        0, SubGridTreeConsts.CellsPerSubGrid * cellArea);
    }

    [Fact]
    public void Finalise_Cut()
    {
      var cellArea = SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize;
      var aggr = BuildFilterToFilterAggregatorWithOneAggregation(1.0f, 0.0f);

      aggr.Finalise();

      CheckAggregationstate(aggr.AggregationStates[0], SubGridTreeConsts.CellsPerSubGrid * cellArea,
        new CutFillVolume(SubGridTreeConsts.CellsPerSubGrid * cellArea, 0),
        0, SubGridTreeConsts.CellsPerSubGrid, SubGridTreeConsts.CellsPerSubGrid, 0, SubGridTreeConsts.CellsPerSubGrid,
        SubGridTreeConsts.CellsPerSubGrid * cellArea, 0);
    }

    [Fact]
    public void AggregateWith_Fill()
    {
      var cellArea = SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize;
      var aggr1 = BuildFilterToFilterAggregatorWithOneAggregation(0.0f, 1.0f);
      var aggr2 = BuildFilterToFilterAggregatorWithOneAggregation(0.0f, 1.0f);

      aggr1.AggregateWith(aggr2);
      aggr1.Finalise();

      CheckAggregationstate(aggr1.AggregationStates[0], 2 * SubGridTreeConsts.CellsPerSubGrid * cellArea,
        new CutFillVolume(0, 2 * SubGridTreeConsts.CellsPerSubGrid * cellArea),
        0, 2 * SubGridTreeConsts.CellsPerSubGrid, 2 * SubGridTreeConsts.CellsPerSubGrid, 2 * SubGridTreeConsts.CellsPerSubGrid, 0,
        0, 2 * SubGridTreeConsts.CellsPerSubGrid * cellArea);
    }

    [Fact]
    public void AggregateWith_Cut()
    {
      var cellArea = SubGridTreeConsts.DefaultCellSize * SubGridTreeConsts.DefaultCellSize;
      var aggr1 = BuildFilterToFilterAggregatorWithOneAggregation(1.0f, 0.0f);
      var aggr2 = BuildFilterToFilterAggregatorWithOneAggregation(1.0f, 0.0f);

      aggr1.AggregateWith(aggr2);
      aggr1.Finalise();

      CheckAggregationstate(aggr1.AggregationStates[0], 2 * SubGridTreeConsts.CellsPerSubGrid * cellArea,
        new CutFillVolume(2 * SubGridTreeConsts.CellsPerSubGrid * cellArea, 0),
        0, 2 * SubGridTreeConsts.CellsPerSubGrid, 2 * SubGridTreeConsts.CellsPerSubGrid, 
        0, 2 * SubGridTreeConsts.CellsPerSubGrid, 
        2 * SubGridTreeConsts.CellsPerSubGrid * cellArea, 0);
    }
  }
}
