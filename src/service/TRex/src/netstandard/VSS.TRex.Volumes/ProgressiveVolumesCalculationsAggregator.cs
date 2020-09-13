using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Volumes
{
  /// <summary>
  /// Defines an aggregator that summaries progressive volume information for sub grids
  /// Each elevation sub grid passed to it forms context which may be used to compute one element in the progressive volume series
  /// with the exception of the first sub grid in the case of filter to filter progressive volumes
  /// </summary>
  public class ProgressiveVolumesCalculationsAggregator : ISubGridRequestsAggregator, IAggregateWith<ProgressiveVolumesCalculationsAggregator>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<ProgressiveVolumesCalculationsAggregator>();

    private readonly object _lockObj = new object();

    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    public double CellSize { get; set; }

    public DesignDescriptor DesignDescriptor = DesignDescriptor.Null(); // no {get;set;} intentionally

    /// <summary>
    /// The design being used to compare heights derived from production data against to calculate per-cell volumes.
    /// Also contains the offset for a reference surface.
    /// </summary>
    public IDesignWrapper ActiveDesign { get; set; }

    // References necessary for correct calculation of aggregated state
    public ILiftParameters LiftParams { get; set; } = new LiftParameters();

    public ISiteModel SiteModel { get; set; }

    // The sum of the aggregated summarized information relating to volumes summary based reports

    /// <summary>
    /// The collection of all aggregation states holding progressive volume calculation results
    /// </summary>
    public ProgressiveVolumeAggregationState[] AggregationStates { get; set; }

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

    public ProgressiveVolumesCalculationsAggregator()
    { }

    private void ProcessVolumeInformationForSubGrid(ClientProgressiveHeightsLeafSubGrid subGrid)
    {
      if (subGrid == null)
      {
        // This is kind of a bad thing, make a note of it for now
        _log.LogDebug("Sub grid passed to ProcessVolumeInformationForSubGrid is null, ignoring");
        return;
      }

      // Compute the two planes of elevations to be compared and supply them to ProcessVolumeInformationForSubGrid
      // Iterate across all the Heights planes in the subGrid. If there is a design to be compared to them request the elevation
      // plane for that design just once
      // DesignHeights represents all the valid spot elevations for the cells in the sub grid being processed
      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult profilerRequestResult) getDesignHeightsResult = (null, DesignProfilerRequestResult.UnknownError);

      // Query the patch of elevations from the surface model for this sub grid
      if (ActiveDesign != null)
      {
        getDesignHeightsResult = ActiveDesign.Design.GetDesignHeightsViaLocalCompute(SiteModel, ActiveDesign.Offset, subGrid.OriginAsCellAddress(), CellSize);

        if (getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.OK &&
            getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          _log.LogError($"Design profiler sub grid elevation request for {subGrid.OriginAsCellAddress()} failed with error {getDesignHeightsResult.profilerRequestResult}");
          return;
        }
      }

      var designHeights = getDesignHeightsResult.designHeights?.Cells ?? ClientHeightLeafSubGrid.NullCells;

      lock (_lockObj)
      {
        switch (VolumeType)
        {
          case VolumeComputationType.Between2Filters:
            for (var i = 0; i < subGrid.NumberOfHeightLayers - 1; i++)
              AggregationStates[i].ProcessElevationInformationForSubGrid(subGrid.OriginX, subGrid.OriginY, subGrid.Heights[i], subGrid.Heights[i + 1]);
            break;
          case VolumeComputationType.BetweenDesignAndFilter:
            for (var i = 0; i < subGrid.NumberOfHeightLayers; i++)
              AggregationStates[i].ProcessElevationInformationForSubGrid(subGrid.OriginX, subGrid.OriginY, subGrid.Heights[i], designHeights);
            break;
          case VolumeComputationType.BetweenFilterAndDesign:
            for (var i = 0; i < subGrid.NumberOfHeightLayers; i++)
              AggregationStates[i].ProcessElevationInformationForSubGrid(subGrid.OriginX, subGrid.OriginY, designHeights, subGrid.Heights[i]);
            break;
          default:
            throw new ArgumentException($"Unsupported volume type {VolumeType}");
        }
      }
    }

    /// <summary>
    /// Summarizes the client height grid derived from sub grid processing into the running volumes aggregation state
    /// </summary>
    private void SummarizeSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      foreach (var subGridResult in subGrids)
      {
        if (subGridResult != null)
        {
          var baseSubGrid = subGridResult[0];

          if (baseSubGrid == null)
            _log.LogWarning("#W# SummarizeSubGridResult BaseSubGrid is null");
          else
          {
            ProcessVolumeInformationForSubGrid(baseSubGrid as ClientProgressiveHeightsLeafSubGrid);
          }
        }
      }
    }

    /// <summary>
    /// Provides a human readable form of the aggregator state
    /// </summary>
    public override string ToString()
    {
      return $"VolumeType:{VolumeType}, CellSize:{CellSize}, ReferenceDesign:{DesignDescriptor}";
    }

    /// <summary>
    /// Combine this aggregator with another progressive volumes aggregator and store the result in this aggregator
    /// </summary>
    public ProgressiveVolumesCalculationsAggregator AggregateWith(ProgressiveVolumesCalculationsAggregator other)
    {
      if ((AggregationStates?.Length ?? 0) != (other.AggregationStates?.Length ?? 0))
      {
        throw new ArgumentException($"Progressive volumes aggregator collections should have same length: {AggregationStates?.Length ?? 0} versus {other.AggregationStates?.Length ?? 0}");
      }

      if (AggregationStates != null && other != null)
      {
        for (var i = 0; i < AggregationStates.Length; i++)
        {
          AggregationStates[i].AggregateWith(other.AggregationStates[i]);
        }
      }

      return this;
    }

    /// <summary>
    /// Implement the sub grids request aggregator method to process sub grid results...
    /// </summary>
    public void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      SummarizeSubGridResult(subGrids); 
    }

    /// <summary>
    /// Instructs each individual aggregator in the progressive set of volumes to finalize itself
    /// </summary>
    public void Finalise()
    {
      if (AggregationStates == null) 
        return;

      foreach (var aggregator in AggregationStates)
      {
        aggregator.Finalise();
      }
    }
  }
}
