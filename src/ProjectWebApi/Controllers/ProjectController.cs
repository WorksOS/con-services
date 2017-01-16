using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using KafkaConsumer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Utils;
using VSS.Project.Service.WebApiModels.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Project.WebApi.Configuration.Principal.Models;


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
            _projectService = projectRepo;
            kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                             store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
        }

        [Route("v1")]
        [HttpGet]
        public Dictionary<long, ProjectDescriptor> CreateProject()
        {
            //Secure with project list
            if (!(this.User as ProjectsPrincipal).AvailableProjects.Any())
            {
                throw new AuthenticationException();
            }

            return (this.User as ProjectsPrincipal).AvailableProjects;
        }

        // POST: api/project
        /// <summary>
        /// Create Project
        /// </summary>
        /// <param name="project">CreateProjectEvent model</param>
        /// <remarks>Create new project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("v1")]
        [HttpPost]
        public void CreateProject([FromBody] CreateProjectEvent project)
        {
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
        [Route("v1")]
        [HttpPut]
        public void UpdateProject([FromBody] UpdateProjectEvent project)
        {
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
        /// <param name="projectUID">DeleteProjectEvent model</param>
        /// <param name="userUID">DeleteProjectEvent model</param>
        /// <param name="actionUTC">DeleteProjectEvent model</param>
        /// <remarks>Deletes project with projectUID</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>

        [Route("v1")]
        [HttpDelete]
        public void DeleteProject(Guid projectUID, DateTime actionUTC)
        {
            var project = new DeleteProjectEvent();
            project.ProjectUID = projectUID;
            project.ActionUTC = actionUTC;

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
        [Route("v2/AssociateCustomer")]
        public void AssociateCustomerProject([FromBody] AssociateProjectCustomer customerProject)
        {
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
        [Route("v1/DissociateCustomer")]
        public void DissociateCustomerProject([FromBody] DissociateProjectCustomer customerProject)
        {
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
        [Route("v1/AssociateGeofence")]
        public void AssociateGeofenceProject([FromBody] AssociateProjectGeofence geofenceProject)
        {
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
