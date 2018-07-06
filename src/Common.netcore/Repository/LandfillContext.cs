using System;
using System.Collections.Generic;
using System.Linq;
using Common.Models;
using Common.Utilities;
using LandfillService.Common.Models;
using Microsoft.Extensions.Logging.Abstractions;
using MySql.Data.MySqlClient;
using NodaTime;
using VSS.ConfigurationStore;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace Common.Repository
{
  /// <summary>
  ///   Encapsulates DB queries
  /// </summary>
  public class LandfillDb
  {
    private static readonly string connString =
      new GenericConfiguration(new NullLoggerFactory()).GetConnectionString("VSPDB");


    /// <summary>
    ///   Wrapper for generating a MySQL connection
    /// </summary>
    /// <param name="body">Code to execute</param>
    /// <returns>The result of executing body()</returns>
    private static T WithConnection<T>(Func<MySqlConnection, T> body)
    {
      using (var conn = new MySqlConnection(connString))
      {
        conn.Open();
        var res = body(conn);
        conn.Close();
        return res;
      }
    }

    #region (Customer)

    public static List<Customer> GetAssociatedCustomerbyUserUid(Guid userUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT c.* 
            FROM Customer c 
              JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID 
            WHERE cu.UserUID = @userUid";
        var customers = new List<Customer>();

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@userUid", userUid)))
        {
          while (reader.Read())
            customers.Add(new Customer
            {
              CustomerName = reader.GetString(reader.GetOrdinal("Name")),
              CustomerType = (CustomerType) reader.GetInt32(reader.GetOrdinal("fk_CustomerTypeID")),
              CustomerUID = reader.GetString(reader.GetOrdinal("CustomerUID"))
            });
        }

        return customers;
      });
    }

    public static Customer GetCustomer(Guid customerUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT * 
             FROM Customer 
             WHERE CustomerUID = @customerUid";
        var customer = new Customer();

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@customerUid", customerUid)))
        {
          while (reader.Read())
            customer = new Customer
            {
              CustomerName = reader.GetString(reader.GetOrdinal("Name")),
              CustomerType = (CustomerType) reader.GetInt32(reader.GetOrdinal("fk_CustomerTypeID")),
              CustomerUID = reader.GetString(reader.GetOrdinal("CustomerUID"))
            };
        }

        return customer;
      });
    }

    #endregion(Customer)

    #region(Projects)

    public static IEnumerable<Project> GetLandfillProjectsForUser(string userUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT 
              p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, 
              cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, s.SubscriptionUID, 
              p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
              p.fk_ProjectTypeID AS ProjectType, p.GeometryWKT, 
              s.StartDate AS SubStartDate, s.EndDate AS SubEndDate
          FROM Project p  
              JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
              JOIN CustomerUser cu ON cu.fk_CustomerUID = cp.fk_CustomerUID
              JOIN ProjectSubscription ps ON ps.fk_ProjectUID = p.ProjectUID
              JOIN Subscription s ON s.SubscriptionUID = ps.fk_SubscriptionUID
          WHERE cu.UserUID = @userUid and p.IsDeleted = 0 AND p.fk_ProjectTypeID = 1";


        var projects = new List<Project>();

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@userUid", userUid)))
        {
          while (reader.Read())
            projects.Add(new Project
            {
              LegacyProjectID = reader.GetUInt16(reader.GetOrdinal("LegacyProjectID")),
              ProjectUID = reader.GetString(reader.GetOrdinal("ProjectUID")),
              Name = reader.GetString(reader.GetOrdinal("Name")),
              LandfillTimeZone = reader.GetString(reader.GetOrdinal("LandfillTimeZone")),
              ProjectTimeZone = reader.GetString(reader.GetOrdinal("ProjectTimeZone")),
              LegacyCustomerID = reader.GetInt64(reader.GetOrdinal("LegacyCustomerID")),
              SubscriptionEndDate = reader.GetDateTime(reader.GetOrdinal("SubEndDate")),
              GeometryWKT = reader.GetString(reader.GetOrdinal("GeometryWKT")) == null
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("GeometryWKT")),
              IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
            });
        }

        return projects;
      });
    }

    public static IEnumerable<ProjectResponse> GetProjects(string userUid, string customerUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT 
              p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, 
              cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, s.SubscriptionUID, 
              p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
              p.fk_ProjectTypeID AS ProjectType, p.GeometryWKT, 
              s.StartDate AS SubStartDate, s.EndDate AS SubEndDate
            FROM Project p  
              JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
              JOIN CustomerUser cu ON cu.fk_CustomerUID = cp.fk_CustomerUID
              JOIN ProjectSubscription ps ON ps.fk_ProjectUID = p.ProjectUID
              JOIN Subscription s ON s.SubscriptionUID = ps.fk_SubscriptionUID
            WHERE cu.UserUID = @userUid AND cu.fk_CustomerUID = @customerUid AND p.IsDeleted = 0 AND p.fk_ProjectTypeID = 1";

        var projects = new List<ProjectResponse>();

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@userUid", userUid),
          new MySqlParameter("@customerUid", customerUid)))
        {
          while (reader.Read())
          {
            var subStartDate = reader.GetDateTime(reader.GetOrdinal("SubStartDate"));
            var subEndDate = reader.GetDateTime(reader.GetOrdinal("SubEndDate"));
            var utcNowDate = DateTime.UtcNow.Date;
            var daysToSubExpiry = -1;
            if (subEndDate > utcNowDate)
            {
              var subIsLeapDay = subStartDate.Month == 2 && subStartDate.Day == 29;
              var nowIsLeapYear = DateTime.IsLeapYear(utcNowDate.Year);
              var day = nowIsLeapYear || !subIsLeapDay ? subStartDate.Day : 28;
              var anniversaryDate = new DateTime(utcNowDate.Year, subStartDate.Month, day);
              if (anniversaryDate < utcNowDate)
                anniversaryDate = anniversaryDate.AddYears(1);
              daysToSubExpiry = (anniversaryDate - utcNowDate).Days;
            }
            else if (subEndDate == utcNowDate)
            {
              daysToSubExpiry = 0;
            }

            projects.Add(new ProjectResponse
            {
              id = reader.GetUInt32(reader.GetOrdinal("LegacyProjectID")),
              projectUid = reader.GetString(reader.GetOrdinal("ProjectUID")),
              name = reader.GetString(reader.GetOrdinal("Name")),
              timeZoneName = reader.GetString(reader.GetOrdinal("LandfillTimeZone")),
              legacyTimeZoneName = reader.GetString(reader.GetOrdinal("ProjectTimeZone")),
              legacyCustomerID = reader.GetInt64(reader.GetOrdinal("LegacyCustomerID")),
              daysToSubscriptionExpiry = daysToSubExpiry
            });
          }
        }

        return projects;
      });
    }

    public static List<ProjectResponse> GetListOfAvailableProjects()
    {
      return WithConnection(conn =>
      {
        var command =
          @"SELECT DISTINCT p.ProjectUID,  p.Name, p.LegacyProjectID AS ProjectID, 
                p.LandfillTimeZone as TimeZone,
                cp.LegacyCustomerID
              FROM Project p  
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN CustomerUser cu ON cu.fk_CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps ON ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s ON s.SubscriptionUID = ps.fk_SubscriptionUID
              WHERE p.fk_ProjectTypeID = 1 AND p.IsDeleted = 0";
        using (var reader = MySqlHelper.ExecuteReader(conn, command))
        {
          var projects = new List<ProjectResponse>();
          while (reader.Read())
            projects.Add(new ProjectResponse
            {
              id = reader.GetUInt32(reader.GetOrdinal("ProjectID")),
              timeZoneName = reader.GetString(reader.GetOrdinal("TimeZone")),
              projectUid = reader.GetString(reader.GetOrdinal("ProjectUID")),
              name = reader.GetString(reader.GetOrdinal("Name")),
              legacyCustomerID = reader.GetInt64(reader.GetOrdinal("LegacyCustomerID"))
            });
          return projects;
        }
      });
    }

    public static List<ProjectResponse> GetProject(string projectUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT ProjectUID, LegacyProjectID AS ProjectID, Name, 
                             ProjectTimeZone, LandfillTimeZone
                          FROM Project
                          WHERE ProjectUID = @projectUid";

        var projects = new List<ProjectResponse>();

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", projectUid)))
        {
          while (reader.Read())
            projects.Add(new ProjectResponse
            {
              id = reader.GetUInt32(reader.GetOrdinal("ProjectID")),
              projectUid = reader.GetString(reader.GetOrdinal("ProjectUID")),
              name = reader.GetString(reader.GetOrdinal("Name")),
              timeZoneName = reader.GetString(reader.GetOrdinal("LandfillTimeZone")),
              legacyTimeZoneName = reader.GetString(reader.GetOrdinal("ProjectTimeZone"))
            });
        }

        return projects;
      });
    }

    #endregion

    #region(Entries)

    /// <summary>
    ///   Retrieves data entries for a given projectResponse. If date range is not specified, returns data
    ///   for 2 years ago to today in projectResponse time zone. If geofence is not specified returns data for
    ///   entire projectResponse area otherwise for geofenced area.
    /// </summary>
    /// <param name="projectResponse">Project</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="startDate">Start date in projectResponse time zone</param>
    /// <param name="endDate">End date in projectResponse time zone</param>
    /// <returns>A list of data entries</returns>
    public static IEnumerable<DayEntry> GetEntries(ProjectResponse projectResponse, string geofenceUid,
      DateTime? startDate,
      DateTime? endDate)
    {
      var projectGeofenceUid = GetGeofenceUidForProject(projectResponse);
      if (string.IsNullOrEmpty(geofenceUid))
        geofenceUid = projectGeofenceUid;

      return WithConnection(conn =>
      {
        var dateRange = CheckDateRange(projectResponse.timeZoneName, startDate, endDate);

        var entriesLookup = (from dr in dateRange
          select new DayEntry
          {
            date = dr.Date,
            entryPresent = false,
            weight = 0.0,
            volume = 0.0
          }).ToDictionary(k => k.date, v => v);

        //Now get the actual data and merge       
        var command = @"SELECT Date, Weight, Volume 
                            FROM Entries 
                          WHERE Date >= CAST(@startDate AS DATE) AND Date <= CAST(@endDate AS DATE)
                            AND ProjectUID = @projectUid AND GeofenceUID = @geofenceUid
                          ORDER BY Date";

        using (var reader = MySqlHelper.ExecuteReader(conn, command,
          new MySqlParameter("@projectUid", projectResponse.projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@startDate", dateRange.First()),
          new MySqlParameter("@endDate", dateRange.Last())
        ))
        {
          const double EPSILON = 0.001;

          while (reader.Read())
          {
            var date = reader.GetDateTime(reader.GetOrdinal("Date"));
            var entry = entriesLookup[date];
            entry.entryPresent = true;
            entry.weight = reader.GetDouble(reader.GetOrdinal("Weight"));
            var volume = 0.0;
            if (!reader.IsDBNull(reader.GetOrdinal("Volume")))
            {
              volume = reader.GetDouble(reader.GetOrdinal("Volume"));
              if (volume <= EPSILON)
                volume = 0.0;
            }

            entry.volume = volume;
          }

          return entriesLookup.Select(v => v.Value).ToList();
        }
      });
    }

    /// <summary>
    ///   Saves a weight entry for a given projectResponse
    /// </summary>
    /// <param name="projectResponse">Project</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="entry">Weight entry from the client</param>
    /// <returns></returns>
    public static void SaveEntry(ProjectResponse projectResponse, string geofenceUid, WeightEntry entry)
    {
      WithConnection<object>(conn =>
      {
        var command = @"INSERT INTO Entries 
                              (ProjectID, ProjectUID, Date, Weight, GeofenceUID) 
                            VALUES (@projectId, @projectUid, @date, @weight, @geofenceUid) 
                            ON DUPLICATE KEY UPDATE Weight = @weight";

        MySqlHelper.ExecuteNonQuery(conn, command,
          new MySqlParameter("@projectId", projectResponse.id),
          new MySqlParameter("@projectUid", projectResponse.projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@date", entry.date),
          new MySqlParameter("@weight", entry.weight));

        return null;
      });
    }

    /// <summary>
    ///   Saves a volume for a given projectResponse, geofence and date
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date</param>
    /// <param name="volume">Volume</param>
    /// <returns></returns>
    public static void SaveVolume(string projectUid, string geofenceUid, DateTime date, double volume)
    {
      //If we are getting 0 value whereas the actual value was non zero assume 3d subsystmen failure and exit
      var project = GetProject(projectUid).First();
      /*  var entries = LandfillDb.GetEntries(projectResponse, geofenceUid, date, date);
          if (entries.Any())
            if (entries.First().volume > 0 && volume == 0)
              return;*/

      WithConnection<object>(conn =>
      {
        // replace negative volumes with 0; they are possible (e.g. due to extra compaction 
        // without new material coming in) but don't make sense in the context of the application
        var command = @"INSERT Entries 
                              (ProjectID, ProjectUID, Date, GeofenceUID, Volume, VolumeNotRetrieved, VolumeNotAvailable, VolumesUpdatedTimestampUTC)
                            VALUES
                              (@projectId, @projectUid, @date, @geofenceUid, GREATEST(@volume, 0.0),0,0,UTC_TIMESTAMP())
                              ON DUPLICATE KEY UPDATE
                              Volume = GREATEST(@volume, 0.0), VolumeNotRetrieved = 0, VolumeNotAvailable = 0, VolumesUpdatedTimestampUTC = UTC_TIMESTAMP(), ProjectId=@projectId";

        MySqlHelper.ExecuteNonQuery(conn, command,
          new MySqlParameter("@projectId", project.id),
          new MySqlParameter("@volume", volume),
          new MySqlParameter("@projectUid", projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@date", date));

        return null;
      });
    }

    /// <summary>
    ///   Marks an entry with "volume not retrieved" so it can be retried later
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date of the entry</param>
    /// <returns></returns>
    public static void MarkVolumeNotRetrieved(string projectUid, string geofenceUid, DateTime date)
    {
      WithConnection<object>(conn =>
      {
        var command = @"UPDATE Entries 
                            SET VolumeNotRetrieved = 1
                          WHERE ProjectUID = @projectUid 
                            AND Date = @date 
                            AND GeofenceUID = @geofenceUid";

        MySqlHelper.ExecuteNonQuery(conn, command,
          new MySqlParameter("@projectUid", projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@date", date));

        return null;
      });
    }


    /// <summary>
    ///   Marks an entry with "volume not available" to indicate that there is no volume information in Raptor for that date
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date of the entry</param>
    /// <returns></returns>
    public static void MarkVolumeNotAvailable(string projectUid, string geofenceUid, DateTime date)
    {
      WithConnection<object>(conn =>
      {
        var command = @"UPDATE Entries 
                              SET VolumeNotAvailable = 1, VolumeNotRetrieved = 0, VolumesUpdatedTimestampUTC = UTC_TIMESTAMP()
                              WHERE ProjectUID = @projectUid AND Date = @date AND GeofenceUID = @geofenceUid";

        MySqlHelper.ExecuteNonQuery(conn, command,
          new MySqlParameter("@projectUid", projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@date", date));

        return null;
      });
    }

    /// <summary>
    ///   Gets today's date in the projectResponse time zone.
    /// </summary>
    /// <param name="timeZoneName">Project time zone name</param>
    /// <returns></returns>
    public static DateTime GetTodayInProjectTimeZone(string timeZoneName)
    {
      var projTimeZone = DateTimeZoneProviders.Tzdb[timeZoneName];
      var utcNow = DateTime.UtcNow;
      var projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
      return (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date;
    }

    private static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
    {
      if (endDate < startDate)
        throw new ArgumentException("endDate must be greater than or equal to startDate");

      while (startDate <= endDate)
      {
        yield return startDate;
        startDate = startDate.AddDays(1);
      }
    }

    private static IEnumerable<DateTime> CheckDateRange(string timeZoneName, DateTime? startDate, DateTime? endDate)
    {
      //Check date range within 2 years ago to today in projectResponse time zone
      var projTimeZone = DateTimeZoneProviders.Tzdb[timeZoneName];
      var utcNow = DateTime.UtcNow;
      var projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
      var todayinProjTimeZone = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date;
      var twoYearsAgo = todayinProjTimeZone.AddYears(-2);
      //DateTime yesterday = todayinProjTimeZone.AddDays(-1);
      if (!startDate.HasValue)
        startDate = twoYearsAgo;
      if (!endDate.HasValue)
        endDate = todayinProjTimeZone;
      if (startDate < twoYearsAgo || endDate > todayinProjTimeZone.AddDays(1))
        throw new ArgumentException("Invalid date range. Valid range is 2 years ago to today.");
      return GetDateRange(startDate.Value, endDate.Value);
    }

    #endregion

    #region Geofences

    /// <summary>
    ///   Gets the geofence UID for the projectResponse from the GeofenceResponse table
    /// </summary>
    /// <param name="projectResponse">Project</param>
    /// <returns>The geofence UID. If none was specified returns the projectResponse geofence UID</returns>
    public static string GetGeofenceUidForProject(ProjectResponse projectResponse)
    {
      return WithConnection(conn =>
      {
        //Get the projectResponse geofence uid
        string projectGeofenceUid = null;
        var command = @"SELECT g.GeofenceUID
                            FROM Geofence g 
                              JOIN ProjectGeofence pg ON g.GeofenceUID = pg.fk_GeofenceUID 
                            WHERE pg.fk_ProjectUID = @projectUid AND g.fk_GeofenceTypeID = 1"; //Project type
        using (var reader =
          MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", projectResponse.projectUid)))
        {
          while (reader.Read()) projectGeofenceUid = reader.GetString(0);
        }

        return projectGeofenceUid;
      });
    }

    /// <summary>
    ///   Retrieves the geofences associated with the projectResponse.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <returns>A list of geofences</returns>
    public static IEnumerable<GeofenceResponse> GetGeofences(string projectUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT g.GeofenceUID, g.Name, g.fk_GeofenceTypeID, g.GeometryWKT 
                          FROM Geofence g
                            JOIN ProjectGeofence pg on g.GeofenceUID = pg.fk_GeofenceUID
                          WHERE pg.fk_ProjectUID = @projectUid AND g.IsDeleted = 0 
                                AND (g.fk_GeofenceTypeID = 1 OR g.fk_GeofenceTypeID = 10)";

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", projectUid)))
        {
          var geofences = new List<GeofenceResponse>();
          while (reader.Read())
            geofences.Add(new GeofenceResponse
            {
              uid = Guid.Parse(reader.GetString(reader.GetOrdinal("GeofenceUID"))),
              name = reader.GetString(reader.GetOrdinal("Name")),
              type = reader.GetInt32(reader.GetOrdinal("fk_GeofenceTypeID")),
              bbox = GetBoundingBox(reader.GetString(reader.GetOrdinal("GeometryWKT")))
            });
          return geofences;
        }
      });
    }

    /// <summary>
    ///   Gets the bounding box of a geofence.
    /// </summary>
    /// <param name="geometryWKT">The geofence points</param>
    /// <returns>Bounding box in decimal degrees</returns>
    private static BoundingBox GetBoundingBox(string geometryWKT)
    {
      var latlngs = ConversionUtil.GeometryToPoints(geometryWKT, false);
      var bbox = new BoundingBox
      {
        minLat = double.MaxValue,
        minLng = double.MaxValue,
        maxLat = double.MinValue,
        maxLng = double.MinValue
      };
      foreach (var latLng in latlngs)
      {
        if (latLng.Lat < bbox.minLat) bbox.minLat = latLng.Lat;
        if (latLng.Lat > bbox.maxLat) bbox.maxLat = latLng.Lat;
        if (latLng.Lon < bbox.minLng) bbox.minLng = latLng.Lon;
        if (latLng.Lon > bbox.maxLng) bbox.maxLng = latLng.Lon;
      }

      return bbox;
    }

    /// <summary>
    ///   Retrieves a geofence boundary
    /// </summary>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <returns>A list of WGS84 points</returns>
    public static IEnumerable<WGSPoint> GetGeofencePoints(string geofenceUid)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT GeometryWKT 
                          FROM Geofence
                          WHERE GeofenceUID = @geofenceUid";

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@geofenceUid", geofenceUid)))
        {
          IEnumerable<WGSPoint> latlngs = null;
          while (reader.Read()) latlngs = ConversionUtil.GeometryToPoints(reader.GetString(0), true);
          return latlngs;
        }
      });
    }

    #endregion

    #region CCA

    /// <summary>
    ///   Saves a CCA summary for a given projectResponse, geofence, date, machine and lift
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date</param>
    /// <param name="machineId">Machine ID</param>
    /// <param name="liftId">Lift/Layer ID</param>
    /// <param name="incomplete">Incomplete %</param>
    /// <param name="complete">Complete %</param>
    /// <param name="overcomplete">Over complete %</param>
    /// <returns></returns>
    public static void SaveCCA(string projectUid, string geofenceUid, DateTime date, long machineId, int? liftId,
      double incomplete, double complete, double overcomplete)
    {
      UpsertCCA(projectUid, geofenceUid, date, machineId, liftId, incomplete, complete, overcomplete, false, false);
    }

    /// <summary>
    ///   Marks an entry with "CCA not retrieved" so it can be retried later
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date of the entry</param>
    /// <param name="machineId">Machine ID</param>
    /// <param name="liftId">Lift/Layer ID</param>
    /// <returns></returns>
    public static void MarkCCANotRetrieved(string projectUid, string geofenceUid, DateTime date, long machineId,
      int? liftId)
    {
      UpsertCCA(projectUid, geofenceUid, date, machineId, liftId, null, null, null, true, false);
    }

    /// <summary>
    ///   Marks an entry with "CCA not available" to indicate that there is no CCA information in Raptor for that date
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date of the entry</param>
    /// <param name="machineId">Machine ID</param>
    /// <param name="liftId">Lift/Layer ID</param>
    /// <returns></returns>
    public static void MarkCCANotAvailable(string projectUid, string geofenceUid, DateTime date, long machineId,
      int? liftId)
    {
      UpsertCCA(projectUid, geofenceUid, date, machineId, liftId, null, null, null, false, true);
    }

    /// <summary>
    ///   Inserts or updates a CCA entry in the database.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="date">Date of the entry</param>
    /// <param name="machineId">Machine ID</param>
    /// <param name="liftId">Lift/Layer ID</param>
    /// <param name="incomplete">Incomplete %</param>
    /// <param name="complete">Complete %</param>
    /// <param name="overcomplete">Over complete %</param>
    /// <param name="notRetrieved">Flag to indicate CCA not retrieved from Raptor</param>
    /// <param name="notAvailable">Flag to indicate no CCA value available from Raptor</param>
    private static void UpsertCCA(string projectUid, string geofenceUid, DateTime date, long machineId, int? liftId,
      double? incomplete, double? complete, double? overcomplete, bool notRetrieved, bool notAvailable)
    {
      //Assume Raptor failure and if we have some data in the Database don't wipe them out
      var cca = GetCCA(GetProject(projectUid).First(), geofenceUid, date, date, machineId,
        liftId);
      if (cca.Any())
        if ((cca.First().complete > 0 || cca.First().incomplete > 0 || cca.First().overcomplete > 0) &&
            incomplete == 0 && complete == 0 && overcomplete == 0)
          return;

      WithConnection<object>(conn =>
      {
        var command =
          @"INSERT INTO CCA 
                      (ProjectUID, Date, Incomplete, Complete, Overcomplete, GeofenceUID, MachineID, LiftID, 
                          CCANotRetrieved, CCANotAvailable, CCAUpdatedTimestampUTC)
                    VALUES (@projectUid, @date, @incomplete, @complete, @overcomplete, @geofenceUid, @machineId, 
                          @liftId, @notRetrieved, @notAvailable, UTC_TIMESTAMP())
                    ON DUPLICATE KEY UPDATE
                      Incomplete = @incomplete, Complete = @complete, Overcomplete = @overcomplete, 
                      CCANotRetrieved = @notRetrieved, CCANotAvailable = @notAvailable, CCAUpdatedTimestampUTC = UTC_TIMESTAMP()";

        MySqlHelper.ExecuteNonQuery(conn, command,
          new MySqlParameter("@incomplete", (object) incomplete ?? DBNull.Value),
          new MySqlParameter("@complete", (object) complete ?? DBNull.Value),
          new MySqlParameter("@overcomplete", (object) overcomplete ?? DBNull.Value),
          new MySqlParameter("@projectUid", projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@machineId", machineId),
          new MySqlParameter("@liftId", (object) liftId ?? -1),
          new MySqlParameter("@date", date),
          new MySqlParameter("@notRetrieved", notRetrieved ? 1 : 0),
          new MySqlParameter("@notAvailable", notAvailable ? 1 : 0));

        return null;
      });
    }

    /// <summary>
    ///   Gets CCA data for the projectResponse for all machines for all lifts. If date range is not specified, returns data
    ///   for 2 years ago to today in projectResponse time zone. If geofence is not specified returns data for
    ///   entire projectResponse area otherwise for geofenced area. If machineId is not specified returns data for all
    ///   machines.
    ///   If liftId is not specified returns data for all lifts.
    /// </summary>
    /// <param name="projectResponse">Project</param>
    /// <param name="geofenceUid">GeofenceResponse UID</param>
    /// <param name="startDate">Start date in projectResponse time zone</param>
    /// <param name="endDate">End date in projectResponse time zone</param>
    /// <param name="machineId">Machine ID</param>
    /// <param name="liftId">Lift ID</param>
    /// <returns>A list of CCA entries</returns>
    public static IEnumerable<CCA> GetCCA(ProjectResponse projectResponse, string geofenceUid, DateTime? startDate,
      DateTime? endDate,
      long? machineId, int? liftId)
    {
      var projectGeofenceUid = GetGeofenceUidForProject(projectResponse);
      if (string.IsNullOrEmpty(geofenceUid))
        geofenceUid = projectGeofenceUid;

      return WithConnection(conn =>
      {
        var dateRange = CheckDateRange(projectResponse.timeZoneName, startDate, endDate).ToList();
        var firstDate = dateRange.First();
        var lastDate = dateRange.Last();
        liftId = liftId ?? -1;

        //Get the actual data 
        var command = @"SELECT Date, GeofenceUID, MachineID, LiftID, Incomplete, Complete, Overcomplete 
                          FROM CCA 
                          WHERE Date >= CAST(@startDate AS DATE) AND Date <= CAST(@endDate AS DATE)
                            AND ProjectUID = @projectUid 
                            AND GeofenceUID = @geofenceUid 
                            AND LiftID = @liftId ";
        if (machineId.HasValue)
          command += " AND MachineID = @machineId ";

        command += " ORDER BY MachineId, Date ";

        var parms = new List<MySqlParameter>
        {
          new MySqlParameter("@projectUid", projectResponse.projectUid),
          new MySqlParameter("@geofenceUid", geofenceUid),
          new MySqlParameter("@startDate", firstDate),
          new MySqlParameter("@endDate", lastDate),
          new MySqlParameter("@liftId", liftId)
        };
        if (machineId.HasValue)
          parms.Add(new MySqlParameter("@machineId", machineId));

        var actualData = new List<CCA>();
        using (var reader = MySqlHelper.ExecuteReader(conn, command, parms.ToArray()))
        {
          while (reader.Read())
            actualData.Add(
              new CCA
              {
                date = reader.GetDateTime(reader.GetOrdinal("Date")),
                geofenceUid = reader.GetString(reader.GetOrdinal("GeofenceUID")),
                machineId = reader.GetUInt32(reader.GetOrdinal("MachineID")),
                liftId = reader.GetOrdinal("LiftID") == -1 ? (int?) null : reader.GetInt16(reader.GetOrdinal("LiftID")),
                incomplete = reader.IsDBNull(reader.GetOrdinal("Incomplete"))
                  ? 0
                  : reader.GetDouble(reader.GetOrdinal("Incomplete")),
                complete = reader.IsDBNull(reader.GetOrdinal("Complete"))
                  ? 0
                  : reader.GetDouble(reader.GetOrdinal("Complete")),
                overcomplete = reader.IsDBNull(reader.GetOrdinal("Overcomplete"))
                  ? 0
                  : reader.GetDouble(reader.GetOrdinal("Overcomplete"))
              });
        }

        //Now add the missing data for each machine
        var machineIds = actualData.Select(a => a.machineId).Distinct().ToList();
        foreach (var machId in machineIds)
        {
          var actualDates = actualData.Where(a => a.machineId == machId).Select(d => d.date);
          var missingData = from dr in dateRange
            where !actualDates.Contains(dr.Date)
            select new CCA
            {
              date = dr.Date,
              geofenceUid = geofenceUid,
              machineId = machId,
              liftId = liftId,
              incomplete = 0,
              complete = 0,
              overcomplete = 0
            };
          actualData.AddRange(missingData);
        }

        return actualData.OrderBy(a => a.machineId).ThenBy(a => a.date);
      });
    }

    #endregion

    #region Machines

    /// <summary>
    ///   Get the ID of the machine with the given details. If it doesn't exist then create it.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="details">Machine details</param>
    /// <returns>ID of the machine</returns>
    public static long GetMachineId(string projectUid, MachineDetails details)
    {
      return WithConnection(conn =>
      {
        {
          var parms = new List<MySqlParameter>
          {
            new MySqlParameter("@assetId", details.assetId),
            new MySqlParameter("@machineName", details.machineName),
            new MySqlParameter("@isJohnDoe", details.isJohnDoe),
            new MySqlParameter("@projectUid", projectUid)
          }.ToArray();

          var existingId = GetMachineId(conn, parms, details.machineName);
          if (existingId == 0)
          {
            var command =
              @"INSERT INTO Machine (AssetID, MachineName, IsJohnDoe, ProjectUID)
                  VALUES (@assetId, @machineName, @isJohnDoe, @projectUid)";

            MySqlHelper.ExecuteNonQuery(conn, command, parms);
            existingId = GetMachineId(conn, parms, details.machineName);
          }

          return existingId;
        }
      });
    }

    /// <summary>
    ///   Gets the ID of a machine from the Landfill database.
    /// </summary>
    /// <param name="sqlConn">SQL database connection</param>
    /// <param name="sqlParams">SQL parameters for the machine to get</param>
    /// <param name="machineName">Machine name. Updates this in database if necessary</param>
    /// <returns>The machine ID</returns>
    private static long GetMachineId(MySqlConnection sqlConn, MySqlParameter[] sqlParams, string machineName)
    {
      //Match on AssetID and IsJohnDoe only as MachineName can change.
      var query = @"SELECT ID, MachineName FROM Machine
                      WHERE AssetID = @assetId AND IsJohnDoe = @isJohnDoe AND ProjectUID = @projectUid and MachineName = @machineName";

      long existingId = 0;
      var updateName = false;
      using (var reader = MySqlHelper.ExecuteReader(sqlConn, query, sqlParams))
      {
        while (reader.Read())
        {
          existingId = reader.GetUInt32(reader.GetOrdinal("ID"));
          updateName =
            !machineName.Equals(reader.GetString(reader.GetOrdinal("MachineName")),
              StringComparison.OrdinalIgnoreCase);
        }
      }

      if (updateName)
      {
        var command = @"UPDATE Machine SET MachineName = @machineName WHERE ID = @machineId";
        MySqlHelper.ExecuteNonQuery(sqlConn, command, new MySqlParameter("@machineId", existingId),
          new MySqlParameter("@machineName", machineName));
      }

      return existingId;
    }


    /// <summary>
    ///   Gets the machine details for the specified machine.
    /// </summary>
    /// <param name="machineId">ID of machine to get</param>
    /// <returns>Machine details</returns>
    public static MachineDetails GetMachine(long machineId)
    {
      return WithConnection(conn =>
      {
        var command = @"SELECT AssetID, MachineName, IsJohnDoe FROM Machine WHERE ID = @machineId";

        using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@machineId", machineId)))
        {
          MachineDetails machine = null;
          while (reader.Read())
            machine = new MachineDetails
            {
              assetId = reader.GetInt64(reader.GetOrdinal("AssetID")),
              machineName = reader.GetString(reader.GetOrdinal("MachineName")),
              isJohnDoe = reader.GetInt16(reader.GetOrdinal("IsJohnDoe")) == 1
            };
          return machine;
        }
      });
    }

    #endregion
  }
}