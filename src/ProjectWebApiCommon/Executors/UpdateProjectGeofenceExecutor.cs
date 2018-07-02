using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which gets the geofences for the customer and/or project
  /// </summary>
  public class UpdateProjectGeofenceExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the UpdateProjectGeofenceAssociation request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ProjectGeofenceResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var updateProjectGeofenceRequest = item as UpdateProjectGeofenceRequest;
      if (updateProjectGeofenceRequest == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);

      var project = await ProjectRequestHelper.GetProject(updateProjectGeofenceRequest.ProjectUid.ToString(),
        customerUid,
        log, serviceExceptionHandler, projectRepo).ConfigureAwait(false);

      // only Landfill Project and Landfill geofence combination currently supported
      if (project.ProjectType != ProjectType.LandFill)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 102);
      }

      ProjectDataValidator.ValidateGeofenceTypes(updateProjectGeofenceRequest.GeofenceTypes, project.ProjectType);

      // Validate GeofencesExist, and are of correct type for project type, and are not assigned to another project
      // get all geofences of the required type for the customer
      var allGeofencesOfTypes = new List<GeofenceWithAssociation>();
      try
      {
        allGeofencesOfTypes =
          (await ProjectRequestHelper.GetCustomerGeofenceList(customerUid, updateProjectGeofenceRequest.GeofenceTypes,
            log, projectRepo)).ToList();
        log.LogInformation($"UpdateProjectGeofenceExecutor() allGeofencesOfTypes: {JsonConvert.SerializeObject(allGeofencesOfTypes)}");
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 106, e.Message);
      }

      // do we have all the GeofenceUids requested, in the database?
      var requestedGeofenceUids = allGeofencesOfTypes
        .Where(g => updateProjectGeofenceRequest.GeofenceGuids.Contains(Guid.Parse(g.GeofenceUID))).ToList();
      if (requestedGeofenceUids.Count
          != updateProjectGeofenceRequest.GeofenceGuids.Count)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 104);
      }

      // are any already assigned to a different project?
      if (requestedGeofenceUids
        .Where(g => g.ProjectUID != null && g.ProjectUID != updateProjectGeofenceRequest.ProjectUid.ToString()).ToList()
        .Any())
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 105);
      }

      try
      {
        // dissassociate not supported. Current associations should be included in requested list.
        var associationsMissing = allGeofencesOfTypes
          .Where(g => (!string.IsNullOrEmpty(g.ProjectUID) && String.Compare(g.ProjectUID,
                         updateProjectGeofenceRequest.ProjectUid.ToString(), StringComparison.OrdinalIgnoreCase) ==
                       0) &&
                      !updateProjectGeofenceRequest.GeofenceGuids.Contains(Guid.Parse(g.GeofenceUID))
          ).ToList();

        if (associationsMissing.Any())
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 107);
        }

        // Associate any new geofences - ignore any already associated
        foreach (var newGeofenceGuid in updateProjectGeofenceRequest.GeofenceGuids)
        {
          if (requestedGeofenceUids.Any(g => g.GeofenceUID == newGeofenceGuid.ToString()
                                             && g.ProjectUID == null))
          {
            var geofenceProject = new AssociateProjectGeofence()
            {
              GeofenceUID = newGeofenceGuid,
              ProjectUID = updateProjectGeofenceRequest.ProjectUid,
              ActionUTC = DateTime.UtcNow
            };
            await ProjectRequestHelper.AssociateGeofenceProject(geofenceProject, projectRepo,
              log, serviceExceptionHandler,
              producer, kafkaTopicName);
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }

      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}