using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.RequestStatistics;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.Executors
{
    /// <summary>
    /// Computes a simple volumes calculation within a partition in the cache compute cluster
    /// </summary>
    public class ComputeSimpleVolumes_Coordinator
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(ComputeSimpleVolumes_Coordinator));

        /// <summary>
        /// The ID of the site model the volume is being calculated for 
        /// </summary>
        public Guid SiteModelID;
       
        //ExternalDescriptor : TASNodeRequestDescriptor;

        /// <summary>
        /// The volume computation method to use when calculating volume information
        /// </summary>
        public VolumeComputationType VolumeType;

        // FLiftBuildSettings : TICLiftBuildSettings;

        /// <summary>
        /// BaseFilter and TopFilter reference two sets of filter settings
        /// between which we may calculate volumes. At the current time, it is
        /// meaningful for a filter to have a spatial extent, and to denote aa
        /// 'as-at' time only.
        /// </summary>
        public ICombinedFilter BaseFilter;

        public ICombinedFilter TopFilter;

        /// <summary>
        /// The ID of the 'base' design. This is the design forming the 'from' surface in 
        /// the volumes calculation
        /// </summary>
        Guid BaseDesignID;

        /// <summary>
        /// The ID of the 'to or top' design. This is the design forming the 'to or top' surface in 
        /// the volumes calculation
        /// </summary>
        Guid TopDesignID;

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
        ///  Local reference to the sitemodel to be used during processing
        /// </summary>
        private ISiteModel siteModel;

        /// <summary>
        /// Performs functional initialiaation of ComputeVolumes state that is dependent on the initial state
        /// set via the constructor
        /// </summary>
        /// <param name="ComputeVolumes"></param>
        private void InitialiseVolumesCalculator(VolumesCalculator ComputeVolumes)
        {
            // Set up the volumes calc parameters
            switch (VolumeType)
            {
                case VolumeComputationType.Between2Filters:
                    ComputeVolumes.FromSelectionType = ProdReportSelectionType.Filter;
                    ComputeVolumes.ToSelectionType = ProdReportSelectionType.Filter;
                    break;

                case VolumeComputationType.BetweenFilterAndDesign:
                    ComputeVolumes.FromSelectionType = ProdReportSelectionType.Filter;
                    ComputeVolumes.ToSelectionType = ProdReportSelectionType.Filter;
                    break;

                case VolumeComputationType.BetweenDesignAndFilter:
                    ComputeVolumes.FromSelectionType = ProdReportSelectionType.Surface;
                    ComputeVolumes.ToSelectionType = ProdReportSelectionType.Filter;
                    break;
            }

            ComputeVolumes.UseEarliestData = BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass;

            ComputeVolumes.RefOriginal = BaseDesignID == Guid.Empty ? null : siteModel.Designs.Locate(BaseDesignID);
            ComputeVolumes.RefDesign = TopDesignID == Guid.Empty ? null : siteModel.Designs.Locate(TopDesignID);

            if (ComputeVolumes.FromSelectionType == ProdReportSelectionType.Surface)
                ComputeVolumes.ActiveDesign = ComputeVolumes.RefOriginal;
            else
                ComputeVolumes.ActiveDesign = ComputeVolumes.ToSelectionType == ProdReportSelectionType.Surface ? ComputeVolumes.RefDesign : null;

            // Assign the active design into the aggregator for use
            Aggregator.ActiveDesign = ComputeVolumes.ActiveDesign;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="volumeType"></param>
        /// <param name="baseFilter"></param>
        /// <param name="topFilter"></param>
        /// <param name="baseDesignID"></param>
        /// <param name="topDesignID"></param>
        /// <param name="additionalSpatialFilter"></param>
        /// <param name="cutTolerance"></param>
        /// <param name="fillTolerance"></param>
        public ComputeSimpleVolumes_Coordinator(Guid siteModelID,
                                    //ExternalDescriptor : TASNodeRequestDescriptor;
                                    //LiftBuildSettings : TICLiftBuildSettings;
                                    VolumeComputationType volumeType,
                                    ICombinedFilter baseFilter,
                                    ICombinedFilter topFilter,
                                    Guid baseDesignID,
                                    Guid topDesignID,
                                    ICombinedFilter additionalSpatialFilter,
                                    double cutTolerance,
                                    double fillTolerance)
        {
            SiteModelID = siteModelID;
            VolumeType = volumeType;
            BaseFilter = baseFilter;
            TopFilter = topFilter;
            BaseDesignID = baseDesignID;
            TopDesignID = topDesignID;
            AdditionalSpatialFilter = additionalSpatialFilter;
            CutTolerance = cutTolerance;
            FillTolerance = fillTolerance;
        }

        /// <summary>
        /// Executes the simple volumes computation returning a SimpleVolumesResponse with the results
        /// </summary>
        /// <returns></returns>
        public SimpleVolumesResponse Execute()
        {
            SimpleVolumesResponse VolumesResult = new SimpleVolumesResponse();

            BoundingWorldExtent3D ResultBoundingExtents = BoundingWorldExtent3D.Null();
//            BoundingWorldExtent3D SpatialExtent = BoundingWorldExtent3D.Null();
//            long[] SurveyedSurfaceExclusionList = new long[0];

            Guid RequestDescriptor = Guid.NewGuid(); // TODO ASNodeImplInstance.NextDescriptor;

            Log.LogInformation($"#In# Performing {nameof(ComputeSimpleVolumes_Coordinator)}.Execute for DataModel:{SiteModelID}");

            try
            {
                try
                {
                    /*
                     if Assigned(ASNodeImplInstance.RequestCancellations) and
                         ASNodeImplInstance.RequestCancellations.IsRequestCancelled(FExternalDescriptor) then
                         begin
                            SIGLogMessage.PublishNoODS(Self, 'Request cancelled: ' + FExternalDescriptor.ToString, slmcDebug);
                            ResultStatus:= asneRequestHasBeenCancelled;
                            Exit;
                         end;

                        ScheduledWithGovernor:= ASNodeImplInstance.Governor.Schedule(FExternalDescriptor, Self, gqVolumes, ResultStatus);
                        if not ScheduledWithGovernor then
                          Exit;

                        if ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetDataModelSpatialExtents(FDataModelID, SurveyedSurfaceExclusionList, SpatialExtent, CellSize, IndexOriginOffset) <> icsrrNoError then
                          begin
                            ResultStatus:= asneFailedToRequestDatamodelStatistics;
                            Exit;
                          end;
                    */

                    ApplicationServiceRequestStatistics.Instance.NumVolumeRequests.Increment();

                    // Prepare filters for use in the request
                    RequestErrorStatus ResultStatus = FilterUtilities.PrepareFiltersForUse(new [] { BaseFilter, TopFilter, AdditionalSpatialFilter }, SiteModelID);
                    if (ResultStatus != RequestErrorStatus.OK)
                        return VolumesResult;

                    // Obtain the site model context for the request
                    siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(SiteModelID);

                    if (siteModel == null)
                        return VolumesResult;

                    // Create and configure the aggregator that contains the business logic for the 
                    // underlying volume calculation
                    Aggregator = new SimpleVolumesCalculationsAggregator
                    {
                        RequiresSerialisation = true,
                        SiteModelID = SiteModelID,
                        //LiftBuildSettings := LiftBuildSettings;
                        CellSize = siteModel.Grid.CellSize,
                        VolumeType = VolumeType,
                        CutTolerance = CutTolerance,
                        FillTolerance = FillTolerance
                    };

                    // Create and configure the volumes calculation engine
                    VolumesCalculator ComputeVolumes = new VolumesCalculator
                    {
                        RequestDescriptor = RequestDescriptor,
                        SiteModel = siteModel,
                        Aggregator = Aggregator,
                        BaseFilter = BaseFilter,
                        TopFilter = TopFilter,
                        VolumeType = VolumeType
                    };

                    InitialiseVolumesCalculator(ComputeVolumes);

                    // Perform the volume computation
                    if (ComputeVolumes.ComputeVolumeInformation())
                        ResultStatus = RequestErrorStatus.OK;
                    else
                        if (ComputeVolumes.AbortedDueToTimeout)
                        ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
                    else
                        ResultStatus = RequestErrorStatus.Unknown;

                    if (ResultStatus != RequestErrorStatus.OK)
                    {
                      Log.LogInformation($"Summary volume result: Failure, error = {ResultStatus}");

                      // Send the (empty) results back to the caller
                      return VolumesResult;
                    }

                    // Instruct the Aggregator to perform any finalization logic before reading out the results
                    Aggregator.Finalise();

                    Log.LogInformation($"#Result# Summary volume result: Cut={Aggregator.CutFillVolume.CutVolume:F3}, Fill={Aggregator.CutFillVolume.FillVolume:F3}, Area={Aggregator.CoverageArea:F3}");

                    if (!Aggregator.BoundingExtents.IsValidPlanExtent)
                    {
                        Log.LogInformation("Summary volume invalid PlanExtents. Possibly no data found");

                        if (Aggregator.CoverageArea == 0 && Aggregator.CutFillVolume.CutVolume == 0 && Aggregator.CutFillVolume.FillVolume == 0)
                            ResultStatus = RequestErrorStatus.NoProductionDataFound;
                        else
                            ResultStatus = RequestErrorStatus.InvalidPlanExtents;

                        return VolumesResult;
                    }

                    // Fill in the result object to pass back to the caller
                    VolumesResult.Cut = Aggregator.CutFillVolume.CutVolume;
                    VolumesResult.Fill = Aggregator.CutFillVolume.FillVolume;
                    VolumesResult.TotalCoverageArea = Aggregator.CoverageArea;
                    VolumesResult.CutArea = Aggregator.CutArea;
                    VolumesResult.FillArea = Aggregator.FillArea;
                    VolumesResult.BoundingExtentGrid = Aggregator.BoundingExtents;
                    VolumesResult.BoundingExtentLLH = ResultBoundingExtents;
                }
                finally
                {
                    ApplicationServiceRequestStatistics.Instance.NumVolumeRequestsCompleted.Increment();
                    if (VolumesResult.ResponseCode != SubGridRequestsResponseResult.OK)
                        ApplicationServiceRequestStatistics.Instance.NumVolumeRequestsFailed.Increment();
                }
            }
            catch (Exception E)
            {
                Log.LogError("Exception:", E);
            }

            return VolumesResult;
        }
    }
}
