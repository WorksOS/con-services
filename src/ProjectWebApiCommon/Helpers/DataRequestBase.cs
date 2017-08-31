using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger log;
    protected IConfigurationStore configStore;
    protected ProjectRepository projectRepo;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, ProjectRepository projectRepo)
    {
      this.log = log;
      configStore = configurationStore;
      this.projectRepo = projectRepo;
    }

    ///// <summary>
    ///// Validates a project identifier.
    ///// </summary>
    ///// <param name="customerUid"></param>
    ///// <param name="projectUid">The project uid.</param>
    ///// <returns></returns>
    //public async Task ValidateProjectWithCustomer(string customerUid, string projectUid)
    //{
    //  var project = (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(prj => string.Equals(prj.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

    //  if (project == null)
    //  {
    //    serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
    //  }

    //  log.LogInformation($"projectUid {projectUid} validated");
    //}

    //private string GetFilespaceId()
    //{
    //  var filespaceId = configStore.GetValueString("TCCFILESPACEID");
    //  if (!string.IsNullOrEmpty(filespaceId))
    //  {
    //    return filespaceId;
    //  }

    //  const string errorString = "Your application is missing an environment variable TCCFILESPACEID";
    //  log.LogError(errorString);
    //  throw new InvalidOperationException(errorString);
    //}
  }
}
