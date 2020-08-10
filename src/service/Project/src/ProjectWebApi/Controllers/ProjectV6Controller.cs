using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project controller v6
  ///    UI interface for getting projects i.e. user context
  ///    CWS interface for validating their create/update of project
  ///    CWS interface for notifying of create/update of project
  /// </summary>
  public class ProjectV6Controller : BaseController<ProjectV6Controller>
  {
    /// <summary>
    /// Gets or sets the httpContextAccessor.
    /// </summary>
    protected readonly IHttpContextAccessor HttpContextAccessor;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ProjectV6Controller(IHttpContextAccessor httpContextAccessor)
    {
      HttpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    ///        and both active and archived projects.
    /// </summary>
    [Route("api/v4/project")] // temporary kludge until ccssscon-219 
    [Route("api/v6/project")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsListResult> GetProjectsV6([FromQuery]CwsProjectType? projectType, [FromQuery] ProjectStatus projectStatus = ProjectStatus.Active, [FromQuery] bool onlyAdmin = true, [FromQuery ]bool includeBoundaries = false)
    {
      Logger.LogInformation($"{nameof(GetProjectsV6)}");
      var projects = await ProjectRequestHelper.GetProjectListForCustomer(new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, projectType, projectStatus, onlyAdmin, includeBoundaries, customHeaders);

      return new ProjectV6DescriptorsListResult
      {
        ProjectDescriptors = projects.Select(project =>
            AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project))
          .ToImmutableList()
      };
    }

    /// <summary>
    /// Gets a project for a customer. 
    /// </summary>
    /// <returns>A project data</returns>
    [Route("api/v4/project/{projectUid}")]
    [Route("api/v6/project/{projectUid}")]
    [HttpGet]
    public async Task<ProjectV6DescriptorsSingleResult> GetProjectV6(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetProjectV6)}");

      var project = await ProjectRequestHelper.GetProject(new Guid(projectUid), new Guid(CustomerUid), new Guid(UserId), Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders).ConfigureAwait(false);
      return new ProjectV6DescriptorsSingleResult(AutoMapperUtility.Automapper.Map<ProjectV6Descriptor>(project));
    }

    /// <summary>
    /// Called from CWS before a create or update can go ahead, returns pass or fail
    /// </summary>
    /// <param name="validateDto">Update Model</param>
    [HttpPost("api/v6/project/validate")]
    public async Task<ContractExecutionResult> IsProjectValid([FromBody]ProjectValidateDto validateDto)
    {
      Logger.LogInformation($"{nameof(IsProjectValid)} Project Validation Check {JsonConvert.SerializeObject(validateDto)}");

      // Nothing to validate for a non 3d-enabled project
      if (validateDto.ProjectType.HasValue && !validateDto.ProjectType.Value.HasFlag(CwsProjectType.AcceptsTagFiles))
        return new ContractExecutionResult();

      var data = AutoMapperUtility.Automapper.Map<ProjectValidation>(validateDto);
      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<ValidateProjectExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord, dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient, cwsDesignClient: CwsDesignClient,
            cwsProfileSettingsClient: CwsProfileSettingsClient)
          .ProcessAsync(data)
      );
      return result;
    }

    /// <summary>
    /// Called from CWS when a project or Coordinate System has been created / updated / deleted
    /// </summary>
    /// <param name="updateDto">Update Model</param>
    [HttpPost("api/v6/project/notifychange")]
    public async Task<IActionResult> OnProjectChangeNotify([FromBody]ProjectChangeNotificationDto updateDto)
    {
      Logger.LogInformation($"{nameof(OnProjectChangeNotify)} Update Notification {JsonConvert.SerializeObject(updateDto)}");

      await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<ProjectChangedExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
            CustomerUid, UserId, null, customHeaders,
            Productivity3dV1ProxyCoord, dataOceanClient: DataOceanClient, authn: Authorization,
            cwsProjectClient: CwsProjectClient, cwsDesignClient: CwsDesignClient,
            cwsProfileSettingsClient: CwsProfileSettingsClient, notificationHubClient: NotificationHubClient)
          .ProcessAsync(updateDto)
      );

      Logger.LogInformation($"{nameof(OnProjectChangeNotify)} Processed Notification");

      return Ok();
    }

    /// <summary>
    /// Used for CWS to notify us on association changes, so we can update cache
    /// </summary>
    /// <param name="trns">A list of TRNs that have been changed</param>
    /// <returns>Ok</returns>
    [HttpPost("api/v1/project/notifyassociation")]
    public async Task<IActionResult> OnProjectAssociationChange([FromBody] List<string> trns)
    {
      Logger.LogInformation($"{nameof(OnProjectAssociationChange)} Associations Updated: {JsonConvert.SerializeObject(trns)}");

      // We don't actually do anything with this data yet, other than clear cache
      // Since we call out to CWS for data
      var tasks = new Task[trns.Count];
      for (var i = 0; i < trns.Count; i++)
      {
        var trn = trns[i];
        var guid = TRNHelper.ExtractGuid(trn);
        if (!guid.HasValue)
          continue;

        Logger.LogInformation($"Clearing cache related to TRN: {guid.Value}");
        tasks[i] = NotificationHubClient.Notify(new ProjectChangedNotification(guid.Value));
      }

      await Task.WhenAll(tasks);

      return Ok();
    }

  }
}
