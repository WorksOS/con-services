using System;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.Volumes
{
  public class ProgressiveVolumeAggregationState
  {
    // CoverageMap maps the area of cells that we have considered and successfully
    // computed volume information from
    public readonly SubGridTreeBitMask CoverageMap;

    /// <summary>
    /// The date for which this volume is being computed with in the progressive volume series
    /// </summary>
    public DateTime Date { get; set; }

    // CellsUsed records how many cells were used in the volume calculation
    public long CellsUsed { get; set; }
    public long CellsUsedCut { get; set; }
    public long CellsUsedFill { get; set; }

    // CellsScanned records the total number of cells that were considered by
    // the engine. This includes cells outside of reference design fence boundaries
    // and cells where both base and top values may have been null.
    public long CellsScanned { get; set; }

    // CellsDiscarded records how many cells were discarded because filtered value was null
    public long CellsDiscarded { get; set; }
    public readonly double CellSize;

    // Volume is the calculated volume determined by simple difference between
    // cells. It does not take into account cut/fill differences (see FCut|FillVolume)
    // This volume is the sole output for operations that apply levels to the surfaces
    public double Volume { get; set; }

    // CutFillVolume is the calculated volume of material that has been 'cut' and 'filled' when the
    // base surface is compared to the top surface. ie: If the top surface is below
    // the base surface at a point then that point is in 'cut'.
    public CutFillVolume CutFillVolume = new CutFillVolume(0, 0);

    public double CoverageArea { get; set; }
    public double CutArea { get; set; }
    public double FillArea { get; set; }
    public double TotalArea { get; set; }
    public BoundingWorldExtent3D BoundingExtents { get; set; } = BoundingWorldExtent3D.Inverted();

    // CutTolerance determines the tolerance (in meters) that the 'From' surface
    // needs to be above the 'To' surface before the two surfaces are not
    // considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    // be cut
    public double CutTolerance { get; set; } = VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE;

    // FillTolerance determines the tolerance (in meters) that the 'To' surface
    // needs to be above the 'From' surface before the two surfaces are not
    // considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    // be cut
    public double FillTolerance { get; set; } = VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE;

    public ProgressiveVolumeAggregationState(double cellSize)
    {
      CellSize = cellSize;
      CoverageMap = new SubGridTreeBitMask(cellSize);
    }

    public void Finalise()
    {
      CoverageArea = CellsUsed * CellSize * CellSize;
      CutArea = CellsUsedCut * CellSize * CellSize;
      FillArea = CellsUsedFill * CellSize * CellSize;
      TotalArea = CellsScanned * CellSize * CellSize;
      BoundingExtents = CoverageMap.ComputeCellsWorldExtents();
    }

    public void ProcessElevationInformationForSubGrid(int cellOriginX, int cellOriginY, float[,] baseSubGrid, float[,] topSubGrid)
    {
      // FCellArea is a handy place to store the cell area, rather than calculate it all the time (value wont change);
      var cellArea = CellSize * CellSize;

      var bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      CellsScanned += SubGridTreeConsts.SubGridTreeCellsPerSubGrid;

      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          var topZ = topSubGrid[i, j];
          var baseZ = baseSubGrid[i, j];

          if (baseZ != Consts.NullHeight && topZ != Consts.NullHeight)
          {
            CellsUsed++;

            //  Note the fact we have processed this cell in the coverage map
            bits.SetBit(i, j);

            var cellUsedInVolumeCalc = (topZ - baseZ >= FillTolerance) || (baseZ - topZ >= CutTolerance);

            // Accumulate volumes
            if (cellUsedInVolumeCalc)
            {
              var volumeDifference = cellArea * (topZ - baseZ);

              // Accumulate the 'surplus' volume. Ie: the simple summation of
              // all cuts and fills.
              Volume += volumeDifference;

              // Accumulate the cuts and fills into discrete cut and fill quantities
              if (topZ < baseZ)
              {
                CellsUsedCut++;
                CutFillVolume.AddCutVolume(Math.Abs(volumeDifference));
              }
              else
              {
                CellsUsedFill++;
                CutFillVolume.AddFillVolume(Math.Abs(volumeDifference));
              }
            }
            else
            {
              // Note the fact there was no volume change in this cell
              // NoChangeMap.Cells[BaseScanSubGrid.OriginX + I, BaseScanSubGrid.OriginY + J] := True;
            }
          }
          else
          {
            CellsDiscarded++;
          }
        }
      }

      // Record the bits for this sub grid in the coverage map by requesting the whole sub grid
      // of bits from the leaf level and setting it in one operation under an exclusive lock
      if (!bits.IsEmpty())
      {
        var coverageMapSubGrid = CoverageMap.ConstructPathToCell(cellOriginX, cellOriginY, SubGridPathConstructionType.CreateLeaf);
        ((SubGridTreeLeafBitmapSubGrid)coverageMapSubGrid).Bits = bits;
      }
    }

    public ProgressiveVolumeAggregationState AggregateWith(ProgressiveVolumeAggregationState other)
    {
      //  SIGLogMessage.PublishNoODS(Self, Format('Aggregating From:%s', [Source.ToString]), slmcDebug);
      //  SIGLogMessage.PublishNoODS(Self, Format('Into:%s', [ToString]), slmcDebug);

      CellsUsed += other.CellsUsed;
      CellsUsedCut += other.CellsUsedCut;
      CellsUsedFill += other.CellsUsedFill;
      CellsScanned += other.CellsScanned;
      CellsDiscarded += other.CellsDiscarded;

      CoverageArea += other.CoverageArea;
      CutArea += other.CutArea;
      FillArea += other.FillArea;
      TotalArea += other.TotalArea;
      BoundingExtents.Include(other.BoundingExtents);

      Volume += other.Volume;
      CutFillVolume.AddCutFillVolume(other.CutFillVolume.CutVolume, other.CutFillVolume.FillVolume);

      return this;
    }

    /// <summary>
    /// Provides a human readable form of the aggregator state
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"CellSize:{CellSize}, CoverageArea:{CoverageArea}, Bounding:{BoundingExtents}, " +
             $"Volume:{Volume}, Cut:{CutFillVolume.CutVolume}, Fill:{CutFillVolume.FillVolume}, " +
             $"Cells Used/Discarded/Scanned:{CellsUsed}/{CellsDiscarded}/{CellsScanned}";
    }
  }
}
