using ASNode.CMVChange.RPC;
using ASNode.ElevationStatistics.RPC;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.GridReport.RPC;
using ASNode.RequestSummaryVolumesAlignmentProfile.RPC;
using ASNode.RequestSummaryVolumesProfile.RPC;
using ASNode.SpeedSummary.RPC;
using ASNode.StationOffsetReport.RPC;
using ASNode.ThicknessSummary.RPC;
using ASNode.UserPreferences;
using ASNode.Volumes.RPC;
using ASNodeDecls;
using ASNodeRPC;
using BoundingExtents;
using DesignProfiler.ComputeDesignBoundary.RPC;
using DesignProfiler.ComputeProfile.RPC;
using DesignProfilerDecls;
using ShineOn.Rtl;
using SVOICDecls;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using SVOICOptionsDecls;
using SVOICStatistics;
using SVOICVolumeCalculationsDecls;
using System;
using System.IO;
using ASNode.DXF.RequestBoundaries.RPC;
using DesignProfiler.ComputeDesignFilterBoundary.RPC;
using VLPDDecls;

namespace VSS.Productivity3D.Common.Interfaces
{
  /// <summary>
  /// Interface for Raptor AS Node
  /// </summary>
  public interface IASNodeClient
  {
    bool GetProductionDataExport(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
        TASNodeUserPreferences UserPreferences, int ExportType, string CallerId,
        TICFilterSettings Filter, TICLiftBuildSettings LiftBuildSettings, bool TimeStampRequired,
        bool CellSizeRequired, bool RawData, bool RestrictSize,
        bool ZipFile, double Tolerance, bool IncludeSurveydSurface, bool Precheckonly, string Filename,
        TMachine[] MachineList, int CoordType, int OutputType,
        TDateTime DateFromUTC, TDateTime DateToUTC, TTranslation[] Translations, T3DBoundingWorldExtent ProjectExtents,
        out TDataExport DataExport);

    TASNodeErrorStatus GetPassCountSummary(long projectID, TASNodeRequestDescriptor externalRequestDescriptor,
        TPassCountSettings passCountSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
        out TPassCountSummary passCountSummary);

    TASNodeErrorStatus GetPassCountDetails(long projectID, TASNodeRequestDescriptor externalRequestDescriptor,
        TPassCountSettings passCountSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
        out TPassCountDetails passCountDetails);

    bool GetDataModelStatistics(long projectID, TSurveyedSurfaceID[] exclusionList,
              out TICDataModelStatistics statistics);

    TASNodeErrorStatus PassSelectedCoordinateSystemFile(Stream csFileContent, string csFileName, long projectID, out TCoordinateSystemSettings csSettings);

    TASNodeErrorStatus RequestCoordinateSystemDetails(long projectId, out TCoordinateSystemSettings csSettings);

    TDesignProfilerRequestResult UpdateCacheWithDesign(long dataModelId, string designFileName, long designId, bool deleteTTM);

    TASNodeErrorStatus GetSummaryVolumes(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
              TComputeICVolumesType VolumeType,
              TICFilterSettings BaseFilter, TVLPDDesignDescriptor BaseDesign,
              TICFilterSettings TopFilter, TVLPDDesignDescriptor TopDesign,
              TICFilterSettings AdditionalSpatialFilter,
              TICLiftBuildSettings LiftBuildSettings,
              out TASNodeSimpleVolumesResult Results);


    TASNodeErrorStatus GetSummaryVolumes(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
      TComputeICVolumesType VolumeType, TICFilterSettings BaseFilter, TVLPDDesignDescriptor BaseDesign,
      TICFilterSettings TopFilter, TVLPDDesignDescriptor TopDesign, TICFilterSettings AdditionalSpatialFilter,
      double CutTolerance, double FillTolerance,
      TICLiftBuildSettings LiftBuildSettings, out TASNodeSimpleVolumesResult Results);

    bool GetSummaryThickness(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
              TICFilterSettings BaseFilter,
              TICFilterSettings TopFilter,
              TICFilterSettings AdditionalSpatialFilter,
              TICLiftBuildSettings LiftBuildSettings,
              out TASNodeThicknessSummaryResult Results);

    TASNodeErrorStatus GetSummarySpeed(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
      TICFilterSettings filter,
      TICLiftBuildSettings LiftBuildSettings,
      out TASNodeSpeedSummaryResult Results);

    int GetOnMachineLayers(long DataModelID, out TDesignLayer[] LayerList);


    bool GetDataModelExtents(long DataModelID, VLPDDecls.TSurveyedSurfaceID[] SurveyedSurfaceExclusionList,
        out BoundingExtents.T3DBoundingWorldExtent extents);

    /// <summary>
    /// Gets the machine details for the project
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns></returns>
    TMachineDetail[] GetMachineIDs(long projectId);

