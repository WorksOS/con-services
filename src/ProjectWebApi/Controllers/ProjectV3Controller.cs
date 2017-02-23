using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Models;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using VSS.Project.Data;
using VSS.Project.Service.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSP.MasterData.Project.WebAPI.Controllers.V3
{
  public class ProjectV3Controller : Controller
    {

        private readonly IKafka _producer;
        private readonly IRepository<IProjectEvent> _projectService;
        private readonly string kafkaTopicName;

        public ProjectV3Controller(IKafka producer, IRepository<IProjectEvent> projectRepo, IConfigurationStore store)
        {
            _producer = producer;
            _producer.InitProducer(store);
            _projectService = projectRepo;
            kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                             store.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");
        }

        /// <summary>
        /// Gets a list of projects for a customer. The list includes projects of all project types
        /// and both active and archived projects.
        /// </summary>
        /// <returns>A list of projects</returns>
        [Route("api/v3/project")]
        [HttpGet]
        public List<ProjectDescriptor> GetProjectsV3()
        {
          Console.WriteLine("GetProjectsV3");
          var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
          Console.WriteLine("CustomerUID=" + customerUid + " and user=" + User);
          var projects = (_projectService as ProjectRepository).GetProjectsForCustomer(customerUid).Result;

          var projectList = new List<ProjectDescriptor>();
          foreach (var project in projects)
          {
            Console.WriteLine("Build list ProjectName=" + project.Name);
            projectList.Add(
              new ProjectDescriptor
              {
                ProjectType = project.ProjectType,
                Name = project.Name,
                ProjectTimeZone = project.ProjectTimeZone,
                IsArchived = project.IsDeleted || project.SubscriptionEndDate < DateTime.UtcNow,
                StartDate = project.StartDate.ToString("O"),
                EndDate = project.EndDate.ToString("O"),
                ProjectUid = project.ProjectUID,
                LegacyProjectId = project.LegacyProjectID,
                ProjectGeofenceWKT = project.GeometryWKT
              });
          }
          return projectList;
        }

    /// <summary>
    /// Gets the projects for a customer. This includes projects of all project types
    /// and both active and archived projects.
    /// </summary>
    /// <returns>A dictionary of projects keyed on legacy project id</returns>
    [Route("v1")]
    [HttpGet]
    public Dictionary<long, ProjectDescriptor> GetProjectsV1()
    {
      Console.WriteLine("GetProjectsV1 ");
      return GetProjectsV3().ToDictionary(k => (long)k.LegacyProjectId, v => v);
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
            const string polygonStr = "POLYGON";

            Console.WriteLine("POST CreateProjectV3 - ");

            ProjectDataValidator.Validate(project, _projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            //TODO this should return valid error reponses if the request is not valid!

            // Check whether the ProjectBoundary is in WKT format. Convert to the old format if it is. 
            if (project.ProjectBoundary.Contains(polygonStr))
                project.ProjectBoundary =
                    project.ProjectBoundary.Replace(polygonStr + "((", "")
                        .Replace("))", "")
                        .Replace(',', ';')
                        .Replace(' ', ',') + ';';

            ProjectBoundaryValidator.Validate(project.ProjectBoundary);

            var messagePayload = JsonConvert.SerializeObject(new {CreateProjectEvent = project});
            _producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await _projectService.StoreEvent(project).ConfigureAwait(false);
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
      CreateProjectV3(project);
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
        ProjectDataValidator.Validate(project, _projectService);
        project.ReceivedUTC = DateTime.UtcNow;
        
        var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
        _producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
            });
        await _projectService.StoreEvent(project).ConfigureAwait(false);

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
    await UpdateProjectV3(project);
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
        ProjectDataValidator.Validate(project, _projectService);
        project.ReceivedUTC = DateTime.UtcNow;

        var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
        _producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
            });
        await _projectService.StoreEvent(project).ConfigureAwait(false);
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
      await DeleteProjectV3(project);
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
      var messagePayload = JsonConvert.SerializeObject(new { RestoreProjectEvent = project });

      _producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
           new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
        });
        //throw new Exception("Failed to publish message to Kafka");
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
        ProjectDataValidator.Validate(customerProject, _projectService);
        customerProject.ReceivedUTC = DateTime.UtcNow;

        var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
        _producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
            });
        await _projectService.StoreEvent(customerProject).ConfigureAwait(false);
    }

    /// <summary>
    /// Associate customer and project
    /// </summary>
    /// <param name="customerProject">Customer - project</param>
    /// <remarks>Associate customer and asset</remarks>
    /// <response code="200">Ok</response>
    /// <response code="500">Internal Server Error</response>
    [Route("v2/AssociateCustomer")]
    [HttpPost]
    public async Task AssociateCustomerProjectV2([FromBody] AssociateProjectCustomer customerProject)
    {
        await AssociateCustomerProjectV3(customerProject);
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
        ProjectDataValidator.Validate(customerProject, _projectService);
        customerProject.ReceivedUTC = DateTime.UtcNow;

        var messagePayload = JsonConvert.SerializeObject(new {DissociateProjectCustomer = customerProject});
        _producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
            });
        await _projectService.StoreEvent(customerProject).ConfigureAwait(false);
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
      await DissociateCustomerProjectV3(customerProject);
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
        ProjectDataValidator.Validate(geofenceProject, _projectService);
        geofenceProject.ReceivedUTC = DateTime.UtcNow;

        var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectGeofence = geofenceProject});
        _producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>(geofenceProject.ProjectUID.ToString(), messagePayload)
            });
        await _projectService.StoreEvent(geofenceProject).ConfigureAwait(false);
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
      await AssociateGeofenceProjectV3(geofenceProject);
    }

    }
}
