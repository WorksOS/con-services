using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using Repositories.DBModels;

namespace Repositories
{
  public class GeofenceRepository : RepositoryBase, IRepository<IGeofenceEvent>
  {
    private readonly ILogger log;

    public GeofenceRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString, logger)
    {
      log = logger.CreateLogger<GeofenceRepository>();
    }

    public async Task<int> StoreEvent(IGeofenceEvent evt)
    {
      var upsertedCount = 0;


      // since this is a masterDataService (not landfill specific but will be used for compaction and potentially other apps), 
      //  lets just store all geofence types
      GeofenceType geofenceType = GetGeofenceType(evt);

      var geofence = new Geofence();
      string eventType = "Unknown";
      if (evt is CreateGeofenceEvent)
      {
        var geofenceEvent = (CreateGeofenceEvent)evt;
        geofence.GeofenceUID = geofenceEvent.GeofenceUID.ToString();
        geofence.Name = geofenceEvent.GeofenceName;
        geofence.GeofenceType = geofenceType;
        geofence.GeometryWKT = geofenceEvent.GeometryWKT;
        geofence.FillColor = geofenceEvent.FillColor;
        geofence.IsTransparent = geofenceEvent.IsTransparent;
        geofence.IsDeleted = false;
        geofence.Description = geofenceEvent.Description;
        geofence.CustomerUID = geofenceEvent.CustomerUID.ToString();
        geofence.UserUID = geofenceEvent.UserUID.ToString();
        geofence.LastActionedUTC = geofenceEvent.ActionUTC;
        eventType = "CreateGeofenceEvent";
      }
      else if (evt is UpdateGeofenceEvent)
      {
        var geofenceEvent = (UpdateGeofenceEvent)evt;
        geofence.GeofenceUID = geofenceEvent.GeofenceUID.ToString();//Select existing with this
        geofence.Name = geofenceEvent.GeofenceName;
        //cannot update GeofenceType/GeometryWKT in update event,
        //  use them to initialise the table 
        //  as we update is received before the create
        geofence.GeofenceType = geofenceType;
        geofence.GeometryWKT = geofenceEvent.GeometryWKT;

        geofence.FillColor = geofenceEvent.FillColor;
        geofence.IsTransparent = geofenceEvent.IsTransparent;
        geofence.Description = geofenceEvent.Description;
        geofence.UserUID = geofenceEvent.UserUID.ToString();
        geofence.LastActionedUTC = geofenceEvent.ActionUTC;
        eventType = "UpdateGeofenceEvent";
      }
      else if (evt is DeleteGeofenceEvent)
      {
        var geofenceEvent = (DeleteGeofenceEvent)evt;
        geofence.GeofenceUID = geofenceEvent.GeofenceUID.ToString();
        geofence.LastActionedUTC = geofenceEvent.ActionUTC;
        eventType = "DeleteGeofenceEvent";
      }

      upsertedCount = await UpsertGeofenceDetail(geofence, eventType);
      return upsertedCount;
    }

