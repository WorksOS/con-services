using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Utilities;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;
using static VSS.VisionLink.Raptor.Volumes.VolumesCalculatorBase;
using VSS.VisionLink.Raptor.SiteModels;

namespace VSS.VisionLink.Raptor.Volumes.Executors
{
    /// <summary>
    /// Computes a simple volumes calculation within a partition in the cache compute cluster
    /// </summary>
    public class ComputeSimpleVolumes_Coordinator
    {
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
        public CombinedFilter BaseFilter = null;
        public CombinedFilter TopFilter = null;

        // DesignDescriptor BaseDesign = DesignDescriptor.Null();
        // DesignDescriptor TopDesign = DesignDescriptor.Null();

        long BaseDesignID = long.MinValue;
        long TopDesignID = long.MinValue;

        /// <summary>
        /// BaseSpatialFilter, TopSpatialFilter and AdditionalSpatialFilter are
        /// three filters used to implement the spatial restrictions represented by
        /// spatial restrictions in the base and top filters and the additional
        /// boundary specified by the user.
        /// </summary>
        public CombinedFilter AdditionalSpatialFilter = null;

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
        /// Performs funcional initialisation of ComnputeVolumes state that is dependent on the initial state
        /// set via the constructor
        /// </summary>
        /// <param name="ComputeVolumes"></param>
        private void InitialiseVolumesCalculator(VolumesCalculator ComputeVolumes)
        {
            // Set up the volumes calc parameters
            switch (ComputeVolumes.Aggregator.VolumeType)
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

        public SimpleVolumesResponse Execute()
        {
            SimpleVolumesResponse VolumesResult = new SimpleVolumesResponse();

            BoundingWorldExtent3D ResultBoundingExtents = BoundingWorldExtent3D.Null();
            BoundingWorldExtent3D SpatialExtent = BoundingWorldExtent3D.Null();
            long[] SurveyedSurfaceExclusionList = new long[0];

            RequestErrorStatus ResultStatus = RequestErrorStatus.Unknown;

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

                        SetLength(SurveyedSurfaceExclusionList, 0);

                        if ASNodeImplInstance.PSLoadBalancer.LoadBalancedPSService.GetDataModelSpatialExtents(FDataModelID, SurveyedSurfaceExclusionList, SpatialExtent, CellSize, IndexOriginOffset) <> icsrrNoError then
                          begin
                            ResultStatus:= asneFailedToRequestDatamodelStatistics;
                            Exit;
                          end;
                    */

                    // InterlockedIncrement64(ASNodeRequestStats.NumVolumeRequests);

                    ResultStatus = FilterUtilities.PrepareFiltersForUse(new CombinedFilter[] { BaseFilter, TopFilter, AdditionalSpatialFilter }, SiteModelID);
                    if (ResultStatus != RequestErrorStatus.OK)
                    {
                        return VolumesResult;
                    }

                    SiteModel SiteModel = SiteModels.SiteModels.Instance().GetSiteModel(SiteModelID);

                    if (SiteModel == null)
                    {
                        return VolumesResult;
                    }

                    SimpleVolumesCalculationsAggregator Aggregator = new SimpleVolumesCalculationsAggregator()
                    {
                        RequiresSerialisation = true,
                        SiteModelID = SiteModelID,
                        //LiftBuildSettings := LiftBuildSettings;
                        CellSize = SiteModel.Grid.CellSize,
                        VolumeType = VolumeType,
                        CutTolerance = CutTolerance,
                        FillTolerance = FillTolerance
                    };

                    VolumesCalculator ComputeVolumes = new VolumesCalculator()
                    {
                        RequestDescriptor = RequestDescriptor,
                        SiteModel = SiteModel,
                        Aggregator = Aggregator,
                        BaseFilter = BaseFilter,
                        TopFilter = TopFilter
                    };

                    InitialiseVolumesCalculator(ComputeVolumes);

                    // Perform the volume computation
                    if (ComputeVolumes.ComputeVolumeInformation())
                    {
                        ResultStatus = RequestErrorStatus.OK;
                    }
                    else
                    {
                        if (ComputeVolumes.AbortedDueToTimeout)
                        {
                            ResultStatus = RequestErrorStatus.AbortedDueToPipelineTimeout;
                        }
                        else
                        {
                            ResultStatus = RequestErrorStatus.Unknown;
                        }
                    }

                    // Send the results back to the caller
                    if (ResultStatus != RequestErrorStatus.OK)
                    {
                        // TODO Readd when logging available
                        // SIGLogMessage.PublishNoODS(Self, Format('Summary volume result: Failure, error = %d', [Ord(ResultStatus)]), slmcMessage);
                        return VolumesResult;
                    }

                    /* TODO: Readd when logging available
                    SIGLogMessage.PublishNoODS(Self, Format('#Result# Summary volume result: Cut=%.3f, Fill=%.3f, Area=%.3f',            
                                              [CutFillVolume.CutVolume, CutFillVolume.FillVolume, CoverageArea]), slmcMessage);
                    */

                    Aggregator.Finalise();
                    if (!Aggregator.BoundingExtents.IsValidPlanExtent)
                    {
                        // TODO: Read when loggin available
                        //SIGLogMessage.PublishNoODS(Self, 'Summary volume invalid PlanExtents. Possibly no data found', slmcMessage);
                        if (Aggregator.CoverageArea == 0 && Aggregator.CutFillVolume.CutVolume == 0 && Aggregator.CutFillVolume.FillVolume == 0)
                        {
                            ResultStatus = RequestErrorStatus.NoProductionDataFound;
                        }
                        else
                        {
                            ResultStatus = RequestErrorStatus.InvalidPlanExtents;
                        }

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

                    VolumesResult = new SimpleVolumesResponse()
                    {
                        Cut = Aggregator.CutFillVolume.CutVolume,
                        Fill = Aggregator.CutFillVolume.FillVolume,
                        TotalCoverageArea = Aggregator.CoverageArea,
                        CutArea = Aggregator.CutArea,
                        FillArea = Aggregator.FillArea,
                        BoundingExtentGrid = Aggregator.BoundingExtents,
                        BoundingExtentLLH = ResultBoundingExtents
                    };

                    return VolumesResult;
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
