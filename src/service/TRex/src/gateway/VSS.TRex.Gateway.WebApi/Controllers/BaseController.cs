using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Serilog.Extensions;
using VSS.TRex.Gateway.Common.Helpers;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Common base class for TRex gateway controllers.
  /// </summary>
  public class BaseController : Controller
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
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory loggerFactory, ILogger log, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
    {
      LoggerFactory = loggerFactory;
      Log = log;
      ServiceExceptionHandler = serviceExceptionHandler;
      ConfigStore = configStore;
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
    /// Async form of WithServiceExceptionTryExecute
    /// </summary>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) where TResult : ContractExecutionResult
    {
      var result = default(TResult);
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
    /// Filter validation common to APIs passing 3dp Filter/s
    /// </summary>
    /// <param name="method"></param>
    /// <param name="projectUid"></param>
    /// <param name="filterResult"></param>
    /// <exception cref="ServiceException"></exception>
    protected void ValidateFilterMachines(string method, Guid? projectUid, FilterResult filterResult)
    {
      if (projectUid == null || projectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Invalid project UID {projectUid}"));
      }
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(method, projectUid.Value);

      if (filterResult != null && filterResult.ContributingMachines != null)
        GatewayHelper.ValidateMachines(filterResult.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);

      if (filterResult != null && !string.IsNullOrEmpty(filterResult.OnMachineDesignName))
      {
        var machineDesign = siteModel.SiteModelMachineDesigns.Locate(filterResult.OnMachineDesignName);
        if (machineDesign == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Unknown DesignName: {filterResult.OnMachineDesignName}."));
        }

        filterResult.OnMachineDesignId = machineDesign.Id;
      }
    }
  }
}
