using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Base for all Project controllers
  /// </summary>
  public class ProjectBaseController : BaseController<ProjectBaseController>
  {
    protected async Task<ImmutableList<ProjectDatabaseModel>> GetProjectListForCustomer()
    {
      LogCustomerDetails("GetProjectList", "");
      var projects = (await ProjectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      Logger.LogInformation($"Project list contains {projects.Count} projects");
      return projects;
    }
  }
}
