using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Volumes.Executors
{
  /// <summary>
  /// Computes a progressive volumes calculation within a partition in the cache compute cluster
  /// </summary>
  public class ComputeProgressiveVolumes_Coordinator
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeProgressiveVolumes_Coordinator>();

    /// <summary>
    /// The ID of the site model the volume is being calculated for 
    /// </summary>
    public Guid SiteModelID;

    /// <summary>
    /// The volume computation method to use when calculating volume information
    /// </summary>
    public VolumeComputationType VolumeType;

    public ICombinedFilter Filter;

    /// <summary>
    /// The ID of the 'base' design together with its offset for a reference surface.
    /// This is the design forming the 'from' surface in  the volumes calculation
    /// </summary>
    private readonly DesignOffset _baseDesign;

    /// <summary>
    /// The ID of the 'to or top' design together with its offset for a reference surface.
    /// This is the design forming the 'to or top' surface in  the volumes calculation
    /// </summary>
    private readonly DesignOffset _topDesign;

    /// <summary>
    /// AdditionalSpatialFilter is an additional boundary specified by the user to bound the result of the query
    /// </summary>
    public ICombinedFilter AdditionalSpatialFilter;

    /// <summary>
    /// CutTolerance determines the tolerance (in meters) that the 'From' surface
    /// needs to be above the 'To' surface before the two surfaces are not
    /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    /// be cut
    /// </summary>
    public double CutTolerance;

    /// <summary>
    /// FillTolerance determines the tolerance (in meters) that the 'To' surface
    /// needs to be above the 'From' surface before the two surfaces are not
    /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    /// be filled
    /// </summary>
    public double FillTolerance;

    /// <summary>
    /// The aggregator to be used to compute the volumes related results
    /// </summary>
    public ProgressiveVolumesCalculationsAggregator Aggregator { get; set; }

    /// <summary>
    ///  Local reference to the site model to be used during processing
    /// </summary>
    private ISiteModel _siteModel;

    /// <summary>
    /// Parameters for lift analysis
    /// </summary>
    private readonly ILiftParameters _liftParams;

    public readonly DateTime StartDate;
    public readonly DateTime EndDate;
    public readonly TimeSpan Interval;

    /// <summary>
    /// Performs functional initialization of ComputeVolumes state that is dependent on the initial state
    /// set via the constructor
    /// </summary>
    /// <param name="computeVolumes"></param>
    private void InitialiseVolumesCalculator(ProgressiveVolumesCalculator computeVolumes)
    {
      // Set up the volumes calc parameters
      switch (VolumeType)
      {
        case VolumeComputationType.Between2Filters:
          computeVolumes.FromSelectionType = ProdReportSelectionType.Filter;
          computeVolumes.ToSelectionType = ProdReportSelectionType.Filter;
          break;

        case VolumeComputationType.BetweenFilterAndDesign:
          computeVolumes.FromSelectionType = ProdReportSelectionType.Filter;
          computeVolumes.ToSelectionType = ProdReportSelectionType.Filter;
          break;

        case VolumeComputationType.BetweenDesignAndFilter:
          computeVolumes.FromSelectionType = ProdReportSelectionType.Surface;
          computeVolumes.ToSelectionType = ProdReportSelectionType.Filter;
          break;
      }

      computeVolumes.RefOriginal = _baseDesign == null || _baseDesign.DesignID == Guid.Empty ? null : _siteModel.Designs.Locate(_baseDesign.DesignID);
      computeVolumes.RefDesign = _topDesign == null || _topDesign.DesignID == Guid.Empty ? null : _siteModel.Designs.Locate(_topDesign.DesignID);

      if (computeVolumes.FromSelectionType == ProdReportSelectionType.Surface)
      {
        computeVolumes.ActiveDesign = computeVolumes.RefOriginal != null ? new DesignWrapper(_baseDesign, computeVolumes.RefOriginal) : null;
      }
      else
      {
        computeVolumes.ActiveDesign = computeVolumes.ToSelectionType == ProdReportSelectionType.Surface && computeVolumes.RefDesign != null
          ? new DesignWrapper(_topDesign, computeVolumes.RefDesign)
          : null;
      }

      // Assign the active design into the aggregator for use
      Aggregator.ActiveDesign = computeVolumes.ActiveDesign;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="startDate">The date at which to start calculating progressive sub grids</param>
    /// <param name="endDate">The date beyond which no further progressive sub grids will be calculated</param>
    /// <param name="interval">The time interval between successive calculations of progressive sub grids</param>
    public ComputeProgressiveVolumes_Coordinator(Guid siteModelId,
      ILiftParameters liftParams,
      VolumeComputationType volumeType,
      ICombinedFilter filter,
      DesignOffset baseDesign,
      DesignOffset topDesign,
      ICombinedFilter additionalSpatialFilter,
      double cutTolerance,
      double fillTolerance,
      DateTime startDate,
      DateTime endDate,
      TimeSpan interval)
    {
      SiteModelID = siteModelId;
      VolumeType = volumeType;
      Filter = filter;
      _baseDesign = baseDesign;
      _topDesign = topDesign;
      AdditionalSpatialFilter = additionalSpatialFilter;
      CutTolerance = cutTolerance;
      FillTolerance = fillTolerance;
      _liftParams = liftParams;
      StartDate = startDate;
      EndDate = endDate;
      Interval = interval;
    }

    /// <summary>
    /// Executes the progressive volumes computation returning a ProgressiveVolumesResponse with the results
    /// </summary>
    /// <returns></returns>
    public async Task<ProgressiveVolumesResponse> ExecuteAsync()
    {
      var volumesResult = new ProgressiveVolumesResponse();
      var resultBoundingExtents = BoundingWorldExtent3D.Null();
      var requestDescriptor = Guid.NewGuid(); // TODO ASNodeImplInstance.NextDescriptor;

      Log.LogInformation($"#In# Performing {nameof(ComputeProgressiveVolumes_Coordinator)}.Execute for DataModel:{SiteModelID}");

      try
      {
        try
        {
          ApplicationServiceRequestStatistics.Instance.NumProgressiveVolumeRequests.Increment();

          // Prepare filter for use in the request
          var resultStatus = await FilterUtilities.PrepareFiltersForUse(new[] { Filter, AdditionalSpatialFilter }, SiteModelID);
          if (resultStatus != RequestErrorStatus.OK)
            return volumesResult;

          // Obtain the site model context for the request
          _siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(SiteModelID);

          if (_siteModel == null)
            return volumesResult;

          // Determine the number of progressions that are required and establish the required aggregation states in the aggregator
          var numProgressions = (int)((EndDate.Ticks - StartDate.Ticks) / Interval.Ticks) + 1;
          if ((EndDate.Ticks - StartDate.Ticks) % Interval.Ticks == 0)
          {
            numProgressions++;
          }

          if (numProgressions > ClientProgressiveHeightsLeafSubGrid.MaxNumProgressions)
          {
            throw new ArgumentException($"No more than {ClientProgressiveHeightsLeafSubGrid.MaxNumProgressions} progressions may be requested at one time");
          }

          // Create and configure the aggregator that contains the business logic for the underlying volume calculation
          Aggregator = new ProgressiveVolumesCalculationsAggregator
          {
            SiteModelID = SiteModelID,
            LiftParams = _liftParams,
            CellSize = _siteModel.CellSize,
            VolumeType = VolumeType,
            CutTolerance = CutTolerance,
            FillTolerance = FillTolerance,
            AggregationStates = Enumerable
              .Range(0, numProgressions)
              .Select(x => StartDate + x * Interval)
              .Select(d => new ProgressiveVolumeAggregationState
            {
                Date = d,
                VolumeType = VolumeType,
                CellSize = _siteModel.CellSize,
                CutTolerance = CutTolerance,
                FillTolerance = FillTolerance
              }).ToArray()
          };

          // Create and configure the volumes calculation engine
          var computeVolumes = new ProgressiveVolumesCalculator
          {
            RequestDescriptor = requestDescriptor,
            SiteModel = _siteModel,
            Aggregator = Aggregator,
            Filter = Filter,
            VolumeType = VolumeType,
            LiftParams = _liftParams,
            StartDate = StartDate,
            EndDate = EndDate,
            Interval = Interval
          };

          InitialiseVolumesCalculator(computeVolumes);

          // Perform the volume computation
          if (await computeVolumes.ComputeVolumeInformation())
          {
            resultStatus = RequestErrorStatus.OK;
          }
          else
          {
            resultStatus = computeVolumes.AbortedDueToTimeout ? RequestErrorStatus.AbortedDueToPipelineTimeout : RequestErrorStatus.Unknown;
          }

          if (resultStatus != RequestErrorStatus.OK)
          {
            Log.LogInformation($"Progressive volume result: Failure, error = {resultStatus}");

            // Send the (empty) results back to the caller
            return volumesResult;
          }

          // Instruct the Aggregator to perform any finalization logic before reading out the results
          Aggregator.Finalise();

          foreach (var aggregator in Aggregator.AggregationStates)
          {
            Log.LogInformation($"#Result# Progressive volume result: Cut={aggregator.CutFillVolume.CutVolume:F3}, Fill={aggregator.CutFillVolume.FillVolume:F3}, Area={aggregator.CoverageArea:F3}");

            if (!aggregator.BoundingExtents.IsValidPlanExtent)
            {
              if (aggregator.CoverageArea == 0 && aggregator.CutFillVolume.CutVolume == 0 && aggregator.CutFillVolume.FillVolume == 0)
                resultStatus = RequestErrorStatus.NoProductionDataFound;
              else
                resultStatus = RequestErrorStatus.InvalidPlanExtents;

              Log.LogInformation($"Progressive volume invalid plan extents or no data found: {resultStatus}");
            }
          }

          volumesResult.ResultStatus = resultStatus;
          if (resultStatus != RequestErrorStatus.OK)
          {
            volumesResult.Volumes = Aggregator.AggregationStates.Select(aggregator => new ProgressiveVolumeResponseItem
            {
              Date = aggregator.Date,
              Volume = new SimpleVolumesResponse
              {
                Cut = aggregator.CutFillVolume.CutVolume,
                Fill = aggregator.CutFillVolume.FillVolume,
                TotalCoverageArea = aggregator.CoverageArea,
                CutArea = aggregator.CutArea,
                FillArea = aggregator.FillArea,
                BoundingExtentGrid = aggregator.BoundingExtents,
                BoundingExtentLLH = resultBoundingExtents
               }
            }).ToArray();
          }
        }
        finally
        {
          ApplicationServiceRequestStatistics.Instance.NumProgressiveVolumeRequestsCompleted.Increment();
          if (volumesResult.ResultStatus != RequestErrorStatus.OK)
          {
            ApplicationServiceRequestStatistics.Instance.NumProgressiveVolumeRequestsFailed.Increment();
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Failed to compute the progressive volumes. Site Model ID: {SiteModelID}");
      }

      return volumesResult;
    }
  }
}
