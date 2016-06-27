using Common.Utilities;
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
                              WHERE cu.fk_UserUID = @userUid and prj.IsDeleted = 0";
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
                                WHERE ProjectUID = @projectUid AND 
                                (RetrievalStartedAt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL " + 
                                lockTimeout.ToString() + " HOUR) OR " +
                                @"(SELECT COUNT(*) FROM Entries 
                                   WHERE ProjectUID = @projectUid AND
                                   Volume IS NULL AND VolumeNotAvailable = 0) > 0)";

                var count = MySqlHelper.ExecuteScalar(conn, command, new MySqlParameter("@projectUid", project.projectUid));
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
                      WHERE ProjectUID = @projectUid AND 
                      RetrievalStartedAt < DATE_SUB(UTC_TIMESTAMP(), INTERVAL " + lockTimeout.ToString() + " HOUR)"
                    :
                    @"UPDATE Project SET RetrievalStartedAt = DATE_SUB(UTC_TIMESTAMP(), INTERVAL 10 YEAR)
                      WHERE ProjectUID = @projectUid AND 
                      RetrievalStartedAt >= DATE_SUB(UTC_TIMESTAMP(), INTERVAL " + lockTimeout.ToString() + " HOUR)";

                var rowsAffected = MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@projectUid", project.projectUid));
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
        /// <param name="project">Project</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="entry">Weight entry from the client</param>
        /// <returns></returns>
        public static void SaveEntry(Project project, string geofenceUid, WeightEntry entry)
        {
            WithConnection<object>((conn) =>
            {
                var command = @"INSERT INTO Entries (ProjectID, ProjectUID, Date, Weight, GeofenceUID) 
                                VALUES (@projectId, @projectUid, @date, @weight, @geofenceUid) 
                                ON DUPLICATE KEY UPDATE Weight = @weight";

              MySqlHelper.ExecuteNonQuery(conn, command,
                  new MySqlParameter("@projectId", project.id),
                  new MySqlParameter("@projectUid", project.projectUid),
                  new MySqlParameter("@geofenceUid", geofenceUid),
                  new MySqlParameter("@date", entry.date),
                  new MySqlParameter("@weight", entry.weight));

                return null;
            });
        }


        /// <summary>
        /// Saves a volume for a given project, geofence and date
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date</param>
        /// <param name="volume">Volume</param>
        /// <returns></returns>
        public static void SaveVolume(string projectUid, string geofenceUid, DateTime date, double volume)
        {
            WithConnection<object>((conn) =>
            {
                // replace negative volumes with 0; they are possible (e.g. due to extra compaction 
                // without new material coming in) but don't make sense in the context of the application
              var command = @"UPDATE Entries 
                              SET Volume = GREATEST(@volume, 0.0), VolumeNotRetrieved = 0, 
                                  VolumeNotAvailable = 0, VolumesUpdatedTimestampUTC = UTC_TIMESTAMP()
                              WHERE ProjectUID = @projectUid AND Date = @date AND GeofenceUID = @geofenceUid";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@volume", volume),
                    new MySqlParameter("@projectUid", projectUid),
                    new MySqlParameter("@geofenceUid", geofenceUid),
                    new MySqlParameter("@date", date));

                return null;
            });
        }

        /// <summary>
        /// Marks an entry with "volume not retrieved" so it can be retried later
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date of the entry</param>
        /// <returns></returns>
        public static void MarkVolumeNotRetrieved(string projectUid, string geofenceUid, DateTime date)
        {
            WithConnection<object>((conn) =>
            {
                var command = @"UPDATE Entries SET VolumeNotRetrieved = 1
                                WHERE ProjectUID = @projectUid AND Date = @date AND GeofenceUID = @geofenceUid";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectUid", projectUid),
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
                          LEFT JOIN Entries etr ON prj.ProjectUID = etr.ProjectUID 
                          WHERE etr.Weight IS NOT NULL AND prj.IsDeleted = 0";
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
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date of the entry</param>
        /// <returns></returns>
        public static void MarkVolumeNotAvailable(string projectUid, string geofenceUid, DateTime date)
        {
            WithConnection<object>((conn) =>
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
                            WHERE etr.ProjectUID = @projectUid AND geo.IsDeleted = 0 AND
                            (etr.VolumeNotRetrieved = 1 OR (etr.Volume IS NULL AND etr.VolumeNotAvailable = 0) OR 
                            (etr.VolumesUpdatedTimestampUTC IS NULL) OR 
                            ( (etr.VolumesUpdatedTimestampUTC < SUBDATE(UTC_TIMESTAMP(), INTERVAL 1 DAY)) AND 
                              (etr.VolumesUpdatedTimestampUTC > SUBDATE(UTC_TIMESTAMP(), INTERVAL 30 DAY))  ))";

                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", project.projectUid)))
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
        /// for 2 years ago to today in project time zone. If geofence is not specified returns data for 
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
                var dateRange = CheckDateRange(project.timeZoneName, startDate, endDate);

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
                                AND ProjectUID = @projectUid AND GeofenceUID = @geofenceUid
                                ORDER BY Date";

                using (var reader = MySqlHelper.ExecuteReader(conn, command,
                  new MySqlParameter("@projectUid", project.projectUid),
                  new MySqlParameter("@geofenceUid", geofenceUid),
                  new MySqlParameter("@startDate", dateRange.First()), 
                  new MySqlParameter("@endDate", dateRange.Last())
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
        /// Gets today's date in the project time zone.
        /// </summary>
        /// <param name="timeZoneName">Project time zone name</param>
        /// <returns></returns>
        public static DateTime GetTodayInProjectTimeZone(string timeZoneName)
        {
          var projTimeZone = DateTimeZoneProviders.Tzdb[timeZoneName];
          DateTime utcNow = DateTime.UtcNow;
          Offset projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
          return (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date;
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
                              WHERE ProjectUID = @projectUid AND fk_GeofenceTypeID = 1";//Project type
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
                    "UPDATE Entries SET GeofenceUID = @geofenceUid WHERE ProjectUID = @projectUid AND GeofenceUID IS NULL";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectUid", project.projectUid),
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

        private static IEnumerable<DateTime> CheckDateRange(string timeZoneName, DateTime? startDate, DateTime? endDate)
        {
          //Check date range within 2 years ago to today in project time zone
          var projTimeZone = DateTimeZoneProviders.Tzdb[timeZoneName];
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
          return GetDateRange(startDate.Value, endDate.Value);       
        }

        #endregion

      #region Geofences
        /// <summary>
        /// Retrieves the geofences associated with the project.
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <returns>A list of geofences</returns>
        public static IEnumerable<Geofence> GetGeofences(string projectUid)
        {
          return WithConnection((conn) =>
          {
            var command = @"SELECT GeofenceUID, Name, fk_GeofenceTypeID FROM Geofence
                            WHERE ProjectUID = @projectUid AND IsDeleted = 0 
                                AND (fk_GeofenceTypeID = 1 OR fk_GeofenceTypeID = 10)";

            using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", projectUid)))
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
          return WithConnection((conn) =>
          {
            var command = @"SELECT GeometryWKT FROM Geofence
                            WHERE GeofenceUID = @geofenceUid";

            using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@geofenceUid", geofenceUid)))
            {
              IEnumerable<WGSPoint> latlngs = null;
              while (reader.Read())
              {
                latlngs = ConversionUtil.GeometryToPoints(reader.GetString(0));
              }
              return latlngs;
            }
          });
        }
      #endregion

      #region CCA
        /// <summary>
        /// Saves a CCA summary for a given project, geofence, date, machine and lift
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date</param>
        /// <param name="machineId">Machine ID</param>
        /// <param name="liftId">Lift/Layer ID</param>
        /// <param name="incomplete">Incomplete %</param>
        /// <param name="complete">Complete %</param>
        /// <param name="overcomplete">Over complete %</param>
        /// <returns></returns>
        public static void SaveCCA(string projectUid, string geofenceUid, DateTime date, long machineId, int? liftId, double incomplete, double complete, double overcomplete)
        {
          UpsertCCA(projectUid, geofenceUid, date, machineId, liftId, incomplete, complete, overcomplete, false, false);
        }

        /// <summary>
        /// Marks an entry with "CCA not retrieved" so it can be retried later
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date of the entry</param>
        /// <param name="machineId">Machine ID</param>
        /// <param name="liftId">Lift/Layer ID</param>
        /// <returns></returns>
        public static void MarkCCANotRetrieved(string projectUid, string geofenceUid, DateTime date, long machineId, int? liftId)
        {
          UpsertCCA(projectUid, geofenceUid, date, machineId, liftId, null, null, null, true, false);
        }

        /// <summary>
        /// Marks an entry with "CCA not available" to indicate that there is no CCA information in Raptor for that date
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
        /// <param name="date">Date of the entry</param>
        /// <param name="machineId">Machine ID</param>
        /// <param name="liftId">Lift/Layer ID</param>
        /// <returns></returns>
        public static void MarkCCANotAvailable(string projectUid, string geofenceUid, DateTime date, long machineId, int? liftId)
        {
          UpsertCCA(projectUid, geofenceUid, date, machineId, liftId, null, null, null, false, true);
        }

        /// <summary>
        /// Inserts or updates a CCA entry in the database.
        /// </summary>
        /// <param name="projectUid">Project UID</param>
        /// <param name="geofenceUid">Geofence UID</param>
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
          WithConnection<object>((conn) =>
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
                new MySqlParameter("@incomplete", (object)incomplete ?? DBNull.Value),
                new MySqlParameter("@complete", (object)complete ?? DBNull.Value),
                new MySqlParameter("@overcomplete", (object)overcomplete ?? DBNull.Value),
                new MySqlParameter("@projectUid", projectUid),
                new MySqlParameter("@geofenceUid", geofenceUid),
                new MySqlParameter("@machineId", machineId),
                new MySqlParameter("@liftId", (object)liftId ?? DBNull.Value),
                new MySqlParameter("@date", date),
                new MySqlParameter("@notRetrieved", notRetrieved ? 1 : 0),
                new MySqlParameter("@notAvailable", notAvailable ? 1 : 0));

            return null;
          });  
        }

        /// <summary>
        /// Retrieves a list of entries for which CCA couldn't be retrieved previously (used to retry retrieval)
        /// </summary>
        /// <param name="project">Project for which to retrieve data</param>
        /// <returns>A list of CCA entries</returns>
        public static IEnumerable<CCA> GetEntriesWithCCANotRetrieved(Project project)
        {
          return WithConnection((conn) =>
          {
            // selects entries where CCA retrieval failed OR was never completed
            var command = @"SELECT cca.Date, cca.GeofenceUID, cca.MachineID, cca.LiftID FROM CCA cca 
                              JOIN Geofence geo ON cca.GeofenceUID = geo.GeofenceUID
                            WHERE cca.ProjectUID = @projectUid AND geo.IsDeleted = 0 AND
                            (cca.CCANotRetrieved = 1 OR (cca.Complete IS NULL AND cca.CCANotAvailable = 0) OR 
                            (cca.CCAUpdatedTimestampUTC IS NULL) OR 
                            ( (cca.CCAUpdatedTimestampUTC < SUBDATE(UTC_TIMESTAMP(), INTERVAL 1 DAY)) AND 
                              (cca.CCAUpdatedTimestampUTC > SUBDATE(UTC_TIMESTAMP(), INTERVAL 30 DAY))  ))";

            using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", project.projectUid)))
            {
              var ccaEntries = new List<CCA>();
              while (reader.Read())
              {
                ccaEntries.Add(new CCA
                               {
                                   date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                   geofenceUid = reader.GetString(reader.GetOrdinal("GeofenceUID")),
                                   machineId = reader.GetUInt32(reader.GetOrdinal("MachineID")),
                                   liftId = reader.IsDBNull(reader.GetOrdinal("LiftID")) ? (int?)null : reader.GetInt16(reader.GetOrdinal("LiftID"))
                               });
              }
              return ccaEntries;
            }
          });
        }
      /// <summary>
      /// Gets a list of dates with no CCA data used to retry retrieval.
      /// </summary>
      /// <param name="project">Project for which to retrieve data</param>
      /// <returns>A list of dates</returns>
      public static IEnumerable<DateTime> GetDatesWithNoCCA(Project project)
      {
        return WithConnection((conn) =>
        {
          var command = @"SELECT DISTINCT cca.Date FROM CCA cca 
                          JOIN Geofence geo ON cca.GeofenceUID = geo.GeofenceUID
                          WHERE cca.ProjectUID = @projectUid AND geo.IsDeleted = 0
                          ORDER BY cca.Date";

          //Get the dates that have actual data
          var ccaDates = new List<DateTime>();
          using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectUid", project.projectUid)))
          {
            while (reader.Read())
            {
              ccaDates.Add(reader.GetDateTime(0));
            }
          }
          //If no data at all, do nothing
          if (ccaDates.Count == 0)
            return ccaDates;

          //This is the entire date range
          var dateRange = CheckDateRange(project.timeZoneName, ccaDates.First(), ccaDates.Last()).ToList();
          //Return missing dates
          return (from dr in dateRange where !ccaDates.Contains(dr) select dr).ToList();

        });       
      }

      /// <summary>
      /// Gets CCA data for the project for all machines for all lifts. If date range is not specified, returns data 
      /// for 2 years ago to today in project time zone. If geofence is not specified returns data for 
      /// entire project area otherwise for geofenced area. If machineId is not specified returns data for all machines.
      /// If liftId is not specified returns data for all lifts.
      /// </summary>
      /// <param name="project">Project</param>
      /// <param name="geofenceUid">Geofence UID</param>
      /// <param name="startDate">Start date in project time zone</param>
        /// <param name="endDate">End date in project time zone</param>
        /// <param name="machineId">Machine ID</param>
        /// <param name="liftId">Lift ID</param>
        /// <returns>A list of CCA entries</returns>
      public static IEnumerable<CCA> GetCCA(Project project, string geofenceUid, DateTime? startDate, DateTime? endDate, long? machineId, int? liftId)
      {
        string projectGeofenceUid = UpdateEntriesIfRequired(project, geofenceUid);
        if (string.IsNullOrEmpty(geofenceUid))
          geofenceUid = projectGeofenceUid;

        return WithConnection((conn) =>
        {
          var dateRange = CheckDateRange(project.timeZoneName, startDate, endDate).ToList();
          var firstDate = dateRange.First();
          var lastDate = dateRange.Last();

          //Get the actual data 
          var command = @"SELECT Date, GeofenceUID, MachineID, LiftID, Incomplete, Complete, Overcomplete FROM CCA 
                          WHERE Date >= CAST(@startDate AS DATE) AND Date <= CAST(@endDate AS DATE)
                            AND ProjectUID = @projectUid AND GeofenceUID = @geofenceUid ";
          if (machineId.HasValue)
            command += " AND MachineID = @machineId ";
          if (liftId.HasValue)
            command += " AND LiftID = @liftId ";
          else         
            command += " AND LiftID IS NULL ";
          
          command += " ORDER BY MachineId, Date ";
     
          List<MySqlParameter> parms = new List<MySqlParameter>
                                       {
                                           new MySqlParameter("@projectUid", project.projectUid),
                                           new MySqlParameter("@geofenceUid", geofenceUid),
                                           new MySqlParameter("@startDate", firstDate),
                                           new MySqlParameter("@endDate", lastDate)
                                       };
          if (machineId.HasValue)
            parms.Add(new MySqlParameter("@machineId", machineId));
          if (liftId.HasValue)
            parms.Add(new MySqlParameter("@liftId", liftId));

          var actualData = new List<CCA>();
          using (var reader = MySqlHelper.ExecuteReader(conn, command, parms.ToArray()))
          {
            while (reader.Read())
            {
              actualData.Add(
                new CCA
                {
                  date = reader.GetDateTime(reader.GetOrdinal("Date")),
                  geofenceUid = reader.GetString(reader.GetOrdinal("GeofenceUID")),
                  machineId = reader.GetUInt32(reader.GetOrdinal("MachineID")),
                  liftId = reader.IsDBNull(reader.GetOrdinal("LiftID")) ? (int?)null : reader.GetInt16(reader.GetOrdinal("LiftID")),
                  incomplete = reader.IsDBNull(reader.GetOrdinal("Incomplete")) ? 0 : reader.GetDouble(reader.GetOrdinal("Incomplete")),
                  complete = reader.IsDBNull(reader.GetOrdinal("Complete")) ? 0 : reader.GetDouble(reader.GetOrdinal("Complete")),
                  overcomplete = reader.IsDBNull(reader.GetOrdinal("Overcomplete")) ? 0 : reader.GetDouble(reader.GetOrdinal("Overcomplete"))
                });  
            }
          }
          //Now add the missing data for each machine
          var machineIds = actualData.Select(a => a.machineId).Distinct().ToList();
          foreach (var machId in machineIds)
          {
            var actualDates = actualData.Where(a => a.machineId == machId).Select(d => d.date);
            var missingData = (from dr in dateRange
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
                                        });
            actualData.AddRange(missingData);
          }
          return actualData.OrderBy(a => a.machineId).ThenBy(a => a.date);
        });
      }
      #endregion

      #region Machines
      /// <summary>
      /// Get the ID of the machine with the given details. If it doesn't exist then create it.
      /// </summary>
      /// <param name="details">Machine details</param>
      /// <returns>ID of the machine</returns>
      public static long GetMachineId(MachineDetails details)
      {
        return WithConnection((conn) =>
        {
          var parms = new List<MySqlParameter>
          {
            new MySqlParameter("@assetId", details.assetId),
            new MySqlParameter("@machineName", details.machineName),
            new MySqlParameter("@isJohnDoe", details.isJohnDoe)
          }.ToArray();

          var existingId = GetMachineId(conn, parms, details.machineName);
          if (existingId == 0)
          {
            var command =
                @"INSERT INTO Machine (AssetID, MachineName, IsJohnDoe)
                  VALUES (@assetId, @machineName, @isJohnDoe)";

            MySqlHelper.ExecuteNonQuery(conn, command, parms);
            existingId = GetMachineId(conn, parms, details.machineName);
          }
          return existingId;
        });
      }

      /// <summary>
      /// Gets the ID of a machine from the Landfill database.
      /// </summary>
      /// <param name="sqlConn">SQL database connection</param>
      /// <param name="sqlParams">SQL parameters for the machine to get</param>
      /// <param name="machineName">Machine name. Updates this in database if necessary</param>
      /// <returns>The machine ID</returns>
      private static long GetMachineId(MySqlConnection sqlConn, MySqlParameter[] sqlParams, string machineName)
      {
        //Match on AssetID and IsJohnDoe only as MachineName can change.
        var query = @"SELECT ID, MachineName FROM Machine
                      WHERE AssetID = @assetId AND IsjohnDoe = @isJohnDoe";

        long existingId = 0;
        bool updateName = false;
        using (var reader = MySqlHelper.ExecuteReader(sqlConn, query, sqlParams))
        {
          while (reader.Read())
          {
            existingId = reader.GetUInt32(reader.GetOrdinal("ID"));
            updateName = reader.GetString(reader.GetOrdinal("MachineName")) != machineName;
          }
        }
        if (updateName)
        {
          var command = @"UPDATE Machine SET MachineName = @machineName";
          MySqlHelper.ExecuteNonQuery(sqlConn, command, sqlParams);
        }
        return existingId;
      }

      /*
      /// <summary>
      /// Gets the machine details for the specified machines.
      /// </summary>
      /// <param name="machineIds">IDs of machines to get</param>
      /// <returns>List of machine details</returns>
      public static IEnumerable<MachineDetails> GetMachines(IEnumerable<long> machineIds)
      {
        return WithConnection((conn) =>
        {
          var command = @"SELECT AssetID, MachineName, IsJohnDoe FROM Machine WHERE ID IN @machineIds";

          string machineIdList = string.Format("({0})", string.Join(",", machineIds));
          using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@machineIds", machineIdList)))
          {
            List<MachineDetails> machines = new List<MachineDetails>();
            while (reader.Read())
            {
              machines.Add(new MachineDetails
              {
                assetId = reader.GetUInt32(reader.GetOrdinal("AssetID")),
                machineName = reader.GetString(reader.GetOrdinal("MachineName")),
                isJohnDoe = reader.GetInt16(reader.GetOrdinal("IsJohnDoe")) == 1,
              });

            }
            return machines;
          }
        });                
      }
       */

      /// <summary>
      /// Gets the machine details for the specified machine.
      /// </summary>
      /// <param name="machineId">ID of machine to get</param>
      /// <returns>Machine details</returns>
      public static MachineDetails GetMachine(long machineId)
      {
        return WithConnection((conn) =>
        {
          var command = @"SELECT AssetID, MachineName, IsJohnDoe FROM Machine WHERE ID = @machineId";

          using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@machineId", machineId)))
          {
            MachineDetails machine = null;
            while (reader.Read())
            {
              machine = new MachineDetails
              {
                assetId = reader.GetUInt32(reader.GetOrdinal("AssetID")),
                machineName = reader.GetString(reader.GetOrdinal("MachineName")),
                isJohnDoe = reader.GetInt16(reader.GetOrdinal("IsJohnDoe")) == 1,
              };

            }
            return machine;
          }
        });
      }

      #endregion

    }

}