using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class UpsertBoundaryExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient.
    /// </summary>
    public UpsertBoundaryExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler,
        projectListProxy, raptorProxy,
        repository, producer, kafkaTopicName, auxRepository)
    { }

    /// <summary>
    /// Parameterless constructor is required for the <see cref="RequestExecutorContainer"/> factory.
    /// </summary>
    public UpsertBoundaryExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Asynchronously processes the upsert Request.
    /// </summary>
    /// <typeparam Name="T">The type of <see cref="BoundaryRequest"/> to be created.</typeparam>
    /// <param name="item">The instance of <see cref="BoundaryRequest"/> to be created.</param>
    /// <returns>Returns a BoundarysResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var boundaryRequest = item as BoundaryRequestFull;
      if (boundaryRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 54);
      }

    //  return await ProcessRequest(request).ConfigureAwait(false);
    //}

    //private async Task<GeofenceDataSingleResult> ProcessRequest(BoundaryRequestFull boundaryRequest)
    //{
      // if BoundaryUid supplied, then exception as cannot update a boundary (for now at least)
      if (!string.IsNullOrEmpty(boundaryRequest.Request.BoundaryUid))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 61);
      }

      //Create the geofence
      boundaryRequest.Request.BoundaryUid = Guid.NewGuid().ToString();

      var createBoundaryEvent = AutoMapperUtility.Automapper.Map<CreateGeofenceEvent>(boundaryRequest);
      createBoundaryEvent.ActionUTC = DateTime.UtcNow;
      var createdCount = 0;
      try
      {
        log.LogDebug($"Jeannie before StoreEvent to geofenceRepo");
        createdCount = await ((IGeofenceRepository) Repository).StoreEvent(createBoundaryEvent).ConfigureAwait(false);
        log.LogDebug($"Jeannie after StoreEvent to geofenceRepo");
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, e.Message);
      }

      if (createdCount == 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 58);
      }

      try
      {
        var payload = JsonConvert.SerializeObject(new { CreateBoundaryEvent = createBoundaryEvent });

        log.LogDebug($"Jeannie before kafka CreateBoundaryEvent");
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
              new KeyValuePair<string, string>(createBoundaryEvent.GeofenceUID.ToString(), payload)
          });
        log.LogDebug($"Jeannie after kafka CreateBoundaryEvent");
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 56, e.Message);
      }

      //and associate it with the project

      //TODO
      //The code check for association below is not needed until we can update a boundary

      //Boundary may be used in many filters but only create association between boundary and project once.
      var retrievedAssociation = (await ((IProjectRepository) auxRepository)
          .GetAssociatedGeofences(boundaryRequest.ProjectUid)
          .ConfigureAwait(false))
        .SingleOrDefault(a => a.GeofenceUID.ToString() == boundaryRequest.Request.BoundaryUid);

      if (retrievedAssociation == null)
      {
        var associateProjectGeofence =
          AutoMapperUtility.Automapper.Map<AssociateProjectGeofence>(
            ProjectGeofenceRequest.Create(boundaryRequest.ProjectUid, boundaryRequest.Request.BoundaryUid));
        associateProjectGeofence.ActionUTC = DateTime.UtcNow;

        var associatedCount = await ((IProjectRepository)auxRepository).StoreEvent(associateProjectGeofence).ConfigureAwait(false);
        if (associatedCount == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 55);
        }

        try
        {
          var payload = JsonConvert.SerializeObject(new { CreateBoundaryEvent = associateProjectGeofence });

          // todo why is this a CreateBoundaryEvent rather than AssociateProjectGeofence?
          log.LogDebug($"Jeannie before kafka AssociateProjectGeofence???");
          producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>
            {
              new KeyValuePair<string, string>(associateProjectGeofence.GeofenceUID.ToString(), payload)
            });
          log.LogDebug($"Jeannie after kafka AssociateProjectGeofence???");
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 56, e.Message);
        }
      }

      log.LogDebug($"Jeannie before geofenceRepo.GetGeofence");
      var retrievedBoundary = await ((IGeofenceRepository)Repository)
        .GetGeofence(boundaryRequest.Request.BoundaryUid)
        .ConfigureAwait(false);
      log.LogDebug($"Jeannie after geofenceRepo.GetGeofence");

      return new GeofenceDataSingleResult(AutoMapperUtility.Automapper.Map<GeofenceData>(retrievedBoundary));
    }
  }
}
