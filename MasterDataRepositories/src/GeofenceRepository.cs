using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Productivity3D.Repo;
using VSS.Productivity3D.Repo.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Repositories
{
  public class GeofenceRepository : RepositoryBase, IRepository<IGeofenceEvent>
  {
    private readonly ILogger log;

    public GeofenceRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(
      _connectionString, logger)
    {
      log = logger.CreateLogger<GeofenceRepository>();
    }

    #region store

    public async Task<int> StoreEvent(IGeofenceEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        log.LogWarning("Unsupported geofence event type");
        return 0;
      }


      // since this is a masterDataService (not landfill specific but will be used for compaction and potentially other apps), 
      //  lets just store all geofence types
      var geofenceType = GetGeofenceType(evt);

      var geofence = new Geofence();
      var eventType = "Unknown";
      log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateGeofenceEvent)
      {
        var geofenceEvent = (CreateGeofenceEvent) evt;
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
        var geofenceEvent = (UpdateGeofenceEvent) evt;
        geofence.GeofenceUID = geofenceEvent.GeofenceUID.ToString(); //Select existing with this
        geofence.Name = geofenceEvent.GeofenceName;
        //cannot update GeometryWKT in update event,
        //  may be used to create geofence if it doesn't exist 
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
        var geofenceEvent = (DeleteGeofenceEvent) evt;
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
        geofenceType = (evt as CreateGeofenceEvent).GeofenceType;
      if (evt is UpdateGeofenceEvent)
        geofenceType = (evt as UpdateGeofenceEvent).GeofenceType;
      return string.IsNullOrEmpty(geofenceType)
        ? GeofenceType.Generic
        : (GeofenceType) Enum.Parse(typeof(GeofenceType), geofenceType, true);
    }


    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="geofence"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertGeofenceDetail(Geofence geofence, string eventType)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<Geofence>
      (@"SELECT 
              GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
              IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
              LastActionedUTC   
            FROM Geofence
            WHERE GeofenceUID = @geofenceUid",
        new {geofenceUid = geofence.GeofenceUID}
      )).FirstOrDefault();

      if (eventType == "CreateGeofenceEvent")
        upsertedCount = await CreateGeofence(geofence, existing);

      if (eventType == "UpdateGeofenceEvent")
        upsertedCount = await UpdateGeofence(geofence, existing);

      if (eventType == "DeleteGeofenceEvent")
        upsertedCount = await DeleteGeofence(geofence, existing);

      return upsertedCount;
    }

    private async Task<int> CreateGeofence(Geofence geofence, Geofence existing)
    {
      log.LogDebug($"GeofenceRepository/CreateGeofence: geofence={JsonConvert.SerializeObject(geofence)}");
      var upsertedCount = 0;

      if (existing == null)
      {
        log.LogDebug($"GeofenceRepository/CreateGeofence: going to create geofence={geofence.GeofenceUID}");

        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, geofence);
        log.LogDebug($"GeofenceRepository/CreateGeofence upserted {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");
        return upsertedCount;
      }

      log.LogDebug($"GeofenceRepository/CreateGeofence: can't create as already exists geofence={geofence.GeofenceUID}, going to update it");
      return await UpdateGeofence(geofence, existing);
    }

    private async Task<int> UpdateGeofence(Geofence geofence, Geofence existing)
    {
      log.LogDebug($"GeofenceRepository/UpdateGeofence: geofence={JsonConvert.SerializeObject(geofence)}");
      var upsertedCount = 0;

      if (existing != null)
      {
        if (!geofence.IsTransparent.HasValue)
          geofence.IsTransparent = existing.IsTransparent;
        if (!geofence.FillColor.HasValue)
          geofence.FillColor = existing.FillColor;
        if (string.IsNullOrEmpty(geofence.Name))
          geofence.Name = existing.Name;
        if (string.IsNullOrEmpty(geofence.CustomerUID) && !string.IsNullOrEmpty(existing.CustomerUID))
          geofence.CustomerUID = existing.CustomerUID;
        // can't change TO generic
        if (geofence.GeofenceType == GeofenceType.Generic)
          geofence.GeofenceType = existing.GeofenceType;
        Guid gotUpdatedGuid = Guid.Empty;
        if (!Guid.TryParse(geofence.UserUID, out gotUpdatedGuid) || (gotUpdatedGuid == Guid.Empty))
          geofence.UserUID = existing.UserUID;

        if (geofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug($"GeofenceRepository/UpdateGeofence: going to update geofence={geofence.GeofenceUID}");

          const string update =
            @"UPDATE Geofence                
                  SET Name = @Name, fk_GeofenceTypeID = @GeofenceType, GeometryWKT = @GeometryWKT, FillColor = @FillColor, IsTransparent = @IsTransparent, fk_CustomerUID = @CustomerUID, Description = @Description, LastActionedUTC = @LastActionedUTC                  
                WHERE GeofenceUID = @GeofenceUID";

          upsertedCount = await ExecuteWithAsyncPolicy(update, geofence);
          log.LogDebug($"UpdateGeofence (update): upserted {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");
          return upsertedCount;
        }
        log.LogDebug($"GeofenceRepository/UpdateGeofence: old update event ignored geofence={geofence.GeofenceUID}");
      }
      else
      {
        log.LogDebug($"GeofenceRepository/UpdateGeofence: update received before the Create. Creating geofence={geofence.GeofenceUID}");
        // need to do this so that if the Create comes later,
        //  the fact that this is updated, is not lost
        geofence.Name = "";
        geofence.GeofenceType = GeofenceType.Generic;
        geofence.GeometryWKT = "";
        geofence.FillColor = 0;
        geofence.IsDeleted = true;
        geofence.Description = "";
        geofence.CustomerUID = "";
        geofence.UserUID = "";

        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, geofence);
        log.LogDebug($"GeofenceRepository/CreateGeofence upserted {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");
        return upsertedCount;
      }

      return upsertedCount;
    }



    private async Task<int> DeleteGeofence(Geofence geofence, Geofence existing)
    {
      log.LogDebug($"GeofenceRepository/DeleteGeofence: going to update geofence={JsonConvert.SerializeObject(geofence)}");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (geofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug($"GeofenceRepository/DeleteGeofence: going to update geofence={geofence.GeofenceUID}");

          const string update =
            @"UPDATE Geofence                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC
                WHERE GeofenceUID = @GeofenceUID";

          upsertedCount = await ExecuteWithAsyncPolicy(update, geofence);
          log.LogDebug($"GeofenceRepository/DeleteGeofence: (update): upserted {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");
          return upsertedCount;
        }
        log.LogDebug($"GeofenceRepository/DeleteGeofence: old delete event ignored geofence={geofence.GeofenceUID}");
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

        log.LogDebug($"GeofenceRepository/DeleteGeofence: going to insert a deleted dummy geofence={geofence.GeofenceUID}");

        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";
        upsertedCount = await ExecuteWithAsyncPolicy(insert, geofence);
        log.LogDebug($"DeleteGeofence (insert): upserted {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");
        return upsertedCount;
      }
      return upsertedCount;
    }

    #endregion store

    #region getters

    /// <summary>
    ///     Returns all geofences for the Customer
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Geofence>> GetCustomerGeofences(string customerUid)
    {
      return await QueryWithAsyncPolicy<Geofence>
      (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC
              FROM Geofence 
              WHERE fk_CustomerUID = @customerUid AND IsDeleted = 0",
        new {customerUid}
      );
    }

    // obsolete as the project boundary is now included in the project object
    ///// <summary>
    /////     Returns all project geofences for the Project - 
    ///// </summary>
    ///// <returns></returns>
    //public async Task<IEnumerable<Geofence>> GetProjectGeofencesByProjectUID(string projectUid)
    //{
    //  var projectGeofences = await QueryWithAsyncPolicy<Geofence>
    //  (@"SELECT 
    //            GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
    //            IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
    //            g.LastActionedUTC
    //          FROM Geofence g
    //            JOIN ProjectGeofence pg ON pg.fk_GeofenceUID = g.GeofenceUID 
    //          WHERE fk_ProjectUID = @projectUid AND IsDeleted = 0",
    //    new {projectUid}
    //  );


    //  return projectGeofences;
    //}

    public async Task<Geofence> GetGeofence_UnitTest(string geofenceUid)
    {
      return (await QueryWithAsyncPolicy<Geofence>
      (@"SELECT 
               GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid"
        , new {geofenceUid}
      )).FirstOrDefault();
    }

    #endregion getters
  }
}