using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using ProjectWebApi.ResultsHandling;
using Repositories.DBModels;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Utilities;
using VSP.MasterData.Project.WebAPI.Controllers.V3;

namespace VSP.MasterData.Project.WebAPI.Controllers.V4
{
    public class ProjectV4Controller : ProjectBaseController
    {
        public ProjectV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, ILoggerFactory logger)
            : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, logger)
        {
        }


        /// <summary>
        /// Gets a list of projects for a customer. The list includes projects of all project types
        /// and both active and archived projects.
        /// </summary>
        /// <returns>A list of projects</returns>
        [Route("api/v4/projects")]
        [HttpGet]
        public async Task<ProjectDescriptorsListResult> GetProjectsV4()
        {
            log.LogInformation("GetProjectsV4");
            return new ProjectDescriptorsListResult()
            {
                ProjectDescriptors = await GetProjectList().ConfigureAwait(false)
            };
        }

        /// <summary>
        /// Gets a project for a customer. 
        /// </summary>
        /// <returns>A project data</returns>
        [Route("api/v4/project")]
        [HttpGet]
        public async Task<ProjectDescriptor> GetProjectV4([FromQuery] string projectUid)
        {
            log.LogInformation("GetProjectV4");
            return await GetProject(projectUid).ConfigureAwait(false);
        }


        /// <summary>
        /// Gets available subscription for a customer
        /// </summary>
        /// <returns>List of available subscriptions</returns>
        [Route("api/v4/subscriptions")]
        [HttpGet]
        public async Task<SubscriptionsListResult> GetSubscriptionsV4()
        {
            log.LogInformation("GetSubscriptionsV4");
            var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
            log.LogInformation("CustomerUID=" + customerUid + " and user=" + User);

            //return empty list if no subscriptions available
            return new SubscriptionsListResult()
            {
                SubscriptionDescriptors =
                    (await GetFreeSubs(customerUid).ConfigureAwait(false)).Select(
                        SubscriptionDescriptor.FromSubscription).ToImmutableList()
            };
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
        public async Task<ContractExecutionResult> CreateProjectV4([FromBody] CreateProjectEvent project)
        {
            log.LogInformation("CreateProjectV4. Project: {0}", JsonConvert.SerializeObject(project));

            // validate project
            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;
            ProjectBoundaryValidator.ValidateWKT(project.ProjectBoundary);

            string wktBoundary = project.ProjectBoundary;
            log.LogDebug($"Testing if there are overlapping projects for project {project.ProjectName}");
            await DoesProjectOverlap(project, wktBoundary);

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
                RelationType = RelationType.Owner,
                ActionUTC = project.ActionUTC,
                ReceivedUTC = project.ReceivedUTC
            };

            ProjectDataValidator.Validate(customerProject, projectService);
            customerProject.ReceivedUTC = DateTime.UtcNow;

            await ValidateAssociateSubscriptions(project).ConfigureAwait(false);

            await CreateProject(project, kafkaBoundary, wktBoundary).ConfigureAwait(false);

            await CreateAssociateProjectCustomer(customerProject);

            var userUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).Name;
            log.LogDebug($"Creating a geofence for project {project.ProjectName}");
            await geofenceProxy.CreateGeofence(project.CustomerUID, project.ProjectName, "", "", project.ProjectBoundary,
                0,
                true, Guid.Parse(userUid), Request.Headers.GetCustomHeaders()).ConfigureAwait(false);

            log.LogDebug("CreateProjectV4. completed succesfully");
            return new ContractExecutionResult();
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
        public async Task<ContractExecutionResult> UpdateProjectV4([FromBody] UpdateProjectEvent project)
        {
            log.LogInformation("UpdateProjectV4. Project: {0}", JsonConvert.SerializeObject(project));

            // validation includes check that project must exist - otherwise there will be a null legacyID.
            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(project).ConfigureAwait(false);

            log.LogInformation("UpdateProjectV4. Completed successfully");
            return new ContractExecutionResult();
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
        public async Task<ContractExecutionResult> DeleteProjectV4([FromBody] DeleteProjectEvent project)
        {
            log.LogInformation("DeleteProjectV4. Project: {0}", JsonConvert.SerializeObject(project));

            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(project).ConfigureAwait(false);

            log.LogInformation("DeleteProjectV4. Completed succesfully");
            return new ContractExecutionResult();
        }

        /// <summary>
        /// Creates a project. Handles both old and new project boundary formats.
        /// </summary>
        /// <param name="project">The create project event</param>
        /// <param name="kafkaProjectBoundary">The project boundary in the old format (coords comma separated, points semicolon separated)</param>
        /// <param name="databaseProjectBoundary">The project boundary in the new format (WKT)</param>
        /// <returns></returns>
        protected override async Task CreateProject(CreateProjectEvent project, string kafkaProjectBoundary,
            string databaseProjectBoundary)
        {
            log.LogDebug($"Creating the project {project.ProjectName}");
            await projectService.StoreEvent(project).ConfigureAwait(false);
            if (project.ProjectID <= 0)
            {
                var existing = await projectService.GetProjectOnly(project.ProjectUID.ToString()).ConfigureAwait(false);
                if (existing != null && existing.LegacyProjectID > 0)
                    project.ProjectID = existing.LegacyProjectID;
                else
                {
                    throw new ServiceException(HttpStatusCode.InternalServerError,
                        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
                            "LegacyProjectId has not been generated"));
                }
            }
            log.LogDebug($"Using Legacy projectId {project.ProjectID} for project {project.ProjectName}");
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
            var overlaps =
                await projectService.DoesPolygonOverlap(project.CustomerUID.ToString(), databaseProjectBoundary,
                    project.ProjectStartDate, project.ProjectEndDate).ConfigureAwait(false);
            if (overlaps)
            {
                log.LogWarning(
                    $"There are overlappitn projects for {project.ProjectName}, dates {project.ProjectStartDate}:{project.ProjectEndDate}, geofence {databaseProjectBoundary}");
                throw new ServiceException(HttpStatusCode.Forbidden,
                    new ContractExecutionResult(ContractExecutionStatesEnum.NoValidSubscription,
                        "Project boundary overlaps another project, for this customer and time span"));
            }
            log.LogDebug($"No overlapping projects for {project.ProjectName}");
            return overlaps;
        }


        /// <summary>
        /// Creates an association.
        /// </summary>
        /// <param name="customerProject">The create projectCustomer event</param>
        /// <returns></returns>
        private async Task CreateAssociateProjectCustomer(AssociateProjectCustomer customerProject)
        {
            log.LogDebug($"Associating Project {customerProject.ProjectUID} with Customer {customerProject.CustomerUID}");
            var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(customerProject).ConfigureAwait(false);
        }
    }
}

