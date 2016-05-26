using LandfillService.Common.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using NodaTime;

namespace LandfillService.Common.Context
{
    /// <summary>
    /// Encapsulates DB queries
    /// </summary>
    public class LandfillDb
    {
        private static string connString = ConfigurationManager.ConnectionStrings["LandfillContext"].ConnectionString;
        private static int lockTimeout = 1; // hour

        /// <summary>
        /// Wrapper for generating a MySQL connection
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


        /// <summary>
        /// Wrapper for executing MySQL queries/statements in a DB transaction; rolls back the transaction in case of errors
        /// </summary>
        /// <param name="body">Code to execute</param>
        /// <returns>The result of executing body()</returns>
        private static T InTransaction<T>(Func<MySqlConnection, T> body) 
        {
            return WithConnection<T>((conn) =>
            {
                MySqlTransaction transaction = null;

                try
                {
                    transaction = conn.BeginTransaction();
                    var result = body(conn);
                    transaction.Commit();
                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }


        #region(Projects)

        /// <summary>
        /// Retrieves a list of projects for a given user (via session ID)
        /// </summary>
        /// <param name="userUid">User ID used to associate projects with a user</param>
        /// <returns>A list of projects</returns>
        public static IEnumerable<Project> GetProjects(string userUid)
        {
            return InTransaction((conn) =>
            {
              var command = @"SELECT prj.ProjectID, prj.Name, prj.LandfillTimeZone,
                                     prj.ProjectUID, prj.ProjectTimeZone,
                                     sub.StartDate AS SubStartDate, sub.EndDate AS SubEndDate 
                              FROM Project prj  
                              JOIN CustomerUser cu ON prj.CustomerUID = cu.fk_CustomerUID
                              JOIN Subscription sub ON prj.SubscriptionUID = sub.SubscriptionUID
                              WHERE cu.fk_UserUID = @userUid and prj.IsDeleted = 0;";
              var projects = new List<Project>();

              using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@userUid", userUid)))
                {
                    while (reader.Read())
                    {
                      var subStartDate = reader.GetDateTime(reader.GetOrdinal("SubStartDate"));
                      var subEndDate = reader.GetDateTime(reader.GetOrdinal("SubEndDate"));
                      var utcNowDate = DateTime.UtcNow.Date;
                      var daysToSubExpiry = -1;
                      if (subEndDate > utcNowDate)
                      {
                        bool subIsLeapDay = subStartDate.Month == 2 && subStartDate.Day == 29;
                        bool nowIsLeapYear = DateTime.IsLeapYear(utcNowDate.Year);
                        int day = nowIsLeapYear || !subIsLeapDay ? subStartDate.Day : 28;
                        var anniversaryDate = new DateTime(utcNowDate.Year, subStartDate.Month, day);
                        if (anniversaryDate < utcNowDate)
                          anniversaryDate = anniversaryDate.AddYears(1);
                        daysToSubExpiry = (anniversaryDate - utcNowDate).Days;
                      }
                      else if (subEndDate == utcNowDate)
                      {
                        daysToSubExpiry = 0;
                      }                       

                        projects.Add(new Project { id = reader.GetUInt32(reader.GetOrdinal("ProjectID")),
                                                   projectUid = reader.GetString(reader.GetOrdinal("ProjectUID")),
                                                   name = reader.GetString(reader.GetOrdinal("Name")),
                                                   timeZoneName = reader.GetString(reader.GetOrdinal("LandfillTimeZone")),
                                                   legacyTimeZoneName = reader.GetString(reader.GetOrdinal("ProjectTimeZone")),
                                                   daysToSubscriptionExpiry = daysToSubExpiry
                        });
                    }
                }

              return projects;
            });
        }

   

        /// <summary>
        /// Returns the status of volume retrieval for a project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>true if volume retrieval is in progress, false otherwise</returns>
        public static bool RetrievalInProgress(Project project)
        {
            return WithConnection((conn) =>
            {
                var command = @"SELECT COUNT(*) FROM Project 
                                WHERE ProjectID = @projectId AND 
                                (RetrievalStartedAt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL " + 
                                lockTimeout.ToString() + " HOUR) OR " +
                                @"(SELECT COUNT(*) FROM Entries 
                                   WHERE ProjectID = @projectId AND
                                   Volume IS NULL AND VolumeNotAvailable = 0) > 0)";

                var count = MySqlHelper.ExecuteScalar(conn, command, new MySqlParameter("@projectId", project.id));
                return Convert.ToUInt32(count) > 0;
            });
        }

        /// <summary>
        /// Locks or unlocks a project for volume retrieval (only one retrieval process at a time should be happening for a given project)
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="shouldLock">true to lock, false to unlock</param>
        /// <returns>true if successful, false otherwise</returns>
        public static bool LockForRetrieval(Project project, bool shouldLock = true)
        {
            return WithConnection((conn) =>
            {          
                var command = shouldLock ? 
                    @"UPDATE Project SET RetrievalStartedAt = UTC_TIMESTAMP()
                      WHERE ProjectID = @projectId AND 
                      RetrievalStartedAt < DATE_SUB(UTC_TIMESTAMP(), INTERVAL " + lockTimeout.ToString() + " HOUR)"
                    :
                    @"UPDATE Project SET RetrievalStartedAt = DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 YEAR)
                      WHERE ProjectID = @projectId AND 
                      RetrievalStartedAt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL " + lockTimeout.ToString() + " HOUR)";

                var rowsAffected = MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@projectId", project.id));
                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Unlocks all projects for volume retrieval (only expected to be called at service startup)
        /// </summary>
        /// <returns></returns>
        public static void UnlockAllProjects()
        {
            WithConnection<object>((conn) =>
            {
                var command = @"UPDATE Project SET RetrievalStartedAt = DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 YEAR)";
                MySqlHelper.ExecuteNonQuery(conn, command);
                return null;
            });
        }


        #endregion

        #region(Entries)

        /// <summary>
        /// Saves a weight entry for a given project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="entry">Weight entry from the client</param>
        /// <returns></returns>
        public static void SaveEntry(uint projectId, string geofenceUid, WeightEntry entry)
        {
            WithConnection<object>((conn) =>
            {
                var command = @"INSERT INTO Entries (ProjectID, Date, Weight, GeofenceUID) 
                                VALUES (@projectId, @date, @weight, @geofenceUid) 
                                ON DUPLICATE KEY UPDATE Weight = @weight";

              MySqlHelper.ExecuteNonQuery(conn, command,
                  new MySqlParameter("@projectId", projectId),
                  new MySqlParameter("@geofenceUid", geofenceUid),
                  new MySqlParameter("@date", entry.date),
                  new MySqlParameter("@weight", entry.weight));

                return null;
            });
        }


        /// <summary>
        /// Saves a volume for a given project, geofence and date
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date</param>
        /// <param name="volume">Volume</param>
        /// <returns></returns>
        public static void SaveVolume(uint projectId, string geofenceUid, DateTime date, double volume)
        {
            WithConnection<object>((conn) =>
            {
                // replace negative volumes with 0; they are possible (e.g. due to extra compaction 
                // without new material coming in) but don't make sense in the context of the application
              var command = @"UPDATE Entries 
                              SET Volume = GREATEST(@volume, 0.0), VolumeNotRetrieved = 0, 
                                  VolumeNotAvailable = 0, VolumesUpdatedTimestampUTC = UTC_TIMESTAMP()
                              WHERE ProjectID = @projectId AND Date = @date AND GeofenceUID = @geofenceUid";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@volume", volume),
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@geofenceUid", geofenceUid),
                    new MySqlParameter("@date", date));

                return null;
            });
        }

        /// <summary>
        /// Marks an entry with "volume not retrieved" so it can be retried later
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date of the entry</param>
        /// <returns></returns>
        public static void MarkVolumeNotRetrieved(uint projectId, string geofenceUid, DateTime date)
        {
            WithConnection<object>((conn) =>
            {
                var command = @"UPDATE Entries SET VolumeNotRetrieved = 1
                                WHERE ProjectID = @projectId AND Date = @date AND GeofenceUID = @geofenceUid";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@geofenceUid", geofenceUid),
                    new MySqlParameter("@date", date));

                return null;
            });
        }


