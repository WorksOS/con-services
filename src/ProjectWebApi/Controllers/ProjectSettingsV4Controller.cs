using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApi.Filters;
using VSS.Productivity3D.ProjectWebApiCommon.Executors;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Productivity3D.ProjectWebApi.Controllers
{
  public class ProjectSettingsV4Controller : Controller
  {
    private readonly ProjectRepository projectRepo;
    private readonly IConfigurationStore configStore;
    private readonly ILoggerFactory logger;
    private readonly ILogger log;
    private readonly IServiceExceptionHandler serviceExceptionHandler;

    public ProjectSettingsV4Controller(IRepository<IProjectEvent> projectRepo, 
      IConfigurationStore configStore, 
      ILoggerFactory logger, 
      IServiceExceptionHandler serviceExceptionHandler)
    {
      this.projectRepo = projectRepo as ProjectRepository;
      this.configStore = configStore;
      this.logger = logger;
      log = logger.CreateLogger<ProjectSettingsV4Controller>(); 
      this.serviceExceptionHandler = serviceExceptionHandler;
    }


    /// <summary>
    /// Gets the project settings for a project.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    [Route("api/v4/projectsettings/{projectUid}")]
    [HttpGet]
    public async Task<ProjectSettingsResult> GetProjectSettings(string projectUid)
    {
      LogCustomerDetails("GetProjectSettings", projectUid);
      if (string.IsNullOrEmpty(projectUid))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "File Id is required"));
      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, configStore, logger, serviceExceptionHandler);
      var result = await executor.ProcessAsync(projectUid);

      log.LogResult(this.ToString(), projectUid, result);
      return result as ProjectSettingsResult;
    }


    private string LogCustomerDetails(string functionName, string projectUid)
    {
      var customerUid = (User as TIDCustomPrincipal).CustomerUid;
      log.LogInformation($"{functionName}: CustomerUID={customerUid} and projectUid={projectUid}");

      return customerUid;
    }
  }
}
