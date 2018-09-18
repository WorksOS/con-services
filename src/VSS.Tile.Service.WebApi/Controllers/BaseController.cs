using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Tile.Service.Common.Authentication;
using VSS.Tile.Service.Common.Models;
using VSS.Tile.Service.Common.Services;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class BaseController<T> : Controller where T : BaseController<T>
  {
    private readonly IPreferenceProxy prefProxy;
    private readonly IRaptorProxy raptorProxy;
    private readonly IFileListProxy fileListProxy;
    private readonly IMapTileGenerator tileGenerator;
    protected readonly IGeofenceProxy geofenceProxy;
    private ILogger<T> logger;
    private IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    protected ILogger<T> Log => logger ?? (logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IServiceExceptionHandler ServiceExceptionHandler => serviceExceptionHandler ?? (serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());



    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(IRaptorProxy raptorProxy, IPreferenceProxy prefProxy, IFileListProxy fileListProxy, IMapTileGenerator tileGenerator, IGeofenceProxy geofenceProxy)
    {
      this.raptorProxy = raptorProxy;
      this.prefProxy = prefProxy;
      this.fileListProxy = fileListProxy;
      this.tileGenerator = tileGenerator;
      this.geofenceProxy = geofenceProxy;
    }

    /// <summary>
    /// Executes the request with exception handling.
    /// </summary>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) //where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = action.Invoke();
        Log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, errorMessage1: ex.Message, innerException: ex);
      }
      finally
      {
        if (result is ContractExecutionResult)
        {
          var exResult = result as ContractExecutionResult;
          Log.LogInformation($"Executed {action.Method.Name} with the result {exResult?.Code}");
        }
      }

      return result;
    }

    /// <summary>
    /// Executes the request asynchronously with exception handling.
    /// </summary>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) //where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = await action.Invoke();
        Log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");

      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, ex.Message, innerException: ex);
      }
      finally
      {
        if (result is ContractExecutionResult)
        {
          var exResult = result as ContractExecutionResult;
          Log.LogInformation($"Executed {action.Method.Name} with the result {exResult?.Code}");
        }
      }
      return result;
    }


    /// <summary>
    /// Get the generated tile for the request
    /// </summary>
    protected async Task<FileResult> GetGeneratedTile(Guid projectUid, Guid? filterUid, Guid? cutFillDesignUid, Guid? baseUid, Guid? topUid, VolumeCalcType? volumeCalcType,
      TileOverlayType[] overlays, int width, int height, string bbox, MapType? mapType, DisplayMode? mode, string language, bool adjustBoundingBox)
    {
      var overlayTypes = overlays.ToList();
      if (overlays.Contains(TileOverlayType.AllOverlays))
      {
        overlayTypes = new List<TileOverlayType>((TileOverlayType[])Enum.GetValues(typeof(TileOverlayType)));
        overlayTypes.Remove(TileOverlayType.AllOverlays);
      }

      var project = await ((TilePrincipal)User).GetProject(projectUid);
      var dxfFiles = overlayTypes.Contains(TileOverlayType.DxfLinework)
        ? await GetFilesOfType(projectUid, ImportedFileType.Linework)
        : new List<FileData>();
      var haveFilter = filterUid.HasValue || baseUid.HasValue || topUid.HasValue;
      var customFilterBoundary = haveFilter && overlayTypes.Contains(TileOverlayType.FilterCustomBoundary)
        ? (await raptorProxy.GetFilterPointsList(projectUid, filterUid, baseUid, topUid, FilterBoundaryType.Polygon, CustomHeaders)).PointsList
        : new List<List<WGSPoint>>();
      var designFilterBoundary = haveFilter && overlayTypes.Contains(TileOverlayType.FilterDesignBoundary)
        ? (await raptorProxy.GetFilterPointsList(projectUid, filterUid, baseUid, topUid, FilterBoundaryType.Design, CustomHeaders)).PointsList
        : new List<List<WGSPoint>>();
      var alignmentFilterBoundary = haveFilter && overlayTypes.Contains(TileOverlayType.FilterAlignmentBoundary)
        ? (await raptorProxy.GetFilterPointsList(projectUid, filterUid, baseUid, topUid, FilterBoundaryType.Alignment, CustomHeaders)).PointsList
        : new List<List<WGSPoint>>();
      var designUid = !volumeCalcType.HasValue || volumeCalcType == VolumeCalcType.None ||
                      volumeCalcType == VolumeCalcType.GroundToGround
        ? cutFillDesignUid
        : (volumeCalcType == VolumeCalcType.DesignToGround ? baseUid : topUid);
      var designBoundary = designUid.HasValue && overlayTypes.Contains(TileOverlayType.CutFillDesignBoundary)
        ? (await raptorProxy.GetDesignBoundaryPoints(projectUid, designUid.Value, CustomHeaders)).PointsList
        : new List<List<WGSPoint>>();
      var alignmentPoints = overlayTypes.Contains(TileOverlayType.Alignments)
        ? (await raptorProxy.GetAlignmentPointsList(projectUid, CustomHeaders)).PointsList
        : new List<List<WGSPoint>>();

      language = string.IsNullOrEmpty(language) ? (await GetShortCachedUserPreferences()).Language : language;
      ////var geofences = overlayTypes.Contains(TileOverlayType.Geofences)
      ////  ? await geofenceProxy.GetGeofences((User as TilePrincipal).CustomerUid, CustomHeaders)
      ////  : new List<GeofenceData>();
      var geofences = new List<GeofenceData>();

      if (string.IsNullOrEmpty(bbox))
      {
        bbox = await raptorProxy.GetBoundingBox(projectUid, overlays, filterUid, cutFillDesignUid, baseUid, topUid,
          volumeCalcType, CustomHeaders);
      }

      var mapParameters = tileGenerator.GetMapParameters(bbox, width, height, overlayTypes.Contains(TileOverlayType.ProjectBoundary), adjustBoundingBox);

      var request = TileGenerationRequest.CreateTileGenerationRequest(filterUid, baseUid, topUid, cutFillDesignUid, volumeCalcType, geofences,
        alignmentPoints, customFilterBoundary, designFilterBoundary, alignmentFilterBoundary, designBoundary,
        dxfFiles, overlayTypes, width, height, mapType, mode, language, project, mapParameters, CustomHeaders);

      request.Validate();

      var byteResult = await WithServiceExceptionTryExecuteAsync(async () =>
        await tileGenerator.GetMapData(request));

      return new FileStreamResult(new MemoryStream(byteResult), "image/png");

    }

    /// <summary>
    /// Get the generated tile for the request. Used for geofence thumbnails.
    /// </summary>
    protected async Task<FileResult> GetGeneratedTile(GeofenceData geofence,
      TileOverlayType[] overlays, int width, int height, string bbox, MapType? mapType, 
      string language, bool adjustBoundingBox)
    {
      var overlayTypes = overlays.ToList();

      language = string.IsNullOrEmpty(language) ? (await GetShortCachedUserPreferences()).Language : language;

      var geofences = new List<GeofenceData>{geofence};

      var mapParameters = tileGenerator.GetMapParameters(bbox, width, height, overlayTypes.Contains(TileOverlayType.GeofenceBoundary), adjustBoundingBox);

      var request = TileGenerationRequest.CreateTileGenerationRequest(null, null, null, null, null, geofences,
        null, null, null, null, null, null, overlayTypes, width, height, mapType, null, language, null, mapParameters, CustomHeaders);

      request.Validate();

      var byteResult = await WithServiceExceptionTryExecuteAsync(async () =>
        await tileGenerator.GetMapData(request));

      return new FileStreamResult(new MemoryStream(byteResult), "image/png");
    }

    /// <summary>
      /// Get user preferences
      /// </summary>
      private async Task<UserPreferenceData> GetShortCachedUserPreferences()
    {
      var userPreferences = await prefProxy.GetShortCachedUserPreferences(((TilePrincipal)User).UserEmail, TimeSpan.FromSeconds(10), CustomHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }

      return userPreferences;
    }

    /// <summary>
    /// Gets the imported files of the specified type in a project
    /// </summary>
    /// <param name="projectUid">The project UID</param>
    /// <param name="fileType">The type of files to retrieve</param>
    /// <returns>List of active imported files of specified type</returns>
    private async Task<List<FileData>> GetFilesOfType(Guid projectUid, ImportedFileType fileType)
    {
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return new List<FileData>();
      }

      return fileList.Where(f => f.ImportedFileType == fileType && f.IsActivated).ToList();
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    protected string GetUserId()
    {
      if (User is TilePrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

  }

}
