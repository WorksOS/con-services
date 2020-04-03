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
    public class ComputeSimpleVolumes_Coordinator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<ComputeSimpleVolumes_Coordinator>();

        /// <summary>
        /// The ID of the site model the volume is being calculated for 
        /// </summary>
        public Guid SiteModelID;
       
        /// <summary>
        /// The volume computation method to use when calculating volume information
        /// </summary>
        public VolumeComputationType VolumeType;

        /// <summary>
        /// BaseFilter and TopFilter reference two sets of filter settings
        /// between which we may calculate volumes. At the current time, it is
        /// meaningful for a filter to have a spatial extent, and to denote aa
        /// 'as-at' time only.
        /// </summary>
        public ICombinedFilter BaseFilter;

        public ICombinedFilter TopFilter;

        /// <summary>
        /// The ID of the 'base' design together with its offset for a reference surface.
        /// This is the design forming the 'from' surface in  the volumes calculation
        /// </summary>
        private readonly DesignOffset BaseDesign;

        /// <summary>
        /// The ID of the 'to or top' design together with its offset for a reference surface.
        /// This is the design forming the 'to or top' surface in  the volumes calculation
        /// </summary>
        private readonly DesignOffset TopDesign;

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
        private ISiteModel siteModel;

        /// <summary>
        /// Parameters for lift analysis
        /// </summary>
        private ILiftParameters LiftParams;

        /// <summary>
        /// Performs functional initialization of ComputeVolumes state that is dependent on the initial state
        /// set via the constructor
        /// </summary>
        /// <param name="computeVolumes"></param>
        private void InitialiseVolumesCalculator(VolumesCalculator computeVolumes)
        {
            // Set up the volumes calc parameters
            VolumesUtilities.SetProdReportSelectionType(VolumeType, out var fromSelectionType, out var toSelectionType);
            computeVolumes.FromSelectionType = fromSelectionType;
            computeVolumes.ToSelectionType = toSelectionType;

            computeVolumes.UseEarliestData = BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass;

            computeVolumes.RefOriginal = BaseDesign == null || BaseDesign.DesignID == Guid.Empty ? null : siteModel.Designs.Locate(BaseDesign.DesignID);
            computeVolumes.RefDesign = TopDesign == null || TopDesign.DesignID == Guid.Empty ? null : siteModel.Designs.Locate(TopDesign.DesignID);

            if (computeVolumes.FromSelectionType == ProdReportSelectionType.Surface)
            {
              computeVolumes.ActiveDesign = computeVolumes.RefOriginal != null ? new DesignWrapper(BaseDesign, computeVolumes.RefOriginal) : null;
            }
            else
            {
              computeVolumes.ActiveDesign = computeVolumes.ToSelectionType == ProdReportSelectionType.Surface && computeVolumes.RefDesign != null
                ? new DesignWrapper(TopDesign, computeVolumes.RefDesign) 
                : null;
            }

            // Assign the active design into the aggregator for use
            Aggregator.ActiveDesign = computeVolumes.ActiveDesign;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ComputeSimpleVolumes_Coordinator(Guid siteModelID,
                                    ILiftParameters liftParams,
                                    VolumeComputationType volumeType,
                                    ICombinedFilter baseFilter,
                                    ICombinedFilter topFilter,
                                    DesignOffset baseDesign,
                                    DesignOffset topDesign,
                                    ICombinedFilter additionalSpatialFilter,
                                    double cutTolerance,
                                    double fillTolerance)
        {
            SiteModelID = siteModelID;
            VolumeType = volumeType;
            BaseFilter = baseFilter;
            TopFilter = topFilter;
            BaseDesign = baseDesign;
            TopDesign = topDesign;
            AdditionalSpatialFilter = additionalSpatialFilter;
            CutTolerance = cutTolerance;
            FillTolerance = fillTolerance;
            LiftParams = liftParams;
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
                    ApplicationServiceRequestStatistics.Instance.NumSimpleVolumeRequests.Increment();

                    // Prepare filters for use in the request
                    var resultStatus = await FilterUtilities.PrepareFiltersForUse(new [] { BaseFilter, TopFilter, AdditionalSpatialFilter }, SiteModelID);
                    if (resultStatus != RequestErrorStatus.OK)
                        return volumesResult;

                    // Obtain the site model context for the request
                    siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(SiteModelID);

                    if (siteModel == null)
                        return volumesResult;

                    // Create and configure the aggregator that contains the business logic for the 
                    // underlying volume calculation
                    Aggregator = new SimpleVolumesCalculationsAggregator
                    {
                        RequiresSerialisation = true,
                        SiteModelID = SiteModelID,
                        LiftParams = LiftParams,
                        CellSize = siteModel.CellSize,
                        VolumeType = VolumeType,
                        CutTolerance = CutTolerance,
                        FillTolerance = FillTolerance
                    };

                    // Create and configure the volumes calculation engine
                    var computeVolumes = new VolumesCalculator
                    {
                        RequestDescriptor = requestDescriptor,
                        SiteModel = siteModel,
                        Aggregator = Aggregator,
                        BaseFilter = BaseFilter,
                        TopFilter = TopFilter,
                        VolumeType = VolumeType,
                        LiftParams = LiftParams
                    };

                    InitialiseVolumesCalculator(computeVolumes);

                    // Perform the volume computation
                    if (computeVolumes.ComputeVolumeInformation())
                        resultStatus = RequestErrorStatus.OK;
                    else
                        if (computeVolumes.AbortedDueToTimeout)
                        resultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
                    else
                        resultStatus = RequestErrorStatus.Unknown;

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

                        Log.LogInformation($"Summary volume invalid PlanExtents or no data found: {resultStatus}");

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
                    ApplicationServiceRequestStatistics.Instance.NumSimpleVolumeRequestsCompleted.Increment();
                    if (volumesResult.ResponseCode != SubGridRequestsResponseResult.OK)
                        ApplicationServiceRequestStatistics.Instance.NumSimpleVolumeRequestsFailed.Increment();
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
