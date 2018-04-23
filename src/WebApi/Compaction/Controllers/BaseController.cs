using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Common base class for Raptor service controllers.
  /// </summary>
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    protected ILogger Log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    protected readonly ILoggerFactory LoggerFactory;

    /// <summary>
    /// Gets the
    /// </summary>
    protected readonly IServiceExceptionHandler ServiceExceptionHandler;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore ConfigStore;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    protected readonly IFileListProxy FileListProxy;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    protected readonly IProjectSettingsProxy ProjectSettingsProxy;

    /// <summary>
    /// For getting list of persistent filters for a project
    /// </summary>
    protected readonly IFilterServiceProxy FilterServiceProxy;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    protected readonly ICompactionSettingsManager SettingsManager;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the memory cache of previously fetched, and valid, <see cref="FilterResult"/> objects
    /// </summary>
    private IMemoryCache FilterCache => HttpContext.RequestServices.GetService<IMemoryCache>();

    private readonly MemoryCacheEntryOptions filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory loggerFactory, ILogger log, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore, IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, IFilterServiceProxy filterServiceProxy, ICompactionSettingsManager settingsManager)
    {
      this.LoggerFactory = loggerFactory;
      this.Log = log;
      this.ServiceExceptionHandler = serviceExceptionHandler;
      this.ConfigStore = configStore;
      this.FileListProxy = fileListProxy;
      this.ProjectSettingsProxy = projectSettingsProxy;
      this.FilterServiceProxy = filterServiceProxy;
      this.SettingsManager = settingsManager;
    }

    /// <summary>
    /// Returns the legacy ProjectId (long) for a given ProjectUid (Guid).
    /// </summary>
    protected async Task<long> GetLegacyProjectId(Guid projectUid)
    {
      return await ((RaptorPrincipal)this.User).GetLegacyProjectId(projectUid);
    } 

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    protected string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) where TResult : ContractExecutionResult
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
        Log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }

    /// <summary>
    /// Async form of WithServiceExceptionTryExecute
    /// </summary>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) where TResult : ContractExecutionResult
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
        Log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }

    /// <summary>
    /// Gets the ids of the surveyed surfaces to exclude from Raptor calculations. 
    /// This is the deactivated ones.
    /// </summary>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <returns>The list of file ids for the surveyed surfaces to be excluded</returns>
    protected async Task<List<long>> GetExcludedSurveyedSurfaceIds(Guid projectUid)
    {
      var fileList = await this.FileListProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        return null;
      }

      var results = fileList
        .Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface && !f.IsActivated)
        .Select(f => f.LegacyFileId).ToList();

      return results;
    }

    /// <summary>
    /// Gets the <see cref="DesignDescriptor"/> from a given project's fileUid.
    /// </summary>
    protected async Task<DesignDescriptor> GetAndValidateDesignDescriptor(Guid projectUid, Guid? fileUid, bool forProfile = false)
    {
      if (!fileUid.HasValue)
      {
        return null;
      }

      var fileList = await this.FileListProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Project has no appropriate design files."));
      }

      FileData file = null;

      foreach (var f in fileList)
      {
        if (f.ImportedFileUid == fileUid.ToString() && f.IsActivated && (!forProfile || f.IsProfileSupportedFileType()))
        {
          file = f;

          break;
        }
      }

      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to access design file."));
      }

      var tccFileName = file.Name;
      if (file.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        //Note: ':' is an invalid character for filenames in Windows so get rid of them
        tccFileName = Path.GetFileNameWithoutExtension(tccFileName) +
                      "_" + file.SurveyedUtc.Value.ToIso8601DateTimeString().Replace(":", string.Empty) +
                      Path.GetExtension(tccFileName);
      }

      string fileSpaceId = FileDescriptorExtensions.GetFileSpaceId(this.ConfigStore, this.Log);
      FileDescriptor fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, file.Path, tccFileName);

      return DesignDescriptor.CreateDesignDescriptor(file.LegacyFileId, fileDescriptor, 0.0);
    }

    /// <summary>
    /// Gets the project settings targets for the project.
    /// </summary>
    /// <param name="projectUid">The UID of the project.</param>
    /// <returns>The project settings targets.</returns>
    protected async Task<CompactionProjectSettings> GetProjectSettingsTargets(Guid projectUid)
    {
      CompactionProjectSettings ps;
      var jsonSettings = await this.ProjectSettingsProxy.GetProjectSettings(projectUid.ToString(), GetUserId(), CustomHeaders, ProjectSettingsType.Targets);
      if (jsonSettings != null)
      {
        try
        {
          ps = jsonSettings.ToObject<CompactionProjectSettings>();
          ps.Validate();
        }
        catch (Exception ex)
        {
          Log.LogInformation(
            $"JObject conversion to Project Settings targets or validation failure for projectUid {projectUid}. Error is {ex.Message}");
          ps = CompactionProjectSettings.DefaultSettings;
        }
      }
      else
      {
        Log.LogDebug($"No Project Settings targets for projectUid {projectUid}. Using defaults.");
        ps = CompactionProjectSettings.DefaultSettings;
      }
      return ps;
    }

    /// <summary>
    /// Gets the project settings colors for the project.
    /// </summary>
    /// <param name="projectUid">The UID of the project.</param>
    /// <returns>The project settings colors.</returns>
    protected async Task<CompactionProjectSettingsColors> GetProjectSettingsColors(Guid projectUid)
    {
      CompactionProjectSettingsColors ps;
      var jsonSettings = await this.ProjectSettingsProxy.GetProjectSettings(projectUid.ToString(), GetUserId(), CustomHeaders, ProjectSettingsType.Colors);
      if (jsonSettings != null)
      {
        try
        {
          ps = jsonSettings.ToObject<CompactionProjectSettingsColors>();
          ps.Validate();
        }
        catch (Exception ex)
        {
          Log.LogInformation(
            $"JObject conversion to Project Settings colours or validation failure for projectUid {projectUid}. Error is {ex.Message}");
          ps = CompactionProjectSettingsColors.DefaultSettings;
        }
      }
      else
      {
        Log.LogDebug($"No Project Settings colours for projectUid {projectUid}. Using defaults.");
        ps = CompactionProjectSettingsColors.DefaultSettings;
      }
      return ps;
    }

    /// <summary>
    /// Creates an instance of the <see cref="FilterResult"/> class and populates it with data from the <see cref="Filter"/> model class.
    /// </summary>
    /// <returns>An instance of the <see cref="FilterResult"/> class.</returns>
    protected async Task<FilterResult> GetCompactionFilter(Guid projectUid, Guid? filterUid)
    {
      // Filter models are immutable except for their Name.
      // This service doesn't consider the Name in any of it's operations so we don't mind if our 
      // cached object is out of date in this regard.
      if (filterUid.HasValue && this.FilterCache.TryGetValue(filterUid, out FilterResult cachedFilter))
      {
        await ApplyDateRange(projectUid, cachedFilter);

        return cachedFilter;
      }

      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid);
      bool haveExcludedIds = excludedIds != null && excludedIds.Count > 0;
      DesignDescriptor designDescriptor = null;
      DesignDescriptor alignmentDescriptor = null;

      if (filterUid.HasValue)
      {
        try
        {
          var filterData = await GetFilterDescriptor(projectUid, filterUid.Value);
          if (filterData != null)
          {
            Log.LogDebug($"Filter from Filter Svc: {JsonConvert.SerializeObject(filterData)}");
            if (filterData.DesignUid != null && Guid.TryParse(filterData.DesignUid, out Guid designUidGuid))
            {
              designDescriptor = await GetAndValidateDesignDescriptor(projectUid, designUidGuid);
            }

            if (filterData.AlignmentUid != null && Guid.TryParse(filterData.AlignmentUid, out Guid alignmentUidGuid))
            {
              alignmentDescriptor = await GetAndValidateDesignDescriptor(projectUid, alignmentUidGuid);
            }

            if (filterData.HasData() || haveExcludedIds || designDescriptor != null)
            {
              await ApplyDateRange(projectUid, filterData);

              var polygonPoints = filterData.PolygonLL?.ConvertAll(p =>
                Common.Models.WGSPoint.CreatePoint(p.Lat.LatDegreesToRadians(), p.Lon.LonDegreesToRadians()));

              var layerMethod = filterData.LayerNumber.HasValue
                ? FilterLayerMethod.TagfileLayerNumber
                : FilterLayerMethod.None;

              bool? returnEarliest = null;
              if (filterData.ElevationType == ElevationType.First)
              {
                returnEarliest = true;
              }

              var raptorFilter = FilterResult.CreateFilter(filterData, polygonPoints, alignmentDescriptor, layerMethod, excludedIds, returnEarliest, designDescriptor);

              Log.LogDebug($"Filter after filter conversion: {JsonConvert.SerializeObject(raptorFilter)}");

              FilterCache.Set(filterUid, raptorFilter, filterCacheOptions);

              return raptorFilter;
            }
          }
        }
        catch (ServiceException ex)
        {
          Log.LogDebug($"EXCEPTION caught - cannot find filter {ex.Message} {ex.GetContent} {ex.GetResult.Message}");
          throw;
        }
        catch (Exception ex)
        {
          Log.LogDebug("EXCEPTION caught - cannot find filter " + ex.Message);
          throw;
        }
      }

      return haveExcludedIds ? FilterResult.CreateFilter(excludedIds) : null;
    }

    /// <summary>
    /// Dynamically set the date range according to the<see cref= "Filter.DateRangeType" /> property.
    /// </summary>
    /// <remarks>
    /// Custom date range is unaltered. Project extents is always null. Other types are calculated in the project time zone.
    /// </remarks>
    private async Task ApplyDateRange(Guid projectUid, Filter filter)
    {
      var project = await (this.User as RaptorPrincipal)?.GetProject(projectUid);
      if (project == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to retrieve project."));
      }

      filter.ApplyDateRange(project.IanaTimeZone, true);
    }

    private async Task ApplyDateRange(Guid projectUid, FilterResult filter)
    {
      var project = await (this.User as RaptorPrincipal)?.GetProject(projectUid);
      if (project == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to retrieve project."));
      }

      filter.ApplyDateRange(project.IanaTimeZone);
    }

    /// <summary>
    /// Gets the <see cref="FilterDescriptor"/> for a given Filter Uid (by project).
    /// </summary>
    public async Task<Filter> GetFilterDescriptor(Guid projectUid, Guid filterUid)
    {
      var filterDescriptor = await this.FilterServiceProxy.GetFilter(projectUid.ToString(), filterUid.ToString(), Request.Headers.GetCustomHeaders(true));

      return filterDescriptor == null
        ? null
        : JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterJson);
    }

    /// <summary>
    /// Gets the summary volumes parameters according to the calcultion type
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="volumeCalcType">The summary volumes calculation type</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <returns>Tuple of base filter, top filter and volume design descriptor</returns>
    protected async Task<Tuple<FilterResult, FilterResult, DesignDescriptor>> GetSummaryVolumesParameters(Guid projectUid, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid)
    {
      FilterResult baseFilter = null;
      FilterResult topFilter = null;
      DesignDescriptor volumeDesign = null;

      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.GroundToGround:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
          case VolumeCalcType.GroundToDesign:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeTopUid, true);
            break;
          case VolumeCalcType.DesignToGround:
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeBaseUid, true);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
        }
      }

      return new Tuple<FilterResult, FilterResult, DesignDescriptor>(baseFilter, topFilter, volumeDesign);
    }
  }
}
