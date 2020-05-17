using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.Common.Authentication
{
  /// <summary>
  /// Custom principal for tile service with list of projects.
  /// </summary>
  public class TilePrincipal : TIDCustomPrincipal
  {
    private readonly IProjectProxy _projectProxy;
    private readonly IHeaderDictionary _authNContext;

    //We need to delegate Project retrieval downstream as project may not accessible to a user once it has been created
    public TilePrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userEmail, bool isApplication,
      IProjectProxy projectProxy, IHeaderDictionary contextHeaders, string tpaasApplicationName = "")
      : base(identity, customerUid, customerName, userEmail, isApplication, tpaasApplicationName)
    {
      _projectProxy = projectProxy;
      _authNContext = contextHeaders;
    }

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    public async Task<ProjectData> GetProject(long projectId)
    {
      var projectDescr = await _projectProxy.GetProjectForCustomer(CustomerUid, projectId, _authNContext);
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectId}"));
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public async Task<ProjectData> GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      return await GetProject(projectUid.ToString());
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));

      var projectDescr = await _projectProxy.GetProjectForCustomer(CustomerUid, projectUid, _authNContext);
      if (projectDescr != null) return projectDescr;

      throw new ServiceException(HttpStatusCode.Unauthorized,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
          $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
    }

    /// <summary>
    /// Gets the legacy Project Id (long) from a ProjectUid (Guid).
    /// </summary>
    public async Task<long> GetLegacyProjectId(Guid? projectUid)
    {
      if (!(this is TilePrincipal _))
      {
        throw new ArgumentException("Incorrect request context principal.");
      }

      var project = await GetProject(projectUid);
      var projectId = project.ShortRaptorProjectId;
      if (projectId > 0)
      {
        return projectId;
      }

      throw new ServiceException(
        HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));
    }
  }
}