    /// <summary>
    /// Requests the configuration.
    /// </summary>
    /// <param name="xml">The XML configuration.</param>
    void RequestConfig(out string xml);


    int RequestCellProfile(long ADataModelID, SubGridTreesDecls.TSubGridCellAddress ACellAddress, double AProbePositionX,
        double AProbePositionY, bool AProbePositionIsGridCoord,
        TICLiftBuildSettings ALiftBuildSettings, int AGridDataType,
        TICFilterSettings AFilter, out SVOICProfileCell.TICProfileCell ACellProfile);


    bool GetCellProductionData(long projectId, int displayMode, double AProbePositionX,
            double AProbePositionY, TWGS84Point point, bool AProbePositionIsGridCoord, TICFilterSettings AFilter,
            TICLiftBuildSettings ALiftBuildSettings,
            TVLPDDesignDescriptor designDescriptor, out TCellProductionData data);


    TASNodeErrorStatus RequestDataPatchPage(long dataModelID, TASNodeRequestDescriptor requestDescr,
            TICDisplayMode mode, TColourPalettes palettes, bool colored, TICFilterSettings filter1,
            TICFilterSettings filter2, TSVOICOptions options, TVLPDDesignDescriptor design,
            TComputeICVolumesType volumetype, int dataPatchPage, int dataPatchSize, out MemoryStream Patch,
            out int numPatches);

    TASNodeErrorStatus RequestDataPatchPageWithTime(long dataModelID, TASNodeRequestDescriptor requestDescr,
      TICDisplayMode mode, TICFilterSettings filter1, TICFilterSettings filter2, TVLPDDesignDescriptor design,
      TComputeICVolumesType volumetype, TSVOICOptions options, int dataPatchPage, int dataPatchSize, out MemoryStream Patch,
      out int numPatches);

    TASNodeErrorStatus GetRenderedMapTileWithRepresentColor(long projectId, TASNodeRequestDescriptor requestDescr,
      TICDisplayMode mode, TColourPalettes palettes, TWGS84Point bl, TWGS84Point tr, bool coordsAreGrid,
      ushort width, ushort height, TICFilterSettings filter1, TICFilterSettings filter2, TSVOICOptions options,
      TVLPDDesignDescriptor design, TComputeICVolumesType volumetype, uint representColor, out MemoryStream tile);

    MemoryStream GetAlignmentProfile(ASNode.RequestAlignmentProfile.RPC.TASNodeServiceRPCVerb_RequestAlignmentProfile_Args Args);

    MemoryStream GetProfile(ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args Args);
    MemoryStream GetDesignProfile(TDesignProfilerServiceRPCVerb_CalculateDesignProfile_Args Args);

    MemoryStream GetSummaryVolumesProfile(TASNodeServiceRPCVerb_RequestSummaryVolumesProfile_Args Args);

    MemoryStream GetSummaryVolumesAlignmentProfile(
      TASNodeServiceRPCVerb_RequestSummaryVolumesAlignmentProfile_Args Args);

    TDesignName[] GetOnMachineDesignEvents(long dataModelId);

    TDesignName[] GetOverriddenDesigns(long projectId, long assetId);
    TDesignLayer[] GetOverriddenLayers(long projectId, long assetId);

    TASNodeErrorStatus GetMDPSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
           TMDPSettings mdpSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
           out TMDPSummary mdpSummary);

