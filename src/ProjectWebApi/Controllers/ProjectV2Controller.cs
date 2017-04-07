using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using VSP.MasterData.Project.WebAPI.Controllers.V3;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using VSP.MasterData.Project.WebAPI.Controllers;
using VSS.Raptor.Service.Common.Interfaces;

namespace ProjectWebApi.Controllers
{
  public class ProjectV2Controller : ProjectBaseController
    {
    public ProjectV2Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger) 
      : base(producer, projectRepo, subscriptionsRepo, store, 
            subsProxy, geofenceProxy, raptorProxy, logger)
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
      await AssociateProjectCustomer(customerProject);
    }
  }
}
