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
        public static int GetTheHighestMachineId()
        {
            try
            {
                return Convert.ToInt32(ExecuteMySqlQueryResult(Config.MySqlConnString, "SELECT max(ID) FROM " + Config.MySqlDbName + ".Machine"));
            }
            catch (Exception)
            {
                return 0;
            }
        }

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
    }
}
