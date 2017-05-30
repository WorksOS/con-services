using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ProjectWebApi.Filters;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Controllers
{
  /// <summary>
  /// Project controller v3
  /// </summary>
  public class ProjectV3Controller : ProjectBaseController
  {

    public ProjectV3Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
      IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger)
      : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, logger)
    {
    }


    /// <summary>
    /// Gets a list of projects for a customer. The list includes projects of all project types
    /// and both active and archived projects.
    /// </summary>
    /// <returns>A list of projects</returns>
    [Route("api/v3/project")]
    [HttpGet]
    public async Task<ImmutableList<ProjectDescriptor>> GetProjectsV3()
    {
      log.LogInformation("GetProjectsV3");
      var projects = (await GetProjectList());
      var customerUid = (User as TidCustomPrincipal).CustomerUid;
      log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
      return projects.Select(project =>
          new ProjectDescriptor()
          {
            ProjectType = project.ProjectType,
            Name = project.Name,
            ProjectTimeZone = project.ProjectTimeZone,
            IsArchived = project.IsDeleted || project.SubscriptionEndDate < DateTime.UtcNow,
            StartDate = project.StartDate.ToString("O"),
            EndDate = project.EndDate.ToString("O"),
            ProjectUid = project.ProjectUID,
            LegacyProjectId = project.LegacyProjectID,
            ProjectGeofenceWKT = project.GeometryWKT,
            CustomerUID = project.CustomerUID,
            LegacyCustomerId = project.LegacyCustomerID.ToString(),
            CoordinateSystemFileName = project.CoordinateSystemFileName
          }
        ).Where(p => p.CustomerUID == customerUid)
        .ToImmutableList();
    }

    // POST: api/project
    /// <summary>
    /// Create Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v3/project")]
    [HttpPost]
    public async Task CreateProjectV3([FromBody] CreateProjectEvent project)
    {
      ProjectBoundaryValidator.ValidateWKT(project.ProjectBoundary);
      string wktBoundary = project.ProjectBoundary;

      //Convert to old format for Kafka for consistency on kakfa queue
      string kafkaBoundary = project.ProjectBoundary
        .Replace(ProjectBoundaryValidator.POLYGON_WKT, string.Empty)
        .Replace("))", string.Empty)
        .Replace(',', ';')
        .Replace(' ', ',');
      await CreateProject(project, kafkaBoundary, wktBoundary);
    }

    // PUT: api/Project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="project">UpdateProjectEvent model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v3/project")]
    [HttpPut]
    public async Task UpdateProjectV3([FromBody] UpdateProjectEvent project)
    {
      await UpdateProject(project);
    }

    // DELETE: api/Project/
    /// <summary>
    /// Delete Project
    /// </summary>
    /// <param name="project">DeleteProjectEvent model</param>
    /// <remarks>Deletes existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v3/project")]
    [HttpDelete]
    public async Task DeleteProjectV3([FromBody] DeleteProjectEvent project)
    {
      await DeleteProject(project);
    }

    /// <summary>
    /// Associate customer and project
    /// </summary>
    /// <param name="customerProject">Customer - project</param>
    /// <remarks>Associate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("api/v3/project/AssociateCustomer")]
    [HttpPost]
    public async Task AssociateCustomerProjectV3([FromBody] AssociateProjectCustomer customerProject)
    {
      if (customerProject.LegacyCustomerID <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(38),
            contractExecutionStatesEnum.FirstNameWithOffset(38)));
      }
      await AssociateProjectCustomer(customerProject);
    }

    /// <summary>
    /// Dissociate customer and asset
    /// </summary>
    /// <param name="customerProject">Customer - Project model</param>
    /// <remarks>Dissociate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("api/v3/project/DissociateCustomer")]
    [HttpPost]
    public async Task DissociateCustomerProjectV3([FromBody] DissociateProjectCustomer customerProject)
    {
      await DissociateProjectCustomer(customerProject);
    }

    /// <summary>
    /// Associate geofence and project
    /// </summary>
    /// <param name="geofenceProject">Geofence - project</param>
    /// <remarks>Associate geofence and project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("api/v3/project/AssociateGeofence")]
    [HttpPost]
    public async Task AssociateGeofenceProjectV3([FromBody] AssociateProjectGeofence geofenceProject)
    {
      await AssociateGeofenceProject(geofenceProject);
    }

  }
}

