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

        [When(@"I add a weight for yesterday to project '(.*)'")]
        public void WhenIAddAWeightForYesterdayToProject(string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            entries.Add(new WeightEntry 
            { 
                date = WeightsUtils.ConvertToProjectTime(DateTime.Today.AddDays(-1), project.timeZoneName).Date, 
                weight = LandfillCommonUtils.Random.Next(1, 1000) 
            });

            string requestString = JsonConvert.SerializeObject(entries.ToArray());
            RestClientUtil.DoHttpRequest(string.Format("{0}/{1}/weights", Config.LandfillBaseUri, project.id), "POST", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, requestString, System.Net.HttpStatusCode.OK, "Bearer", null);
        }

        [When(@"I add weights for the past (.*) days to project '(.*)'")]
        public void WhenIAddWeightsForThePastDaysToProject(int numDays, string projName)
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
            RestClientUtil.DoHttpRequest(string.Format("{0}/{1}/weights", Config.LandfillBaseUri, project.id), "POST", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, requestString, System.Net.HttpStatusCode.OK, "Bearer", null);
        }

        [When(@"I add weight for '(.*)' to project '(.*)'")]
        public void WhenIAddWeightForToProject(string dateString, string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            entries.Add(new WeightEntry
            {
                date = WeightsUtils.ConvertToProjectTime(DateTime.ParseExact(dateString, "yyyy-MM-dd", null), project.timeZoneName).Date,
                weight = LandfillCommonUtils.Random.Next(1, 1000)
            });

            string requestString = JsonConvert.SerializeObject(entries.ToArray());
            RestClientUtil.DoHttpRequest(string.Format("{0}/{1}/weights", Config.LandfillBaseUri, project.id), "POST", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, requestString, System.Net.HttpStatusCode.OK, "Bearer", null);
        }

        [Then(@"the weight is added for yesterday to project '(.*)'")]
        public void ThenTheWeightIsAddedForYesterdayToProject(string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            string response = RestClientUtil.DoHttpRequest(string.Format("{0}/{1}", Config.LandfillBaseUri, project.id), "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            ProjectData data = JsonConvert.DeserializeObject<ProjectData>(response);

            List<DayEntry> projDataEntries = data.entries.ToList();
            Assert.AreEqual(entries[0].weight, projDataEntries[projDataEntries.Count - 1].weight);
        }

        [Then(@"the weights are added for the past (.*) days to project '(.*)'")]
        public void ThenTheWeightsAreAddedForThePastDaysToProject(int numDays, string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            string response = RestClientUtil.DoHttpRequest(string.Format("{0}/{1}", Config.LandfillBaseUri, project.id), "GET", TPaaS.BearerToken,
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

        [Then(@"the weight is added for '(.*)' to project '(.*)'")]
        public void ThenTheWeightIsAddedForToProject(string dateString, string projName)
        {
            Project project = ProjectsUtils.GetProjectDetails(projName);

            string response = RestClientUtil.DoHttpRequest(string.Format("{0}/{1}", Config.LandfillBaseUri, project.id), "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            ProjectData data = JsonConvert.DeserializeObject<ProjectData>(response);

            List<DayEntry> projDataEntries = data.entries.ToList();
            DateTime date = WeightsUtils.ConvertToProjectTime(DateTime.ParseExact(dateString, "yyyy-MM-dd", null), project.timeZoneName).Date;
            DayEntry entry = projDataEntries.First(e => e.date == date);

            Assert.AreEqual(entries[0].weight, entry.weight, "Weight not equal");
        }

    }
}
