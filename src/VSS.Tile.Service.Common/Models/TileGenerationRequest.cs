using System;
using System.Collections.Generic;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Tile.Service.Common.Services;

namespace VSS.Tile.Service.Common.Models
{
  /// <summary>
  /// Parameters for generating reporting map tiles.
  /// </summary>
  public class TileGenerationRequest
  {
    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;
    private const int MAX_ALK_PIXELS = 2048;

    public Guid? filterUid { get; private set; }
    public Guid? baseUid { get; private set; }
    public Guid? topUid { get; private set; }
    public Guid? cutFillDesignUid { get; private set; }
    public VolumeCalcType? volCalcType { get; private set; }
    public IEnumerable<GeofenceData> geofences { get; private set; }
    public List<List<WGSPoint>> alignmentPointsList { get; private set; }
    public List<List<WGSPoint>> customFilterBoundary { get; private set; }
    public List<List<WGSPoint>> designFilterBoundary { get; private set; }
    public List<List<WGSPoint>> alignmentFilterBoundary { get; private set; }
    public List<List<WGSPoint>> designBoundaryPoints { get; private set; }
    public IEnumerable<FileData> dxfFiles { get; private set; }
    public List<TileOverlayType> overlays { get; private set; }
    public int width { get; private set; }
    public int height { get; private set; }
    public MapType? mapType { get; private set; }
    public DisplayMode? mode { get; private set; }
    public string language { get; private set; }
    public ProjectData project { get; private set; }
    public byte[] productionData { get; private set; }
    public MapParameters mapParameters { get; set; }
    public IDictionary<string, string> customHeaders { get; set; }

    /// <summary>
    /// Create instance of TileGenerationRequest
    /// </summary>
    public static TileGenerationRequest CreateTileGenerationRequest(
      Guid? filterUid,
      Guid? baseUid,
      Guid? topUid,
      Guid? cutFillDesignUid,
      VolumeCalcType? volCalcType,
      IEnumerable<GeofenceData> geofences,
      List<List<WGSPoint>> alignmentPointsList,
      List<List<WGSPoint>> customFilterBoundary,
      List<List<WGSPoint>> designFilterBoundary,
      List<List<WGSPoint>> alignmentFilterBoundary,
      List<List<WGSPoint>> designBoundaryPoints,
      IEnumerable<FileData> dxfFiles,
      List<TileOverlayType> overlays,
      int width,
      int height,
      MapType? mapType,
      DisplayMode? mode,
      string language,
      ProjectData project,
      MapParameters mapParameters,
      IDictionary<string, string> customHeaders
      )
    {
      return new TileGenerationRequest
      {
        filterUid = filterUid,
        baseUid = baseUid,
        topUid = topUid,
        cutFillDesignUid = cutFillDesignUid,
        volCalcType = volCalcType ?? VolumeCalcType.None,
        geofences = geofences,
        alignmentPointsList = alignmentPointsList,
        customFilterBoundary = customFilterBoundary,
        designFilterBoundary = designFilterBoundary,
        alignmentFilterBoundary = alignmentFilterBoundary,
        designBoundaryPoints = designBoundaryPoints,
        dxfFiles = dxfFiles,
        overlays = overlays,
        width = width,
        height = height,
        mapType = mapType,
        mode = mode,
        language = language ?? "en-US",
        project = project,
        mapParameters = mapParameters,
        customHeaders = customHeaders
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (overlays == null || overlays.Count == 0)
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
          if (volCalcType == VolumeCalcType.None && cutFillDesignUid == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing design for cut-fill production data overlay"));
          }
          if ((volCalcType == VolumeCalcType.DesignToGround && baseUid == null) || 
              (volCalcType == VolumeCalcType.GroundToDesign && topUid == null))
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing design for summary volumes production data overlay"));
          }
          if ((volCalcType == VolumeCalcType.GroundToGround || volCalcType == VolumeCalcType.GroundToDesign) &&
              baseUid == null)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Missing base filter for summary volumes production data overlay"));
          }
          if ((volCalcType == VolumeCalcType.GroundToGround || volCalcType == VolumeCalcType.DesignToGround) &&
              topUid == null)
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
