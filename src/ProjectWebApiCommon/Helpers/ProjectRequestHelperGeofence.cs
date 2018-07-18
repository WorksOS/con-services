using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Proxies.Interfaces;
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
    public static void ValidateGeofence(string projectBoundary, IServiceExceptionHandler serviceExceptionHandler)
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
    /// Creates a geofence from the projects boundary
    /// </summary>
    /// <param name="name"></param>
    /// <param name="projectBoundary"></param>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="customerUid"></param>
    /// <param name="customHeaders"></param>
    /// <param name="geofenceProxy"></param>
    /// <returns></returns>
    public static async Task<Guid> CreateGeofenceInGeofenceService(string projectUid, string name,
      string projectBoundary, string customerUid, string userId,
      IHttpContextAccessor httpContextAccessor, ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      IDictionary<string, string> customHeaders, IGeofenceProxy geofenceProxy)
    {
      // This is a temporary work-around of UserAuthorization issue with external applications.
      //     GeofenceService contains UserAuthorization which will fail for TBC, which uses the v2 API
      if (httpContextAccessor != null && httpContextAccessor.HttpContext.Request.Path.Value.Contains("api/v2/projects"))
      {
        log.LogWarning(
          $"Skip creating a geofence for project: {name}, as request has come from the TBC endpoint: {httpContextAccessor.HttpContext.Request.Path.Value}.");
        return Guid.Empty;
      }

      var area = GeofenceValidation.CalculateAreaSqMeters(projectBoundary);
      log.LogDebug($"Creating a geofence for project: {name} customer: {customerUid} userId: {userId} Area: {area}");

      Guid geofenceUidCreated = await geofenceProxy.CreateGeofence(Guid.Parse(customerUid), name, "", "Project",
        projectBoundary, 0, true, Guid.Parse(userId), area, customHeaders).ConfigureAwait(false);

      log.LogDebug($"CreatingGeofence: Has geofenceSvc created the geofence? geofenceUidCreated: {geofenceUidCreated}");
      return geofenceUidCreated;
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

    /// <summary>
    /// Dissociates the geofence from the project.
    /// </summary>
    /// <param name="projectGeofence">The association.</param>
    /// <param name="log"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="projectRepo"></param>
    /// <param name="producer"></param>
    /// <param name="kafkaTopicName"></param>
    /// <returns></returns>
    public static async Task DissociateProjectGeofence(DissociateProjectGeofence projectGeofence,
      IProjectRepository projectRepo,
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      IKafka producer, string kafkaTopicName)
    {
      projectGeofence.ReceivedUTC = DateTime.UtcNow;

      var isUpdated = await projectRepo.StoreEvent(projectGeofence).ConfigureAwait(false);
      if (isUpdated == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 107);

      var messagePayload = JsonConvert.SerializeObject(new { DissociateProjectGeofence = projectGeofence });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>()
        {
          new KeyValuePair<string, string>(projectGeofence.ProjectUID.ToString(), messagePayload)
        });
    }
  }
}
