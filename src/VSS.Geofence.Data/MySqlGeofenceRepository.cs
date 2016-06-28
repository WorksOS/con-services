using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using VSS.Geofence.Data.Interfaces;
using VSS.Geofence.Data.Models;
using VSS.MasterData.Common.Helpers;
using VSS.MasterData.Common.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Geofence.Data
{
  public class MySqlGeofenceRepository : RepositoryBase, IGeofenceService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
   
    public int StoreGeofence(IGeofenceEvent evt)
    {
      var upsertedCount = 0;
      //Only save Project and Landfill type geofences
      GeofenceType geofenceType = GetGeofenceType(evt);
      if (geofenceType == GeofenceType.Project || geofenceType == GeofenceType.Landfill || evt is DeleteGeofenceEvent)
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
          geofence.CustomerUID = geofenceEvent.CustomerUID.ToString();
          geofence.LastActionedUTC = geofenceEvent.ActionUTC;
          eventType = "CreateGeofenceEvent";
        }
        else if (evt is UpdateGeofenceEvent)
        {
          var geofenceEvent = (UpdateGeofenceEvent) evt;
          geofence.GeofenceUID = geofenceEvent.GeofenceUID.ToString();//Select existing with this
          geofence.Name = geofenceEvent.GeofenceName;
          //cannot update the following in update event:
          //GeofenceType, GeometryWKT
          geofence.FillColor = geofenceEvent.FillColor;
          geofence.IsTransparent = geofenceEvent.IsTransparent;
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

        upsertedCount = UpsertGeofenceDetail(geofence, eventType);
      }
      return upsertedCount;
    }

    public GeofenceType GetGeofenceType(IGeofenceEvent evt)
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
    private int UpsertGeofenceDetail(Models.Geofence geofence, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("GeofenceRepository: Upserting eventType={0} geofenceUid={1}", eventType, geofence.GeofenceUID);

      var existing = Connection.Query<Models.Geofence>
        (@"SELECT 
                GeofenceUID, Name, CustomerUID, ProjectUID, GeometryWKT, FillColor, IsTransparent,
                LastActionedUTC, fk_GeofenceTypeID AS GeofenceType, IsDeleted
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid", new { geofenceUid = geofence.GeofenceUID }).FirstOrDefault();

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

      Log.DebugFormat("GeofenceRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int CreateGeofence(Models.Geofence geofence, Models.Geofence existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Geofence
                (GeofenceUID, Name, GeometryWKT, FillColor, IsTransparent, IsDeleted, CustomerUID, ProjectUID, LastActionedUTC, fk_GeofenceTypeID)
            VALUES
                (@GeofenceUID, @Name, @GeometryWKT, @FillColor, IsTransparent, @IsDeleted, @CustomerUID, @ProjectUID, @LastActionedUTC, @GeofenceType)";
        return Connection.Execute(insert, geofence);
      }

      Log.DebugFormat("GeofenceRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", geofence.LastActionedUTC);

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
        else
        {
          Log.DebugFormat("GeofenceRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
            existing.LastActionedUTC, geofence.LastActionedUTC);
        }
      }
      else
      {
        Log.DebugFormat("GeofenceRepository: can't delete as none existing newActionedUTC={0}",
          geofence.LastActionedUTC);
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
        else
        {
          Log.DebugFormat("GeofenceRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
            existing.LastActionedUTC, geofence.LastActionedUTC);
        }
      }
      else
      {
        Log.DebugFormat("GeofenceRepository: can't update as none existing newActionedUTC={0}",
          geofence.LastActionedUTC);
      }
      return 0;
    }

    #region Link Geofences to Projects

    public void AssignApplicableLandfillGeofencesToProject(string projectGeometry, string customerUid, string projectUid)
    {
      //Check for unassigned Landfill geofences and see if they belong to this project
      var unassignedGeofences = GetUnassignedLandfillGeofences(customerUid);
      foreach (var unassignedGeofence in unassignedGeofences)
      {
        if (Geometry.GeofencesOverlap(projectGeometry, unassignedGeofence.GeometryWKT))
        {
          AssignGeofenceToProject(unassignedGeofence.GeofenceUID, projectUid);
        }
      }      
    }

    public IEnumerable<Models.Geofence> GetProjectGeofences(string customerUid)
    {
      PerhapsOpenConnection();

      var projectGeofences = Connection.Query<Models.Geofence>
         (@"SELECT GeofenceUID, Name, CustomerUID, GeometryWKT, ProjectUID
            FROM Geofence 
            WHERE CustomerUID = @customerUid AND IsDeleted = 0 AND fk_GeofenceTypeID = 1",//Project type
          new { customerUid }
         );

      PerhapsCloseConnection();

      Log.DebugFormat("GeofenceRepository: Found {0} Project geofences for customer {1}", projectGeofences.Count(), customerUid);

      return projectGeofences;
    }

    //Public so can do unit test
    public IEnumerable<Models.Geofence> GetUnassignedLandfillGeofences(string customerUid)
    {
      PerhapsOpenConnection();

      var landfillGeofences = Connection.Query<Models.Geofence>
         (@"SELECT GeofenceUID, Name, GeometryWKT
            FROM Geofence 
            WHERE CustomerUID = @customerUid AND ProjectUID IS NULL AND IsDeleted = 0 AND fk_GeofenceTypeID = 10",//Landfill type
          new { customerUid }
         );

      PerhapsCloseConnection();

      Log.DebugFormat("GeofenceRepository: Found {0} Landfill geofences for customer {1}", landfillGeofences.Count(), customerUid);

      return landfillGeofences;
    }

    public int AssignGeofenceToProject(string geofenceUid, string projectUid)
    {
      Log.DebugFormat("GeofenceRepository: Assigning geofence {0} to project {1}", geofenceUid, projectUid);

      PerhapsOpenConnection();

      const string update =
              @"UPDATE Geofence                
                SET ProjectUID = @projectUid                  
              WHERE GeofenceUID = @geofenceUid";
      int rowsUpdated = Connection.Execute(update, new { projectUid, geofenceUid });
      PerhapsCloseConnection();
      return rowsUpdated;
    }

    #endregion


    //for unit tests
    public Models.Geofence GetGeofence(string geofenceUid)
    {
      PerhapsOpenConnection();
      
      var geofence = Connection.Query<Models.Geofence>
          (@"SELECT 
               GeofenceUID, Name, CustomerUID, ProjectUID, GeometryWKT, FillColor, IsTransparent,
                LastActionedUTC, fk_GeofenceTypeID AS GeofenceType, IsDeleted
              FROM Geofence
              WHERE GeofenceUID = @geofenceUid"
          , new { geofenceUid }
        ).FirstOrDefault(); 

      PerhapsCloseConnection();

      return geofence;
    }

  }
}