      public static List<Project> GetListOfAvailableProjects()
      {
        return InTransaction((conn) =>
        {
          var command = @"SELECT DISTINCT prj.ProjectID, prj.LandfillTimeZone as TimeZone, prj.ProjectUID, prj.Name
                          FROM Project prj 
                          LEFT JOIN Entries etr ON prj.ProjectID = etr.ProjectID 
                          WHERE etr.Weight IS NOT NULL AND prj.IsDeleted = 0;";
          using (var reader = MySqlHelper.ExecuteReader(conn, command))
          {
            var projects = new List<Project>();
            while (reader.Read())
            {
              projects.Add(new Project
              {
                id = reader.GetUInt32(reader.GetOrdinal("ProjectID")),
                timeZoneName = reader.GetString(reader.GetOrdinal("TimeZone")),
                projectUid = reader.GetString(reader.GetOrdinal("ProjectUID")),
                name = reader.GetString(reader.GetOrdinal("Name"))
              });
            }
            return projects;
          }
        });
      }

        /// <summary>
        /// Marks an entry with "volume not available" to indicate that there is no volume information in Raptor for that date
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date of the entry</param>
        /// <returns></returns>
        public static void MarkVolumeNotAvailable(uint projectId, string geofenceUid, DateTime date)
        {
            WithConnection<object>((conn) =>
            {
              var command = @"UPDATE Entries 
                              SET VolumeNotAvailable = 1, VolumeNotRetrieved = 0, VolumesUpdatedTimestampUTC = UTC_TIMESTAMP()
                              WHERE ProjectID = @projectId AND Date = @date AND GeofenceUID = @geofenceUid";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@geofenceUid", geofenceUid),
                    new MySqlParameter("@date", date));

                return null;
            });
        }      

        /// <summary>
        /// Retrieves a list of dates for which volumes couldn't be retrieved previously (used to retry retrieval)
        /// </summary>
        /// <param name="project">Project for which to retrieve data</param>
        /// <returns>A list of dates and geofence UIDs</returns>
        public static IEnumerable<DateEntry> GetDatesWithVolumesNotRetrieved(Project project)
        {
            string projectGeofenceUid = UpdateEntriesIfRequired(project, null);

            return WithConnection((conn) =>
            {
                // selects dates where volume retrieval failed OR was never completed OR hasn't yet happened;
                // note that this can cause overlap with newly added dates which are currently waiting to be handled by 
                // a volume retrieval task; this is acceptable because the probability of such overlap is expected to be low
                // BUT it allows the service to tolerate background tasks dying
              var command = @"SELECT etr.Date, etr.GeofenceUID FROM Entries etr
                              JOIN Geofence geo ON etr.GeofenceUID = geo.GeofenceUID
                            WHERE etr.ProjectID = @projectId AND geo.IsDeleted = 0 AND
                            (etr.VolumeNotRetrieved = 1 OR (etr.Volume IS NULL AND etr.VolumeNotAvailable = 0) OR 
                            (etr.VolumesUpdatedTimestampUTC IS NULL) OR 
                            ( (etr.VolumesUpdatedTimestampUTC < SUBDATE(UTC_TIMESTAMP(), INTERVAL 1 DAY)) AND 
                              (etr.VolumesUpdatedTimestampUTC > SUBDATE(UTC_TIMESTAMP(), INTERVAL 30 DAY))  ))";

                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectId", project.id)))
                {
                    var dates = new List<DateEntry>();
                    while (reader.Read())
                    {
                      dates.Add(new DateEntry {date = reader.GetDateTime(0), geofenceUid = reader.GetString(1)});
                    }
                    return dates;
                }
            });
        }

        /// <summary>
        /// Retrieves data entries for a given project. If date range is not specified, returns data 
        /// for 2 years ago to today in poject time zone. If geofence is not specified returns data for 
        /// entire project area otherwise for geofenced area.
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="startDate">Start date in project time zone</param>
        /// <param name="endDate">End date in project time zone</param>
        /// <returns>A list of data entries</returns>
        public static IEnumerable<DayEntry> GetEntries(Project project, string geofenceUid, DateTime? startDate, DateTime? endDate)
        {
            string projectGeofenceUid = UpdateEntriesIfRequired(project, geofenceUid);
            if (string.IsNullOrEmpty(geofenceUid))
              geofenceUid = projectGeofenceUid;

            return WithConnection((conn) =>
            {
                //Check date range within 2 years ago to today in project time zone
                var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];
                DateTime utcNow = DateTime.UtcNow;
                Offset projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
                DateTime todayinProjTimeZone = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date;
                DateTime twoYearsAgo = todayinProjTimeZone.AddYears(-2);
                //DateTime yesterday = todayinProjTimeZone.AddDays(-1);
                if (!startDate.HasValue)
                  startDate = twoYearsAgo;
                if (!endDate.HasValue)
                  endDate = todayinProjTimeZone;
                if (startDate < twoYearsAgo || endDate > todayinProjTimeZone)
                {
                  throw new ArgumentException("Invalid date range. Valid range is 2 years ago to today.");
                }
                var dateRange = GetDateRange(startDate.Value, endDate.Value);

                var entriesLookup = (from dr in dateRange
                                     select new DayEntry
                                          {
                                              date = dr.Date,
                                              entryPresent = false,
                                              weight = 0.0,
                                              volume = 0.0
                                          }).ToDictionary(k => k.date, v => v);
                
                //Now get the actual data and merge       
                var command = @"SELECT Date, Weight, Volume FROM Entries 
                                WHERE Date >= CAST(@startDate AS DATE) AND Date <= CAST(@endDate AS DATE)
                                AND ProjectID = @projectId AND GeofenceUID = @geofenceUid
                                ORDER BY Date";

                using (var reader = MySqlHelper.ExecuteReader(conn, command,
                  new MySqlParameter("@projectId", project.id),
                  new MySqlParameter("@geofenceUid", geofenceUid),
                  new MySqlParameter("@startDate", startDate.Value), //twoYearsAgo
                  new MySqlParameter("@endDate", endDate.Value)//yesterday
                  ))
                {
                    const double EPSILON = 0.001;

                    while (reader.Read())
                    {
                          DateTime date = reader.GetDateTime(reader.GetOrdinal("Date"));
                          DayEntry entry = entriesLookup[date];
                          entry.entryPresent = true;
                          entry.weight = reader.GetDouble(reader.GetOrdinal("Weight"));
                          double volume = 0.0;
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
        /// Updates entries for a project if required. Old landfill projects do not have the geofence UID set.
        /// Checks if the specified geofence UID is the project one and updates corresponding entries.
        /// If the geofence UID is not specified then it gets the project one from the Geofence table
        /// and updates the corresponding entries.
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <returns>The geofence UID. If none was specified returns the project geofence UID</returns>
        public static string UpdateEntriesIfRequired(Project project, string geofenceUid)
        {
          return WithConnection((conn) =>
          {
            //Get the project geofence uid
            string projectGeofenceUid = null;
            var command = @"SELECT GeofenceUID
                              FROM Geofence 
                              WHERE ProjectUID = @projectUid AND fk_GeofenceTypeID = 1;";//Project type
            using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", project.projectUid)))
            {
              while (reader.Read())
              {
                projectGeofenceUid = reader.GetString(0);
              }      
            }

            //See if handling entries for whole project
            if (string.IsNullOrEmpty(geofenceUid) || projectGeofenceUid == geofenceUid)
            {
              if (!string.IsNullOrEmpty(projectGeofenceUid))
              {
                //Update project entries geofence UID
                command =
                    "UPDATE Entries SET GeofenceUID = @geofenceUid WHERE ProjectID = @projectId AND GeofenceUID IS NULL";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", project.id),
                    new MySqlParameter("@geofenceUid", projectGeofenceUid));


                if (string.IsNullOrEmpty(geofenceUid))
                  geofenceUid = projectGeofenceUid;
              }
            }

            return geofenceUid;
          });        
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

        #endregion

      #region Geofences
        /// <summary>
        /// Retrieves the geofences associated with the project.
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>A list of geofences</returns>
        public static IEnumerable<Geofence> GetGeofences(uint projectId)
        {
          return WithConnection((conn) =>
          {
            var command = @"SELECT geo.GeofenceUID, geo.Name, geo.fk_GeofenceTypeID FROM Geofence geo
                            INNER JOIN Project prj ON geo.ProjectUID = prj.ProjectUID
                            WHERE prj.ProjectID = @projectId AND geo.IsDeleted = 0 
                                AND (geo.fk_GeofenceTypeID = 1 OR geo.fk_GeofenceTypeID = 10)";

            using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectId", projectId)))
            {
              List<Geofence> geofences = new List<Geofence>();
              while (reader.Read())
              {
                geofences.Add(new Geofence
                              {
                                uid = Guid.Parse(reader.GetString(reader.GetOrdinal("GeofenceUID"))),
                                name = reader.GetString(reader.GetOrdinal("Name")),
                                type = reader.GetInt32(reader.GetOrdinal("fk_GeofenceTypeID")),
                              });

              }
              return geofences;
            }
          });        
        }

        /// <summary>
        /// Retrieves a geofence boundary
        /// </summary>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <returns>A list of WGS84 points</returns>
        public static IEnumerable<WGSPoint> GetGeofencePoints(string geofenceUid)
        {
          const double DEGREES_TO_RADIANS = Math.PI / 180;

          return WithConnection((conn) =>
          {
            var command = @"SELECT GeometryWKT FROM Geofence
                            WHERE GeofenceUID = @geofenceUid";

            using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@geofenceUid", geofenceUid)))
            {
              List<WGSPoint> latlngs = new List<WGSPoint>();
              while (reader.Read())
              {
                var wkt = reader.GetString(0);
                //Trim off the "POLYGON((" and "))"
                wkt = wkt.Substring(9, wkt.Length - 11);
                var points = wkt.Split(',');
                foreach (var point in points)
                {
                  var parts = point.Split(' ');
                  var lat = double.Parse(parts[0]);
                  var lng = double.Parse(parts[0]);
                  latlngs.Add(new WGSPoint { Lat = lat * DEGREES_TO_RADIANS, Lon = lng * DEGREES_TO_RADIANS });
                }
              }
              return latlngs;
            }
          });
        }
      #endregion

    }

}