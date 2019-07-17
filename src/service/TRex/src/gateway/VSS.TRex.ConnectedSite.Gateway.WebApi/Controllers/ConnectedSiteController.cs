using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;
using VSS.TRex.ConnectedSite.Gateway.Executors;
using VSS.TRex.ConnectedSite.Gateway.Models;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Process Tagfiles Controller
  /// </summary>
  public class ConnectedSiteController : BaseController
  {
    /// <summary>
    /// Controller for tagfile processing
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="configStore"></param>
    public ConnectedSiteController(ILoggerFactory loggerFactory, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore)
        : base(loggerFactory, loggerFactory.CreateLogger<ConnectedSiteController>(), exceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Test endpoint for connected site gateway
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/connectedsite")]
    [HttpGet]
    public string Get()
    {
      // Test endpoint connection
      return $"You have reached api/connectedsite, the time is {DateTime.UtcNow} UTC";
    }

    /// <summary>
    /// Perfoms a prescan to retrieve as much information as possible which is then posted to 
    /// connected site
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/position")]
    [HttpPost]
    public Task<ContractExecutionResult> PostPosition([FromBody]CompactionTagFileRequest request)
    {
      var connectedSiteRequest = new ConnectedSiteRequest(request, ConnectedSiteMessageType.L1PositionMessage);

      var serializedRequest = ConvertObjectForLogging.SerializeObjectIgnoringProperties(request, "Data");
      Log.LogInformation("Position request: " + serializedRequest);
      return ExecuteRequest(connectedSiteRequest);
    }

    /// <summary>
    /// Perfoms a prescan to retrieve as much information as possible which is then posted to 
    /// connected site
    /// </summary>
    // [PostRequestVerifier]
    [Route("api/status")]
    [HttpPost]
    public Task<ContractExecutionResult> PostStatus([FromBody]CompactionTagFileRequest request)
    {
      var connectedSiteRequest = new ConnectedSiteRequest(request, ConnectedSiteMessageType.L2StatusMessage);
      var serializedRequest = ConvertObjectForLogging.SerializeObjectIgnoringProperties(request, "Data");
      Log.LogInformation("Position request: " + serializedRequest);

      return ExecuteRequest(connectedSiteRequest);
    }

    private Task<ContractExecutionResult> ExecuteRequest(ConnectedSiteRequest request)
    {
      return WithServiceExceptionTryExecuteAsync(() => RequestExecutorContainer
                                                     .Build<ConnectedSiteMessageSubmissionExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
                                                     .ProcessAsync(request));
    }
  }
}
