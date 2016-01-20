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
        /// Get all the volume entries from the mySQL database for the pass year
        /// </summary>
        /// <param name="project"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        public static List<DayEntryAll> GetEntries(Project project, UnitsTypeEnum units)
        {
            return WithConnection(conn =>
            {
                var command = @"select dates.date, entries.weight, entries.volume
                    from (
                        select cast(utc_date() as date)  - interval (-1 + a.a + (10 * b.a) + (100 * c.a)) day as date
                        from (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as a
                        cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as b
                        cross join (select 0 as a union all select 1 union all select 2 union all select 3 union all select 4 union all select 5 union all select 6 union all select 7 union all select 8 union all select 9) as c
                    ) dates
                    left join entries on dates.date = entries.date and entries.projectId = @projectId
                    where dates.date between cast(utc_date() as date) - interval 1 year - interval 1 day and 
                                             cast(utc_date() as date) 
                    order by date";

                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectId", project.id), new MySqlParameter("@timeZone", project.timeZoneName)))
                {
                    return AddEntriesFromMySqlDatabase(units, reader);
                }
            });
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
                var command = @"SELECT * FROM landfill.projects where projectId=" + projectId;

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
            return Convert.ToInt32(ExecuteMySqlQueryResult(connString,"SELECT max(projectId) FROM landfill.projects"));
        }


        /// <summary>
        /// Check my SQL database to see if if the landfill project is there. 
        /// </summary>
        /// <param name="projectName">project name</param>
        public static bool WaitForProjectToBeCreated(string projectName)
        {
            int timeElapsed = 0;
            const int timeout = 10000;
            var queryStr = string.Format("SELECT COUNT(*) FROM landfill.projects WHERE name = '{0}'",  projectName);
            while (Convert.ToInt32(ExecuteMySqlQueryResult(connString, queryStr)) < 1)
            {
                Console.WriteLine("Wait for mySQL landfill projects to show - " + DateTime.Now + " project name: " + projectName);
                Thread.Sleep(200);
                timeElapsed += 200;
                if (timeElapsed > timeout)
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

        /// <summary>
        /// Add the entries into a list with the volumes
        /// </summary>
        /// <param name="units"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static List<DayEntryAll> AddEntriesFromMySqlDatabase(UnitsTypeEnum units, MySqlDataReader reader)
        {            
            var entries = new List<DayEntryAll>();
            try
            {
                IterateThroughMySqlDataReader(units, reader, entries);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return entries;
        }

        /// <summary>
        /// Iterate through the results of the query on the mySQL database
        /// </summary>
        /// <param name="units">Type of units</param>
        /// <param name="reader">mysql data reader</param>
        /// <param name="entries">All entries</param>
        private static void IterateThroughMySqlDataReader(UnitsTypeEnum units, MySqlDataReader reader, List<DayEntryAll> entries)
        {
            while (reader.Read())
            {
                if (reader.IsDBNull(reader.GetOrdinal("weight")) || (reader.IsDBNull(reader.GetOrdinal("volume"))))
                {
                }
                else
                {
                    if (reader.GetDouble(reader.GetOrdinal("volume")) > EPSILON)
                    {
                        DateTime entryDate = reader.GetDateTime(reader.GetOrdinal("date"));
                        double entryWeight = reader.GetDouble(reader.GetOrdinal("weight"));
                        double entryVolume = reader.GetDouble(reader.GetOrdinal("volume"));
                        AddANewDayEntryToCollection(units, entries, entryDate, entryWeight, entryVolume);
                    }
                }
            }
        }

        /// <summary>
        /// Add an entry with the weight, density and volume in entries collection
        /// </summary>
        /// <param name="units">Units type - metric or imperial</param>
        /// <param name="entries">Entries collection</param>
        /// <param name="entryDate">EntryDate</param>
        /// <param name="entryWeight">Daily weight</param>
        /// <param name="entryVolume">Daily volume</param>
        private static void AddANewDayEntryToCollection(UnitsTypeEnum units, List<DayEntryAll> entries, DateTime entryDate, double entryWeight, double entryVolume)
        {
            double density = CalculateTheDensity(units, entryWeight, entryVolume);
            entries.Add(new DayEntryAll
            {
                EntryDate = entryDate,
                EntryPresent = true,
                Weight = entryWeight,
                Density = density,
                Volume = entryVolume
            });
        }

        /// <summary>
        /// Calculate the exact density
        /// </summary>
        /// <param name="units">In units</param>
        /// <param name="entryWeight">Weight from mySql</param>
        /// <param name="entryVolume">Volume from mySql</param>
        /// <returns>density</returns>
        private static double CalculateTheDensity(UnitsTypeEnum units, double entryWeight, double entryVolume)
        {
            double density;
            if (units == UnitsTypeEnum.Metric)
                density = entryWeight * 1000 / entryVolume;
            else
                density = entryWeight * M3_PER_YD3 * POUNDS_PER_TON / entryVolume;
            return density;
        }


    }
}
