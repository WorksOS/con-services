using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using VSP.MasterData.Project.WebAPI.Controllers.V3;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApi.Controllers
{
    public class ProjectV2Controller : ProjectV3Controller
    {
        public ProjectV2Controller(IKafka producer, IRepository<IProjectEvent> projectRepo, IConfigurationStore store) : base(producer, projectRepo, store)
        {
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
    }
}
