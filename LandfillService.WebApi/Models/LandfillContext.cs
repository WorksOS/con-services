using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;

namespace LandfillService.WebApi.Models
{
    //[DbConfigurationType(typeof(MySqlEFConfiguration))]
    //public class LandfillContext : DbContext
    //{
    //    public LandfillContext() : base("name=LandfillContext")
    //    {
    //    }
 
    //    public DbSet<Session> Sessions { get; set; }

    //    void OnCreateModel()
    //    {
    //        // this is supposed to help recover from transient connection issues:s
    //        //SetExecutionStrategy(MySqlProviderInvariantName.ProviderName, () => new MySqlExecutionStrategy());
    //    }
    //}

    public class LandfillDb
    {
        private static string connString = ConfigurationManager.ConnectionStrings["LandfillContext"].ConnectionString;

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

        public static User CreateOrGetUser(Credentials credentials)
        {
            return WithConnection((conn) =>
            {
                // supply a dummy value for projectsRetrievedAt such that it indicates that projects have to be retrieved
                var command = "insert ignore into users (name, projectsRetrievedAt) values (@name, date_sub(now(), interval 10 year))";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@name", credentials.userName));

                command = "select * from users where name = @name";
                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@name", credentials.userName)))
                {
                    reader.Read();
                    var user = new User { id = reader.GetUInt32(reader.GetOrdinal("userId")), name = reader.GetString(reader.GetOrdinal("name")) };
                    //reader.Close();
                    return user;
                }
            });
        }

