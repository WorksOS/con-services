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

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class CCASteps
    {
        MDMTestCustomer customer;
        List<MachineDetails> machines;

        List<CCARatioData> ccaRatio;
        List<CCASummaryData> ccaSummary;

        #region Given
        [Given(@"I have a landfill project '(.*)' with landfill sites '(.*)'")]
        public void GivenIHaveALandfillProjectWithLandfillSites(string project, string sites)
        {
            switch (project)
            {
                case "Middleton":
                    customer = MDMTestCustomer.Middleton;
                    break;
                case "Addington":
                    customer = MDMTestCustomer.Addington;
                    break;
                case "Maddington":
                    customer = MDMTestCustomer.Maddington;
                    break;
            }

            foreach (string site in sites.Split(','))
            {
                switch (site)
                {
                    case "MarylandsLandfill":
                        customer.AddLandfillSite(Site.MarylandsLandfill);
                        break;
                    case "AmiStadiumLandfill":
                        customer.AddLandfillSite(Site.AmiStadiumLandfill);
                        break;
                }
            }

            customer.Create();
        }

        [Given(@"I have the following machines")]
        public void GivenIHaveTheFollowingMachines(Table machineTable)
        {
            machines = GeneralSlave.CreateMachines(machineTable);
        }

        [Given(@"I have the following CCA data")]
        public void GivenIHaveTheFollowingCCAData(Table ccaTable)
        {
            GeneralSlave.CreateCcaData(ccaTable, customer, machines);
        } 
        #endregion

        #region When
        [When(@"I request CCA ratio for site '(.*)' for the last '(.*)' days")]
        public void WhenIRequestCCARatioForSiteForTheLastDays(string site, string numDays) 
        {
            // uri parameters
            Guid? geofenceUid = site == "NotSpecified" ? null : (Guid?)(customer.ProjectName.StartsWith(site) ? customer.ProjectGeofenceUid :
                    customer.LandfillGeofences.First(s => s.name.StartsWith(site)).uid);
            DateTime? startDate = numDays == "NotSpecified" ? null : (DateTime?)DateTime.Today.AddDays(-Convert.ToInt32(numDays));
            DateTime? endDate = numDays == "NotSpecified" ? null : (DateTime?)DateTime.Today.AddDays(-1);

            string jwt = Jwt.GetJwtToken(customer.UserUid);

            // get project id by web api request
            string response = RestClientUtil.DoHttpRequest(Config.ConstructGetProjectListUri(), "GET", RestClientConfig.JsonMediaType, null, jwt, HttpStatusCode.OK);
            List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(response);
            uint projectId = projects.First(p => p.name == customer.ProjectName).id;

            // request cca ratio
            string uri = Config.ConstructGetCcaRatioUri(projectId, geofenceUid, startDate, endDate);
            response = RestClientUtil.DoHttpRequest(uri, "GET", RestClientConfig.JsonMediaType, null, jwt, HttpStatusCode.OK);
            ccaRatio = JsonConvert.DeserializeObject<List<CCARatioData>>(response);
        }

        [When(@"I request CCA summary for lift '(.*)' of '(.*)' machine '(.*)' in site '(.*)' for day (.*)")]
        public void WhenIRequestCCASummaryForLiftOfMachineInSiteForDay(string lift, string johnDoe, string machine, string site, int dayToAdd)
        {
            // uri parameters
            int? liftId = lift == "AllLifts" ? null : (int?)Convert.ToInt32(lift);
            Guid? geofenceUid = site == "NotSpecified" ? null : ((Guid?)(customer.ProjectName.StartsWith(site) ? customer.ProjectGeofenceUid :
                    customer.LandfillGeofences.First(s => s.name.StartsWith(site)).uid));
            DateTime date = DateTime.Today.AddDays(dayToAdd);
            uint? assetId = machine == "AllMachines" ? null : (uint?)machines.First(m => m.machineName == machine).assetId;
            bool? isJohnDoe = johnDoe == "JohnDoeOrNonJohnDoe" ? null : (bool?)(johnDoe == "JohnDoe" ? true : false);
            string machineName = machine == "AllMachines" ? null : machine;

            // get project id by web api request
            string response = RestClientUtil.DoHttpRequest(Config.ConstructGetProjectListUri(), "GET",
                RestClientConfig.JsonMediaType, null, Jwt.GetJwtToken(customer.UserUid), HttpStatusCode.OK);
            List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(response);
            uint projectId = projects.First(p => p.name == customer.ProjectName).id;

            // request cca summary
            string uri = Config.ConstructGetCcaSummaryUri(projectId, date, geofenceUid, assetId, machineName, isJohnDoe, liftId);
            response = RestClientUtil.DoHttpRequest(uri, "GET", RestClientConfig.JsonMediaType, null, Jwt.GetJwtToken(customer.UserUid), HttpStatusCode.OK);
            ccaSummary = JsonConvert.DeserializeObject<List<CCASummaryData>>(response);
        } 
        #endregion

        #region Then
        [Then(@"the response contains the following CCA ration data")]
        public void ThenTheResponseContainsTheFollowingCCARationData(Table ccaRatioTable)
        {
            List<CCARatioData> expectedRatio = new List<CCARatioData>();

            List<string> machines = ccaRatioTable.Rows.Select(r => r["Machine"]).Distinct().ToList();

            Dictionary<string, List<CCARatioEntry>> dataDict = new Dictionary<string, List<CCARatioEntry>>();
            foreach (string machine in machines)
            {
                dataDict.Add(machine, new List<CCARatioEntry>());
            }
            foreach (TableRow row in ccaRatioTable.Rows)
            {
                dataDict[row["Machine"]].Add(new CCARatioEntry()
                {
                    date = DateTime.Today.AddDays(Convert.ToInt32(row["DateAsAnOffsetFromToday"])),
                    ccaRatio = Convert.ToDouble(row["CCARatio"])
                });
            }
            foreach (string key in dataDict.Keys)
            {
                expectedRatio.Add(new CCARatioData()
                    {
                        machineName = key,
                        entries = dataDict[key]
                    });
            }

            Assert.IsTrue(LandfillCommonUtils.ListsAreEqual<CCARatioData>(expectedRatio, ccaRatio));
        }

        [Then(@"the response contains two years of CCA ration data")]
        public void ThenTheResponseContainsTwoYearsOfCCARationData()
        {
            int numEntries = ccaRatio[0].entries.Count;
            Assert.IsTrue(numEntries <= 732 && numEntries >= 730, "Incorrect number of cca ratio data entries.");
        }

        [Then(@"the response contains the following CCA summary data")]
        public void ThenTheResponseContainsTheFollowingCCASummaryData(Table ccaSummaryTable)
        {
            List<CCASummaryData> expectedSummary = new List<CCASummaryData>();

            foreach (TableRow row in ccaSummaryTable.Rows)
            {
                expectedSummary.Add(new CCASummaryData()
                    {
                        machineName = row["Machine"],
                        liftId = row["LiftID"] == "null" ? null : (int?)Convert.ToInt32(row["LiftID"]),
                        incomplete = Convert.ToDouble(row["Incomplete"]),
                        complete = Convert.ToDouble(row["Complete"]),
                        overcomplete = Convert.ToDouble(row["Overcomplete"])
                    });
            }

            Assert.IsTrue(LandfillCommonUtils.ListsAreEqual<CCASummaryData>(expectedSummary, ccaSummary));
        } 
        #endregion
    }
}
