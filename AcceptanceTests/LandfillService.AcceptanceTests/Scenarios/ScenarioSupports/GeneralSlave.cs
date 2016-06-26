using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using LandfillService.AcceptanceTests.Scenarios.ScenarioSupports;
using LandfillService.AcceptanceTests.LandFillKafka;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using AutomationCore.API.Framework.Library;
using LandfillService.AcceptanceTests.Models.Landfill;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Auth;
using LandfillService.AcceptanceTests.Utils;
using LandfillService.AcceptanceTests.TestData;

namespace LandfillService.AcceptanceTests.Scenarios.ScenarioSupports
{
    public class GeneralSlave
    {
        public static List<MachineDetails> CreateMachines(Table machineTable)
        {
            List<MachineDetails> machines = new List<MachineDetails>();

            int maxId = LandFillMySqlDb.GetTheHighestMachineId();
            if(maxId == 0)
            {
                throw new Exception("Unable to access database.");
            }

            foreach(TableRow row in machineTable.Rows)
            {
                MachineDetails machine = new MachineDetails()
                                            {
                                                id = ++maxId,
                                                assetId = DateTime.Now.Ticks % 1000000,
                                                machineName = row["Name"],
                                                isJohnDoe = Convert.ToBoolean(row["IsJohnDoe"])
                                            };
                string query = string.Format(@"INSERT INTO {0}.Machine (ID, AssetID, MachineName, IsJohnDoe, InsertUTC, UpdateUTC) 
                                               VALUES ('{1}', '{2}', '{3}', '{4}', {5}, {6});", Config.MySqlDbName,
                                               machine.id, machine.assetId, machine.machineName, Convert.ToInt32(machine.isJohnDoe), "utc_timestamp()", "utc_timestamp()");
                LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query);

                machines.Add(machine);
            }

            return machines;
        }

        public static void CreateCcaData(Table ccaTable, MDMTestCustomer customer, List<MachineDetails> machines)
        {
            foreach(TableRow row in ccaTable.Rows)
            {
                string projectUid = customer.ProjectUid.ToString();
                string geofenceUid = customer.ProjectName.StartsWith(row["Site"]) ? customer.ProjectGeofenceUid.ToString() :
                    customer.LandfillGeofences.First(s => s.name.StartsWith(row["Site"])).uid.ToString();
                string date = DateTime.Today.AddDays(Convert.ToInt32(row["DateAsAnOffsetFromToday"])).ToString("yyyy-MM-dd");
                int machineId = machines.First(m => m.machineName == row["Machine"]).id;

                string query;
                if(row["LiftID"] == "null")
                {
                    query = string.Format(@"INSERT INTO {0}.CCA (ProjectUID, GeofenceUID, Date, MachineID, Incomplete, Complete, Overcomplete, 
                                                                 CCANotRetrieved, CCANotAvailable, CCAUpdatedTimestampUTC, InsertUTC, UpdateUTC) 
                                            VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '0', '0', {8}, {9}, {10});", Config.MySqlDbName,
                                            projectUid, geofenceUid, date, machineId, row["Incomplete"], row["Complete"], row["Overcomplete"],
                                            "utc_timestamp()", "utc_timestamp()", "utc_timestamp()");     
                }                                             
                else
                {
                    query = string.Format(@"INSERT INTO {0}.CCA (ProjectUID, GeofenceUID, Date, MachineID, LiftID, Incomplete, Complete, Overcomplete, 
                                                                 CCANotRetrieved, CCANotAvailable, CCAUpdatedTimestampUTC, InsertUTC, UpdateUTC) 
                                            VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '0', '0', {9}, {10}, {11});", Config.MySqlDbName,
                                            projectUid, geofenceUid, date, machineId, row["LiftID"], row["Incomplete"], row["Complete"], row["Overcomplete"],
                                            "utc_timestamp()", "utc_timestamp()", "utc_timestamp()"); 
                }

                LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query);
            }
        }
    }
}
