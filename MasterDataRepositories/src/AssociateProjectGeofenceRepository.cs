using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public class AssociateProjectGeofenceRepository : RepositoryBase, IRepository<IAssociateProjectGeofenceEvent>,
    IAssociateProjectGeofenceRepository
  {
    private const string RepoName = "AssociateProjectGeofenceRepository";
    private readonly string _typeName = typeof(AssociateProjectGeofence).Name;

    public AssociateProjectGeofenceRepository(IConfigurationStore connectionString, ILoggerFactory logger) : base(
      connectionString, logger)
    {
      log = logger.CreateLogger<GeofenceRepository>();
    }

    public async Task<int> StoreEvent(IAssociateProjectGeofenceEvent evt)
    {
      int upsertedCount;
      if (evt == null)
      {
        log.LogWarning($"Unsupported {_typeName} event type");
        return 0;
      }

      var geofence = new AssociateProjectGeofence();
      const string eventType = "Unknown";
      log.LogDebug($"Event type is {evt.GetType()}");

      if (evt is CreateAssociateProjectGeofenceEvent)
      {
        var geofenceEvent = (CreateAssociateProjectGeofenceEvent)evt;
        geofence.GeofenceUID = geofenceEvent.GeofenceUID;
        geofence.ProjectUID = geofenceEvent.ProjectUID;
        geofence.ActionUTC = geofenceEvent.LastActionedUTC;
      }

      upsertedCount = await UpsertAssociateProjectGeofence(geofence, eventType);
      return upsertedCount;
    }

    public Task<AssociateProjectGeofence> GetAssociateProjectGeofence(string geofenceUid)
    {
      throw new NotImplementedException();
    }
    
    public async Task<IEnumerable<AssociateProjectGeofence>> GetAssociatedProjectGeofences(string projectUid)
    {
      return await QueryWithAsyncPolicy<AssociateProjectGeofence>
      (@"SELECT 
                GeofenceUID, ProjectUid, ActionUTC
              FROM AssociateProjectGeofence 
              WHERE fk_ProjectUID = @projectUid",
        new { projectUid }
      );
    }
    
    private async Task<int> UpsertAssociateProjectGeofence(AssociateProjectGeofence projectGeofence, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<AssociateProjectGeofence>
      (@"SELECT 
              fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, ActionUTC
            FROM AssociateProjectGeofence
            WHERE fk_ProjectUID = @projectUID AND fk_GeofenceUID = @geofenceUID",
        new { projectUID = projectGeofence.ProjectUID, geofenceUID = projectGeofence.GeofenceUID }
      )).FirstOrDefault();

      if (eventType == "CreateAssociateProjectGeofenceEvent")
      {
        upsertedCount = await CreateAssociateProjectGeofence(projectGeofence, existing);
      }

      return upsertedCount;
    }

    private async Task<int> CreateAssociateProjectGeofence(AssociateProjectGeofence associateProjectGeofence, AssociateProjectGeofence existing)
    {
      log.LogDebug($"{RepoName}/CreateGeofence: {_typeName}={JsonConvert.SerializeObject(associateProjectGeofence)}");

      if (existing == null)
      {
        log.LogDebug($"{RepoName}/CreateGeofence: going to create {_typeName}={associateProjectGeofence.GeofenceUID}");

        const string insert =
          @"INSERT AssociateProjectGeofence
                (GeofenceUID, ProjectUID, ActionUTC)
            VALUES
                (@GeofenceUID, @ProjectUID, @ActionUTC)";

        var upsertedCount = await ExecuteWithAsyncPolicy(insert, associateProjectGeofence);
        log.LogDebug($"{RepoName}/Creaet{_typeName} upserted {upsertedCount} rows for: geofenceUid:{associateProjectGeofence.GeofenceUID}");
        return upsertedCount;
      }

      log.LogDebug($"{RepoName}/Create{_typeName}: can't create as already exists {_typeName}={associateProjectGeofence.GeofenceUID}, going to update it");

      return await UpdateAssociateProjectGeofence(associateProjectGeofence, existing);
    }

    private async Task<int> UpdateAssociateProjectGeofence(AssociateProjectGeofence associateProjectGeofence, AssociateProjectGeofence existing)
    {
      log.LogDebug($"{RepoName}/UpdateGeofence: {_typeName}={JsonConvert.SerializeObject(associateProjectGeofence)}");
      var upsertedCount = 0;

      if (existing != null)
      {
        if (associateProjectGeofence.ActionUTC >= existing.ActionUTC)
        {
          log.LogDebug($"{RepoName}/Update{_typeName}: going to update {_typeName}={associateProjectGeofence.GeofenceUID}");

          const string update =
            @"UPDATE AssociateProjectGeofence                
                  SET GeofenceTypeID = @GeofenceType, ProjectUID = @ProjectUID, ActionUTC = @ActionUTC
                WHERE GeofenceUID = @GeofenceUID";

          upsertedCount = await ExecuteWithAsyncPolicy(update, associateProjectGeofence);
          log.LogDebug($"Update{_typeName} (update): upserted {upsertedCount} rows for: geofenceUid:{associateProjectGeofence.GeofenceUID}");

          return upsertedCount;
        }

        log.LogDebug($"{RepoName}/Update{_typeName}: old update event ignored {_typeName}={associateProjectGeofence.GeofenceUID}");
      }
      else
      {
        log.LogDebug($"{RepoName}/Update{_typeName}: update received before the Create. Creating {_typeName}={associateProjectGeofence.GeofenceUID}");

        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, associateProjectGeofence);
        log.LogDebug($"{RepoName}/Create{_typeName} upserted {upsertedCount} rows for: geofenceUid:{associateProjectGeofence.GeofenceUID}");


        return upsertedCount;
      }

      return upsertedCount;
    }
  }
}