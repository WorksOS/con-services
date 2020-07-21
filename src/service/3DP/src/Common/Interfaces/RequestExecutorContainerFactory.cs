using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.TRex.Gateway.Common.Abstractions;

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
      IConfigurationStore configStore = null,
      List<FileData> fileList = null, ICompactionProfileResultHelper profileResultHelper = null,
      ITRexTagFileProxy tRexTagFileProxy = null, ITRexCompactionDataProxy trexCompactionDataProxy = null,
      IHeaderDictionary customHeaders = null, string customerUid = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      ILogger log = null;
      if (logger != null)
      {
        log = logger.CreateLogger<TExecutor>();
      }

      var executor = new TExecutor();

      executor.Initialise(
        logger,
        log,
#if RAPTOR      
        raptorClient,
        tagProcessor,
#endif
        configStore,
        fileList,
        profileResultHelper,
        tRexTagFileProxy,
        trexCompactionDataProxy,
        customHeaders,
        customerUid);

      return executor;
    }
  }
}
