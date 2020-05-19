using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the Device details from a) cws and b) localDB
  /// </summary>
  public class GetProjectsForDeviceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the GetProjectSettings request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deviceIsUid = CastRequestObjectTo<DeviceIsUid>(item, errorCode: 68);

      try
      {
        var projectsFromCws = await cwsDeviceClient.GetProjectsForDevice(new Guid(deviceIsUid.DeviceUid), customHeaders);
        if (projectsFromCws?.Projects == null || !projectsFromCws.Projects.Any())
        {
          var message = "Unable to locate projects for device in cws";
          log.LogInformation($"GetProjectsForDeviceExecutor: {message}");
          return new ProjectDataListResult(code: 105, message: message);
        }

        var projectDataListResult = new ProjectDataListResult();
        foreach (var projectCws in projectsFromCws.Projects)
        {
          var project = await projectRepo.GetProjectOnly(projectCws.ProjectId);

          // use WorksOS data rather than cws as it is the source of truth
          if (project != null)
          {
            if (string.Compare(project.CustomerUID, projectCws.AccountId, StringComparison.OrdinalIgnoreCase) != 0)
            {
              var message= $"Project accountId differs between WorksOS and WorksManager: projectId: {project.ProjectUID} WorksOS customer: {project.CustomerUID}  CWS account: {projectCws.AccountId}";
              log.LogError($"GetProjectsForDeviceExecutor: {message}");
              return new ProjectDataListResult(106, message: message, projectDataListResult.ProjectDescriptors);
            }
           
            projectDataListResult.ProjectDescriptors.Add(AutoMapperUtility.Automapper.Map<ProjectData>(project));
          }
          else
          {
            log.LogInformation($"GetProjectsForDeviceExecutor: A project from cws is missing from our localDB. cwsProject: {JsonConvert.SerializeObject(projectCws)}");
          }
        }

        return projectDataListResult;

      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 104 - 2000, "getProjectsForDeviceExecutor", e.Message);
      }

      return null;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
