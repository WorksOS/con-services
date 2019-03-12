using System;
using VSS.TRex.Analytics.Foundation.Aggregators;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Analytics.ElevationStatistics
{
  /// <summary>
  /// Implements the specific business rules for calculating a Elevation statistics
  /// </summary>
  public class ElevationStatisticsAggregator : AggregatorBase
  {
    /// <summary>
    /// Aggregator state is now single threaded in the context of processing sub grid
    /// information into it as the processing threads access independent sub-state aggregators which
    /// are aggregated together to form the final aggregation result. However, in contexts that do support
    /// threaded access to this structure the FRequiresSerialisation flag should be set
    /// </summary>
    public bool RequiresSerialisation { get; set; }

    /// <summary>
    /// The minimum elevation value of the site model. 
    /// </summary>
    public double MinElevation { get; set; }

    /// <summary>
    /// The maximum elevation value of the site model.
    /// </summary>
    public double MaxElevation { get; set; }

    /// <summary>
    /// Records how many cells were used in the calculation.
    /// </summary>
    public int CellsUsed { get; set; }

    /// <summary>
    /// Records the total number of cells that were considered by
    /// the engine. This includes cells outside of reference design fence boundaries
    /// and cells where both base and top values may have been null.
    /// </summary>
    public int CellsScanned { get; set; }

    /// <summary>
    /// The area of cells that we have considered and successfully computed information from.
    /// </summary>
    public double CoverageArea => CellsUsed * (CellSize * CellSize);

    /// <summary>
    /// The bounding extents of the computed area.
    /// </summary>
    public BoundingWorldExtent3D BoundingExtents = new BoundingWorldExtent3D();

    /// <summary>
    /// The total area of the data cells.
    /// </summary>
    public double TotalArea => CellsScanned * (CellSize * CellSize);


    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ElevationStatisticsAggregator()
    {
      CellsScanned = 0;
      CellsUsed = 0;

      MinElevation = Consts.INITIAL_ELEVATION;
      MaxElevation = -Consts.INITIAL_ELEVATION;

      BoundingExtents.SetInverted();

      RequiresSerialisation = false;
    }

    /// <summary>
    /// Combine this aggregator with another aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public ElevationStatisticsAggregator AggregateWith(ElevationStatisticsAggregator other)
    {
      CellSize = other.CellSize;

      if (other.MinElevation < MinElevation)
        MinElevation = other.MinElevation;

      if (other.MaxElevation > MaxElevation)
        MaxElevation = other.MaxElevation;

      CellsUsed += other.CellsUsed;
      CellsScanned += other.CellsScanned;

      lock (this)
      {
        BoundingExtents.Include(other.BoundingExtents);
      }

      return this;
    }


    public override void Initialise(AggregatorBase state)
    {
      if (state == null)
        return;

      SiteModelID = state.SiteModelID;
      CellSize = state.CellSize;
    }

    public override void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      lock (this)
      {
        foreach (IClientLeafSubGrid[] subGrid in subGrids)
        {
          if ((subGrid?.Length ?? 0) > 0 && subGrid[0] is ClientHeightLeafSubGrid SubGrid)
          {
            CellsScanned += SubGridTreeConsts.SubGridTreeCellsPerSubGrid;

            BoundingExtents.Include(SubGrid.WorldExtents());

            SubGridUtilities.SubGridDimensionalIterator((I, J) =>
            {
              var heightValue = SubGrid.Cells[I, J];

              if (Math.Abs(heightValue - CellPassConsts.NullHeight) > Consts.TOLERANCE_HEIGHT)
              {
                CellsUsed++;

                if (MinElevation > heightValue)
                  MinElevation = heightValue;

                if (MaxElevation < heightValue)
                  MaxElevation = heightValue;
              }
            });
          }
        }
      }
    }

    public override void Finalise()
    {
    }
  }
}
