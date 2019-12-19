using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push.Hubs.Authentication
{
  /// <summary>
  /// Custom principal for Raptor with list of projects.
  /// </summary>
  public class PushPrincipal : TIDCustomPrincipal
  {
    private readonly IProjectProxy _projectProxy;
    private readonly IDictionary<string, string> _authNContext;

    public PushPrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userEmail, bool isApplication, string tpaasApplicationName,
      IProjectProxy projectProxy, IDictionary<string, string> contextHeaders)
      : base(identity, customerUid, customerName, userEmail, isApplication, tpaasApplicationName)
    {
      _projectProxy = projectProxy;
      _authNContext = contextHeaders;
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// Also authenticates project availability and customer/project relationship
    /// </summary>
    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      var projectDescription = await _projectProxy.GetProjectForCustomer(CustomerUid, projectUid, _authNContext);
      if (projectDescription != null)
      {
        return projectDescription;
      }

      throw new ServiceException(HttpStatusCode.Unauthorized,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
          $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
    }

  }
}
