using System;
using System.Globalization;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Models.Landfill;
using AutomationCore.API.Framework.Library;
using LandfillService.AcceptanceTests.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LandfillService.AcceptanceTests.Utils;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class ProjectsSteps
    {
        List<Project> projects;
        ProjectData data;
        List<MachineLiftDetails> liftDetails;
        VolumeTime vt;

        #region When
        [When(@"I try to get a list of all projects")]
        public void WhenITryToGetAListOfAllProjects()
        {
            string response = RestClientUtil.DoHttpRequest(Config.ConstructGetProjectListUri(), "GET",
                RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
            projects = JsonConvert.DeserializeObject<List<Project>>(response);
        }

        [When(@"I try to get data for")]
        public void WhenITryToGetDataFor(Table table)
        {
            string projName = table.Rows[0]["ProjectName"];
            string geoFenceUid = table.Rows[0]["GeofenceUID"];
            string dateRange = table.Rows[0]["DateRange"];
            uint projId = ProjectsUtils.GetProjectDetails(projName).id;
            DateTime startDate;
            DateTime endDate;
            string uri = "";
            switch (dateRange)
            {
                case "OneDay":
                    startDate = DateTime.Today.AddDays(-LandfillCommonUtils.Random.Next(5, 730));
                    endDate = startDate;
                    uri = Config.ConstructGetProjectDataUri(projId, Guid.Parse(geoFenceUid), startDate, endDate);
                    break;
                case "ThreeDays":
                    startDate = DateTime.Today.AddDays(-LandfillCommonUtils.Random.Next(5, 730));
                    endDate = startDate.AddDays(2);
                    uri = Config.ConstructGetProjectDataUri(projId, Guid.Parse(geoFenceUid), startDate, endDate);
                    break;
                case "TwoYears":
                    uri = Config.ConstructGetProjectDataUri(projId);
                    break;
            }

            string response = RestClientUtil.DoHttpRequest(uri, "GET", RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
            data = JsonConvert.DeserializeObject<ProjectData>(response);
        }

        [When(@"I try to get machine lift details for project '(.*)' between dates '(.*)'")]
        public void WhenITryToGetMachineLiftDetailsForProjectBetweenDates(string project, string dateRange)
        {
            uint projId = ProjectsUtils.GetProjectDetails(project).id;
            DateTime? start = null;
            DateTime? end = null;

            if (dateRange != "NotSpecified")
            {
                try
                {
                    start = DateTime.ParseExact(dateRange.Split('&')[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    end = DateTime.ParseExact(dateRange.Split('&')[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                catch(Exception)
                {
                    throw new Exception("Date range format for machine lift details should be 'yyyy-MM-dd&yyyy-MM-dd");
                }
            }

            string response = RestClientUtil.DoHttpRequest(Config.ConstructGetMachineLifts(projId, start, end), "GET",
                RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
            liftDetails = JsonConvert.DeserializeObject<List<MachineLiftDetails>>(response);
        }

        [When(@"I try to get volume and time summary for project '(.*)'")]
        public void WhenITryToGetVolumeAndTimeSummaryForProject(string project)
        {
            uint projId = ProjectsUtils.GetProjectDetails(project).id;
            string uri = Config.ConstructGetVolumeTimeUri(projId);
            string response = RestClientUtil.DoHttpRequest(uri, "GET", RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
            vt = JsonConvert.DeserializeObject<VolumeTime>(response);
        }
        #endregion

        #region Then
        [Then(@"the project '(.*)' is in the list with details")]
        public void ThenTheProjectIsInTheListWithDetails(string projName, Table projDetails)
        {
            Assert.IsTrue(projects.Exists(p => p.name == projName), "Project not found.");

            Project project = projects.First(p => p.name == projName);
            Assert.AreEqual(projDetails.Rows[0]["UID"], project.projectUid, "Incorrect projectUid.");
            Assert.AreEqual(projDetails.Rows[0]["TimezoneName"], project.timeZoneName, "Incorrect project timeZoneName.");
            Assert.AreEqual(projDetails.Rows[0]["LegacyTimezoneName"], project.legacyTimeZoneName, "Incorrect project legacyTimezoneName.");
        }

        [Then(@"the response contains data for the past two years")]
        public void ThenTheResponseContainsDataForThePastTwoYears()
        {
            Assert.IsTrue(data.entries.ToList().Count == 730 || data.entries.ToList().Count == 731,
                "Incorrect number of project data entries.");
        }

        [Then(@"the response contains data for '(.*)'")]
        public void ThenTheResponseContainsDataFor(string dateRange)
        {
            switch (dateRange)
            {
                case "OneDay":
                    Assert.IsTrue(data.entries.ToList().Count == 1,
                        "Incorrect number of project data entries.");
                    break;
                case "ThreeDays":
                    Assert.IsTrue(data.entries.ToList().Count == 3,
                        "Incorrect number of project data entries.");
                    break;
                case "TwoYears":
                    Assert.IsTrue(data.entries.ToList().Count - 730 < 3,
                        "Incorrect number of project data entries.");
                    break;
            }
        }

        [Then(@"the following machine lift details are returned")]
        public void ThenTheFollowingMachineLiftDetailsAreReturned(string liftDetailString)
        {
            List<MachineLiftDetails> expectedLiftDetails = JsonConvert.DeserializeObject<List<MachineLiftDetails>>(liftDetailString);
            Assert.IsTrue(LandfillCommonUtils.ListsAreEqual<MachineLiftDetails>(expectedLiftDetails, liftDetails));
        }

        [Then(@"the remaining volume is (.*) and the remaining time is (.*)")]
        public void ThenTheRemainingVolumeIsAndTheRemainingTimeIs(double expectedRemainingVol, double expectedRemainingTime)
        {
            Assert.IsTrue((int)expectedRemainingVol == (int)vt.remainingVolume && (int)expectedRemainingTime == (int)vt.remainingTime, 
                "Incorrect remaining volume or/and time.");
        }
        #endregion
    }
}