    TASNodeErrorStatus GetCMVSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
              TCMVSettings cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
              out TCMVSummary cmvSummary);


    TASNodeErrorStatus GetCMVChangeSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
              TASNodeCMVChangeSettings cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
              out TASNodeCMVChangeResult cmvSummary);

    TASNodeErrorStatus GetCMVDetails(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
     TCMVSettings cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
     out TCMVDetails cmvDetails);

    TASNodeErrorStatus GetCMVDetailsExt(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
      TCMVSettingsExt cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TCMVDetails cmvDetails);

    TASNodeErrorStatus GetTemperatureSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
      TTemperatureSettings temperatureSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TTemperature temperatureSummary);

    TASNodeErrorStatus GetTemperatureDetails(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
      TTemperatureDetailSettings temperatureDetailsSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TTemperatureDetails temperatureDetails);

    /// <summary>
    /// Stores Surveyed Surface data.
    /// </summary>
    /// <param name="args">Description of the request data.</param>
    /// <returns>True if successfully saved, false - otherwise.</returns>
    /// 
    bool StoreGroundSurfaceFile(ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args);

    /// <summary>
    /// Discards Surveyed Surface data.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    /// <returns>True if successfully deleted, false - otherwise.</returns>
    /// 
    bool DiscardGroundSurfaceFileDetails(long projectId, long surveyedSurfaceId);

    /// <summary>
    /// Gets Surveyed Surface list.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>True if successfully received, false - otherwise.</returns>
    /// 
    bool GetKnownGroundSurfaceFileDetails(long projectId, out TSurveyedSurfaceDetails[] groundSurfaces);

    /// <summary>
    /// Updates Surveyed Surface data.
    /// </summary>
    /// <param name="args">Description of the request data.</param>
    /// <returns>True if successfully updated, false - otherwise.</returns>
    /// 
    bool UpdateGroundSurfaceFile(ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args);

    /// <summary>
    /// Gets a list of grid coordinates converted either from NE or Latitude/Longidude.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <param name="latlongs">The list of NE or Latitude/Longidude coordinates.</param>
    /// <param name="conversionType">Coordinate conversion type: 
    ///                               0 - from Latitude/Longitude to North/East,
    ///                               1 - from North/East to Latitude/Longitude.
    /// </param>
    /// <param name="pointList">The list of converted coordinates.</param>
    /// <returns>An error code: 
    ///                         0 - No error,
    ///                         1 - Unknown error,
    ///                         2 - No connection to Server,
    ///                         3 - Missing coordinates,
    ///                         4 - Failed to convert coordinates.
    /// </returns>
    /// 
    TCoordReturnCode GetGridCoordinates(long projectId, TWGS84FenceContainer latlongs, TCoordConversionType conversionType, out TCoordPointList pointList);

    /// <summary>
    /// Computes the minimum and maximum elevation of the cells matching the filter
    /// </summary>
    TASNodeErrorStatus GetElevationStatistics(long DataModelID,
                                              TASNodeRequestDescriptor ExternalRequestDescriptor,
                                              TICFilterSettings Filter,
                                              TICLiftBuildSettings LiftBuildSettings,
                                              out TASNodeElevationStatisticsResult Results);

    bool GetCCASummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
     TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings, out TCCASummary ccaSummary);

    TASNodeErrorStatus GetCoordinateSystemProjectionFile(long DataModelID, TVLPDDistanceUnits RequestedUnits, out string prjFile);
    TASNodeErrorStatus GetCoordinateSystemHorizontalAdjustmentFile(string CSFileName, long DataModelID, TVLPDDistanceUnits RequestedUnits, out string haFile);
    bool GetDesignBoundary(TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args args, out MemoryStream resultContents, out TDesignProfilerRequestResult designProfilerResult);

    bool GetMachineCCAColourPalettes(long dataModelId, long machineId, DateTime? startUtc, DateTime? endUtc, int? liftId, out TColourPalettes palettes);

    int GetGriddedOrAlignmentCSVExport(long DataModelID,
                                       int ReportType,
                                       TASNodeRequestDescriptor ExternalDescriptor,
                                       TVLPDDesignDescriptor DesignFile, // cutfill profile
                                       double Interval,
                                       bool ElevationReport,
                                       bool CutFillReport,
                                       bool CMVReport,
                                       bool MDPReport,
                                       bool PassCountReport,
                                       bool TemperatureReport,
                                       int ReportOption,
                                       double StartNorthing,
                                       double StartEasting,
                                       double EndNorthing,
                                       double EndEasting,
                                       double Direction,
                                       TICFilterSettings Filter,
                                       TICLiftBuildSettings LiftBuildSettings,
                                       TSVOICOptions ICOptions,
                                       out MemoryStream DataExport);


    TASNodeErrorStatus GetCutFillDetails(long projectID, TASNodeRequestDescriptor externalRequestDescriptor,
      TCutFillSettings cutFillSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TCutFillDetails cutFillDetails);

    bool GetStationExtents(long projectID, TVLPDDesignDescriptor designDescriptor, out double startStation,
      out double endStation);

    bool GetDesignFilterBoundaryAsPolygon(TDesignProfilerServiceRPCVerb_ComputeDesignFilterBoundary_Args args,
      out TWGS84Point[] fence);
    /// <summary>
    /// Gets a grid report of the production data from Raptor.
    /// </summary>
    /// <param name="args">Set of the grid report parameters.</param>
    /// <param name="dataReport">The reports data.</param>
    /// <returns>The Raptor's request result code.</returns>
    int GetReportGrid(TASNodeServiceRPCVerb_GridReport_Args args, out MemoryStream dataReport);

    /// <summary>
    /// Gets a station offset report from Raptor.
    /// </summary>
    int GetReportStationOffset(TASNodeServiceRPCVerb_StationOffsetReport_Args args, out MemoryStream dataReport);

    /// <summary>
    /// 
    /// </summary>
    TASNodeErrorStatus GetBoundariesFromLinework(TASNodeServiceRPCVerb_RequestBoundariesFromLinework_Args args, out TWGS84LineworkBoundary[] lineworkBoundaries);
  }
}
