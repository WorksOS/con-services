using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Executor for adding production data edits using override events
  /// </summary>
  public class AddEditDataExecutor : BaseExecutor
  {
    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public AddEditDataExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AddEditDataExecutor()
    {
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<TRexEditDataRequest>(item);
      var result = new ContractExecutionResult();
      try
      {
        log.LogInformation($"#In# AddEditDataExecutor. Add data edit {JsonConvert.SerializeObject(request)}");
        var overrideRequest = new OverrideEventRequest();
        var arg = new OverrideEventRequestArgument(false, request.ProjectUid, request.AssetUid,
          request.StartUtc, request.EndUtc, request.MachineDesignName, (ushort?) request.LiftNumber);
        var overrideResult = await overrideRequest.ExecuteAsync(arg);
        if (!overrideResult.Success)
        {
          log.LogInformation($"Failed to add data edit: {overrideResult.Message}");
          result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, overrideResult.Message);
        }
      }
      finally
      {
        log.LogInformation($"#Out# AddEditDataExecutor. Add data edit {JsonConvert.SerializeObject(request)}");
      }

      return result;
    }

    /// <summary>
    /// Processes the request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
