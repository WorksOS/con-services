using LandfillService.WebApi.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace LandfillService.AcceptanceTests.Helpers
{
    public class DayEntryAll
    {
        public DateTime date { get; set; }
        public bool entryPresent { get; set; }    // true if the entry has at least the weight value
        public double density { get; set; }       // weight / volume        
        public double weight { get; set; }
        public double volume { get; set; }
    }

    public class LandFillMySqlDb
    {
        private string connString = ConfigurationManager.ConnectionStrings["LandfillContext"].ConnectionString;
        private const double POUNDS_PER_TON = 2000.0;
        private const double M3_PER_YD3 = 0.7645555;
        private const double EPSILON = 0.001;
        /// <summary>
        /// Wrapper for generating a MySQL connection
        /// </summary>
        /// <param name="body">Code to execute</param>
        /// <returns>The result of executing body()</returns>
        private T WithConnection<T>(Func<MySqlConnection, T> body)
        {
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                var res = body(conn);
                conn.Close();
                return res;
            }
        }

        public List<DayEntryAll> GetEntries(Project project, UnitsTypeEnum units)
        {
            return WithConnection((conn) =>
            {
                // The subquery generates a list of dates for the last two years so that the query returns all dates 
                // regardless of what entries are available for the project
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

       //                             where dates.date between cast(utc_date() as date) - interval 2 year - interval 1 day and 
       //                                      cast(utc_date() as date) - interval 1 day


                using (var reader = MySqlHelper.ExecuteReader(conn, command, new MySqlParameter("@projectId", project.id), new MySqlParameter("@timeZone", project.timeZoneName)))
                {
                    return AddEntriesFromMySqlDatabase(units, reader);
                }
            });
        }

        /// <summary>
        /// Add the entries into a list with the volumes
        /// </summary>
        /// <param name="units"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private List<DayEntryAll> AddEntriesFromMySqlDatabase(UnitsTypeEnum units, MySqlDataReader reader)
        {
            double density = 0.0;
            var entries = new List<DayEntryAll>();
            try
            {                
                while (reader.Read())
                {
                    if (reader.IsDBNull(reader.GetOrdinal("weight")) || (reader.IsDBNull(reader.GetOrdinal("volume"))))
                    {

                    }
                    else
                    {

                        if (!reader.IsDBNull(reader.GetOrdinal("volume")) && reader.GetDouble(reader.GetOrdinal("volume")) > EPSILON)
                        {
                            if (units == UnitsTypeEnum.Metric)
                                density = reader.GetDouble(reader.GetOrdinal("weight")) * 1000 / reader.GetDouble(reader.GetOrdinal("volume"));
                            else
                                density = reader.GetDouble(reader.GetOrdinal("weight")) * M3_PER_YD3 * POUNDS_PER_TON / reader.GetDouble(reader.GetOrdinal("volume"));

                            entries.Add(new DayEntryAll
                            {
                                date = reader.GetDateTime(reader.GetOrdinal("date")),
                                entryPresent = true,
                                weight = reader.GetDouble(reader.GetOrdinal("weight")),
                                density = density,
                                volume = reader.GetDouble(reader.GetOrdinal("volume"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return entries;
        }
    }
}
