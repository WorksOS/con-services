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
  /// Executor for removing previously added production data edits
  /// </summary>
  public class RemoveEditDataExecutor : BaseExecutor
  {
    /// <summary>
    /// Constructor with dependency injection
    /// </summary>
    public RemoveEditDataExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public RemoveEditDataExecutor()
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
        log.LogInformation($"#In# RemoveEditDataExecutor. Remove data edit {JsonConvert.SerializeObject(request)}");
        var overrideRequest = new OverrideEventRequest();
        var arg = new OverrideEventRequestArgument(true, request.ProjectUid, request.AssetUid,
          request.StartUtc, request.EndUtc, request.MachineDesignName, (ushort?)request.LiftNumber);
        var overrideResult = await overrideRequest.ExecuteAsync(arg);
        if (!overrideResult.Success)
        {
          log.LogInformation($"Failed to remove data edit: {overrideResult.Message}");
          result = new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, overrideResult.Message);
        }
      }
      finally
      {
        log.LogInformation($"#Out# RemoveEditDataExecutor. Add data edit {JsonConvert.SerializeObject(request)}");
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
