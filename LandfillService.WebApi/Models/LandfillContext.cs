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

        public static User CreateOrGetUser(Credentials credentials)
        {
            return WithConnection((conn) =>
            {
                var command = "insert ignore into users (name) values (@name)";
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

        public static void SaveProjects(string sessionId, IEnumerable<Project> projects)
        {
            InTransaction<object>((conn) =>
            {
                var session = GetSession(sessionId);

                var command = "delete from projects where userId = (select userId from sessions where sessionId = @sessionId)";
                MySqlHelper.ExecuteNonQuery(conn, command, new MySqlParameter("@sessionId", sessionId));

                foreach (Project project in projects)
                {
                    command = "insert into projects (userId, projectId, name) values (@userId, @projectId, @name)";
                    MySqlHelper.ExecuteNonQuery(conn, command,
                        new MySqlParameter("@userId", session.userId),
                        new MySqlParameter("@projectId", project.id),
                        new MySqlParameter("@name", project.name));
                }

                //MySqlHelper.ExecuteNonQuery(conn, "select * from notatables");

                return null;
            });
        }

        public static IEnumerable<Project> GetProjects(string sessionId)
        {
            return InTransaction((conn) =>
            {
                var command = "select * from projects where userId = (select userId from sessions where sessionId = @sessionId) order by name";
                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@sessionId", sessionId)))
                {
                    var projects = new List<Project>();
                    while (reader.Read())
                    {
                        projects.Add(new Project { id = reader.GetUInt32(reader.GetOrdinal("id")), name = reader.GetString(reader.GetOrdinal("name")) });
                    }
                    //reader.Close();
                    return projects;
                }
            });
        }
    }
}