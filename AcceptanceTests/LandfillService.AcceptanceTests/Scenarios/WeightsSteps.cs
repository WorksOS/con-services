using System;
using System.Linq;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Models;
using AutomationCore.API.Framework.Library;
using LandfillService.AcceptanceTests.Helpers;
using LandfillService.AcceptanceTests.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class WeightsSteps
    {
        uint pId;
        double weight;
        StepSupport stepSupport = new StepSupport();

        [When(@"I add a weight for yesterday to project '(.*)'")]
        public void WhenIAddAWeightForYesterdayToProject(string project)
        {
            string response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri, "GET", TPaaS.BearerToken,
                            RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response);
            Project proj = allProjects.FirstOrDefault(p => p.name == project);

            Assert.IsNotNull(proj, "Project not found.");

            pId = proj.id;
            weight = (DateTime.Now.Ticks % 1000) + 1;
            string timeZone = proj.timeZoneName;

            WeightEntry[] request = new WeightEntry[] 
            { 
                new WeightEntry { date = stepSupport.GetYesterdayForTimeZone(timeZone).Date, weight = weight }
            };
            string requestString = JsonConvert.SerializeObject(request);
            response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri + pId + "/weights", "POST", TPaaS.BearerToken,
                            RestClientConfig.JsonMediaType, requestString, System.Net.HttpStatusCode.OK, "Bearer", null);
        }

        [Then(@"the weight is added for yesterday to project '(.*)'")]
        public void ThenTheWeightIsAddedForYesterdayToProject(string project)
        {
            string response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response);
            Project proj = allProjects.FirstOrDefault(p => p.name == project);

            Assert.IsNotNull(proj, "Project not found.");

            uint pId = proj.id;
            response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri + pId, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            ProjectData data = JsonConvert.DeserializeObject<ProjectData>(response);

            Assert.AreEqual(weight, data.entries.ToList()[data.entries.ToList().Count - 1].weight);
        }
    }
}
