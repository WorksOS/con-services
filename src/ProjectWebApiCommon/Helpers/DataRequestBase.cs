using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Repositories;
using VSS.MasterDataProxies.Interfaces;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public abstract class DataRequestBase
  {
    protected ILogger log;
    protected IConfigurationStore configStore;
    protected IServiceExceptionHandler serviceExceptionHandler;
    protected IDictionary<string, string> headers;
    protected ProjectRepository projectRepo;

    public void Initialize(ILogger log, IConfigurationStore configurationStore, IServiceExceptionHandler serviceExceptionHandler, IDictionary<string, string> headers,
        ProjectRepository projectRepo)
    {
      this.log = log;
      configStore = configurationStore;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.headers = headers;

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
