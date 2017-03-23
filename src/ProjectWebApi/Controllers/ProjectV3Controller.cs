using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Models;
using Repositories;
using Repositories.DBModels;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApi.ResultsHandling;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Utilities;

namespace VSP.MasterData.Project.WebAPI.Controllers.V3
{
    public class ProjectV3Controller : Controller
    {
        protected readonly IKafka producer;
        protected readonly ILogger log;
        protected readonly ISubscriptionProxy subsProxy;
        protected readonly IGeofenceProxy geofenceProxy;



        protected readonly ProjectRepository projectService;
        protected readonly string kafkaTopicName;
        private readonly SubscriptionRepository subsService;


        public ProjectV3Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, ILoggerFactory logger)
        {
            log = logger.CreateLogger<ProjectV3Controller>();
            this.producer = producer;
            //We probably want to make this thing singleton?
            this.producer.InitProducer(store);
            //TODO change this pattern, make it safer
            projectService = projectRepo as ProjectRepository;
            subsService = subscriptionsRepo as SubscriptionRepository;
            this.subsProxy = subsProxy;
            this.geofenceProxy = geofenceProxy;

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
        public async Task<List<ProjectDescriptor>> GetProjectsV3()
        {
            log.LogInformation("GetProjectsV3");
            var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
            log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);
            var projects = await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false);

            var projectList = new List<ProjectDescriptor>();
            foreach (var project in projects)
            {
                log.LogInformation("Build list ProjectName=" + project.Name);
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



        /// <summary>
        /// Creates a project. Handles both old and new project boundary formats.
        /// </summary>
        /// <param name="project">The create project event</param>
        /// <param name="kafkaProjectBoundary">The project boundary in the old format (coords comma separated, points semicolon separated)</param>
        /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
        /// <returns></returns>
        protected virtual async Task CreateProject(CreateProjectEvent project, string kafkaProjectBoundary,
            string databaseProjectBoundary)
        {
            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            //Send boundary as old format on kafka queue
            project.ProjectBoundary = kafkaProjectBoundary;
            var messagePayload = JsonConvert.SerializeObject(new {CreateProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            //Save boundary as WKT
            project.ProjectBoundary = databaseProjectBoundary;
            await projectService.StoreEvent(project).ConfigureAwait(false);
        }

        protected async Task ValidateAssociateSubscriptions(CreateProjectEvent project)
        {
            log.LogInformation("GetProjectsV3");
            var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
            log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);

            //Apply here rules validating types of projects I'm able to create (i.e. LF only if there is one available LF subscription available) Performance is not a concern as this request is executed once in a blue moon
            //Retrieve available subscriptions
            //Should be Today used or UTC?
            var availableSubscriptions =
                await subsService.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false);
            var projects = await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false);
            Subscription availableFreeSub = null;

            //let's find out here what project we can create
            if (project.ProjectType == ProjectType.LandFill || project.ProjectType == ProjectType.ProjectMonitoring)
            {
                availableFreeSub = GetFreeSubs(availableSubscriptions, projects, project.ProjectType).First();
                //Assign a new project to a subs
                subsProxy.AssociateProjectSubscription(Guid.Parse(availableFreeSub.SubscriptionUID), project.ProjectUID,
                    Request.Headers.GetCustomHeaders());
            }
        }

        //Method to check free subs for a project
        private IEnumerable<Subscription> GetFreeSubs(IEnumerable<Subscription> availableSubscriptions,
            IEnumerable<Repositories.DBModels.Project> projects, ProjectType type)
        {
            var availableFreSub = availableSubscriptions.Where(s => !projects.Where(p =>
                                                                            p.ProjectType == type && !p.IsDeleted)
                                                                        .Select(p => p.SubscriptionUID)
                                                                        .Contains(s.SubscriptionUID) &&
                                                                    s.ServiceTypeID ==
                                                                    (int) type.MatchSubscriptionType());

            var freeSubs = availableFreSub as IList<Subscription> ?? availableFreSub.ToList();
            if (!freeSubs.Any())
            {
                throw new ServiceException(HttpStatusCode.Forbidden,
                    new ContractExecutionResult(ContractExecutionStatesEnum.NoValidSubscription,
                        "No available subscriptions for the selected customer"));
            }
            return freeSubs;
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
            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(project).ConfigureAwait(false);

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
            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(project).ConfigureAwait(false);
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
            ProjectDataValidator.Validate(customerProject, projectService);
            customerProject.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(customerProject).ConfigureAwait(false);
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
            ProjectDataValidator.Validate(customerProject, projectService);
            customerProject.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {DissociateProjectCustomer = customerProject});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(customerProject).ConfigureAwait(false);
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
            ProjectDataValidator.Validate(geofenceProject, projectService);
            geofenceProject.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectGeofence = geofenceProject});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(geofenceProject.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(geofenceProject).ConfigureAwait(false);
        }
    }
}

