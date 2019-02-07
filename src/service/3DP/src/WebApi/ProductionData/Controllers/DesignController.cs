using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Notification.Controllers;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for Designs resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class DesignController : Controller, IDesignContract
  {
    #region privates

#if RAPTOR
    private readonly IASNodeClient raptorClient;
#endif
    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;


    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;
#endregion

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="fileListProxy">File list proxy</param>
    public DesignController(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, IConfigurationStore configStore, IFileListProxy fileListProxy)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.logger = logger;
      this.log = logger.CreateLogger<NotificationController>();
      this.configStore = configStore;
      this.fileListProxy = fileListProxy;
    }

    /// <summary>
    /// Gets a list of design boundaries in GeoJson format from Raptor.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <param name="tolerance">The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.</param>
    /// <returns>Execution result with a list of design boundaries.</returns>
    [ProjectVerifier]
    [Route("api/v2/designs/boundaries")]
    [HttpGet]
    public async Task<ContractExecutionResult> GetDesignBoundaries([FromQuery] Guid projectUid, [FromQuery] double? tolerance)
    {
      log.LogInformation("GetDesignBoundaries: " + Request.QueryString);

      long projectId = await ((RaptorPrincipal) User).GetLegacyProjectId(projectUid);
      tolerance = tolerance ?? DesignBoundariesRequest.BOUNDARY_POINTS_INTERVAL;

      DesignBoundariesRequest request = DesignBoundariesRequest.CreateDesignBoundariesRequest(projectId, tolerance.Value);

      request.Validate();
#if RAPTOR
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), GetUserId(), Request.Headers.GetCustomHeaders());

      fileList = fileList?.Where(f => f.ImportedFileType == ImportedFileType.DesignSurface && f.IsActivated).ToList();

      return RequestExecutorContainerFactory.Build<DesignExecutor>(logger, raptorClient, configStore: configStore, fileList: fileList).Process(request);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
#if RAPTOR
    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    private string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
#endif
  }
}
