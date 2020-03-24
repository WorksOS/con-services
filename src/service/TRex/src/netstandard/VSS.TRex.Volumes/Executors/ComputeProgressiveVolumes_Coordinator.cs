using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.Common.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Volumes.Executors
{
  /// <summary>
  /// Computes a simple volumes calculation within a partition in the cache compute cluster
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
    public SimpleVolumesCalculationsAggregator Aggregator { get; set; }

    /// <summary>
    ///  Local reference to the site model to be used during processing
    /// </summary>
    private ISiteModel _siteModel;

    /// <summary>
    /// Parameters for lift analysis
    /// </summary>
    private readonly ILiftParameters _liftParams;

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
    public ComputeProgressiveVolumes_Coordinator(Guid siteModelId,
      ILiftParameters liftParams,
      VolumeComputationType volumeType,
      ICombinedFilter filter,
      DesignOffset baseDesign,
      DesignOffset topDesign,
      ICombinedFilter additionalSpatialFilter,
      double cutTolerance,
      double fillTolerance)
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
    }

    /// <summary>
    /// Executes the simple volumes computation returning a SimpleVolumesResponse with the results
    /// </summary>
    /// <returns></returns>
    public async Task<SimpleVolumesResponse> ExecuteAsync()
    {
      var volumesResult = new SimpleVolumesResponse();
      var resultBoundingExtents = BoundingWorldExtent3D.Null();
      var requestDescriptor = Guid.NewGuid(); // TODO ASNodeImplInstance.NextDescriptor;

      Log.LogInformation($"#In# Performing {nameof(ComputeSimpleVolumes_Coordinator)}.Execute for DataModel:{SiteModelID}");

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

          // Create and configure the aggregator that contains the business logic for the 
          // underlying volume calculation
          Aggregator = new SimpleVolumesCalculationsAggregator
          {
            RequiresSerialisation = true,
            SiteModelID = SiteModelID,
            LiftParams = _liftParams,
            CellSize = _siteModel.CellSize,
            VolumeType = VolumeType,
            CutTolerance = CutTolerance,
            FillTolerance = FillTolerance
          };

          // Create and configure the volumes calculation engine
          var computeVolumes = new ProgressiveVolumesCalculator
          {
            RequestDescriptor = requestDescriptor,
            SiteModel = _siteModel,
            Aggregator = Aggregator,
            Filter = Filter,
            VolumeType = VolumeType,
            LiftParams = _liftParams
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
            Log.LogInformation($"Summary volume result: Failure, error = {resultStatus}");

            // Send the (empty) results back to the caller
            return volumesResult;
          }

          // Instruct the Aggregator to perform any finalization logic before reading out the results
          Aggregator.Finalise();

          Log.LogInformation($"#Result# Summary volume result: Cut={Aggregator.CutFillVolume.CutVolume:F3}, Fill={Aggregator.CutFillVolume.FillVolume:F3}, Area={Aggregator.CoverageArea:F3}");

          if (!Aggregator.BoundingExtents.IsValidPlanExtent)
          {
            if (Aggregator.CoverageArea == 0 && Aggregator.CutFillVolume.CutVolume == 0 && Aggregator.CutFillVolume.FillVolume == 0)
              resultStatus = RequestErrorStatus.NoProductionDataFound;
            else
              resultStatus = RequestErrorStatus.InvalidPlanExtents;

            Log.LogInformation($"Summary volume invalid plan extents or no data found: {resultStatus}");

            return volumesResult;
          }

          // Fill in the result object to pass back to the caller
          volumesResult.Cut = Aggregator.CutFillVolume.CutVolume;
          volumesResult.Fill = Aggregator.CutFillVolume.FillVolume;
          volumesResult.TotalCoverageArea = Aggregator.CoverageArea;
          volumesResult.CutArea = Aggregator.CutArea;
          volumesResult.FillArea = Aggregator.FillArea;
          volumesResult.BoundingExtentGrid = Aggregator.BoundingExtents;
          volumesResult.BoundingExtentLLH = resultBoundingExtents;
        }
        finally
        {
          ApplicationServiceRequestStatistics.Instance.NumProgressiveVolumeRequestsCompleted.Increment();
          if (volumesResult.ResponseCode != SubGridRequestsResponseResult.OK)
            ApplicationServiceRequestStatistics.Instance.NumProgressiveVolumeRequestsFailed.Increment();
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Failed to compute the simple volumes. Site Model ID: {SiteModelID}");
      }

      return volumesResult;
    }
  }
}
