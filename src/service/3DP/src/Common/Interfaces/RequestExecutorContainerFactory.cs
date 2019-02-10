using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Common.Interfaces
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    public static TExecutor Build<TExecutor>(ILoggerFactory logger,
#if RAPTOR      
      IASNodeClient raptorClient = null,
      ITagProcessor tagProcessor = null,
#endif
      IConfigurationStore configStore = null, IFileRepository fileRepo = null,
      ITileGenerator tileGenerator = null, List<FileData> fileList = null, ICompactionProfileResultHelper profileResultHelper = null, 
      ITransferProxy transferProxy = null, ITRexTagFileProxy tRexTagFileProxy = null, ITRexCompactionDataProxy trexCompactionDataProxy = null,
      IDictionary<string, string> customHeaders = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<TExecutor>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        log,
#if RAPTOR      
        raptorClient,
        tagProcessor,
#endif
        configStore,
        fileRepo,
        tileGenerator,
        fileList,
        profileResultHelper,
        transferProxy,
        tRexTagFileProxy,
        trexCompactionDataProxy,
        customHeaders);
      
      return executor;
    }
  }
}
