using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - appropriate for v2 and v4 controllers
  /// </summary>
  public class CreateProjectExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Processes the CreateProjectEvent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectSettingsResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      CreateProjectEvent createProjectEvent = item as CreateProjectEvent;
      if (createProjectEvent == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);

      var projectRequestHelper = new ProjectRequestHelper(
        log, configStore, serviceExceptionHandler,
        customerUid, userId, customHeaders,
        producer,
        geofenceProxy, raptorProxy, subscriptionProxy,
        projectRepo, subscriptionRepo, fileRepo);

      ProjectBoundaryValidator.ValidateWKT(((CreateProjectEvent)createProjectEvent).ProjectBoundary);
      await projectRequestHelper.ValidateCoordSystemInRaptor(createProjectEvent).ConfigureAwait(false);

      log.LogDebug($"Testing if there are overlapping projects for project {createProjectEvent.ProjectName}");
      await projectRequestHelper.DoesProjectOverlap(createProjectEvent, createProjectEvent.ProjectBoundary);

      AssociateProjectCustomer customerProject = new AssociateProjectCustomer
      {
        CustomerUID = createProjectEvent.CustomerUID,
        LegacyCustomerID = createProjectEvent.CustomerID,
        ProjectUID = createProjectEvent.ProjectUID,
        RelationType = RelationType.Owner,
        ActionUTC = createProjectEvent.ActionUTC,
        ReceivedUTC = createProjectEvent.ReceivedUTC
      };
      ProjectDataValidator.Validate(customerProject, projectRepo);

      // now making changes, potentially needing rollback 
      createProjectEvent = await projectRequestHelper.CreateProjectInDb(createProjectEvent, customerProject).ConfigureAwait(false);
      await projectRequestHelper.CreateCoordSystemInRaptor(createProjectEvent.ProjectUID, createProjectEvent.ProjectID, createProjectEvent.CoordinateSystemFileName, createProjectEvent.CoordinateSystemFileContent, true).ConfigureAwait(false);
      await projectRequestHelper.AssociateProjectSubscriptionInSubscriptionService(createProjectEvent).ConfigureAwait(false);
      await projectRequestHelper.CreateGeofenceInGeofenceService(createProjectEvent).ConfigureAwait(false);

      // doing this as late as possible in case something fails. We can't cleanup kafka que.
      projectRequestHelper.CreateKafkaEvents(createProjectEvent, customerProject);

      log.LogDebug("CreateProjectV4. completed succesfully");
      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    protected override void ProcessErrorCodes()
    {
    }
  }
}