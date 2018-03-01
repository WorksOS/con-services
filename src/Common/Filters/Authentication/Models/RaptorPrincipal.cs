using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  /// Custom principal for Raptor with list of projects.
  /// </summary>
  public class RaptorPrincipal : ClaimsPrincipal
  {
    //We need to delefgate Project retrieval downstream as project may not accessible to a user once it has been created
    public RaptorPrincipal(ClaimsIdentity identity, string customerUid, List<ProjectDescriptor> projects, string username, string customername, IProjectListProxy projectProxy, bool isApplication = false) : base(identity)
    {
      CustomerUid = customerUid;
      Projects = projects;
      this.IsApplication = isApplication;
      this.UserEmail = username;
      this.CustomerName = customername;
      this.projectProxy = projectProxy;
    }

    private IProjectListProxy projectProxy;

    public string CustomerUid { get; }

    public List<ProjectDescriptor> Projects { get; private set; }

    public string UserEmail { get; }

    public string CustomerName { get; }

    public bool IsApplication { get; }

    private void InvalidateProjectList(IDictionary<string,string> authnContext)
    {
      projectProxy.ClearCacheItem(CustomerUid);
      var customerProjects = projectProxy.GetProjectsV4(CustomerUid, authnContext).Result;
      var authProjects = new List<ProjectDescriptor>();
      if (customerProjects != null)
      {
        foreach (var project in customerProjects)
        {
          var projectDesc = new ProjectDescriptor
          {
            isLandFill = project.ProjectType == ProjectType.LandFill,
            isArchived = project.IsArchived,
            projectUid = project.ProjectUid,
            projectId = project.LegacyProjectId,
            coordinateSystemFileName = project.CoordinateSystemFileName,
            projectGeofenceWKT = project.ProjectGeofenceWKT,
            projectTimeZone = project.ProjectTimeZone,
            ianaTimeZone = project.IanaTimeZone
          };
          authProjects.Add(projectDesc);
        }
        Projects = authProjects;
      }
    }

    /// <summary>
    /// Get the project descriptor for the specified project id.
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(long projectId)
    {
      var projectDescr = Projects.FirstOrDefault(p => p.projectId == projectId);
      if (projectDescr == null)
      {
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectId}"));
      }

      return projectDescr;
    }

    /// <summary>
    ///  Get the project descriptor for the specified project uid.
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(Guid? projectUid)
    {
      if (!projectUid.HasValue)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }
      return GetProject(projectUid.ToString());
    }

    /// <summary>
    ///  Get the project descriptor for the specified project uid.
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Project descriptor</returns>
    public ProjectDescriptor GetProject(string projectUid, IDictionary<string,string> authnContext = null)
    {
      if (string.IsNullOrEmpty(projectUid))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Missing project UID"));
      }

      var projectDescr = Projects.FirstOrDefault(p => string.Equals(p.projectUid, projectUid, StringComparison.OrdinalIgnoreCase));
      if (projectDescr == null)
      {

        if (authnContext!=null)
          InvalidateProjectList(authnContext);
        projectDescr = Projects.FirstOrDefault(p => string.Equals(p.projectUid, projectUid, StringComparison.OrdinalIgnoreCase));

        if (projectDescr != null)
          return projectDescr;

        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            $"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}"));
      }

      return projectDescr;
    }

    /// <summary>
    /// Gets the legacy project id for the specified project uid
    /// </summary>
    /// <param name="projectUid">THe project UID</param>
    /// <returns>Legacy project ID</returns>
    public long GetProjectId(Guid? projectUid)
    {
      long projectId = GetProject(projectUid).projectId;

      if (projectId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project ID"));
      }

      return projectId;
    }
  }
}