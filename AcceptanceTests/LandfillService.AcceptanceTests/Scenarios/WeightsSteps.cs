using System;
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
    public class WeightsSteps
    {
        List<WeightEntry> entries = new List<WeightEntry>();

        [When(@"I add weights for the past (.*) days to site '(.*)' of project '(.*)'")]
        public void WhenIAddWeightsForThePastDaysToSiteOfProject(int numDays, string geoFenceUid, string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            for (int i = numDays; i > 0; --i)
            {
                entries.Add(new WeightEntry
                {
                    date = WeightsUtils.ConvertToProjectTime(DateTime.Today.AddDays(-i), project.timeZoneName).Date,
                    weight = LandfillCommonUtils.Random.Next(1, 1000)
                });
            }

            string requestString = JsonConvert.SerializeObject(entries.ToArray());
            RestClientUtil.DoHttpRequest(string.Format("{0}/{1}/weights?geofenceUid={2}", Config.LandfillBaseUri, project.id, geoFenceUid), "POST", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, requestString, System.Net.HttpStatusCode.OK, "Bearer", null);
        }

        [When(@"I add weight for '(.*)' to site '(.*)' of project '(.*)'")]
        public void WhenIAddWeightForToSiteOfProject(string dateString, string geofenceUid, string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            entries.Add(new WeightEntry
            {
                date = WeightsUtils.ConvertToProjectTime(DateTime.ParseExact(dateString, "yyyy-MM-dd", null), project.timeZoneName).Date,
                weight = LandfillCommonUtils.Random.Next(1, 1000)
            });

            string requestString = JsonConvert.SerializeObject(entries.ToArray());
            RestClientUtil.DoHttpRequest(string.Format("{0}/{1}/weights?geofenceUid={2}", Config.LandfillBaseUri, project.id, geofenceUid), "POST", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, requestString, System.Net.HttpStatusCode.OK, "Bearer", null);
        }

        [Then(@"the weights are added for the past (.*) days to site '(.*)' of project '(.*)'")]
        public void ThenTheWeightsAreAddedForThePastDaysToSiteOfProject(int numDays, string geofenceUid, string projName)
        {
            string uri = string.Format("{0}/{1}?geofenceUid={2}&startDate={3}&endDate={4}", 
                Config.LandfillBaseUri, ProjectsUtils.GetProjectDetails(projName).id, geofenceUid,
                DateTime.Today.AddDays(-numDays).ToString("yyyy-MM-dd"), DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"));

            string response = RestClientUtil.DoHttpRequest(uri, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            ProjectData data = JsonConvert.DeserializeObject<ProjectData>(response);
            List<DayEntry> projDataEntries = data.entries.ToList();

            for (int i = 1; i <= numDays; ++i)
            {
                double expected = entries[entries.Count - i].weight;
                double actual = projDataEntries[projDataEntries.Count - i].weight;

                Assert.AreEqual(expected, actual, "Weight not equal");
            }
        }

        [Then(@"the weight is added for '(.*)' to site '(.*)' of project '(.*)'")]
        public void ThenTheWeightIsAddedForToSiteOfProject(string dateString, string geofenceUid, string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);
            DateTime date = WeightsUtils.ConvertToProjectTime(DateTime.ParseExact(dateString, "yyyy-MM-dd", null), project.timeZoneName).Date;

            string uri = string.Format("{0}/{1}?geofenceUid={2}&startDate={3}&endDate={4}",
                Config.LandfillBaseUri, project.id, geofenceUid, date.ToString("yyyy-MM-dd"), date.ToString("yyyy-MM-dd"));
            string response = RestClientUtil.DoHttpRequest(uri, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);

            DayEntry entry = JsonConvert.DeserializeObject<ProjectData>(response).entries.ToList()[0];
            Assert.AreEqual(entries[0].weight, entry.weight, "Weight not equal");
        }
    }
}
