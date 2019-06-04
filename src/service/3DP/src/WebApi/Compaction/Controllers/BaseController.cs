using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Log4NetExtensions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Extensions;
using VSS.TRex.Gateway.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Common base class for Raptor service controllers.
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {
#if RAPTOR
    private IASNodeClient raptorClient;
#endif
    private ILogger<T> logger;
    private ILoggerFactory loggerFactory;
    private IFilterServiceProxy filterServiceProxy;
    private IProjectSettingsProxy projectSettingsProxy;
    private ITRexCompactionDataProxy tRexCompactionDataProxy;
    private IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Gets the filter service proxy interface.
    /// </summary>
    private IFilterServiceProxy FilterServiceProxy => filterServiceProxy ?? (filterServiceProxy = HttpContext.RequestServices.GetService<IFilterServiceProxy>());

    /// <summary>
    /// Gets the project settings proxy interface.
    /// </summary>
    private IProjectSettingsProxy ProjectSettingsProxy => projectSettingsProxy ?? (projectSettingsProxy = HttpContext.RequestServices.GetService<IProjectSettingsProxy>());

    /// <summary>
    /// Gets the tRex CompactionData proxy interface.
    /// </summary>
    protected ITRexCompactionDataProxy TRexCompactionDataProxy => tRexCompactionDataProxy ?? (tRexCompactionDataProxy = HttpContext.RequestServices.GetService<ITRexCompactionDataProxy>());

    /// <summary>
    /// helper methods for getting project statistics from Raptor/TRex
    /// </summary>
    private ProjectStatisticsHelper _projectStatisticsHelper = null;
    protected ProjectStatisticsHelper ProjectStatisticsHelper => _projectStatisticsHelper ?? (_projectStatisticsHelper = new ProjectStatisticsHelper(LoggerFactory, ConfigStore, FileImportProxy, TRexCompactionDataProxy
#if RAPTOR
         , RaptorClient
#endif
       ));

    /// <summary>
    /// Gets the memory cache of previously fetched, and valid, <see cref="FilterResult"/> objects
    /// </summary>
    private IDataCache FilterCache => HttpContext.RequestServices.GetService<IDataCache>();

    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IServiceExceptionHandler ServiceExceptionHandler => serviceExceptionHandler ?? (serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());
#if RAPTOR
    protected IASNodeClient RaptorClient => raptorClient ?? (raptorClient = HttpContext.RequestServices.GetService<IASNodeClient>());
#endif
    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    protected ILogger<T> Log => logger ?? (logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    /// <summary>
    /// Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders.
    /// </summary>
    protected ILoggerFactory LoggerFactory => loggerFactory ?? (loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore ConfigStore;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    protected readonly IFileImportProxy FileImportProxy;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    protected readonly ICompactionSettingsManager SettingsManager;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly MemoryCacheEntryOptions filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    /// <summary>
    /// Indicates whether to use the TRex Gateway instead of calling to the Raptor client.
    /// </summary>
    protected bool UseTRexGateway(string key) => bool.TryParse(ConfigStore.GetValueString(key), out var useTrexGateway) && useTrexGateway;

    /// <summary>
    /// 
    /// </summary>
    protected BaseController(IConfigurationStore configStore)
    {
      ConfigStore = configStore;
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
    {
      ConfigStore = configStore;
      FileImportProxy = fileImportProxy;
      SettingsManager = settingsManager;
    }

    /// <summary>
    /// Returns the legacy ProjectId (long) for a given ProjectUid (Guid).
    /// </summary>
    protected Task<long> GetLegacyProjectId(Guid projectUid)
    {
      return ((RaptorPrincipal)User).GetLegacyProjectId(projectUid);
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
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
        if (Log.IsTraceEnabled())
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
    /// With the service exception try execute async.
    /// </summary>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = await action.Invoke();
        if (Log.IsTraceEnabled())
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
    /// Gets the <see cref="DesignDescriptor"/> from a given project's fileUid.
    /// </summary>
    protected async Task<DesignDescriptor> GetAndValidateDesignDescriptor(Guid projectUid, Guid? fileUid, OperationType operation = OperationType.General)
    {
      if (!fileUid.HasValue)
      {
        return null;
      }

      var fileList = await FileImportProxy.GetFiles(projectUid.ToString(), GetUserId(), CustomHeaders);
      if (fileList == null || fileList.Count == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Project has no appropriate design files."));
      }

      FileData file = null;

      foreach (var f in fileList)
      {
        bool operationSupported = true;
        switch (operation)
        {
          case OperationType.Profiling:
            operationSupported = f.IsProfileSupportedFileType();
            break;
          case OperationType.GeneratingDxf:
            operationSupported = f.ImportedFileType == ImportedFileType.Alignment;
            break;
          default:
            //All file types supported
            break;
        }
        if (f.ImportedFileUid == fileUid.ToString() && f.IsActivated && operationSupported)
        {
          file = f;

          break;
        }
      }

      if (file == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Unable to access design or alignment file."));
      }

      var tccFileName = file.Name;
      if (file.ImportedFileType == ImportedFileType.SurveyedSurface)
      {
        //Note: ':' is an invalid character for filenames in Windows so get rid of them
        tccFileName = Path.GetFileNameWithoutExtension(tccFileName) +
                      "_" + file.SurveyedUtc.Value.ToIso8601DateTimeString().Replace(":", string.Empty) +
                      Path.GetExtension(tccFileName);
      }

#if RAPTOR
      //For Raptor, need the parent design if it's a reference surface
      if (file.ImportedFileType == ImportedFileType.ReferenceSurface)
      {
        var parent = fileList.FirstOrDefault(f => f.ImportedFileUid == file.ParentUid);
        if (parent == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Unable to access parent design file."));
        }

        fileUid = Guid.Parse(parent.ImportedFileUid);
        file.LegacyFileId = parent.LegacyFileId;
        tccFileName = parent.Name;
        //The file.Path is CustomerUid + ProjectUid which should be the same for both
      }
#endif

      var fileSpaceId = FileDescriptorExtensions.GetFileSpaceId(ConfigStore, Log);
      var fileDescriptor = FileDescriptor.CreateFileDescriptor(fileSpaceId, file.Path, tccFileName);

      return new DesignDescriptor(file.LegacyFileId, fileDescriptor, file.Offset ?? 0.0, fileUid);
    }

    /// <summary>
    /// Gets the project settings targets for the project.
    /// </summary>
    protected async Task<CompactionProjectSettings> GetProjectSettingsTargets(Guid projectUid)
    {
      CompactionProjectSettings ps;
      var jsonSettings = await ProjectSettingsProxy.GetProjectSettings(projectUid.ToString(), GetUserId(), CustomHeaders, ProjectSettingsType.Targets);
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
    protected async Task<CompactionProjectSettingsColors> GetProjectSettingsColors(Guid projectUid)
    {
      CompactionProjectSettingsColors ps;
      var jsonSettings = await ProjectSettingsProxy.GetProjectSettings(projectUid.ToString(), GetUserId(), CustomHeaders, ProjectSettingsType.Colors);
      if (jsonSettings != null)
      {
        try
        {
          ps = jsonSettings.ToObject<CompactionProjectSettingsColors>();
          ps.UpdateCmvDetailsColorsIfRequired();
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
    protected async Task<FilterResult> GetCompactionFilter(Guid projectUid, Guid? filterUid, bool filterMustExist = false)
    {
      var filterKey = filterUid.HasValue ? $"{nameof(FilterResult)} {filterUid.Value}" : string.Empty;
      // Filter models are immutable except for their Name.
      // This service doesn't consider the Name in any of it's operations so we don't mind if our
      // cached object is out of date in this regard.
      var cachedFilter = filterUid.HasValue ? FilterCache.Get<FilterResult>(filterKey) : null;
      if (cachedFilter != null)
      {
        await ApplyDateRange(projectUid, cachedFilter);

        return cachedFilter;
      }

      var excludedIds = await ProjectStatisticsHelper.GetExcludedSurveyedSurfaceIds(projectUid, GetUserId(), CustomHeaders);
      bool haveExcludedIds = excludedIds != null && excludedIds.Count > 0;
      DesignDescriptor designDescriptor = null;
      DesignDescriptor alignmentDescriptor = null;

      if (!filterUid.HasValue)
      {
        return haveExcludedIds
          ? FilterResult.CreateFilter(excludedIds)
          : null;
      }

      try
      {
        var filterData = await GetFilterDescriptor(projectUid, filterUid.Value);

        if (filterMustExist && filterData == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid Filter UID."));
        }

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
              new WGSPoint(p.Lat.LatDegreesToRadians(), p.Lon.LonDegreesToRadians()));

            var layerMethod = filterData.LayerNumber.HasValue
              ? FilterLayerMethod.TagfileLayerNumber
              : FilterLayerMethod.None;

            bool? returnEarliest = null;
            if (filterData.ElevationType == ElevationType.First)
            {
              returnEarliest = true;
            }

            var raptorFilter = new FilterResult(filterUid, filterData, polygonPoints, alignmentDescriptor, layerMethod, excludedIds, returnEarliest, designDescriptor);

            Log.LogDebug($"Filter after filter conversion: {JsonConvert.SerializeObject(raptorFilter)}");

            // The filter will be removed from memory and recalculated to ensure we have the latest filter on any relevant changes
            var filterTags = new List<string>()
            {
              filterUid.Value.ToString(),
              projectUid.ToString()
            };

            FilterCache.Set(filterKey, raptorFilter, filterTags, filterCacheOptions);

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

      return haveExcludedIds ? FilterResult.CreateFilter(excludedIds) : null;
    }

    /// <summary>
    /// Dynamically set the date range according to the<see cref= "DateRangeType" /> property.
    /// </summary>
    /// <remarks>
    /// Custom date range is unaltered. Project extents is always null. Other types are calculated in the project time zone.
    /// </remarks>
    private async Task ApplyDateRange(Guid projectUid, Filter.Abstractions.Models.Filter filter)
    {
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
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
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      if (project == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to retrieve project."));
      }

      filter.ApplyDateRange(project.IanaTimeZone);
    }

    /// <summary>
    /// Gets the <see cref="FilterDescriptor"/> for a given Filter FileUid (by project).
    /// </summary>
    protected async Task<Filter.Abstractions.Models.Filter> GetFilterDescriptor(Guid projectUid, Guid filterUid)
    {
      var filterDescriptor = await FilterServiceProxy.GetFilter(projectUid.ToString(), filterUid.ToString(), Request.Headers.GetCustomHeaders(true));

      return filterDescriptor == null
        ? null
        : JsonConvert.DeserializeObject<Filter.Abstractions.Models.Filter>(filterDescriptor.FilterJson);
    }
  }
}
