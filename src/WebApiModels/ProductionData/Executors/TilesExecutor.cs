using System;
using System.IO;
using System.Net;
using ASNodeDecls;
using log4net.Util;
using Microsoft.Extensions.Logging;
using SVOICFilterSettings;
using SVOICVolumeCalculationsDecls;
using VLPDDecls;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using Common.Executors;

namespace WebApiModels.ProductionData.Executors
{

  /// <summary>
  /// The executor which passes the tile request to Raptor
  /// </summary>
  public class TilesExecutor : TilesBaseExecutor
  {

    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public TilesExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {

    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public TilesExecutor()
    {
    }
    protected override TileRequest GetRequest(object item) => item as TileRequest;
  }
}