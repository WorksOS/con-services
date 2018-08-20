using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  ///
  /// </summary>
  public partial class ProjectRequestHelper
  {
    public static void ValidateProjectBoundary(string projectBoundary, IServiceExceptionHandler serviceExceptionHandler)
    {
      var result = GeofenceValidation.ValidateWKT(projectBoundary);
      if (String.CompareOrdinal(result, GeofenceValidation.ValidationOk) != 0)
      {
        if (String.CompareOrdinal(result, GeofenceValidation.ValidationNoBoundary) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 23);
        }

        if (String.CompareOrdinal(result, GeofenceValidation.ValidationLessThan3Points) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 24);
        }

        if (String.CompareOrdinal(result, GeofenceValidation.ValidationInvalidFormat) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 25);
        }

        if (String.CompareOrdinal(result, GeofenceValidation.ValidationInvalidPointValue) == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 111);
        }
      }
    }

    public static async Task<IEnumerable<GeofenceWithAssociation>> GetCustomerGeofenceList(string customerUid,
      List<GeofenceType> geofenceTypes,
      ILogger log, IProjectRepository projectRepo)
    {
      return (await projectRepo.GetCustomerGeofences(customerUid).ConfigureAwait(false))
        .Where(g => geofenceTypes.Contains(g.GeofenceType));
    }

    /// <summary>
    /// Gets the geofence list available for a customer, 
    ///    or those associated with a project
    /// </summary>
    /// <returns></returns>
    public static async Task<List<GeofenceWithAssociation>> GetGeofenceList(string customerUid, string projectUid,
      List<GeofenceType> geofenceTypes,
      ILogger log, IProjectRepository projectRepo)
    {
      log.LogInformation(
        $"GetGeofenceList: customerUid {customerUid}, projectUid {projectUid}, {JsonConvert.SerializeObject(geofenceTypes)}");

      var geofencesWithAssociation = await GetCustomerGeofenceList(customerUid, geofenceTypes, log, projectRepo);

      if (!string.IsNullOrEmpty(projectUid))
      {
        var geofencesAssociated = geofencesWithAssociation
          .Where(g => g.ProjectUID == projectUid).ToList();

        var associated = geofencesAssociated.ToList();
        log.LogInformation(
          $"Geofence list contains {associated.Count} geofences associated to project {projectUid}");
        return associated;
      }

      // geofences which are not associated with ANY project
      var notAssociated = geofencesWithAssociation
        .Where(g => string.IsNullOrEmpty(g.ProjectUID)).ToList();
      log.LogInformation($"Geofence list contains {notAssociated.Count} available geofences");
      return notAssociated;
    }
   

    /// <summary>
    /// Associates the geofence to the project.
    /// </summary>
    /// <param name="projectGeofence">The association.</param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="projectRepo"></param>
    /// <param name="producer"></param>
    /// <param name="kafkaTopicName"></param>
    /// <returns></returns>
    public static async Task AssociateProjectGeofence(AssociateProjectGeofence projectGeofence,
      IProjectRepository projectRepo,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      IKafka producer, string kafkaTopicName)
    {
      projectGeofence.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectRepo.StoreEvent(projectGeofence).ConfigureAwait(false);
      if (isUpdated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 65);

      var messagePayload = JsonConvert.SerializeObject(new { AssociateProjectGeofence = projectGeofence });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(projectGeofence.ProjectUID.ToString(), messagePayload)
        });
    }
  }
}
