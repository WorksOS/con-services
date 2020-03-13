using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
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
    private IProjectProxy projectProxy;
    private IAccountProxy accountProxy;
    private IDeviceProxy deviceProxy;

    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    protected ServiceTypeMappings serviceTypeMappings = new ServiceTypeMappings();

    protected static DataRepository dataRepository = null;
    

    /// <summary>
    ///   Generates the errorlist for instantiated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      return (from object enumVal in Enum.GetValues(typeof(ContractExecutionStatesEnum))
              select new Tuple<int, string>((int)enumVal, enumVal.ToString())).ToList();
    }


    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    protected abstract ContractExecutionResult ProcessEx<T>(T item); 

    protected abstract Task<ContractExecutionResult> ProcessAsyncEx<T>(T item);

    internal static object Build<T>()
    {
      throw new NotImplementedException();
    }

    /// <summary> </summary>
    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return ProcessEx(item);
    }


    /// <summary> </summary>
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
    public static TExecutor Build<TExecutor>(ILogger logger, IConfigurationStore configStore, 
      IProjectProxy projectProxy, IAccountProxy accountProxy, IDeviceProxy deviceProxy) 
      where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() { log = logger, configStore = configStore, projectProxy = projectProxy, accountProxy = accountProxy, deviceProxy = deviceProxy};
        dataRepository = new DataRepository(logger, configStore, projectProxy, accountProxy, deviceProxy);
      return executor;
    }
    
  }
}
