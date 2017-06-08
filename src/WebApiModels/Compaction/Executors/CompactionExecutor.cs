using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using Common.Executors;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICVolumeCalculationsDecls;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Compaction.Models;

namespace VSS.Raptor.Service.WebApiModels.Compaction.Executors
{
  /// <summary>
  /// Processes the request to xxx
  /// </summary>
  public class CompactionExecutor : TilesBaseExecutor
  {
    public CompactionExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public CompactionExecutor()
    {
    }

    protected override TileRequest GetRequest(object item) => item as CompactionTileV2Request;
  }
}
