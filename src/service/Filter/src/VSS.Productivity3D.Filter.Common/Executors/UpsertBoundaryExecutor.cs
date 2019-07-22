﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
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
      IProjectProxy projectProxy, IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IFileImportProxy fileImportProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, raptorProxy, assetResolverProxy, fileImportProxy, repository, producer, kafkaTopicName, auxRepository, null, null)
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
      var request = CastRequestObjectTo<BoundaryRequestFull>(item, 54);

      // if BoundaryUid supplied, then exception as cannot update a boundary (for now at least)
      if (!string.IsNullOrEmpty(request.Request.BoundaryUid))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 61);
      }

      //Create the geofence
      request.Request.BoundaryUid = Guid.NewGuid().ToString();

      var createBoundaryEvent = AutoMapperUtility.Automapper.Map<CreateGeofenceEvent>(request);
      createBoundaryEvent.ActionUTC = DateTime.UtcNow;
      var createdCount = 0;
      try
      {
        createdCount = await ((IGeofenceRepository)Repository).StoreEvent(createBoundaryEvent).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 57, e.Message);
      }

      if (createdCount == 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 58);
      }

      if (request.SendKafkaMessages)
      {
        try
        {
          var payload = JsonConvert.SerializeObject(new { CreateBoundaryEvent = createBoundaryEvent });

          producer.Send(kafkaTopicName,
            new List<KeyValuePair<string, string>>
            {
              new KeyValuePair<string, string>(createBoundaryEvent.GeofenceUID.ToString(), payload)
            });
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 56, e.Message);
        }
      }

      //and associate it with the project

      //TODO
      //The code check for association below is not needed until we can update a boundary

      //Boundary may be used in many filters but only create association between boundary and project once.
      var retrievedAssociation = (await ((IProjectRepository)auxRepository)
          .GetAssociatedGeofences(request.ProjectUid)
          .ConfigureAwait(false))
        .SingleOrDefault(a => a.GeofenceUID.ToString() == request.Request.BoundaryUid);

      if (retrievedAssociation == null)
      {
        var associateProjectGeofence =
          AutoMapperUtility.Automapper.Map<AssociateProjectGeofence>(
            ProjectGeofenceRequest.Create(request.ProjectUid, request.Request.BoundaryUid));
        associateProjectGeofence.ActionUTC = DateTime.UtcNow;

        var associatedCount = await ((IProjectRepository)auxRepository).StoreEvent(associateProjectGeofence).ConfigureAwait(false);
        if (associatedCount == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 55);
        }

        if (request.SendKafkaMessages)
        {
          try
          {
            var payload = JsonConvert.SerializeObject(new { AssociateProjectBoundaryEvent = associateProjectGeofence });

            producer.Send(kafkaTopicName,
              new List<KeyValuePair<string, string>>
              {
              new KeyValuePair<string, string>(associateProjectGeofence.GeofenceUID.ToString(), payload)
              });
          }
          catch (Exception e)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 56, e.Message);
          }
        }
      }

      var retrievedBoundary = await ((IGeofenceRepository)Repository)
        .GetGeofence(request.Request.BoundaryUid)
        .ConfigureAwait(false);

      return new GeofenceDataSingleResult(AutoMapperUtility.Automapper.Map<GeofenceData>(retrievedBoundary));
    }
  }
}
