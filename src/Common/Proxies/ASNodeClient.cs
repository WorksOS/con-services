using System;
using System.IO;
using System.Linq;
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
using DesignProfiler.ComputeDesignFilterBoundary.RPC;
using DesignProfiler.ComputeProfile.RPC;
using DesignProfilerDecls;
using ShineOn.Rtl;
using SubGridTreesDecls;
using SVOICDecls;
using SVOICFilterSettings;
using SVOICLiftBuildSettings;
using SVOICOptionsDecls;
using SVOICProfileCell;
using SVOICStatistics;
using SVOICVolumeCalculationsDecls;
using VLPDDecls;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Proxies
{
  public class ASNodeClient : IASNodeClient
  {
    public ASNodeClient()
    {
      client = new Velociraptor.PDSInterface.Client.ASNode.ASNodeClient();
    }

    private Velociraptor.PDSInterface.Client.ASNode.ASNodeClient client { get; }

    public bool GetProductionDataExport(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
        TASNodeUserPreferences UserPreferences, int ExportType, string CallerId, TICFilterSettings Filter,
        TICLiftBuildSettings LiftBuildSettings, bool TimeStampRequired, bool CellSizeRequired, bool RawData,
        bool RestrictSize, bool ZipFile, double Tolerance, bool IncludeSurveydSurface, bool Precheckonly, string Filename,
        TMachine[] MachineList, int CoordType, int OutputType, TDateTime DateFromUTC, TDateTime DateToUTC,
        TTranslation[] Translations, T3DBoundingWorldExtent ProjectExtents, out TDataExport DataExport)
    {
      return client.GetProductionDataExport(DataModelID, ExternalRequestDescriptor, UserPreferences, ExportType,
          CallerId, Filter, LiftBuildSettings, TimeStampRequired, CellSizeRequired,
          RawData, RestrictSize, ZipFile, Tolerance, IncludeSurveydSurface, Precheckonly, Filename, MachineList,
          CoordType, OutputType, DateFromUTC, DateToUTC, Translations, ProjectExtents,
          out DataExport);
    }

    public TASNodeErrorStatus GetPassCountSummary(long projectID, TASNodeRequestDescriptor externalRequestDescriptor,
          TPassCountSettings passCountSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
          out TPassCountSummary passCountSummary)
    {
      return client.GetPassCountSummary(projectID, externalRequestDescriptor, passCountSettings, filter,
          liftBuildSettings, out passCountSummary);
    }

    public TASNodeErrorStatus GetPassCountDetails(long projectID, TASNodeRequestDescriptor externalRequestDescriptor,
        TPassCountSettings passCountSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
        out TPassCountDetails passCountDetails)
    {
      return client.GetPassCountDetails(projectID, externalRequestDescriptor, passCountSettings, filter,
          liftBuildSettings, out passCountDetails);
    }


    public bool GetDataModelStatistics(long projectID, TSurveyedSurfaceID[] exclusionList,
        out TICDataModelStatistics statistics)
    {
      //TODO modify shims to provide error status code
      return client.GetDataModelStatistics(projectID, exclusionList, out statistics) == 1;/*icsrrNoError*/
    }


    public TASNodeErrorStatus PassSelectedCoordinateSystemFile(Stream csFileContent, string csFileName, long projectID,
        out TCoordinateSystemSettings csSettings)
    {
      return client.PassSelectedCoordinateSystemFile(csFileContent, csFileName, projectID, out csSettings);
    }

    public TASNodeErrorStatus RequestCoordinateSystemDetails(long projectId, out TCoordinateSystemSettings csSettings)
    {
      return client.RequestCoordinateSystemDetails(projectId, out csSettings);
    }

    public TDesignProfilerRequestResult UpdateCacheWithDesign(long dataModelId, string designFileName, long designId, bool deleteTTM)
    {
      return client.UpdateCacheWithDesign(dataModelId, designFileName, designId, deleteTTM);
    }

    public TASNodeErrorStatus GetSummaryVolumes(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
          TComputeICVolumesType VolumeType, TICFilterSettings BaseFilter, TVLPDDesignDescriptor BaseDesign,
          TICFilterSettings TopFilter, TVLPDDesignDescriptor TopDesign, TICFilterSettings AdditionalSpatialFilter,
          TICLiftBuildSettings LiftBuildSettings, out TASNodeSimpleVolumesResult Results)
    {
      return client.GetSummaryVolumes(DataModelID, ExternalRequestDescriptor, VolumeType, BaseFilter, BaseDesign,
          TopFilter,
          TopDesign, AdditionalSpatialFilter, LiftBuildSettings, out Results);
    }

    public TASNodeErrorStatus GetSummaryVolumes(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
    TComputeICVolumesType VolumeType, TICFilterSettings BaseFilter, TVLPDDesignDescriptor BaseDesign,
    TICFilterSettings TopFilter, TVLPDDesignDescriptor TopDesign, TICFilterSettings AdditionalSpatialFilter, double CutTolerance, double FillTolerance,
    TICLiftBuildSettings LiftBuildSettings, out TASNodeSimpleVolumesResult Results)
    {
      return client.GetSummaryVolumes(DataModelID, ExternalRequestDescriptor, VolumeType, BaseFilter, BaseDesign,
          TopFilter,
          TopDesign, AdditionalSpatialFilter, CutTolerance, FillTolerance, LiftBuildSettings, out Results);
    }


    public bool GetSummaryThickness(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor,
        TICFilterSettings BaseFilter, TICFilterSettings TopFilter, TICFilterSettings AdditionalSpatialFilter,
        TICLiftBuildSettings LiftBuildSettings, out TASNodeThicknessSummaryResult Results)
    {
      return client.GetThicknessSummary(DataModelID, ExternalRequestDescriptor, BaseFilter,
          TopFilter, AdditionalSpatialFilter, LiftBuildSettings, out Results) == TASNodeErrorStatus.asneOK;
    }

    public TASNodeErrorStatus GetSummarySpeed(long DataModelID, TASNodeRequestDescriptor ExternalRequestDescriptor, TICFilterSettings filter,
        TICLiftBuildSettings LiftBuildSettings, out TASNodeSpeedSummaryResult Results)
    {
      return client.GetSpeedSummary(DataModelID, ExternalRequestDescriptor, filter, LiftBuildSettings, out Results);
    }

    public bool GetDataModelExtents(long DataModelID, TSurveyedSurfaceID[] SurveyedSurfaceExclusionList,
          out T3DBoundingWorldExtent extents)
    {
      return client.GetDataModelExtents(DataModelID, SurveyedSurfaceExclusionList, out extents) == 1;/*icsrrNoError*/
    }

    public TMachineDetail[] GetMachineIDs(long projectId)
    {
      return client.GetMachineIDs(projectId, out TMachineDetail[] machineIDs) == 1/*icsrrNoError*/ ? machineIDs : null;
    }

    public void RequestConfig(out string xml)
    {
      client.RequestConfig(out xml);
    }

    public int RequestCellProfile(long ADataModelID, TSubGridCellAddress ACellAddress, double AProbePositionX,
            double AProbePositionY, bool AProbePositionIsGridCoord, TICLiftBuildSettings ALiftBuildSettings,
            int AGridDataType, TICFilterSettings AFilter, out TICProfileCell ACellProfile)
    {

      return client.RequestCellProfile(ADataModelID, ACellAddress, AProbePositionX, AProbePositionY, AProbePositionIsGridCoord,
        ALiftBuildSettings, AGridDataType, AFilter, out ACellProfile);
    }


    public bool GetCellProductionData(long projectId, int displayMode, double AProbePositionX,
            double AProbePositionY, TWGS84Point point, bool AProbePositionIsGridCoord, TICFilterSettings AFilter,
            TICLiftBuildSettings ALiftBuildSettings, TVLPDDesignDescriptor designDescriptor, out TCellProductionData data)
    {
      return client.GetCellProductionData(projectId, displayMode, AProbePositionX, AProbePositionY, point, AProbePositionIsGridCoord, AFilter, ALiftBuildSettings, designDescriptor, out data) == 1;/*icsrrNoError*/
    }

    public TASNodeErrorStatus RequestDataPatchPage(long dataModelID, TASNodeRequestDescriptor requestDescr, TICDisplayMode mode,
            TColourPalettes palettes, bool colored, TICFilterSettings filter1, TICFilterSettings filter2,
            TSVOICOptions options, TVLPDDesignDescriptor design, TComputeICVolumesType volumetype, int dataPatchPage,
            int dataPatchSize, out MemoryStream Patch, out int numPatches)
    {
      return client.RequestDataPatchPage(dataModelID, requestDescr, mode, palettes, colored, filter1, filter2, options,
              design, volumetype, dataPatchPage, dataPatchSize, out Patch, out numPatches);
    }

    public TASNodeErrorStatus RequestDataPatchPageWithTime(
      long dataModelID, TASNodeRequestDescriptor requestDescr, TICDisplayMode mode, TICFilterSettings filter1, TICFilterSettings filter2, TVLPDDesignDescriptor design, TComputeICVolumesType volumetype,
      TSVOICOptions options, int dataPatchPage,
      int dataPatchSize, out MemoryStream Patch, out int numPatches)
    {
      return client.RequestDataPatchPageWithTime(dataModelID, requestDescr, mode, filter1, filter2,
        options, design, volumetype, dataPatchPage, dataPatchSize, out Patch, out numPatches);
    }

    public TASNodeErrorStatus GetRenderedMapTileWithRepresentColor(long projectId, TASNodeRequestDescriptor requestDescr,
      TICDisplayMode mode, TColourPalettes palettes, TWGS84Point bl, TWGS84Point tr, bool coordsAreGrid,
      ushort width, ushort height, TICFilterSettings filter1, TICFilterSettings filter2, TSVOICOptions options,
      TVLPDDesignDescriptor design, TComputeICVolumesType volumetype, uint representColor, out MemoryStream tile)
    {
      return client.GetRenderedMapTileWithRepresentColor(projectId, requestDescr, mode, palettes, bl, tr, coordsAreGrid, width, height,
          filter1, filter2, options, design, volumetype, representColor, out tile);
    }

    public MemoryStream GetAlignmentProfile(ASNode.RequestAlignmentProfile.RPC.TASNodeServiceRPCVerb_RequestAlignmentProfile_Args Args)
    {
      return client.GetAlignmentProfile(Args, out MemoryStream profile) == 1/*icsrrNoError*/ ? profile : null;
    }

    public MemoryStream GetProfile(ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args Args)
    {
      return client.GetProfile(Args, out MemoryStream profile) == 1/*icsrrNoError*/ ? profile : null;
    }

    public MemoryStream GetDesignProfile(TDesignProfilerServiceRPCVerb_CalculateDesignProfile_Args Args)
    {
      return client.GetDesignProfile(Args, out MemoryStream profile) == 1 ? profile : null;
    }

    public MemoryStream GetSummaryVolumesProfile(TASNodeServiceRPCVerb_RequestSummaryVolumesProfile_Args Args)
    {
      return client.GetSummaryVolumesProfile(Args, out MemoryStream profile) == 1 ? profile : null;
    }

    public MemoryStream GetSummaryVolumesAlignmentProfile(TASNodeServiceRPCVerb_RequestSummaryVolumesAlignmentProfile_Args Args)
    {
      return client.GetSummaryVolumesAlignmentProfile(Args, out MemoryStream profile) == 1 ? profile : null;
    }

    public TDesignName[] GetOnMachineDesignEvents(long dataModelId)
    {
      return client.GetOnMachineDesignEvents(dataModelId, out var designNames) == 1
        ? designNames
        : null;
    }

    public int GetOnMachineLayers(long DataModelID, out TDesignLayer[] LayerList)
    {
      return client.GetLayerIDs(DataModelID, out LayerList);
    }

    public TDesignName[] GetOverriddenDesigns(long projectId, long assetId)
    {
      client.GetOverriddenDesigns(projectId, assetId, out TDesignName[] designNames);
      return designNames;
    }

    public TDesignLayer[] GetOverriddenLayers(long projectId, long assetId)
    {
      client.GetOverriddenLayers(projectId, assetId, out TDesignLayer[] layers);
      return layers;
    }

    public TASNodeErrorStatus GetMDPSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
      TMDPSettings mdpSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TMDPSummary mdpSummary)
    {
      return client.GetMDPSummary(projectId, externalRequestDescriptor, mdpSettings, filter,
          liftBuildSettings, out mdpSummary);
    }

    public TASNodeErrorStatus GetCMVSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
        TCMVSettings cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
        out TCMVSummary cmvSummary)
    {
      return client.GetCMVSummary(projectId, externalRequestDescriptor, cmvSettings, filter, liftBuildSettings, out cmvSummary);
    }

    public TASNodeErrorStatus GetCMVChangeSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
        TASNodeCMVChangeSettings cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
        out TASNodeCMVChangeResult cmvSummary)
    {
      return client.GetCMVChangeSummary(projectId, externalRequestDescriptor, filter, liftBuildSettings, cmvSettings,
          out cmvSummary);
    }

    public TASNodeErrorStatus GetCMVDetails(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
        TCMVSettings cmvSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
        out TCMVDetails cmvDetails)
    {
      return client.GetCMVDetails(projectId, externalRequestDescriptor, cmvSettings, filter,
          liftBuildSettings, out cmvDetails);
    }

    public TASNodeErrorStatus GetCMVDetailsExt(long projectId, TASNodeRequestDescriptor externalRequestDescriptor, TCMVSettingsExt cmvSettings,
      TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings, out TCMVDetails cmvDetails)
    {
      return client.GetCMVDetailsExt(projectId, externalRequestDescriptor, cmvSettings, filter,
               liftBuildSettings, out cmvDetails);
    }

    public TASNodeErrorStatus GetTemperatureSummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
      TTemperatureSettings temperatureSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TTemperature temperatureSummary)
    {
      return client.GetTemperatureSummary(projectId, externalRequestDescriptor, temperatureSettings, filter,
          liftBuildSettings, out temperatureSummary);
    }


    /// <summary>
    /// Stores Surveyed Surface data.
    /// </summary>
    /// <param name="args">Description of the request data.</param>
    /// <returns>True if successfully saved, false - otherwise.</returns>
    /// 
    public bool StoreGroundSurfaceFile(ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args)
    {
      bool result = client.GetGroundSurfaceFileDetails(args.DataModelID, out TSurveyedSurfaceDetails[] groundSurfaces) == 1;/*icsrrNoError*/

      if (!result) return false;

      var ss = groundSurfaces.Where(surveyedSurface => surveyedSurface.ID == args.GroundSurfaceID).ToList();

      if (ss.Any())
        return false;

      return client.StoreGroundSurfaceFile(args) == 1;/*icsrrNoError*/
    }

    /// <summary>
    /// Discards Surveyed Surface data.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    /// <returns>True if successfully deleted, false - otherwise.</returns>
    /// 
    public bool DiscardGroundSurfaceFileDetails(long projectId, long surveyedSurfaceId)
    {
      return client.DiscardGroundSurfaceFileDetails(projectId, surveyedSurfaceId) == 1;/*icsrrNoError*/;
    }

    /// <summary>
    /// Gets Surveyed Surface list.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>True if successfully received, false - otherwise.</returns>
    /// 
    public bool GetKnownGroundSurfaceFileDetails(long projectId, out TSurveyedSurfaceDetails[] groundSurfaces)
    {
      return client.GetGroundSurfaceFileDetails(projectId, out groundSurfaces) == 1;/*icsrrNoError*/;
    }

    /// <summary>
    /// Updates Surveyed Surface list.
    /// </summary>
    /// <param name="args">Description of the request data.</param>
    /// <returns>True if successfully updated, false - otherwise.</returns>
    /// 
    public bool UpdateGroundSurfaceFile(ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args)
    {
      bool result = client.GetGroundSurfaceFileDetails(args.DataModelID, out TSurveyedSurfaceDetails[] groundSurfaces) == 1;/*icsrrNoError*/

      if (!result) return false;

      //var ss = (from surveyedSurface in groundSurfaces where surveyedSurface.ID == args.GroundSurfaceID select surveyedSurface).ToList();
      var ss = groundSurfaces.Where(surveyedSurface => surveyedSurface.ID == args.GroundSurfaceID).ToList();

      if (ss.Any())
        result = client.DiscardGroundSurfaceFileDetails(args.DataModelID, args.GroundSurfaceID) == 1;/*icsrrNoError*/

      if (!result) return false;

      result = client.StoreGroundSurfaceFile(args) == 1;/*icsrrNoError*/

      return result;
    }

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
    public TCoordReturnCode GetGridCoordinates(long projectId, TWGS84FenceContainer latlongs, TCoordConversionType conversionType, out TCoordPointList pointList)
    {
      return client.GetGridCoordinates(projectId, latlongs, conversionType, out pointList);
    }

    /// <summary>
    /// Computes the minimum and maximum elevation of the cells matching the filter
    /// </summary>
    public TASNodeErrorStatus GetElevationStatistics(long DataModelID,
                                                     TASNodeRequestDescriptor ExternalRequestDescriptor,
                                                     TICFilterSettings Filter,
                                                     TICLiftBuildSettings LiftBuildSettings,
                                                     out TASNodeElevationStatisticsResult Results)
    {
      return client.GetElevationStatistics(DataModelID, ExternalRequestDescriptor, Filter, LiftBuildSettings, out Results);
    }

    /// <summary>
    /// Gets CCA Summary from Raptor
    /// </summary>
    /// <param name="projectId">Project identifier</param>
    /// <param name="externalRequestDescriptor">Request descriptor with type of request and call id for cancellation</param>
    /// <param name="filter">Filter to apply</param>
    /// <param name="liftBuildSettings">Lift build settings to apply</param>
    /// <param name="ccaSummary">CCA summary result</param>
    /// <returns>True if successful otherwise false</returns>
    public bool GetCCASummary(long projectId, TASNodeRequestDescriptor externalRequestDescriptor,
            TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings, out TCCASummary ccaSummary)
    {
      return client.GetCCASummary(projectId, externalRequestDescriptor, filter,
          liftBuildSettings, out ccaSummary) == TASNodeErrorStatus.asneOK;
    }

    /// <summary>
    /// Gets CCA Minimum Passes value for a machine from Raptor
    /// </summary>
    /// <param name="dataModelId">Data Model/project identifier</param>
    /// <param name="machineId">Machine identifier</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="palettes">Colour palettes result</param>
    /// <returns>True if successful otherwise false</returns>
    public bool GetMachineCCAColourPalettes(long dataModelId, long machineId, DateTime? startUtc, DateTime? endUtc, int? liftId, out TColourPalettes palettes)
    {
      return client.GetMachineCCAColourPalettes(dataModelId, machineId, startUtc ?? DateTime.MinValue, endUtc ?? DateTime.MinValue, liftId ?? 0, out palettes) == 1/*icsrrNoError*/;
    }

    /// <summary>
    /// Gets PRJ file contents from Raptor for a project using the project coordinate system.
    /// </summary>
    /// <param name="dataModelID">Project ID</param>
    /// <param name="requestedUnits">Metric or US units for the file contents</param>
    /// <param name="prjFile">Projection file contents</param>
    /// <returns></returns>
    public TASNodeErrorStatus GetCoordinateSystemProjectionFile(long dataModelID, TVLPDDistanceUnits requestedUnits,
      out string prjFile)
    {
      return client.GetCoordinateSystemProjectionFile(dataModelID, requestedUnits, out prjFile);
    }
    /// <summary>
    /// Gets GM_XFORM file contents from Raptor for a project using the project coordinate system.
    /// </summary>
    /// <param name="csFileName">Coordinate system file name</param>
    /// <param name="dataModelID">Project ID</param>
    /// <param name="requestedUnits">Metric or US units for the file contents</param>
    /// <param name="haFile">Horizontal adjustment file contents</param>
    /// <returns></returns>
    public TASNodeErrorStatus GetCoordinateSystemHorizontalAdjustmentFile(string csFileName, long dataModelID, TVLPDDistanceUnits requestedUnits, out string haFile)
    {
      return client.GetCoordinateSystemHorizontalAdjustmentFile(csFileName, dataModelID, requestedUnits, out haFile);
    }
    /// <summary>
    /// Gets the boundary of a surface. The boundary return type is specified by the Design boundary arguments.
    /// </summary>
    /// <param name="args">Design boundary arguments: The project ID, design surface file design descriptor, type of boundary (List, DXF, JSon),
    /// units and interval to use</param>
    /// <param name="resultContents">The DXF file contents</param>
    /// <param name="designProfilerResult">The result code (0=success)</param>
    /// <returns></returns>
    public bool GetDesignBoundary(TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args args,
      out MemoryStream resultContents, out TDesignProfilerRequestResult designProfilerResult)
    {
      return client.GetDesignBoundary(args, out resultContents, out designProfilerResult);
    }
    /// <summary>
    /// Gets a gridded CSV export of the production data from Raptor
    /// </summary>
    /// <param name="DataModelID"></param>
    /// <param name="ReportType"></param>
    /// <param name="ExternalDescriptor"></param>
    /// <param name="DesignFile"></param>
    /// <param name="Interval"></param>
    /// <param name="ElevationReport"></param>
    /// <param name="CutFillReport"></param>
    /// <param name="CMVReport"></param>
    /// <param name="MDPReport"></param>
    /// <param name="PassCountReport"></param>
    /// <param name="TemperatureReport"></param>
    /// <param name="ReportOption"></param>
    /// <param name="StartNorthing"></param>
    /// <param name="StartEasting"></param>
    /// <param name="EndNorthing"></param>
    /// <param name="EndEasting"></param>
    /// <param name="Direction"></param>
    /// <param name="Filter"></param>
    /// <param name="LiftBuildSettings"></param>
    /// <param name="ICOptions"></param>
    /// <param name="DataExport"></param>
    /// <returns></returns>
    public int GetGriddedOrAlignmentCSVExport(long DataModelID, int ReportType, TASNodeRequestDescriptor ExternalDescriptor, TVLPDDesignDescriptor DesignFile, double Interval, bool ElevationReport, bool CutFillReport, bool CMVReport, bool MDPReport, bool PassCountReport, bool TemperatureReport, int ReportOption, double StartNorthing, double StartEasting, double EndNorthing, double EndEasting, double Direction, TICFilterSettings Filter, TICLiftBuildSettings LiftBuildSettings, TSVOICOptions ICOptions, out MemoryStream DataExport)
    {
      return client.GetGriddedOrAlignmentCSVExport(DataModelID, ReportType, ExternalDescriptor, DesignFile, Interval, ElevationReport, CutFillReport, CMVReport, MDPReport, PassCountReport, TemperatureReport, ReportOption, StartNorthing, StartEasting, EndNorthing, EndEasting, Direction, Filter, LiftBuildSettings, ICOptions, out DataExport);
    }


    public bool GetCutFillDetails(long projectID, TASNodeRequestDescriptor externalRequestDescriptor,
      TCutFillSettings cutFillSettings, TICFilterSettings filter, TICLiftBuildSettings liftBuildSettings,
      out TCutFillDetails cutFillDetails)
    {
      return client.GetCutFillDetails(projectID, externalRequestDescriptor, cutFillSettings, filter,
               liftBuildSettings, out cutFillDetails) == TASNodeErrorStatus.asneOK;
    }

    public bool GetStationExtents(long projectID, TVLPDDesignDescriptor designDescriptor, out double startStation,
      out double endStation)
    {
      return client.GetStationExtents(projectID, designDescriptor, out startStation, out endStation) ==
             1; /*icsrrNoError*/
    }

    public bool GetDesignFilterBoundaryAsPolygon(TDesignProfilerServiceRPCVerb_ComputeDesignFilterBoundary_Args args,
      out TWGS84Point[] fence)
    {
      return client.GetDesignFilterBoundaryAsPolygon(args, out fence) == 1;/*icsrrNoError*/
    }

    /// <summary>
    /// Gets a grid report of the production data from Raptor.
    /// </summary>
    /// <param name="args">Set of the grid report parameters.</param>
    /// <param name="dataReport">The reports data.</param>
    /// <returns>The Raptor's request result code.</returns>
    /// 
    public int GetReportGrid(TASNodeServiceRPCVerb_GridReport_Args args, out MemoryStream dataReport)
    {
      return client.GetGridReport(args, out dataReport);
    }

    /// <inheritdoc />
    public int GetReportStationOffset(TASNodeServiceRPCVerb_StationOffsetReport_Args args, out MemoryStream dataReport)
    {
      return client.GetStationOffsetReport(args, out dataReport);
    }
  }
}
