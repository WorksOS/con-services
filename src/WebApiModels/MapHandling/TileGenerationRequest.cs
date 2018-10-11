using System.Collections.Generic;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Parameters for generating reporting map tiles.
  /// </summary>
  public class TileGenerationRequest
  {
    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;
    private const int MAX_ALK_PIXELS = 2048;

    public DesignDescriptor DesignDescriptor { get; private set; }
    public FilterResult Filter { get; private set; }
    public FilterResult BaseFilter { get; private set; }
    public FilterResult TopFilter { get; private set; }
    public VolumeCalcType? VolCalcType { get; private set; }
    public IEnumerable<GeofenceData> Geofences { get; private set; }
    public IEnumerable<GeofenceData> Boundaries { get; private set; }
    public IEnumerable<DesignDescriptor> AlignmentDescriptors { get; private set; }
    public IEnumerable<FileData> DxfFiles { get; private set; }
    public TileOverlayType[] Overlays { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public MapType? MapType { get; private set; }
    public DisplayMode? Mode { get; private set; }
    public string Language { get; private set; }
    public ProjectData Project { get; private set; }
    public CompactionProjectSettings ProjectSettings { get; private set; }
    public CompactionProjectSettingsColors ProjectSettingsColors { get; private set; }


    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public TileGenerationRequest(
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
      DesignDescriptor = designDescriptor;
      Filter = filter;
      BaseFilter = baseFilter;
      TopFilter = topFilter;
      VolCalcType = volCalcType ?? VolumeCalcType.None;
      Geofences = geofences;
      Boundaries = boundaries;
      AlignmentDescriptors = alignmentDescriptors;
      DxfFiles = dxfFiles;
      Overlays = overlays;
      Width = width;
      Height = height;
      MapType = mapType;
      Mode = mode;
      Language = language ?? "en-US";
      Project = project;
      ProjectSettings = projectSettings;
      ProjectSettingsColors = projectSettingsColors;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (Overlays == null || Overlays.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "At least one type of map tile overlay must be specified"));
      }

      bool hasBaseMap = Overlays.Contains(TileOverlayType.BaseMap);
      int maxPixels = hasBaseMap ? MAX_ALK_PIXELS : MAX_PIXELS;
      if (Width < MIN_PIXELS || Width > maxPixels || Height < MIN_PIXELS || Height > maxPixels)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Tile size must be between {MIN_PIXELS} and {MAX_ALK_PIXELS} with a base map or {MIN_PIXELS} and {MAX_PIXELS} otherwise"));
      }

      if (Overlays.Contains(TileOverlayType.BaseMap) && !MapType.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing map type parameter for base map overlay"));
      }

      if (Overlays.Contains(TileOverlayType.ProductionData))
      {
        if (!Mode.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Missing display mode parameter for production data overlay"));
        }

        if (Mode.Value == DisplayMode.CutFill)
        {
          if (VolCalcType == VolumeCalcType.None && DesignDescriptor == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing design for cut-fill production data overlay"));
          }
          if ((VolCalcType == VolumeCalcType.DesignToGround || VolCalcType == VolumeCalcType.GroundToDesign) &&
              DesignDescriptor == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing design for summary volumes production data overlay"));
          }
          if ((VolCalcType == VolumeCalcType.GroundToGround || VolCalcType == VolumeCalcType.GroundToDesign) &&
              BaseFilter == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing base filter for summary volumes production data overlay"));
          }
          if ((VolCalcType == VolumeCalcType.GroundToGround || VolCalcType == VolumeCalcType.DesignToGround) &&
              TopFilter == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing top filter for summary volumes production data overlay"));
          }
        }
      }
    }
  }
}
