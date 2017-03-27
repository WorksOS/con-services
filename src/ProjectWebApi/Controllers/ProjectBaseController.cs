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

namespace VSP.MasterData.Project.WebAPI.Controllers
{
    public class ProjectBaseController : Controller
    {
        protected readonly IKafka producer;
        protected readonly ILogger log;
        protected readonly ISubscriptionProxy subsProxy;
        protected readonly IGeofenceProxy geofenceProxy;

        protected readonly ProjectRepository projectService;
        protected readonly string kafkaTopicName;
        private readonly SubscriptionRepository subsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBaseController"/> class.
        /// </summary>
        /// <param name="producer">The producer.</param>
        /// <param name="projectRepo">The project repo.</param>
        /// <param name="subscriptionsRepo">The subscriptions repo.</param>
        /// <param name="store">The store.</param>
        /// <param name="subsProxy">The subs proxy.</param>
        /// <param name="geofenceProxy">The geofence proxy.</param>
        /// <param name="logger">The logger.</param>
        public ProjectBaseController(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, ILoggerFactory logger)
        {
            log = logger.CreateLogger<ProjectBaseController>();
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
        /// Validates if there any subscriptions available for the request create project event
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        protected async Task ValidateAssociateSubscriptions(CreateProjectEvent project)
        {
            log.LogInformation("ValidateAssociateSubscriptions");
            var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
            log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);

            //Apply here rules validating types of projects I'm able to create (i.e. LF only if there is one available LF subscription available) Performance is not a concern as this request is executed once in a blue moon
            //Retrieve available subscriptions
            //Should be Today used or UTC?

            //let's find out here what project we can create
            if (project.ProjectType == ProjectType.LandFill || project.ProjectType == ProjectType.ProjectMonitoring)
            {
                var availableFreeSub = (await GetFreeSubs(customerUid, project.ProjectType)).First();
                //Assign a new project to a subs
                await subsProxy.AssociateProjectSubscription(Guid.Parse(availableFreeSub.SubscriptionUID),
                    project.ProjectUID,
                    Request.Headers.GetCustomHeaders()).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the free subs for a project type
        /// </summary>
        /// <param name="customerUid">The customer uid.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="ServiceException"></exception>
        /// <exception cref="ContractExecutionResult">No available subscriptions for the selected customer</exception>
        protected async Task<IEnumerable<Subscription>> GetFreeSubs(string customerUid, ProjectType type)
        {
            var availableSubscriptions =
                await subsService.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false);
            var projects = await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false);

            var availableFreSub = availableSubscriptions.Where(s => !projects.Where(p =>
                                                                            p.ProjectType == type && !p.IsDeleted)
                                                                        .Select(p => p.SubscriptionUID)
                                                                        .Contains(s.SubscriptionUID) &&
                                                                    s.ServiceTypeID ==
                                                                    (int) type.MatchSubscriptionType());

            var freeSubs = availableFreSub as IList<Subscription> ?? availableFreSub.ToList();
            if (availableFreSub == null || !freeSubs.Any())
            {
                throw new ServiceException(HttpStatusCode.Forbidden,
                    new ContractExecutionResult(ContractExecutionStatesEnum.NoValidSubscription,
                        "No available subscriptions for the selected customer"));
            }
            return freeSubs;
        }

        /// <summary>
        /// Gets the free subscription regardless project type.
        /// </summary>
        /// <param name="customerUid">The customer uid.</param>
        /// <returns></returns>
        protected async Task<IEnumerable<Subscription>> GetFreeSubs(string customerUid)
        {
            var availableSubscriptions =
                await subsService.GetSubscriptionsByCustomer(customerUid, DateTime.UtcNow.Date).ConfigureAwait(false);
            var projects = await projectService.GetProjectsForCustomer(customerUid).ConfigureAwait(false);

            var availableFreSub = availableSubscriptions.Where(s => !projects.Where(p =>
                                                                            !p.IsDeleted)
                                                                        .Select(p => p.SubscriptionUID)
                                                                        .Contains(s.SubscriptionUID));

            return availableFreSub as IList<Subscription> ?? availableFreSub.ToList();
        }


        /// <summary>
        /// Gets the project list for a customer
        /// </summary>
        /// <returns></returns>
        protected async Task<List<ProjectDescriptor>> GetProjectList()
        {
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

        /// <summary>
        /// Updates the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        protected async Task UpdateProject(UpdateProjectEvent project)
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

        /// <summary>
        /// Creates a project. Handles both old and new project boundary formats. this method can be overriden
        /// </summary>
        /// <param name="project">The create project event</param>
        /// <param name="kafkaProjectBoundary">The project boundary in the old format (coords comma separated, points semicolon separated)</param>
        /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
        /// <returns></returns>
        protected virtual async Task CreateProject(CreateProjectEvent project, string kafkaProjectBoundary,
            string databaseProjectBoundary)
        {
            ProjectDataValidator.Validate(project, projectService);
            if (project.ProjectID <= 0)
            {
                throw new ServiceException(HttpStatusCode.BadRequest,
                    new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                        "Missing legacy ProjectID"));
            }
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

        /// <summary>
        /// Associates the project customer.
        /// </summary>
        /// <param name="customerProject">The customer project.</param>
        /// <returns></returns>
        protected async Task AssociateProjectCustomer(AssociateProjectCustomer customerProject)
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
        /// Dissociates the project customer.
        /// </summary>
        /// <param name="customerProject">The customer project.</param>
        /// <returns></returns>
        protected async Task DissociateProjectCustomer(DissociateProjectCustomer customerProject)
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
        /// Associates the geofence project.
        /// </summary>
        /// <param name="geofenceProject">The geofence project.</param>
        /// <returns></returns>
        protected async Task AssociateGeofenceProject(AssociateProjectGeofence geofenceProject)
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

        /// <summary>
        /// Deletes the project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        protected async Task DeleteProject(DeleteProjectEvent project)
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

    }
}
