using System;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.Volumes.Executors
{
    /// <summary>
    /// Computes a simple volumes calculation within a partition in the cache compute cluster
    /// </summary>
    public class ComputeSimpleVolumes_Coordinator
    {
        /// <summary>
        /// The ID of the site model the volume is being calculated for 
        /// </summary>
        public long SiteModelID = -1;

        //ExternalDescriptor : TASNodeRequestDescriptor;

        /// <summary>
        /// The volume computation method to use when calculating volume information
        /// </summary>
        public VolumeComputationType VolumeType = VolumeComputationType.None;

        // FLiftBuildSettings : TICLiftBuildSettings;

        /// <summary>
        /// BaseFilter and TopFilter reference two sets of filter settings
        /// between which we may calculate volumes. At the current time, it is
        /// meaingful for a filter to have a spatial extent, and to denote aa
        /// 'as-at' time only.
        /// </summary>
        public CombinedFilter BaseFilter;

        public CombinedFilter TopFilter;

        /// <summary>
        /// The ID of the 'base' design. This is the design forming the 'from' surface in 
        /// the volumes calculation
        /// </summary>
        long BaseDesignID = long.MinValue;

        /// <summary>
        /// The ID of the 'to or top' design. This is the design forming the 'to or top' surface in 
        /// the volumes calculation
        /// </summary>
        long TopDesignID = long.MinValue;

        /// <summary>
        /// AdditionalSpatialFilter is an additional boundary specified by the user to bound the result of the query
        /// </summary>
        public CombinedFilter AdditionalSpatialFilter;

        /// <summary>
        /// CutTolerance determines the tolerance (in meters) that the 'From' surface
        /// needs to be above the 'To' surface before the two surfaces are not
        /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
        /// be cut
        /// </summary>
        public double CutTolerance = VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE;

        /// <summary>
        /// FillTolerance determines the tolerance (in meters) that the 'To' surface
        /// needs to be above the 'From' surface before the two surfaces are not
        /// considered to be equivalent, or 'on-grade', and hence there is material still remaining to
        /// be filled
        /// </summary>
        public double FillTolerance = VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE;

        /// <summary>
        /// The aggregator to be used to compute the volumes related results
        /// </summary>
        public SimpleVolumesCalculationsAggregator Aggregator { get; set; }

        /// <summary>
        /// Performs funcional initialisation of ComnputeVolumes state that is dependent on the initial state
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

            ComputeVolumes.RefOriginal = BaseDesignID == long.MinValue ? null : Services.Designs.DesignsService.Instance().Find(SiteModelID, BaseDesignID);
            ComputeVolumes.RefDesign = TopDesignID == long.MinValue ? null : Services.Designs.DesignsService.Instance().Find(SiteModelID, TopDesignID);

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
        public ComputeSimpleVolumes_Coordinator(long siteModelID,
                                    //ExternalDescriptor : TASNodeRequestDescriptor;
                                    //LiftBuildSettings : TICLiftBuildSettings;
                                    VolumeComputationType volumeType,
                                    CombinedFilter baseFilter,
                                    CombinedFilter topFilter,
                                    long baseDesignID,
                                    long topDesignID,
                                    CombinedFilter additionalSpatialFilter,
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

            long RequestDescriptor = Guid.NewGuid().GetHashCode(); // TODO ASNodeImplInstance.NextDescriptor;

            //NEECoords: TCSConversionCoordinates;
            //LLHCoords: TCSConversionCoordinates;
            //CoordConversionResult: TCoordServiceErrorStatus;

            // TODO: Readd when loggin available
            // SIGLogMessage.PublishNoODS(Self, Format('#In# Performing %s.Execute for DataModel:%d', [Self.ClassName, FDataModelID]), slmcMessage);

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

                    // InterlockedIncrement64(ASNodeRequestStats.NumVolumeRequests);

                    // Prepare filters for use in the request
                    RequestErrorStatus ResultStatus = FilterUtilities.PrepareFiltersForUse(new [] { BaseFilter, TopFilter, AdditionalSpatialFilter }, SiteModelID);
                    if (ResultStatus != RequestErrorStatus.OK)
                        return VolumesResult;

                    // Obtain the site model context for the request
                    SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(SiteModelID);

                    if (SiteModel == null)
                        return VolumesResult;

                    // Create and configure the aggregator that contains the business logic for the 
                    // underlying volume calculation
                    Aggregator = new SimpleVolumesCalculationsAggregator()
                    {
                        RequiresSerialisation = true,
                        SiteModelID = SiteModelID,
                        //LiftBuildSettings := LiftBuildSettings;
                        CellSize = SiteModel.Grid.CellSize,
                        VolumeType = VolumeType,
                        CutTolerance = CutTolerance,
                        FillTolerance = FillTolerance
                    };

                    // Create and configure the volumes calculation engine
                    VolumesCalculator ComputeVolumes = new VolumesCalculator
                    {
                        RequestDescriptor = RequestDescriptor,
                        SiteModel = SiteModel,
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
                        // TODO Readd when logging available
                        // SIGLogMessage.PublishNoODS(Self, Format('Summary volume result: Failure, error = %d', [Ord(ResultStatus)]), slmcMessage);

                        // Send the (empty) results back to the caller
                        return VolumesResult;
                    }

                    /* TODO: Readd when logging available
                    SIGLogMessage.PublishNoODS(Self, Format('#Result# Summary volume result: Cut=%.3f, Fill=%.3f, Area=%.3f',            
                                              [CutFillVolume.CutVolume, CutFillVolume.FillVolume, CoverageArea]), slmcMessage);
                    */

                    // Instruct the Aggregator to perform any finalisation logic before reading out the results
                    Aggregator.Finalise();

                    if (!Aggregator.BoundingExtents.IsValidPlanExtent)
                    {
                        // TODO: Readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, 'Summary volume invalid PlanExtents. Possibly no data found', slmcMessage);
                        if (Aggregator.CoverageArea == 0 && Aggregator.CutFillVolume.CutVolume == 0 && Aggregator.CutFillVolume.FillVolume == 0)
                            ResultStatus = RequestErrorStatus.NoProductionDataFound;
                        else
                            ResultStatus = RequestErrorStatus.InvalidPlanExtents;

                        return VolumesResult;
                    }

                    // Convert bounding extents grid coordinates into WGS84 ones...
                    /* TODO readd when coordinate conversion available
                    SetLength(NEECoords, 2);
                    NEECoords[0].Create(BoundingExtents.MinX, BoundingExtents.MinY);
                    NEECoords[1].Create(BoundingExtents.MaxX, BoundingExtents.MaxY);

                    CoordConversionResult:= ASNodeImplInstance.CoordService.RequestCoordinateConversion(RequestDescriptor, FDataModelID, cctNEEtoLLH, NEECoords, EmptyStr, LLHCoords);

                    if CoordConversionResult <> csOK then
                      begin
                        SIGLogMessage.PublishNoODS(Self, 'Summary volume failure, could not convert bounding area from grid to WGS coordinates', slmcError);
                        ResultStatus:= asneFailedToConvertClientWGSCoords;
                        Exit;
                     end;

                    ResultBoundingExtents = new BoundingWorldExtent3D(RadToDeg(LLHCoords[0].X),
                                                 RadToDeg(LLHCoords[0].Y),
                                                 RadToDeg(LLHCoords[1].X),
                                                 RadToDeg(LLHCoords[1].Y));
                    */

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
                    //InterlockedIncrement64(ASNodeRequestStats.NumVolumeRequestsCompleted);
                    //if ResultStatus <> asneOK then
                    //  InterlockedIncrement64(ASNodeRequestStats.NumVolumeRequestsFailed);
                }
            }
            catch //(Exception E)
            {
                // TODO readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('%s.Execute: Exception "%s"', [Self.ClassName, E.Message]), slmcException);
            }

            return VolumesResult;
        }
    }
}
