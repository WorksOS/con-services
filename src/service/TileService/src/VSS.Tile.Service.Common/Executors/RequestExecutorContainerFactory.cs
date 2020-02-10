using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Tile.Service.Common.Interfaces;
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
      ITPaaSApplicationAuthentication authn = null, IProductivity3dV2ProxyCompactionTile productivity3DProxyCompactionTile = null, IBoundingBoxHelper bboxHelper = null)
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
        productivity3DProxyCompactionTile,
        bboxHelper);

      return executor;
    }
  }
}
