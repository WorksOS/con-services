using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Project Base for all Project controllers
  /// </summary>
  public class ProjectBaseController : BaseController<ProjectBaseController>
  {
   
    /// <summary>
    /// Save for potential rollback
    /// </summary>
    protected Guid subscriptionUidAssigned = Guid.Empty;

    /// <summary>
    ///  Used for rollback
    /// </summary>
    protected Guid geofenceUidCreated = Guid.Empty;


    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectBaseController"/> class.
    /// </summary>
    public ProjectBaseController(IConfigurationStore configStore) : base (configStore)
    { }

    /// <summary>
    /// Gets the project list for a customer
    /// </summary>
    /// <returns></returns>
    protected async Task<ImmutableList<ProjectDatabaseModel>> GetProjectList()
    {
      var customerUid = LogCustomerDetails("GetProjectList", "");
      var projects = (await ProjectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).ToImmutableList();

      Logger.LogInformation($"Project list contains {projects.Count} projects");
      return projects;
    }
    
  }
}
