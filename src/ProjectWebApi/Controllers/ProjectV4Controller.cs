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
using VSP.MasterData.Project.WebAPI.Controllers.V3;

namespace VSP.MasterData.Project.WebAPI.Controllers.V4
{
    public class ProjectV4Controller : ProjectV3Controller
    {
        //TODO COnvert

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
        [Route("api/v4/project")]
        [HttpGet]
        public async Task<List<ProjectDescriptor>> GetProjectsV4()
        {
            log.LogInformation("GetProjectsV4"); // todo logging
            return await GetProjectsV3().ConfigureAwait(false);
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

            // todo ensure it has a legacyProjectId - another US

            // validate project
            ProjectDataValidator.Validate(project, projectService);
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

            ProjectDataValidator.Validate(customerProject, projectService);
            customerProject.ReceivedUTC = DateTime.UtcNow;

            await ValidateAssociateSubscriptions(project).ConfigureAwait(false);

            await CreateProject(project, kafkaBoundary, wktBoundary).ConfigureAwait(false);

            await CreateAssociateProjectCustomer(customerProject);

            var userUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).Name;
            geofenceProxy.CreateGeofence(project.CustomerUID, project.ProjectName, "", "", project.ProjectBoundary, 0,
                true, Guid.Parse(userUid), Request.Headers.GetCustomHeaders());

            log.LogDebug("CreateProjectV4. completed succesfully");
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
                throw new ServiceException(HttpStatusCode.Forbidden,
                    new ContractExecutionResult(ContractExecutionStatesEnum.NoValidSubscription,
                        "Project boundary overlaps another project, for this customer and time span"));
            }
            return overlaps;
        }


        /// <summary>
        /// Creates an association.
        /// </summary>
        /// <param name="customerProject">The create projectCustomer event</param>
        /// <returns></returns>
        private async Task CreateAssociateProjectCustomer(AssociateProjectCustomer customerProject)
        {
            var messagePayload = JsonConvert.SerializeObject(new {AssociateProjectCustomer = customerProject});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(customerProject.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(customerProject).ConfigureAwait(false);

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

            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            // todo ensure it has a legacyProjectId - another US
            //   i.e. don't create if doesn't exist, with a null legacyID, it MUST exist in DB

            var messagePayload = JsonConvert.SerializeObject(new {UpdateProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(project).ConfigureAwait(false);

            Console.WriteLine("UpdateProjectV4. Completed successfully");
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

            ProjectDataValidator.Validate(project, projectService);
            project.ReceivedUTC = DateTime.UtcNow;

            var messagePayload = JsonConvert.SerializeObject(new {DeleteProjectEvent = project});
            producer.Send(kafkaTopicName,
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>(project.ProjectUID.ToString(), messagePayload)
                });
            await projectService.StoreEvent(project).ConfigureAwait(false);

            Console.WriteLine("DeleteProjectV4. Completed succesfully");
        }

    }
}