        public static void SaveSession(User user, string sessionId)
        {
            WithConnection<object>((conn) =>
            {
                var command = "insert into sessions (userId, sessionId) values (@userId, @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@userId", user.id), new MySqlParameter("@sessionId", sessionId));
                return null;
            });
        }

        public static void DeleteSession(string sessionId)
        {
            WithConnection<object>((conn) =>
            {
                var command = "delete from sessions where sessionId = @sessionId";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));
                return null;
            });
        }

        public static void DeleteStaleSessions()
        {
            WithConnection<object>((conn) =>
            {
                var command = "delete from sessions where createdAt < date_sub(now(), interval 30 day)";
                MySqlHelper.ExecuteNonQuery(conn, command);
                return null;
            });
        }

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
                    //reader.Close();
                    return session;
                }
            });
        }

        #endregion

        #region(Projects)

        public static void SaveProjects(string sessionId, IEnumerable<Project> projects)
        {
            InTransaction<object>((conn) =>
            {
                var command = "delete from usersprojects where userId = (select userId from sessions where sessionId = @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));

                foreach (var project in projects)
                {
                    command = @"insert into projects (projectId, name, timeZone) values (@projectId, @name, @timeZone)
                                    on duplicate key update name = @name, timeZone = @timeZone";
                    MySqlHelper.ExecuteNonQuery(conn, command,
                        new MySqlParameter("@projectId", project.id),
                        new MySqlParameter("@name", project.name),
                        new MySqlParameter("@timeZone", project.timeZoneName));

                    command = @"insert into usersprojects (userId, projectId) 
                                values ((select userId from sessions where sessionId = @sessionId), @projectId)";

                    MySqlHelper.ExecuteNonQuery(conn, command,
                        new MySqlParameter("@sessionId", sessionId),
                        new MySqlParameter("@projectId", project.id));

                }

                command = @"update users set projectsRetrievedAt = now()
                            where userId = (select userId from sessions where sessionId = @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));

                //MySqlHelper.ExecuteNonQuery(conn, "select * from notatables");

                return null;
            });
        }

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
                        projects.Add(new Project { id = reader.GetUInt32(reader.GetOrdinal("projectId")), 
                                                   name = reader.GetString(reader.GetOrdinal("name")),
                                                   timeZoneName = reader.GetString(reader.GetOrdinal("timeZone")) });
                    }
                    //reader.Close();
                    return projects;
                }
            });
        }

        public static ulong GetProjectListAgeInHours(string sessionId)
        {
            return WithConnection((conn) =>
            {
                var command = @"select timestampdiff(hour, projectsRetrievedAt, now()) as hours 
                                from users where userId = (select userId from sessions where sessionId = @sessionId)";
                var result = MySqlHelper.ExecuteScalar(conn, command, new MySqlParameter("@sessionId", sessionId)); 
                return Convert.ToUInt32(result);
            });
        }

        #endregion

        #region(Entries)

        public static void SaveEntry(uint projectId, WeightEntry entry)
        {
            WithConnection<object>((conn) =>
            {
                // on update, the id has to be updated in order to get the correct value out of last_insert_id() later
//                var command = @"insert into entries (projectId, date, weight) values (@projectId, @date, @weight) 
//                              on duplicate key update entryId = last_insert_id(entryId), weight = @weight";

                var command = @"insert into entries (projectId, date, weight) values (@projectId, @date, @weight) 
                                on duplicate key update weight = @weight";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", entry.date),
                    new MySqlParameter("@weight", entry.weight));

                //command = "select last_insert_id()";  // last id is tracked on a per-connection basis
                //var entryId = (uint)MySqlHelper.ExecuteScalar(conn, command);
                return null;
            });
        }

        public static void SaveVolume(uint projectId, DateTime date, double volume)
        {
            WithConnection<object>((conn) =>
            {
                // replace negative volumes with 0; they are possible (e.g. due to extra compaction 
                // without new material coming in) but don't make sense in the context of the application
                var command = @"update entries set volume = greatest(@volume, 0.0), volumeNotRetrieved = 0, volumeNotAvailable = 0 
                                where projectId = @projectId and date = @date";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@volume", volume),
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", date));

                return null;
            });
        }

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

        public static void MarkVolumeNotAvailable(uint projectId, DateTime date)
        {
            WithConnection<object>((conn) =>
            {
                var command = @"update entries set volumeNotAvailable = 1, volumeNotRetrieved = 0 
                                where projectId = @projectId and date = @date";

                MySqlHelper.ExecuteNonQuery(conn, command,
                    new MySqlParameter("@projectId", projectId),
                    new MySqlParameter("@date", date));

                return null;
            });
        }

        public static IEnumerable<DateTime> GetDatesWithVolumesNotRetrieved(uint projectId)
        {
            return WithConnection((conn) =>
            {
                var command = "select date from entries where projectId = @projectId and volumeNotRetrieved = 1";

                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectId", projectId)))
                {
                    var dates = new List<DateTime>();
                    while (reader.Read())
                    {
                        dates.Add(reader.GetDateTime(0));
                    }
                    //reader.Close();
                    return dates;
                }
            });
        }

        public static IEnumerable<DayEntry> GetEntries(Project project)
        {
            return WithConnection((conn) =>
            {
//                var command = @"select * from entries where projectId = @projectId and 
//                                volume is not null and volumeNotRetrieved = 0 and volumeNotAvailable = 0
//                                order by date";

                // The subquery generates a list of dates for the last two years
                var command = @"select dates.date, entries.weight, entries.volume
                    from (
                        select cast(convert_tz(curdate(), @@global.time_zone, @timeZone) as date)  - interval (-1 + a.a + (10 * b.a) + (100 * c.a)) day as date
                        from (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as a
                        cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as b
                        cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as c
                    ) dates
                    left join entries on dates.date = entries.date and entries.projectId = @projectId and 
                                         entries.volume is not null and entries.volumeNotRetrieved = 0 and entries.volumeNotAvailable = 0
                    where dates.date between cast(convert_tz(curdate(), @@global.time_zone, @timeZone) as date) - interval 2 year - interval 1 day and 
                                             cast(convert_tz(curdate(), @@global.time_zone, @timeZone) as date) - interval 1 day
                    order by date";

                using (var reader = MySqlHelper.ExecuteReader(conn, command, 
                    new MySqlParameter("@projectId", project.id), new MySqlParameter("@timeZone", project.timeZoneName)))
                {
                    const double POUNDS_PER_TON = 2000.0;
                    const double M3_PER_YD3 = 0.7645555;
                    const double EPSILON = 0.001;

                    var entries = new List<DayEntry>();
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(reader.GetOrdinal("weight")))
                            entries.Add(new DayEntry
                            {
                                date = reader.GetDateTime(reader.GetOrdinal("date")),
                                entryPresent = false,
                                weight = 0.0,
                                density = 0.0
                            });
                        else
                        {
                            double density = 0.0;
                            if (reader.GetDouble(reader.GetOrdinal("volume")) > EPSILON)
                                density = reader.GetDouble(reader.GetOrdinal("weight")) * POUNDS_PER_TON * M3_PER_YD3 / reader.GetDouble(reader.GetOrdinal("volume"));
                            entries.Add(new DayEntry
                            {
                                date = reader.GetDateTime(reader.GetOrdinal("date")),
                                entryPresent = true,
                                weight = reader.GetDouble(reader.GetOrdinal("weight")),
                                density = density
                            });
                        }
                     }
                    //reader.Close();
                    return entries;
                }
            });
        }

        #endregion

    }

}