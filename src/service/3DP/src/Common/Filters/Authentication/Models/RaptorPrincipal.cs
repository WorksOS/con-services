using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  /// Custom principal for Raptor with list of projects.
  /// </summary>
  public class RaptorPrincipal : TIDCustomPrincipal
  {
    private readonly IProjectProxy projectProxy;
    private readonly IHeaderDictionary authNContext;
    private static readonly ConcurrentDictionary<Guid, long> legacyProjectIdsCache;
    private static readonly ConcurrentDictionary<long, Guid> ProjectUidsCache;

    static RaptorPrincipal()
    {
      legacyProjectIdsCache = new ConcurrentDictionary<Guid, long>();
      ProjectUidsCache = new ConcurrentDictionary<long, Guid>();
    }

    //We need to delegate Project retrieval downstream as project may not accessible to a user once it has been created
    public RaptorPrincipal(ClaimsIdentity identity, string customerUid, string customerName, string userEmail, bool isApplication, string tpaasApplicationName,
      IProjectProxy projectProxy, IHeaderDictionary contextHeaders)
      : base(identity, customerUid, customerName, userEmail, isApplication, tpaasApplicationName)
    {
      this.projectProxy = projectProxy;
      authNContext = contextHeaders;
    }

    // the api/v2/device/patches endpoint doesn't come with a customerUid 
    //     which is needed for downstream service calls e.g. ProjectSvc.
    //     For this endpoint we obtain it from TFA and add it to our customHeaders
    public new bool SetCustomerUid(string customerUid)
    {
      if (string.IsNullOrEmpty(CustomerUid) && !authNContext.ContainsKey(HeaderConstants.X_VISION_LINK_CUSTOMER_UID))
      {
        base.SetCustomerUid(customerUid);
        authNContext[HeaderConstants.X_VISION_LINK_CUSTOMER_UID] = customerUid;
        return true;
      }
      return false;
    }

    public IHeaderDictionary GetAuthNContext() => authNContext;

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    public async Task<ProjectData> GetProject(long projectId)
    {
      var projectDescr = await projectProxy.GetProjectForCustomer(CustomerUid, projectId, authNContext);
      if (projectDescr != null)
      {
        return projectDescr;
      }

      throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectId}"));
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public Task<ProjectData> GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      return GetProject(projectUid.ToString());
    }

    /// <summary>
    /// Get the project descriptor for the specified project uid.
    /// </summary>
    public async Task<ProjectData> GetProject(string projectUid)
    {
      if (string.IsNullOrEmpty(projectUid))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      var projectDescr = await projectProxy.GetProjectForCustomer(CustomerUid, projectUid, authNContext);
      if (projectDescr != null)
      {
        return projectDescr;
      }

      throw new ServiceException(HttpStatusCode.Unauthorized,
        new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
          $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
    }

    /// <summary>
    /// Gets the legacy Project Id (long) from a ProjectUid (Guid).
    /// </summary>
    public Task<long> GetLegacyProjectId(Guid projectUid)
    {
      return legacyProjectIdsCache.TryGetValue(projectUid, out var legacyId)
        ? Task.FromResult(legacyId)
        : GetProjectId();

      async Task<long> GetProjectId()
      {
        var project = await GetProject(projectUid);
        var projectId = project.ShortRaptorProjectId;

        if (projectId > 0)
        {
          legacyProjectIdsCache.TryAdd(projectUid, projectId);

          return projectId;
        }

        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));
      }
    }

    /// <summary>
    /// Gets the Project Uid (Guid) from a ProjectId (long).
    /// </summary>
    public Task<Guid> GetProjectUid(long projectId)
    {
      return ProjectUidsCache.TryGetValue(projectId, out var tempProjectUid)
        ? Task.FromResult(tempProjectUid)
        : GetProjectUid();

      async Task<Guid> GetProjectUid()
      {
        var project = await GetProject(projectId);

        if (Guid.TryParse(project.ProjectUID, out var projectUid))
        {
          ProjectUidsCache.TryAdd(projectId, projectUid);

          return projectUid;
        }

        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project UID"));
      }
    }

  }
}
