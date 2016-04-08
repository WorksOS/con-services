using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using LandfillService.AcceptanceTests.Models;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class DayEntryAll
    {
        public DateTime EntryDate { get; set; }
        public bool EntryPresent { get; set; }    // true if the entry has at least the weight value
        public double Density { get; set; }       // weight / volume        
        public double Weight { get; set; }
        public double Volume { get; set; }
    }

    public static class LandFillMySqlDb
    {
        private static readonly string connString = ConfigurationManager.ConnectionStrings["LandfillContext"].ConnectionString;
        private const double POUNDS_PER_TON = 2000.0;
        private const double M3_PER_YD3 = 0.7645555;
        private const double EPSILON = 0.001;
        private static readonly string mySqlDbName = ConfigurationManager.AppSettings["MySqlDBName"];
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

        public static void ExecuteMySqlCommand(string connectionString, string commandString)
        {
            using (MySqlConnection mySqlConnection = new MySqlConnection(connectionString))
            {
                //Open connection 
                mySqlConnection.Open();

                MySqlCommand mySqlCommand = new MySqlCommand(commandString, mySqlConnection);
                mySqlCommand.ExecuteNonQuery();
            }

        }
        
        /// <summary>
        /// Using the project ID retrieve all the details for the project for the mySql database
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns>Project</returns>
        public static Project GetProject(int projectId)
        {
            return WithConnection(conn =>
            {
                var command = @"SELECT * FROM " + mySqlDbName + ".projects where projectId=" + projectId;

                using (var reader = MySqlHelper.ExecuteReader(conn, command))
                {
                    return GetProjectDetailsFromMySql(reader);
                }
            });
        }

        /// <summary>
        /// Get the highest project Id in the mySql table
        /// </summary>
        /// <returns>projcet ID</returns>
        public static int GetTheHighestProjectId()
        {
            try
            {
                return Convert.ToInt32(ExecuteMySqlQueryResult(connString, "SELECT max(projectId) FROM " + mySqlDbName + ".projects"));
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
            var queryStr = string.Format("SELECT COUNT(*) FROM " + mySqlDbName + ".projects WHERE name = '{0}'", projectName);
            while (Convert.ToInt32(ExecuteMySqlQueryResult(connString, queryStr)) < 1)
            {
                Console.WriteLine("Wait for mySQL landfill projects to show - " + DateTime.Now + " project name: " + projectName);
                Thread.Sleep(1000);
                retries++;
                if (retries > maxRetries)
                    { return false; }
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
            var queryStr = string.Format("SELECT IsDeleted FROM " + mySqlDbName + ".projects WHERE name = '{0}'", projectName);
            while (Convert.ToInt32(ExecuteMySqlQueryResult(connString, queryStr)) != 1)
            {
                Console.WriteLine("Wait for deleted mySQL landfill project - " + DateTime.Now + " project name: " + projectName);
                Thread.Sleep(1000);
                retries++;
                if (retries > maxRetries)
                { return false; }
            }
            return true;
        }


        private static Project GetProjectDetailsFromMySql(MySqlDataReader reader)
        {
            Project projectDetails = new Project();
            while (reader.Read())
            {
                projectDetails.id = reader.GetUInt32("projectId");
                projectDetails.name = reader.GetString("name");
                projectDetails.timeZoneName = reader.GetString("timeZone");
                projectDetails.daysToSubscriptionExpiry = reader.GetInt32("daysToSubscriptionExpiry");
                break;
            }
            return projectDetails;
        }
    }
}
