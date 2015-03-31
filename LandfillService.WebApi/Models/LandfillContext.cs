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

        public static User CreateOrGetUser(Credentials credentials)
        {
            var command = "insert ignore into users (name) values (@name)";
            MySqlHelper.ExecuteNonQuery(connString, command, new MySqlParameter("@name", credentials.userName));

            command = "select * from users where name = @name";
            var result = MySqlHelper.ExecuteDataRow(connString, command, new MySqlParameter("@name", credentials.userName));
            return new User {userId = result.Field<uint>("userId"), name = result.Field<string>("name")};
        }

        public static void SaveSession(User user, string sessionId)
        {
            var command = "insert into sessions (userId, sessionId) values (@userId, @sessionId)";
            MySqlHelper.ExecuteNonQuery(connString, command, new MySqlParameter("@userId", user.userId), new MySqlParameter("@sessionId", sessionId));
        }

        public static void DeleteSession(string sessionId)
        {
            var command = "delete from sessions where sessionId = @sessionId";
            MySqlHelper.ExecuteNonQuery(connString, command, new MySqlParameter("@sessionId", sessionId));
        }

        public static DataRow GetSession(string sessionId)
        {
            var command = "select * from sessions where sessionId = @sessionId";
            return MySqlHelper.ExecuteDataRow(connString, command, new MySqlParameter("@sessionId", sessionId));
        }


    }
}