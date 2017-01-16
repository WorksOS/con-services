using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using KafkaConsumer;
using VSS.Project.Service.Repositories;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Geofence.Data.Models;
using System;

namespace VSS.Geofence.Data
{
  public class GeofenceRepository : RepositoryBase, IRepository<IGeofenceEvent>
  {
    // private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public GeofenceRepository(IConfigurationStore _connectionString) : base(_connectionString)
    {
    }

    public async Task<int> StoreEvent(IGeofenceEvent evt)
    {
      var upsertedCount = 0;
      
      
      // since this is a masterDataService (not landfill specific but will be used for compaction and potentially other apps), 
      //  lets just store all geofence types
      GeofenceType geofenceType = GetGeofenceType(evt);

      //if (geofenceType == GeofenceType.Project || geofenceType == GeofenceType.Landfill || evt is DeleteGeofenceEvent)
      {
        var geofence = new Models.Geofence();
        string eventType = "Unknown";
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
          var geofenceEvent = (DeleteGeofenceEvent) evt;
          geofence.GeofenceUID = geofenceEvent.GeofenceUID.ToString();
          geofence.LastActionedUTC = geofenceEvent.ActionUTC;
          eventType = "DeleteGeofenceEvent";
        }

        upsertedCount = await UpsertGeofenceDetail(geofence, eventType);
      }
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
    private async Task<int> UpsertGeofenceDetail(Models.Geofence geofence, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      // Log.DebugFormat("GeofenceRepository: Upserting eventType={0} geofenceUid={1}", eventType, geofence.GeofenceUID);

      var existing = Connection.Query<Models.Geofence>
        (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC   
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid", 
            new { geofenceUid = geofence.GeofenceUID }).FirstOrDefault();

      if (eventType == "CreateGeofenceEvent")
      {
        upsertedCount = CreateGeofence(geofence, existing);
      }

      if (eventType == "UpdateGeofenceEvent")
      {
        upsertedCount = UpdateGeofence(geofence, existing);
      }

      if (eventType == "DeleteGeofenceEvent")
      {
        upsertedCount = DeleteGeofence(geofence, existing);
      }

      //Log.DebugFormat("GeofenceRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int CreateGeofence(Models.Geofence geofence, Models.Geofence existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";
        return Connection.Execute(insert, geofence);
      }

      // Log.DebugFormat("GeofenceRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", geofence.LastActionedUTC);

      return UpdateGeofence(geofence, existing);
    }

    private int DeleteGeofence(Models.Geofence geofence, Models.Geofence existing)
    {
      if (existing != null)
      {
        if (geofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string update =
            @"UPDATE Geofence                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC
                WHERE GeofenceUID = @GeofenceUID";
          return Connection.Execute(update, geofence);
        }
        //else
        //{
        //  Log.DebugFormat("GeofenceRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
        //    existing.LastActionedUTC, geofence.LastActionedUTC);
        //}
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

        const string insert =
            @"INSERT Geofence
                (GeofenceUID, Name, Description, GeometryWKT, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @Description, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType)";
        return Connection.Execute(insert, geofence);
        //  Log.DebugFormat("GeofenceRepository: can't delete as none existing newActionedUTC={0}",
        //    geofence.LastActionedUTC);
      }
      return 0;
    }

    private int UpdateGeofence(Models.Geofence geofence, Models.Geofence existing)
    {
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
          const string update =
            @"UPDATE Geofence                
                SET Name = @Name, FillColor = @FillColor, IsTransparent = @IsTransparent, LastActionedUTC = @LastActionedUTC                  
              WHERE GeofenceUID = @GeofenceUID";
          return Connection.Execute(update, geofence);
        }
        //else
        //{
          //Log.DebugFormat("GeofenceRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
          //  existing.LastActionedUTC, geofence.LastActionedUTC);
        //}
      }
      //else
      //{
        //Log.DebugFormat("GeofenceRepository: update received before Create. Add what we canng newActionedUTC={0}",
        //  geofence.LastActionedUTC);
      //}
      return 0;
    }

    /// <summary>
    /// Returns all geofences for the Customer
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Geofence>> GetProjectGeofences(string customerUid)
    {
      await PerhapsOpenConnection();

      var projectGeofences = Connection.Query<Models.Geofence>
         (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC
              FROM Geofence 
              WHERE fk_CustomerUID = @customerUid AND IsDeleted = 0",
          new { customerUid }
         );

      PerhapsCloseConnection();

      //Log.DebugFormat("GeofenceRepository: Found {0} Project geofences for customer {1}", projectGeofences.Count(), customerUid);

      return projectGeofences;
    }

    /// <summary>
    /// Returns all geofences for the Customer
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Geofence>> GetProjectGeofencesByProjectUID(string projectUid)
    {
      await PerhapsOpenConnection();

      var projectGeofences = Connection.Query<Models.Geofence>
         (@"SELECT 
                GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                g.LastActionedUTC
              FROM Geofence g
                JOIN ProjectGeofence pg ON pg.fk_GeofenceUID = g.GeofenceUID 
              WHERE fk_ProjectUID = @projectUid AND IsDeleted = 0",
          new {projectUid }
         );

      PerhapsCloseConnection();

      //Log.DebugFormat("GeofenceRepository: Found {0} Project geofences for customer {1}", projectGeofences.Count(), customerUid);

      return projectGeofences;
    }

    public async Task<Models.Geofence> GetGeofence_UnitTest(string geofenceUid)
    {
      await PerhapsOpenConnection();
      
      var geofence = Connection.Query<Models.Geofence>
          (@"SELECT 
               GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, GeometryWKT, FillColor, IsTransparent,
                IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID,
                LastActionedUTC
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid"
          , new { geofenceUid }
        ).FirstOrDefault(); 

      PerhapsCloseConnection();

      return geofence;
    }

  }
}
