using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using CCSS.Productivity3D.Service.Common;
using CCSS.Productivity3D.Service.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Common base class for Raptor service controllers.
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {
    private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IFilterServiceProxy _filterServiceProxy;
    private IProjectSettingsProxy _projectSettingsProxy;
    private ITRexCompactionDataProxy _tRexCompactionDataProxy;
    private ITagFileAuthProjectV5Proxy _tagFileAuthProjectV5Proxy;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private ProjectStatisticsHelper _projectStatisticsHelper;

    /// <summary>
    /// Gets the filter service proxy interface.
    /// </summary>
    protected IFilterServiceProxy FilterServiceProxy => _filterServiceProxy ??= HttpContext.RequestServices.GetService<IFilterServiceProxy>();

    /// <summary>
    /// Gets the project settings proxy interface.
    /// </summary>
    private IProjectSettingsProxy ProjectSettingsProxy => _projectSettingsProxy ??= HttpContext.RequestServices.GetService<IProjectSettingsProxy>();

    /// <summary>
    /// Gets the tRex CompactionData proxy interface.
    /// </summary>
    protected ITRexCompactionDataProxy TRexCompactionDataProxy => _tRexCompactionDataProxy ??= HttpContext.RequestServices.GetService<ITRexCompactionDataProxy>();

    /// <summary>
    /// Gets the tagfile authorization proxy interface.
    /// </summary>
    protected ITagFileAuthProjectV5Proxy TagFileAuthProjectV5Proxy => _tagFileAuthProjectV5Proxy ??= HttpContext.RequestServices.GetService<ITagFileAuthProjectV5Proxy>();

    /// <summary>
    /// helper methods for getting project statistics from Raptor/TRex
    /// </summary>
    protected ProjectStatisticsHelper ProjectStatisticsHelper => _projectStatisticsHelper ??= new ProjectStatisticsHelper(LoggerFactory, ConfigStore, FileImportProxy, TRexCompactionDataProxy);

    /// <summary>
    /// Gets the memory cache of previously fetched, and valid, <see cref="FilterResult"/> objects
    /// </summary>
    protected IDataCache FilterCache => HttpContext.RequestServices.GetService<IDataCache>();

    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ??= HttpContext.RequestServices.GetService<IServiceExceptionHandler>();

    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    protected ILogger<T> Log => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();

    /// <summary>
    /// Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders.
    /// </summary>
    protected ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();

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
    protected IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly MemoryCacheEntryOptions _filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    /// <summary>
    /// Indicates whether to use the TRex Gateway instead of calling to the Raptor client.
    /// </summary>
    protected bool UseTRexGateway(string key) => ConfigStore.GetValueBool(key) ?? false;

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
    protected Task<DesignDescriptor> GetAndValidateDesignDescriptor(Guid projectUid, Guid? fileUid, OperationType operation = OperationType.General)
    {
      return DesignUtilities.GetAndValidateDesignDescriptor(projectUid, fileUid, GetUserId(), CustomHeaders, FileImportProxy, ConfigStore, Log);
    }

    /// <summary>
    /// Gets the project settings targets for the project.
    /// </summary>
    protected Task<CompactionProjectSettings> GetProjectSettingsTargets(Guid projectUid)
    {
      return ProjectSettingsProxy.GetProjectSettingsTargets(projectUid.ToString(), GetUserId(), CustomHeaders, ServiceExceptionHandler);
    }

    /// <summary>
    /// Gets the project settings colors for the project.
    /// </summary>
    protected Task<CompactionProjectSettingsColors> GetProjectSettingsColors(Guid projectUid)
    { ;
      return ProjectSettingsProxy.GetProjectSettingsColors(projectUid.ToString(), GetUserId(), CustomHeaders, ServiceExceptionHandler);
    }

    protected FilterResult SetupCompactionFilter(Guid projectUid, BoundingBox2DGrid boundingBox)
    {
      var filterResult = new FilterResult();
      filterResult.SetBoundary(new List<Point>
      {
        new Point(boundingBox.BottomleftY, boundingBox.BottomLeftX),
        new Point(boundingBox.BottomleftY, boundingBox.TopRightX),
        new Point(boundingBox.TopRightY, boundingBox.TopRightX),
        new Point(boundingBox.TopRightY, boundingBox.BottomLeftX)
      });
      return filterResult;
    }

    /// <summary>
    /// Creates an instance of the <see cref="FilterResult"/> class and populates it with data from the <see cref="Filter"/> model class.
    /// </summary>
    protected async Task<FilterResult> GetCompactionFilter(Guid projectUid, Guid? filterUid, bool filterMustExist = false)
    {
      var projectTimeZone = await ProjectTimeZone(projectUid);
      return await FilterUtilities.GetCompactionFilter(projectUid, projectTimeZone, GetUserId(), filterUid, FilterCache, CustomHeaders, Log, FilterServiceProxy, FileImportProxy, ConfigStore, filterMustExist);
    }

    /// <summary>
    /// Dynamically set the date range according to the<see cref= "DateRangeType" /> property.
    /// </summary>
    /// <remarks>
    /// Custom date range is unaltered. Project extents is always null. Other types are calculated in the project time zone.
    /// </remarks>
    private async Task ApplyDateRange(Guid projectUid, Filter.Abstractions.Models.Filter filter)
    {
      var projectTimeZone = await ProjectTimeZone(projectUid);
      filter.ApplyDateRange(projectTimeZone, true);
    }

    /// <summary>
    /// Gets the project time zone.
    /// </summary>
    protected async Task<string> ProjectTimeZone(Guid projectUid)
    {
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      if (project == null)
      {
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Failed to retrieve project."));
      }

      return project.IanaTimeZone;
    }

    /// <summary>
    /// Gets the <see cref="FilterDescriptor"/> for a given Filter FileUid (by project).
    /// </summary>
    protected async Task<Filter.Abstractions.Models.Filter> GetFilterDescriptor(Guid projectUid, Guid filterUid)
    {
      var filterDescriptor = await FilterServiceProxy.GetFilter(projectUid.ToString(), filterUid.ToString(), Request.Headers.GetCustomHeaders());

      return filterDescriptor == null
        ? null
        : JsonConvert.DeserializeObject<Filter.Abstractions.Models.Filter>(filterDescriptor.FilterJson);
    }

    /// <summary>
    /// Gets the summary volumes parameters according to the calculation type
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="volumeCalcType">The summary volumes calculation type</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <returns>Tuple of base filter, top filter and volume design descriptor</returns>
    protected async Task<Tuple<FilterResult, FilterResult, DesignDescriptor>> GetSummaryVolumesParameters(Guid projectUid, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid)
    {
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);

      return await VolumesUtilities.GetSummaryVolumesParameters(
        projectUid, volumeCalcType, volumeBaseUid, volumeTopUid, GetUserId(), CustomHeaders, FileImportProxy, ConfigStore, Log, FilterServiceProxy, FilterCache, project.IanaTimeZone);
    }
  }
}
