using System.Net;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using ProjectWebApiCommon.ResultsHandling;
using TCCFileAccess;
using VSP.MasterData.Project.WebAPI.Controllers;
using VSS.Raptor.Service.Common.Interfaces;

namespace ProjectWebApi.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  public class ProjectV2Controller : ProjectBaseController
    {
    public ProjectV2Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, IFileRepository fileRepo, ILoggerFactory logger) 
      : base(producer, projectRepo, subscriptionsRepo, store, 
            subsProxy, geofenceProxy, raptorProxy, fileRepo, logger)
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
      if (customerProject.LegacyCustomerID <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Legacy CustomerID must be provided"));
      }
      await AssociateProjectCustomer(customerProject);
    }
  }
}
