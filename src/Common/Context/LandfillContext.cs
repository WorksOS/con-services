using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        #region(UsersAndSessions)

        /// <summary>
        /// Retrieves the user object for a given name (and creates a DB record if needed)
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>User object</returns>
        public static User CreateOrGetUser(string userName, int units)
        {
            return WithConnection((conn) =>
            {
                // supply a dummy value for projectsRetrievedAt such that it indicates that projects have to be retrieved
                var command = "insert ignore into users (name, projectsRetrievedAt, unitsId) values (@name, date_sub(UTC_TIMESTAMP(), interval 10 year), @units)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@name", userName), new MySqlParameter("@units", units));

                command = "update users set unitsId = @units where name = @name";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@name", userName), new MySqlParameter("@units", units));


                command = "select * from users where name = @name";
                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@name", userName)))
                {
                    reader.Read();
                    var user = new User
                               {
                                   id = reader.GetUInt32(reader.GetOrdinal("userId")), name = reader.GetString(reader.GetOrdinal("name")),
                                   unitsId = reader.GetUInt32(reader.GetOrdinal("unitsId"))
                               };
                    return user;
                }
            });
        }

        /// <summary>
        /// Saves the session ID for a given user
        /// </summary>
        /// <param name="user">User object</param>
        /// <param name="sessionId">Session ID</param>
        /// <returns></returns>
        public static void SaveSession(User user, string sessionId)
        {
            WithConnection<object>((conn) =>
            {
                var command = "insert ignore into sessions (userId, sessionId) values (@userId, @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@userId", user.id), new MySqlParameter("@sessionId", sessionId));
                return null;
            });
        }

        /// <summary>
        /// Deletes the given session ID from the DB
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns></returns>
        public static void DeleteSession(string sessionId)
        {
            WithConnection<object>((conn) =>
            {
                var command = "delete from sessions where sessionId = @sessionId";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));
                return null;
            });
        }

        /// <summary>
        /// Deletes sessions older than 30 days from the DB
        /// </summary>
        /// <returns></returns>
        public static void DeleteStaleSessions()
        {
            WithConnection<object>((conn) =>
            {
              var command = "delete from sessions where createdAt < date_sub(UTC_TIMESTAMP(), interval 30 day)";
                MySqlHelper.ExecuteNonQuery(conn, command);
                return null;
            });
        }

        /// <summary>
        /// Retrieves the session with a given ID
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Session object</returns>
        public static Session GetSession(string sessionId)
        {
            return WithConnection((conn) =>
            {
                var command = "select * from sessions where sessionId = @sessionId";
                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@sessionId", sessionId)))
                {
                    if (!reader.Read() || !reader.HasRows)
                    {
                        reader.Close();
                        throw new ApplicationException("Invalid session " + sessionId);
                    }
                    var session = new Session { id = reader.GetString(reader.GetOrdinal("sessionId")), userId = reader.GetUInt32(reader.GetOrdinal("userId")) };
                    return session;
                }
            });
        }

        #endregion

        #region(Projects)

        /// <summary>
        /// Saves a list of projects to the DB
        /// </summary>
        /// <param name="sessionId">Session ID used to associate projects with a user</param>
        /// <param name="projects">List of projects</param>
        /// <returns></returns>
        public static void SaveProjects(string sessionId, IEnumerable<Project> projects)
        {
            InTransaction<object>((conn) =>
            {
                // delete any existing projects associated with the user
                var command = "delete from usersprojects where userId = (select userId from sessions where sessionId = @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));

                foreach (var project in projects)
                {
                  command = @"insert into projects (projectId, name, timeZone, retrievalStartedAt, daysToSubscriptionExpiry) 
                                values (@projectId, @name, @timeZone, date_sub(UTC_TIMESTAMP(), interval 10 year), @daysToSubscriptionExpiry)
                                    on duplicate key update name = @name, timeZone = @timeZone, daysToSubscriptionExpiry = @daysToSubscriptionExpiry";
 
                  MySqlHelper.ExecuteNonQuery(conn, command,
                      new MySqlParameter("@projectId", project.id),
                      new MySqlParameter("@name", project.name),
                      new MySqlParameter("@timeZone", project.timeZoneName),
                      new MySqlParameter("@daysToSubscriptionExpiry", project.daysToSubscriptionExpiry));

                    command = @"insert into usersprojects (userId, projectId) 
                                values ((select userId from sessions where sessionId = @sessionId), @projectId)";

                    MySqlHelper.ExecuteNonQuery(conn, command,
                        new MySqlParameter("@sessionId", sessionId),
                        new MySqlParameter("@projectId", project.id));

                }

                command = @"update users set projectsRetrievedAt = UTC_TIMESTAMP()
                            where userId = (select userId from sessions where sessionId = @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));

                return null;
            });
        }

        /// <summary>
        /// Retrieves a list of projects for a given user (via session ID)
        /// </summary>
        /// <param name="sessionId">Session ID used to associate projects with a user</param>
        /// <returns>A list of projects</returns>
        public static IEnumerable<Project> GetProjects(string sessionId)
        {
            return InTransaction((conn) =>
            {
                var command = @"select * from projects where projectId in 
                                (select projectId from usersprojects where userId = (select userId from sessions where sessionId = @sessionId)) 
                                order by name";
                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@sessionId", sessionId)))
                {
                    var projects = new List<Project>();
                    while (reader.Read())
                    {
                      int indxExpiry = reader.GetOrdinal("daysToSubscriptionExpiry");
                      bool hasExpiry = !reader.IsDBNull(indxExpiry);

                        projects.Add(new Project { id = reader.GetUInt32(reader.GetOrdinal("projectId")), 
                                                   name = reader.GetString(reader.GetOrdinal("name")),
                                                   timeZoneName = reader.GetString(reader.GetOrdinal("timeZone")),
                                                   daysToSubscriptionExpiry = hasExpiry ? reader.GetInt32(indxExpiry) : (int?)null
                        });
                    }
                    return projects;
                }
            });
        }

   

      public static void AddDaysToSubscriptionExpiryToProjects()
      {
         /* WithConnection<object>((conn) =>
          {
            var command = @"select count(*) from information_schema.columns
                          where table_schema = 'landfill' and table_name = 'projects' and column_name = 'daysToSubscriptionExpiry'";
            var result = MySqlHelper.ExecuteScalar(conn, command);
            bool exists = Convert.ToUInt32(result) > 0;
            if (!exists)
            {
              command = @"alter table projects add daysToSubscriptionExpiry int";//nullable
              result = MySqlHelper.ExecuteNonQuery(conn, command);
            }
            return null;
          });*/
      }

        /// <summary>
        /// Retrieves the age of the project list for a given user (found via session ID)
        /// </summary>
        /// <param name="sessionId">Session ID used to associate projects with a user</param>
        /// <returns>Age of project list in hours</returns>
        public static ulong GetProjectListAgeInHours(string sessionId)
        {
            return WithConnection((conn) =>
            {
                var command = @"select timestampdiff(hour, projectsRetrievedAt, UTC_TIMESTAMP()) as hours 
                                from users where userId = (select userId from sessions where sessionId = @sessionId)";
                var result = MySqlHelper.ExecuteScalar(conn, command, new MySqlParameter("@sessionId", sessionId)); 
                return Convert.ToUInt32(result);
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
                var command = @"select count(*) from projects 
                                where projectId = @projectId and (retrievalStartedAt >= date_sub(UTC_TIMESTAMP(), interval " + lockTimeout.ToString() + " hour) or " +
                                "(select count(*) from entries where projectId = @projectId and volume is null and volumeNotAvailable = 0) > 0)";

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
                    @"update projects set retrievalStartedAt = UTC_TIMESTAMP()
                      where projectId = @projectId and retrievalStartedAt < date_sub(UTC_TIMESTAMP(), interval " + lockTimeout.ToString() + " hour)"
                    :
                    @"update projects set retrievalStartedAt = date_sub(UTC_TIMESTAMP(), interval 10 year)
                      where projectId = @projectId and retrievalStartedAt >= date_sub(UTC_TIMESTAMP(), interval " + lockTimeout.ToString() + " hour)";

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
                var command = @"update projects set retrievalStartedAt = date_sub(UTC_TIMESTAMP(), interval 10 year)";
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
                var command = @"insert into entries (projectId, date, weight) values (@projectId, @date, @weight) 
                                on duplicate key update weight = @weight";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", entry.date),
                    new MySqlParameter("@weight", entry.weight));

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
              var command = @"update entries set volume = greatest(@volume, 0.0), volumeNotRetrieved = 0, volumeNotAvailable = 0, volumesUpdatedTimestamp = UTC_TIMESTAMP()
                                where projectId = @projectId and date = @date";

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
                var command = "update entries set volumeNotRetrieved = 1 where projectId = @projectId and date = @date";

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
          var command = @"SELECT distinct prj.projectId, prj.timeZone FROM landfill.projects as prj left join landfill.entries as etr on prj.projectId = etr.projectId where weight is not null;";
          using (var reader = MySqlHelper.ExecuteReader(conn, command))
          {
            var projects = new List<Project>();
            while (reader.Read())
            {
              projects.Add(new Project
              {
                id = reader.GetUInt32(reader.GetOrdinal("projectId")),
                timeZoneName = reader.GetString(reader.GetOrdinal("timeZone"))
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
              var command = @"update entries set volumeNotAvailable = 1, volumeNotRetrieved = 0, volumesUpdatedTimestamp = UTC_TIMESTAMP()
                                where projectId = @projectId and date = @date";

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
              var command = "select date from entries where projectId = @projectId and (volumeNotRetrieved = 1 or (volume is null and volumeNotAvailable = 0) or (volumesUpdatedTimestamp is null) or ( (volumesUpdatedTimestamp < SUBDATE(UTC_TIMESTAMP(), INTERVAL 1 DAY)) and (volumesUpdatedTimestamp > SUBDATE(UTC_TIMESTAMP(), INTERVAL 30 DAY))  ))";

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
        public static IEnumerable<DayEntry> GetEntries(Project project, UnitsTypeEnum units)
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
                                              density = 0.0
                                          }).ToDictionary(k => k.date, v => v);
                //Now get the actual data and merge
                var command = @"select entries.date, entries.weight, entries.volume
                    from entries 
                    where entries.date >= cast(@startDate as date) and entries.date <= cast(@endDate as date)
                          and entries.projectId = @projectId
                    order by date";

                using (var reader = MySqlHelper.ExecuteReader(conn, command,
                  new MySqlParameter("@projectId", project.id), 
                  new MySqlParameter("@startDate", twoYearsAgo), 
                  new MySqlParameter("@endDate", yesterday)))
                {
                    const double POUNDS_PER_TON = 2000.0;
                    const double M3_PER_YD3 = 0.7645555;
                    const double EPSILON = 0.001;

                    while (reader.Read())
                    {
                          DateTime date = reader.GetDateTime(reader.GetOrdinal("date"));
                          DayEntry entry = entriesLookup[date];
                          entry.entryPresent = true;
                          entry.weight = reader.GetDouble(reader.GetOrdinal("weight"));
                          double density = 0.0;
                          if (!reader.IsDBNull(reader.GetOrdinal("volume")) && reader.GetDouble(reader.GetOrdinal("volume")) > EPSILON)
                              if (units == UnitsTypeEnum.Metric)
                                density = reader.GetDouble(reader.GetOrdinal("weight")) * 1000 / reader.GetDouble(reader.GetOrdinal("volume"));
                              else
                                density = reader.GetDouble(reader.GetOrdinal("weight")) * M3_PER_YD3 * POUNDS_PER_TON / reader.GetDouble(reader.GetOrdinal("volume"));
                          entry.density = density;                        
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

      public static UnitsTypeEnum GetUnits(string sessionId)
      {
        return (UnitsTypeEnum) WithConnection((conn) =>
                                              {

                                                var command = "select unitsId from users left join landfill.sessions on users.userId = sessions.userId where sessions.sessionId = @sessionId";

                                                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@sessionId", sessionId)))
                                                {
                                                  reader.Read();
                                                  return reader.GetUInt16(0);
                                                }
                                              });
      }
    }

}