using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees;
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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProgressiveVolumesCalculationsAggregator>();

    // CoverageMap maps the area of cells that we have considered and successfully
    // computed volume information from
    public readonly SubGridTreeBitMask CoverageMap = new SubGridTreeBitMask();

    /// <summary>
    /// The design being used to compare heights derived from production data against to calculate per-cell volumes.
    /// Also contains the offset for a reference surface.
    /// </summary>
    public IDesignWrapper ActiveDesign { get; set; }

    // References necessary for correct calculation of aggregated state
    public ILiftParameters LiftParams { get; set; } = new LiftParameters();

    public Guid SiteModelID { get; set; } = Guid.Empty;

    // The sum of the aggregated summarized information relating to volumes summary based reports

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
    public double CellSize { get; set; }

    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    // Volume is the calculated volume determined by simple difference between
    // cells. It does not take into account cut/fill differences (see FCut|FillVolume)
    // This volume is the sole output for operations that apply levels to the surfaces
    public double Volume { get; set; }

    // CutFillVolume is the calculated volume of material that has been 'cut' and 'filled' when the
    // base surface is compared to the top surface. ie: If the top surface is below
    // the base surface at a point then that point is in 'cut'.
    public CutFillVolume CutFillVolume = new CutFillVolume(0, 0);

    public DesignDescriptor DesignDescriptor = DesignDescriptor.Null(); // no {get;set;} intentionally

    public double TopLevel { get; set; } = 0;
    public double BaseLevel { get; set; } = 0;
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

    public ProgressiveVolumesCalculationsAggregator()
    {
      // NOTE: This aggregator state is now single threaded in the context of processing sub grid
      // information into it as the processing threads access independent sub-state aggregators which
      // are aggregated together to form the final aggregation result. However, in contexts that do support
      // threaded access to this structure the RequiresSerialisation flag should be set

      // if Assigned(Source) then
      //    Initialise(Source);
    }

    public void Finalise()
    {
      CoverageArea = CellsUsed * CellSize * CellSize;
      CutArea = CellsUsedCut * CellSize * CellSize;
      FillArea = CellsUsedFill * CellSize * CellSize;
      TotalArea = CellsScanned * CellSize * CellSize;
      BoundingExtents = CoverageMap.ComputeCellsWorldExtents();
    }

    protected async Task ProcessVolumeInformationForSubGrid(ClientHeightLeafSubGrid subGrid)
    {
      // TODO: This implementation will be different to 
    }

    /// <summary>
    /// Summarises the client height grid derived from sub grid processing into the running volumes aggregation state
    /// </summary>
    /// <param name="subGrids"></param>
    public async Task SummariseSubgridResultAsync(IClientLeafSubGrid[][] subGrids)
    {
      var taskList = new List<Task>(subGrids.Length);

      foreach (var subGridResult in subGrids)
      {
        if (subGridResult != null)
        {
          var baseSubGrid = subGridResult[0];

          if (baseSubGrid == null)
            Log.LogWarning("#W# SummariseSubGridResult BaseSubGrid is null");
          else
          {
            taskList.Add(ProcessVolumeInformationForSubGrid(baseSubGrid as ClientHeightLeafSubGrid));
          }
        }
      }

      await Task.WhenAll(taskList);
    }

    /// <summary>
    /// Provides a human readable form of the aggregator state
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"VolumeType:{VolumeType}, CellSize:{CellSize}, CoverageArea:{CoverageArea}, Bounding:{BoundingExtents}, " +
          $"Volume:{Volume}, Cut:{CutFillVolume.CutVolume}, Fill:{CutFillVolume.FillVolume}, " +
          $"Cells Used/Discarded/Scanned:{CellsUsed}/{CellsDiscarded}/{CellsScanned}, ReferenceDesign:{DesignDescriptor}";
    }

    /// <summary>
    /// Combine this aggregator with another progressive volumes aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    public ProgressiveVolumesCalculationsAggregator AggregateWith(ProgressiveVolumesCalculationsAggregator other)
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
    /// Implement the sub grids request aggregator method to process sub grid results...
    /// </summary>
    /// <param name="subGrids"></param>
    public void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      SummariseSubgridResultAsync(subGrids).WaitAndUnwrapException();
    }
  }
}
