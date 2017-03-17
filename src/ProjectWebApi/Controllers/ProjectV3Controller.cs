using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Models;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApi.ResultsHandling;

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
    [Route("api/v4/project")]
    [HttpGet]
    public List<ProjectDescriptor> GetProjectsV4()
    {
      Console.WriteLine("GetProjectsV4");
      return GetProjectsV3();
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
      Console.WriteLine("GetProjectsVn: CustomerUID=" + customerUid + " and user=" + User);
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
    ///    as of v4 this creates a project AND the association to Customer
    /// </summary>
    /// <param name="project">CreateProjectEvent model</param>
    /// <remarks>Create new project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project")]
    [HttpPost]
    public async Task CreateProjectV4([FromBody] CreateProjectEvent project)
    {
      Console.WriteLine("CreateProjectV4. Project: {0}", JsonConvert.SerializeObject(project));

      // validate project
      ProjectDataValidator.Validate(project, _projectService);
      project.ReceivedUTC = DateTime.UtcNow;
      ProjectBoundaryValidator.ValidateWKT(project.ProjectBoundary);

      string wktBoundary = project.ProjectBoundary;
      await DoesProjectOverlap(project, wktBoundary);
      // todo ensure a legacyProjectID and legacyCustomerId exist & put in project - another US 

      //Convert to old format for Kafka for consistency on kakfa queue
      string kafkaBoundary = project.ProjectBoundary
              .Replace(ProjectBoundaryValidator.POLYGON_WKT, string.Empty)
              .Replace("))", string.Empty)
              .Replace(',', ';')
              .Replace(' ', ',');


      // validate projectCustomer
      AssociateProjectCustomer customerProject = new AssociateProjectCustomer()
      {
        CustomerUID = project.CustomerUID,
        LegacyCustomerID = project.CustomerID,
        ProjectUID = project.ProjectUID,
        RelationType = RelationType.Owner, // todo? not in CreateProjectEvent
        ActionUTC = project.ActionUTC,
        ReceivedUTC = project.ReceivedUTC
      };

      ProjectDataValidator.Validate(customerProject, _projectService);
      customerProject.ReceivedUTC = DateTime.UtcNow;
    

      await CreateProject(project, kafkaBoundary, wktBoundary);
      
      await CreateAssociateProjectCustomer(customerProject);
      Console.WriteLine("CreateProjectV4. completed succesfully");
    }

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
    /// Determines if the project boundary overlaps any exising project for the customer in time and space.
    ///    not needed for v1 or 3 and they come via CGen which already does this overlap checking.
    /// </summary>    
    /// <param name="project">The create project event</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    private async Task<bool> DoesProjectOverlap(CreateProjectEvent project, string databaseProjectBoundary)
    {
      var overlaps = await (_projectService as ProjectRepository).DoesPolygonOverlap(project.CustomerUID.ToString(), databaseProjectBoundary, project.ProjectStartDate, project.ProjectEndDate).ConfigureAwait(false);
      if (overlaps)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
                  new ContractExecutionResult(ContractExecutionStatesEnum.OverlappingProjects,
                                          "Project boundary overlaps another project, for this customer and time span"));
      }
      return overlaps;
    }

    /// <summary>
    /// Creates a project. Handles both old and new project boundary formats.
    /// </summary>
    /// <param name="project">The create project event</param>
    /// <param name="kafkaProjectBoundary">The project boundary in the old format (coords comma separated, points semicolon separated)</param>
    /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
    /// <returns></returns>
    private async Task CreateProject(CreateProjectEvent project, string kafkaProjectBoundary, string databaseProjectBoundary)
    {
      ProjectDataValidator.Validate(project, _projectService);
      project.ReceivedUTC = DateTime.UtcNow;

      //Send boundary as old format on kafka queue
      project.ProjectBoundary = kafkaProjectBoundary;
      var messagePayload = JsonConvert.SerializeObject(new { CreateProjectEvent = project });
      _producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>()
          {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
          });
      //Save boundary as WKT
      project.ProjectBoundary = databaseProjectBoundary;
      await _projectService.StoreEvent(project).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates an association.
    /// </summary>
    /// <param name="customerProject">The create projectCustomer event</param>
     /// <returns></returns>
    private async Task CreateAssociateProjectCustomer(AssociateProjectCustomer customerProject)
    {
      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectCustomer = customerProject });
      _producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>()
          {
                new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
          });
      await _projectService.StoreEvent(customerProject).ConfigureAwait(false);

    }

    // PUT: api/Project
    /// <summary>
    /// Update Project
    /// </summary>
    /// <param name="project">UpdateProjectEvent model</param>
    /// <remarks>Updates existing project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/project")]
    [HttpPut]
    public async Task UpdateProjectV4([FromBody] UpdateProjectEvent project)
    {
      Console.WriteLine("UpdateProjectV4. Project: {0}", JsonConvert.SerializeObject(project));

      ProjectDataValidator.Validate(project, _projectService);
      project.ReceivedUTC = DateTime.UtcNow;

      // todo ensure it has a legacyProjectId - another US

      var messagePayload = JsonConvert.SerializeObject(new { UpdateProjectEvent = project });
      _producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>()
          {
                new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
          });
      await _projectService.StoreEvent(project).ConfigureAwait(false);

      Console.WriteLine("UpdateProjectV4. Completed successfully");
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

      // todo ensure it has a legacyProjectId - another US

      var messagePayload = JsonConvert.SerializeObject(new { UpdateProjectEvent = project });
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
    [Route("api/v4/project")]
    [HttpDelete]
    public async Task DeleteProjectV4([FromBody] DeleteProjectEvent project)
    {
      Console.WriteLine("DeleteProjectV4. Project: {0}", JsonConvert.SerializeObject(project));

      await DeleteProjectV3(project);

      Console.WriteLine("DeleteProjectV4. Completed succesfully");
    }


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

      var messagePayload = JsonConvert.SerializeObject(new { DeleteProjectEvent = project });
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

      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectCustomer = customerProject });
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

      var messagePayload = JsonConvert.SerializeObject(new { DissociateProjectCustomer = customerProject });
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

      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectGeofence = geofenceProject });
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

