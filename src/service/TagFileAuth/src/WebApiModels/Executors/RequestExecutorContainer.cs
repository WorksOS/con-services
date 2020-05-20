using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.WebApi.Common;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.Models.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to separate executor logic
  ///   from
  ///   controller logic for testability and possible executor version.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    protected ILogger log;
    private IConfigurationStore configStore;
    private ITPaaSApplicationAuthentication authorization;
    private IHeaderDictionary requestCustomHeaders;

    private ICwsAccountClient cwsAccountClient;
    private IProjectInternalProxy projectProxy;
    private IDeviceInternalProxy deviceProxy;

    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    protected ServiceTypeMappings serviceTypeMappings = new ServiceTypeMappings();

    protected static DataRepository dataRepository = null;

    protected readonly ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();


    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    protected abstract Task<ContractExecutionResult> ProcessAsyncEx<T>(T item);

    internal static object Build<T>()
    {
      throw new NotImplementedException();
    }

    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return ProcessEx(item);
    }

    public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return await ProcessAsyncEx(item);
    }

    /// <summary>
    ///   Builds this instance for specified executor type.
    /// </summary>
    public static TExecutor Build<TExecutor>(ILogger logger, IConfigurationStore configStore, ITPaaSApplicationAuthentication authorization,
      ICwsAccountClient cwsAccountClient, IProjectInternalProxy projectProxy, IDeviceInternalProxy deviceProxy,
      IHeaderDictionary requestCustomHeaders)
      where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() { log = logger, configStore = configStore, authorization = authorization, cwsAccountClient = cwsAccountClient, projectProxy = projectProxy, deviceProxy = deviceProxy, requestCustomHeaders = requestCustomHeaders };
      dataRepository = new DataRepository(authorization, cwsAccountClient, projectProxy, deviceProxy, requestCustomHeaders);
      return executor;
    }
  }
}
