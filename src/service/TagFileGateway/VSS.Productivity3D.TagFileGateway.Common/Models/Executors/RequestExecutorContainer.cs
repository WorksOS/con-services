using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;

namespace VSS.Productivity3D.TagFileGateway.Common.Models.Executors
{
    public abstract class RequestExecutorContainer
    {
      protected ILogger Logger { get; private set; }
      protected IConfigurationStore ConfigStore { get; private set; }
      protected IDataCache DataCache { get; private set; }

      protected abstract Task<ContractExecutionResult> ProcessAsyncEx<T>(T item);

      /// <summary> </summary>
      public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
      {
        if (item == null)
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
        return await ProcessAsyncEx(item);
      }

      public static TExecutor Build<TExecutor>(ILogger logger, 
        IConfigurationStore configStore, 
        IDataCache dataCache, 
        ITagFileForwarder tagFileForwarder) 
        where TExecutor : RequestExecutorContainer, new()
      {
        var executor = new TExecutor()
        {
          Logger = logger,
          ConfigStore = configStore,
          DataCache = dataCache
        };
        return executor;
      }
    }

    public class TagFileProcessExecutor : RequestExecutorContainer
    {

    }
}
