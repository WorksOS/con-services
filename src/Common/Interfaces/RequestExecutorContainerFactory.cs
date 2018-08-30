using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.TCCFileAccess;

namespace VSS.Productivity3D.Common.Interfaces
{
  public class RequestExecutorContainerFactory
  {
    /// <summary>
    /// Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    public static TExecutor Build<TExecutor>(ILoggerFactory logger, IASNodeClient raptorClient = null,
      ITagProcessor tagProcessor = null, IConfigurationStore configStore = null, IFileRepository fileRepo = null,
      ITileGenerator tileGenerator = null, List<FileData> fileList = null, ICompactionProfileResultHelper profileResultHelper = null, ITransferProxy transferProxy = null, ITRexTagFileProxy tRexTagFileProxy = null, IDictionary<string, string> customHeaders = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<RequestExecutorContainer>();
      }

      var executor = new TExecutor();

      executor.Initialise(
      log,
      raptorClient,
      tagProcessor,
      configStore,
      fileRepo,
      tileGenerator,
      fileList,
      profileResultHelper,
      transferProxy,
      tRexTagFileProxy,
      customHeaders);

      return executor;
    }
  }
}
