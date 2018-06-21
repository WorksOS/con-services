using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.MapHandling;

namespace VSS.Productivity3D.WebApiModels.MapHandling
{
  /// <summary>
  /// Parameters for generating reporting map tiles.
  /// </summary>
  public class TileGenerationRequest
  {
    public DesignDescriptor designDescriptor { get; private set; }
    public FilterResult filter { get; private set; }
    public FilterResult baseFilter { get; private set; }
    public FilterResult topFilter { get; private set; }
    public VolumeCalcType? volCalcType { get; private set; }
    public IEnumerable<GeofenceData> geofences { get; private set; }
    public IEnumerable<GeofenceData> boundaries { get; private set; }
    public IEnumerable<DesignDescriptor> alignmentDescriptors { get; private set; }
    public IEnumerable<FileData> dxfFiles { get; private set; }
    public TileOverlayType[] overlays { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }
    public MapType? mapType { get; private set; }
    public DisplayMode? mode { get; private set; }
    public string language { get; private set; }
    public ProjectData project { get; private set; }
    public CompactionProjectSettings projectSettings { get; private set; }
    public CompactionProjectSettingsColors ProjectSettingsColors { get; private set; }


    /// <summary>
    /// Create instance of TileGenerationRequest
    /// </summary>
    public static TileGenerationRequest CreateTileGenerationRequest(
      DesignDescriptor designDescriptor,
      FilterResult filter,
      FilterResult baseFilter,
      FilterResult topFilter,
      VolumeCalcType? volCalcType,
      IEnumerable<GeofenceData> geofences,
      IEnumerable<GeofenceData> boundaries,
      IEnumerable<DesignDescriptor> alignmentDescriptors,
      IEnumerable<FileData> dxfFiles,
      TileOverlayType[] overlays,
      int width,
      int height,
      MapType? mapType,
      DisplayMode? mode,
      string language,
      ProjectData project,
      CompactionProjectSettings projectSettings,
      CompactionProjectSettingsColors projectSettingsColors
      )
    {
      return new TileGenerationRequest
      {
        designDescriptor = designDescriptor,
        filter = filter,
        baseFilter = baseFilter,
        topFilter = topFilter,
        volCalcType = volCalcType ?? VolumeCalcType.None,
        geofences = geofences,
        boundaries = boundaries,
        alignmentDescriptors = alignmentDescriptors,
        dxfFiles = dxfFiles,
        overlays = overlays,
        width = width,
        height = height,
        mapType = mapType,
        mode = mode,
        language = language ?? "en-US",
        project = project,
        projectSettings = projectSettings,
        ProjectSettingsColors = projectSettingsColors
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (overlays == null || overlays.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one type of map tile overlay must be specified"));
      }

      bool hasBaseMap = overlays.Contains(TileOverlayType.BaseMap);
      int maxPixels = hasBaseMap ? MAX_ALK_PIXELS : MAX_PIXELS;
      if (width < MIN_PIXELS || width > maxPixels || height < MIN_PIXELS || height > maxPixels)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Tile size must be between {MIN_PIXELS} and {MAX_ALK_PIXELS} with a base map or {MIN_PIXELS} and {MAX_PIXELS} otherwise"));
      }

      if (overlays.Contains(TileOverlayType.BaseMap) && !mapType.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing map type parameter for base map overlay"));
      }

      if (overlays.Contains(TileOverlayType.ProductionData))
      {
        if (!mode.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Missing display mode parameter for production data overlay"));
        }



        if (mode.Value == DisplayMode.CutFill)
        {
          if (volCalcType == VolumeCalcType.None && designDescriptor == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing design for cut-fill production data overlay"));
          }
          if ((volCalcType == VolumeCalcType.DesignToGround || volCalcType == VolumeCalcType.GroundToDesign) &&
              designDescriptor == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing design for summary volumes production data overlay"));
          }
          if ((volCalcType == VolumeCalcType.GroundToGround || volCalcType == VolumeCalcType.GroundToDesign) &&
              baseFilter == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing base filter for summary volumes production data overlay"));
          }
          if ((volCalcType == VolumeCalcType.GroundToGround || volCalcType == VolumeCalcType.DesignToGround) &&
              topFilter == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing top filter for summary volumes production data overlay"));
          }
        }
      }
    }

    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;
    private const int MAX_ALK_PIXELS = 2048;
  }
}
