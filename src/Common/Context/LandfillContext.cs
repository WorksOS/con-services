using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using NodaTime;

namespace LandfillService.WebApi.Models
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
              var command = @"SELECT prj.ProjectID, prj.Name, prj.TimeZone, 
                                     sub.StartDate AS SubStartDate, sub.EndDate AS SubEndDate 
                              FROM Project prj  
                              JOIN CustomerUser cu ON prj.CustomerUID = cu.fk_CustomerUID
                              JOIN Subscription sub ON prj.SubscriptionUID = sub.SubscriptionUID
                              WHERE cu.fk_UserUID = @userUid;";
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
                                                   name = reader.GetString(reader.GetOrdinal("Name")),
                                                   timeZoneName = reader.GetString(reader.GetOrdinal("TimeZone")),
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
        /// <param name="project">Project</param>
        /// <param name="entry">Weight entry from the client</param>
        /// <returns></returns>
        public static void SaveEntry(uint projectId, WeightEntry entry)
        {
            WithConnection<object>((conn) =>
            {
                var command = @"INSERT INTO Entries (ProjectID, Date, Weight, InsertUTC, UpdateUTC) 
                                VALUES (@projectId, @date, @weight, @insertUtc, @updateUtc) 
                                ON DUPLICATE KEY UPDATE Weight = @weight";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", entry.date),
                    new MySqlParameter("@weight", entry.weight),
                    new MySqlParameter("@insertUtc", DateTime.UtcNow),
                    new MySqlParameter("@updateUtc", DateTime.UtcNow));

                return null;
            });
        }


        /// <summary>
        /// Saves a volume for a given project and date
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="date">Date</param>
        /// <param name="volume">Volume</param>
        /// <returns></returns>
        public static void SaveVolume(uint projectId, DateTime date, double volume)
        {
            WithConnection<object>((conn) =>
            {
                // replace negative volumes with 0; they are possible (e.g. due to extra compaction 
                // without new material coming in) but don't make sense in the context of the application
              var command = @"UPDATE Entries 
                              SET Volume = GREATEST(@volume, 0.0), VolumeNotRetrieved = 0, 
                                  VolumeNotAvailable = 0, VolumesUpdatedTimestamp = UTC_TIMESTAMP()
                              WHERE ProjectID = @projectId AND Date = @date";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@volume", volume),
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", date));

                return null;
            });
        }

        /// <summary>
        /// Marks an entry with "volume not retrieved" so it can be retried later
        /// </summary>
        /// <param name="projectId">Project id</param>
        /// <param name="date">Date of the entry</param>
        /// <returns></returns>
        public static void MarkVolumeNotRetrieved(uint projectId, DateTime date)
        {
            WithConnection<object>((conn) =>
            {
                var command = "UPDATE Entries SET VolumeNotRetrieved = 1 WHERE ProjectID = @projectId AND Date = @date";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", date));

                return null;
            });
        }


      public static List<Project> GetListOfAvailableProjects()
      {
        return InTransaction((conn) =>
        {
          var command = @"SELECT DISTINCT prj.ProjectID, prj.TimeZone 
                          FROM Project prj 
                          LEFT JOIN Entries etr ON prj.ProjectID = etr.ProjectID 
                          WHERE Weight IS NOT NULL;";
          using (var reader = MySqlHelper.ExecuteReader(conn, command))
          {
            var projects = new List<Project>();
            while (reader.Read())
            {
              projects.Add(new Project
              {
                id = reader.GetUInt32(reader.GetOrdinal("ProjectID")),
                timeZoneName = reader.GetString(reader.GetOrdinal("TimeZone"))
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
        /// <param name="date">Date of the entry</param>
        /// <returns></returns>
        public static void MarkVolumeNotAvailable(uint projectId, DateTime date)
        {
            WithConnection<object>((conn) =>
            {
              var command = @"UPDATE Entries 
                              SET VolumeNotAvailable = 1, VolumeNotRetrieved = 0, VolumesUpdatedTimestamp = UTC_TIMESTAMP()
                              WHERE ProjectID = @projectId AND Date = @date";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", date));

                return null;
            });
        }

        /// <summary>
        /// Retrieves a list of dates for which volumes couldn't be retrieved previously (used to retry retrieval)
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <returns>A list of dates</returns>
        public static IEnumerable<DateTime> GetDatesWithVolumesNotRetrieved(uint projectId)
        {
            return WithConnection((conn) =>
            {
                // selects dates where volume retrieval failed OR was never completed OR hasn't yet happened;
                // note that this can cause overlap with newly added dates which are currently waiting to be handled by 
                // a volume retrieval task; this is acceptable because the probability of such overlap is expected to be low
                // BUT it allows the service to tolerate background tasks dying
              var command = @"SELECT Date FROM Entries
                            WHERE ProjectID = @projectId AND 
                            (VolumeNotRetrieved = 1 OR (Volume IS NULL AND VolumeNotAvailable = 0) OR 
                            (VolumesUpdatedTimestamp IS NULL) OR 
                            ( (VolumesUpdatedTimestamp < SUBDATE(UTC_TIMESTAMP(), INTERVAL 1 DAY)) AND 
                              (VolumesUpdatedTimestamp > SUBDATE(UTC_TIMESTAMP(), INTERVAL 30 DAY))  ))";

                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectId", projectId)))
                {
                    var dates = new List<DateTime>();
                    while (reader.Read())
                    {
                        dates.Add(reader.GetDateTime(0));
                    }
                    return dates;
                }
            });
        }

        /// <summary>
        /// Retrieves data entries for a given project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>A list of data entries</returns>
        public static IEnumerable<DayEntry> GetEntries(Project project)
        {
            return WithConnection((conn) =>
            {
                //Create last 2 years of dates in project time zone
                var projTimeZone = DateTimeZoneProviders.Tzdb[project.timeZoneName];
                DateTime utcNow = DateTime.UtcNow;
                Offset projTimeZoneOffsetFromUtc = projTimeZone.GetUtcOffset(Instant.FromDateTimeUtc(utcNow));
                DateTime todayinProjTimeZone = (utcNow + projTimeZoneOffsetFromUtc.ToTimeSpan()).Date;
                //Date range is 2 years ago to yesterday
                DateTime twoYearsAgo = todayinProjTimeZone.AddYears(-2);
                DateTime yesterday = todayinProjTimeZone.AddDays(-1);
                var dateRange = GetDateRange(twoYearsAgo, yesterday);

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
                                AND ProjectID = @projectId
                                ORDER BY Date";

                using (var reader = MySqlHelper.ExecuteReader(conn, command,
                  new MySqlParameter("@projectId", project.id), 
                  new MySqlParameter("@startDate", twoYearsAgo), 
                  new MySqlParameter("@endDate", yesterday)))
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

        public static IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
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

    }

}