using System;
using System.Collections.Generic;
using System.Security.Principal;
using KafkaConsumer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Models;
using VSS.Project.Data;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSP.MasterData.Project.WebAPI.Controllers.V1
{
    public class ProjectV1Controller : Controller
    {

        private readonly IKafka _producer;
        private readonly IRepository<IProjectEvent> _projectService;
        private readonly string kafkaTopicName;

        public ProjectV1Controller(IKafka producer, IRepository<IProjectEvent> projectRepo, IConfigurationStore store)
            : base()
        {
            _producer = producer;
            _producer.InitProducer(store);
            _projectService = projectRepo;
            kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                             store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
        }

        [Route("api/v1/project")]
        [HttpGet]
        public List<ProjectDescriptor> GetProjects()
        {
          var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
          var projects = (_projectService as ProjectRepository).GetProjectsForCustomer(customerUid).Result;

          var projectList = new List<ProjectDescriptor>();
          foreach (var project in projects)
          {
            projectList.Add(
              new ProjectDescriptor
              {
                ProjectType = project.ProjectType,
                Name = project.Name,
                ProjectTimeZone = project.ProjectTimeZone,
                IsArchived = project.IsDeleted || project.SubscriptionEndDate < DateTime.UtcNow,
                StartDate = project.StartDate.ToString("O"),
                EndDate = project.StartDate.ToString("O"),
                ProjectUid = project.ProjectUID,
                LegacyProjectId = project.LegacyProjectID,
                ProjectGeofenceWKT = project.GeometryWKT
              });
          }
          return projectList;
        }

        // POST: api/project
        /// <summary>
        /// Create Project
        /// </summary>
        /// <param name="project">CreateProjectEvent model</param>
        /// <remarks>Create new project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("api/v1/project")]
        [HttpPost]
        public void CreateProject([FromBody] CreateProjectEvent project)
        {
            ProjectDataValidator.Validate(project, _projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {CreateProjectEvent = project});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            _projectService.StoreEvent(project);
        }

        // PUT: api/Project
        /// <summary>
        /// Update Project
        /// </summary>
        /// <param name="project">UpdateProjectEvent model</param>
        /// <remarks>Updates existing project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("api/v1/project")]
        [HttpPut]
        public void UpdateProject([FromBody] UpdateProjectEvent project)
        {
            ProjectDataValidator.Validate(project, _projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            _projectService.StoreEvent(project);

        }

        // DELETE: api/Project/
        /// <summary>
        /// Delete Project
        /// </summary>
        /// <param name="project">DeleteProjectEvent model</param>
        /// <remarks>Deletes existing project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("api/v1/project")]
        [HttpDelete]
        public void DeleteProject([FromBody] DeleteProjectEvent project)
        {
            ProjectDataValidator.Validate(project, _projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            _projectService.StoreEvent(project);
        }

        /// <summary>
        /// Associate customer and project
        /// </summary>
        /// <param name="customerProject">Customer - project</param>
        /// <param name="topic">(Optional)Topic to publish on. Used for test purposes.</param>
        /// <remarks>Associate customer and asset</remarks>
        /// <response code="200">Ok</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Route("api/v1/project/AssociateCustomer")]
        public void AssociateCustomerProject([FromBody] AssociateProjectCustomer customerProject)
        {
            ProjectDataValidator.Validate(customerProject, _projectService);
            customerProject.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
                });
            _projectService.StoreEvent(customerProject);
        }

        /// <summary>
        /// Dissociate customer and asset
        /// </summary>
        /// <param name="customerProject">Customer - Project model</param>
        /// <param name="topic">(Optional)Topic to publish on. Used for test purposes.</param>
        /// <remarks>Dissociate customer and asset</remarks>
        /// <response code="200">Ok</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Route("api/v1/project/DissociateCustomer")]
        public void DissociateCustomerProject([FromBody] DissociateProjectCustomer customerProject)
        {
            ProjectDataValidator.Validate(customerProject, _projectService);
            customerProject.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {DissociateProjectCustomer = customerProject});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
                });
            _projectService.StoreEvent(customerProject);
        }
       

    /// <summary>
    /// Associate geofence and project
    /// </summary>
    /// <param name="geofenceProject">Geofence - project</param>
    /// <remarks>Associate geofence and project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
        [Route("api/v1/project/AssociateGeofence")]
        public void AssociateGeofenceProject([FromBody] AssociateProjectGeofence geofenceProject)
        {
            ProjectDataValidator.Validate(geofenceProject, _projectService);
            geofenceProject.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectGeofence = geofenceProject});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(geofenceProject.ProjectUID.ToString(), messagePayload)
                });
            _projectService.StoreEvent(geofenceProject);
        }

    }
}
