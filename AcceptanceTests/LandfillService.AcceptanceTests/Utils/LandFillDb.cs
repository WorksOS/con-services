using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using LandfillService.AcceptanceTests.Models.Landfill;

namespace LandfillService.AcceptanceTests.Utils
{
    public static class LandFillMySqlDb
    {
        /// <summary>
        /// Execute a MySQL query
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public static string ExecuteMySqlQueryResult(string connectionString, string queryString)
        {

            string queryResult = null;
            using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
            {
                mySqlConnection.Open();
                MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
                MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();

                while (mySqlDataReader.Read())
                {
                    queryResult = mySqlDataReader[0].ToString();
                }
            }
            return queryResult;
        }

        /// <summary>
        /// Get the highest project Id in the mySql table
        /// </summary>
        /// <returns>projcet ID</returns>
        public static int GetTheHighestProjectId()
        {
            try
            {
                return Convert.ToInt32(ExecuteMySqlQueryResult(Config.MySqlConnString, "SELECT max(ProjectID) FROM " + Config.MySqlDbName + ".Project"));
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /// <summary>
        /// Check my SQL database to see if if the landfill project is there. 
        /// </summary>
        /// <param name="projectName">project name</param>
        public static bool WaitForProjectToBeCreated(string projectName)
        {
            var retries = 0;
            const int maxRetries = 30;
            var queryStr = string.Format("SELECT COUNT(*) FROM " + Config.MySqlDbName + ".Project WHERE Name = '{0}'", projectName);
            while (Convert.ToInt32(ExecuteMySqlQueryResult(Config.MySqlConnString, queryStr)) < 1)
            {
                Console.WriteLine("Wait for mySQL landfill projects to show - " + DateTime.Now + " project name: " + projectName);
                Thread.Sleep(1000);
                retries++;
                if (retries > maxRetries)
                {
                    return false; 
                }
            }
            return true;
        }
        /// <summary>
        /// Check my SQL database to see if if the landfill project is there. 
        /// </summary>
        /// <param name="projectName">project name</param>
        public static bool WaitForProjectToBeDeleted(string projectName)
        {
            var retries = 0;
            const int maxRetries = 30;
            var queryStr = string.Format("SELECT IsDeleted FROM " + Config.MySqlDbName + ".Project WHERE Name = '{0}'", projectName);
            while (Convert.ToInt32(ExecuteMySqlQueryResult(Config.MySqlConnString, queryStr)) != 1)
            {
                Console.WriteLine("Wait for deleted mySQL landfill project - " + DateTime.Now + " project name: " + projectName);
                Thread.Sleep(1000);
                retries++;
                if (retries > maxRetries)
                {
                    return false; 
                }
            }
            return true;
        }


        ///// <summary>
        ///// Wrapper for generating a MySQL connection
        ///// </summary>
        ///// <param name="body">Code to execute</param>
        ///// <returns>The result of executing body()</returns>
        //private static T WithConnection<T>(Func<MySqlConnection, T> body)
        //{
        //    using (var conn = new MySqlConnection(Config.MySqlConnString))
        //    {
        //        conn.Open();
        //        var res = body(conn);
        //        conn.Close();
        //        return res;
        //    }
        //}
        ///// <summary>
        ///// Using the project ID retrieve all the details for the project for the mySql database
        ///// </summary>
        ///// <param name="projectId"></param>
        ///// <returns>Project</returns>
        //public static Project GetProject(int projectId)
        //{
        //    return WithConnection(conn =>
        //    {
        //        var command = @"SELECT * FROM " + Config.MySqlDbName + ".Project where ProjectID=" + projectId;

        //        using (var reader = MySqlHelper.ExecuteReader(conn, command))
        //        {
        //            return GetProjectDetailsFromMySql(reader);
        //        }
        //    });
        //}
        //private static Project GetProjectDetailsFromMySql(MySqlDataReader reader)
        //{
        //    Project projectDetails = new Project();
        //    while (reader.Read())
        //    {
        //        projectDetails.id = reader.GetUInt32("projectId");
        //        projectDetails.name = reader.GetString("name");
        //        projectDetails.timeZoneName = reader.GetString("timeZone");
        //        projectDetails.daysToSubscriptionExpiry = reader.GetInt32("daysToSubscriptionExpiry");
        //        break;
        //    }
        //    return projectDetails;
        //}
    }
}
