using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Models;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Interfaces;

namespace VSP.MasterData.Project.WebAPI.Controllers.V1
{
  public class ProjectV1Controller : ProjectBaseController
    {

        public ProjectV1Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger)
            : base(producer, projectRepo, subscriptionsRepo, store,
                subsProxy, geofenceProxy, raptorProxy, logger)
        {
        }

      /// <summary>
      /// Gets the projects for a customer. This includes projects of all project types
      /// and both active and archived projects.
      /// </summary>
      /// <returns>A dictionary of projects keyed on legacy project id</returns>
      [Route("v1")]
      [HttpGet]
      public async Task<Dictionary<long, ProjectDescriptor>> GetProjectsV1()
      {
        Console.WriteLine("GetProjectsV1 ");

        var projects = (await GetProjectList());
        var projectList = projects.Select(project =>
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
          )
          .ToImmutableList();
        return projectList.ToDictionary(k => (long) k.LegacyProjectId, v => v);
      }

      /// <summary>
    /// Create Project
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("v1")]
        [HttpPost]
        public async Task CreateProjectV1([FromBody] CreateProjectEvent project)
        {
            ProjectBoundaryValidator.ValidateV1(project.ProjectBoundary);
            string kafkaBoundary = project.ProjectBoundary;
            //Convert to WKT format for saving in database
            string wktBoundary = ProjectBoundaryValidator.POLYGON_WKT + project.ProjectBoundary
                                     .Replace(',', ' ')
                                     .Replace(';', ',') + "))";
            await CreateProject(project, kafkaBoundary, wktBoundary);
        }

        /// <summary>
        /// Update Project
        /// </summary>
        /// <param name="project">UpdateProjectEvent model</param>
        /// <remarks>Updates existing project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("v1")]
        [HttpPut]
        public async Task UpdateProjectV1([FromBody] UpdateProjectEvent project)
        {
            await UpdateProject(project);
        }

        /// <summary>
        /// Delete Project
        /// </summary>
        /// <param name="projectUID">DeleteProjectEvent model</param>
        /// <param name="actionUTC">DeleteProjectEvent model</param>
        /// <remarks>Deletes project with projectUID</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("v1")]
        [HttpDelete]
        public async Task DeleteProjectV1(Guid projectUID, DateTime actionUTC)
        {
            var project = new DeleteProjectEvent();
            project.ProjectUID = projectUID;
            project.ActionUTC = actionUTC;
            await DeleteProject(project);
        }

        // POST: api/project
        /// <summary>
        /// Restore Project
        /// </summary>
        /// <param name="project">CreateProjectEvent model</param>
        /// <remarks>Create new project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("v1/Restore")]
        [HttpPost]
        public void RestoreProjectV1([FromBody] RestoreProjectEvent project)
        {
            /*This is only for debugging no actual project can be restored*/

            project.ReceivedUTC = DateTime.UtcNow;
            var messagePayload = JsonConvert.SerializeObject(new {RestoreProjectEvent = project});

            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            //throw new Exception("Failed to publish message to Kafka");
        }

        /// <summary>
        /// Dissociate customer and asset
        /// </summary>
        /// <param name="customerProject">Customer - Project model</param>
        /// <remarks>Dissociate customer and asset</remarks>
        /// <response code="200">Ok</response>
        /// <response code="500">Internal Server Error</response>
        [Route("v1/DissociateCustomer")]
        [HttpPost]
        public async Task DissociateCustomerProjectV1([FromBody] DissociateProjectCustomer customerProject)
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
        [Route("v1/AssociateGeofence")]
        [HttpPost]
        public async Task AssociateGeofenceProjectV1([FromBody] AssociateProjectGeofence geofenceProject)
        {
            await AssociateGeofenceProject(geofenceProject);
        }

    }
}
