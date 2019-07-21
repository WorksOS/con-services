using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.Common.Executors
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <typeparam name="TLog">The type of the logger.</typeparam>
    public static TExecutor Build<TExecutor, TLog>(ILogger<TLog> logger, IConfigurationStore configStore = null,  
      IDictionary<string, string> customHeaders = null, IDataOceanClient dataOceanClient = null,
      ITPaaSApplicationAuthentication authn = null, IFileRepository tccFileRepo = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = logger;
      var executor = new TExecutor();

      executor.Initialise(
        log,
        configStore,
        customHeaders,
        dataOceanClient,
        authn,
        tccFileRepo);

      return executor;
    }
  }
}
