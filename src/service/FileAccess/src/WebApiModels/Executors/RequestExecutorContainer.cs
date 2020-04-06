using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.FileAccess.WebAPI.Models.Executors
{
  /// <summary>
  /// Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  /// from controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    protected ILogger log;
    protected IConfigurationStore configStore;
    protected IFileRepository fileAccess;

    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    protected RequestExecutorContainer()
    { }

    protected RequestExecutorContainer(ILoggerFactory logger, IConfigurationStore configStore, IFileRepository fileAccess)
    {
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      this.configStore = configStore;
      this.fileAccess = fileAccess;
    }

    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      }

      return ProcessEx(item);
    }

    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    public static TExecutor Build<TExecutor>(ILoggerFactory logger, IConfigurationStore configStore = null, IFileRepository fileAccess = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      return new TExecutor { log = logger.CreateLogger<TExecutor>(), configStore = configStore, fileAccess = fileAccess };
    }
  }
}