    private GeofenceType GetGeofenceType(IGeofenceEvent evt)
    {
      string geofenceType = null;
      if (evt is CreateGeofenceEvent)
      {
        geofenceType = (evt as CreateGeofenceEvent).GeofenceType;
      }
      if (evt is UpdateGeofenceEvent)
      {
        geofenceType = (evt as UpdateGeofenceEvent).GeofenceType;
      }
      return string.IsNullOrEmpty(geofenceType) ?
        GeofenceType.Generic : (GeofenceType)Enum.Parse(typeof(GeofenceType), geofenceType, true);
    }


    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    ///    on deletion, a flag will be set.
    /// </summary>
    /// <param name="geofence"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertGeofenceDetail(Geofence geofence, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<Geofence>
        (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC   
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid",
            new { geofenceUid = geofence.GeofenceUID }
            )).FirstOrDefault();

      if (eventType == "CreateGeofenceEvent")
      {
        upsertedCount = await CreateGeofence(geofence, existing);
      }

      if (eventType == "UpdateGeofenceEvent")
      {
        upsertedCount = await UpdateGeofence(geofence, existing);
      }

      if (eventType == "DeleteGeofenceEvent")
      {
        upsertedCount = await DeleteGeofence(geofence, existing);
      }
      
      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> CreateGeofence(Geofence geofence, Geofence existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        log.LogDebug("GeofenceRepository/CreateGeofence: going to create geofence={0}", JsonConvert.SerializeObject(geofence));

        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, geofence);
          log.LogDebug("GeofenceRepository/CreateGeofence upserted {0} rows (1=insert, 2=update) for: geofenceUid:{1}", upsertedCount, geofence.GeofenceUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("GeofenceRepository/CreateGeofence: can't create as already exists geofence={0}", JsonConvert.SerializeObject(geofence));
      return await UpdateGeofence(geofence, existing);
    }

    private async Task<int> DeleteGeofence(Geofence geofence, Geofence existing)
    {
      var upsertedCount = 0;
      if (existing != null)
      {
        if (geofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug("GeofenceRepository/DeleteGeofence: going to update geofence={0}", JsonConvert.SerializeObject(geofence));

          const string update =
            @"UPDATE Geofence                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC
                WHERE GeofenceUID = @GeofenceUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, geofence);
            log.LogDebug("GeofenceRepository/DeleteGeofence: (update): upserted {0} rows (1=insert, 2=update) for: geofenceUid:{1}", upsertedCount, geofence.GeofenceUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("GeofenceRepository/DeleteGeofence: old delete event ignored geofence={0}", JsonConvert.SerializeObject(geofence));
        }
      }
      else
      {
        // need to do this so that if the Create comes later,
        //  the fact that this is deleted, is not lost
        geofence.Name = "";
        geofence.GeofenceType = GeofenceType.Generic;
        geofence.GeometryWKT = "";
        geofence.FillColor = 0;
        geofence.IsDeleted = true;
        geofence.Description = "";
        geofence.CustomerUID = "";
        geofence.UserUID = "";

        log.LogDebug("GeofenceRepository/DeleteGeofence: going to insert a deleted dummy geofence={0}", JsonConvert.SerializeObject(geofence));

        const string insert =
            @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";

        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, geofence);
          log.LogDebug("DeleteGeofence (insert): upserted {0} rows (1=insert, 2=update) for: geofenceUid:{1}", upsertedCount, geofence.GeofenceUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }
      return upsertedCount;
    }

    private async Task<int> UpdateGeofence(Geofence geofence, Geofence existing)
    {
      var upsertedCount = 0;
      if (existing != null)
      {
        if (!geofence.IsTransparent.HasValue)
          geofence.IsTransparent = existing.IsTransparent;
        if (!geofence.FillColor.HasValue)
          geofence.FillColor = existing.FillColor;
        if (string.IsNullOrEmpty(geofence.Name))
          geofence.Name = existing.Name;

        if (geofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug("GeofenceRepository/UpdateGeofence: going to insert geofence={0}", JsonConvert.SerializeObject(geofence));

          const string update =
            @"UPDATE Geofence                
                SET Name = @Name, FillColor = @FillColor, IsTransparent = @IsTransparent, LastActionedUTC = @LastActionedUTC                  
              WHERE GeofenceUID = @GeofenceUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, geofence);
            log.LogDebug("UpdateGeofence (update): upserted {0} rows (1=insert, 2=update) for: geofenceUid:{1}", upsertedCount, geofence.GeofenceUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("GeofenceRepository/UpdateGeofence: old update event ignored geofence={0}", JsonConvert.SerializeObject(geofence));          
        }
      }
      else
      {
        log.LogDebug("GeofenceRepository/UpdateGeofence: update received before Create ignored geofence={0}", JsonConvert.SerializeObject(geofence));
      }
      
      return upsertedCount;
    }

    /// <summary>
    /// Returns all geofences for the Customer
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Geofence>> GetProjectGeofences(string customerUid)
    {
      await PerhapsOpenConnection();

      var projectGeofences = (await Connection.QueryAsync<Geofence>
         (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC
              FROM Geofence 
              WHERE fk_CustomerUID = @customerUid AND IsDeleted = 0",
          new { customerUid }
         ));

      PerhapsCloseConnection();
      return projectGeofences;
    }

    /// <summary>
    /// Returns all geofences for the Customer
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Geofence>> GetProjectGeofencesByProjectUID(string projectUid)
    {
      await PerhapsOpenConnection();

      var projectGeofences = (await Connection.QueryAsync<Geofence>
         (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                g.LastActionedUTC
              FROM Geofence g
                JOIN ProjectGeofence pg ON pg.fk_GeofenceUID = g.GeofenceUID 
              WHERE fk_ProjectUID = @projectUid AND IsDeleted = 0",
          new { projectUid }
         ));

      PerhapsCloseConnection();      
      return projectGeofences;
    }

    public async Task<Geofence> GetGeofence_UnitTest(string geofenceUid)
    {
      await PerhapsOpenConnection();

      var geofence = (await Connection.QueryAsync<Geofence>
          (@"SELECT 
               GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid"
          , new { geofenceUid }
        )).FirstOrDefault();

      PerhapsCloseConnection();

      return geofence;
    }

  }
}
