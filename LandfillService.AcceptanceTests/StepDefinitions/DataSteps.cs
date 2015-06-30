using LandfillService.WebApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.StepDefinitions
{
    [Binding, Scope(Feature = "Data")]
    public class DataSteps : CommonSteps
    {
        public static WeightEntry[] weightForYesterday = new WeightEntry[] { new WeightEntry (){date = DateTime.Now.AddDays(-5), weight = 12345} };

        [Given(@"Get a list of all projects")]
        public async void GivenGetAListOfAllProjects()
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            var projects = await response.Content.ReadAsAsync<Project[]>();
            ScenarioContext.Current.Pending();
        }

        [Then(@"check the \(Project (.*)\) is in the list")]
        public void ThenCheckTheProjectIsInTheList(int projectId)
        {
            List<Project> allProjects = JsonConvert.DeserializeObject<List<Project>>(response.Content.ReadAsStringAsync().Result);
            if (allProjects != null)
            {                
                if(allProjects.Any(prj => prj.id != projectId))
                {
                    Assert.Fail("Project " + projectId + " does not exist ");
                }
            }
            else
            {
                Assert.Fail("Cannot find any projects ");
            }
        }
        [Given(@"Get project data for project \(Project (.*)\)")]
        public async void GivenGetProjectDataForProjectProject(int projectId)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects/" + projectId), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", sessionId);
            response = httpClient.SendAsync(request).Result;
            var projectData = await response.Content.ReadAsAsync<ProjectData>();
        }

        [Then(@"check there is (.*) days worth of data for project \(Project (.*)\)")]
        public void ThenCheckThereIsDaysWorthOfDataForProjectProject(int dayslimit, int projectId)
        {
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);
            if (projectData != null)
            {
                Assert.IsTrue(projectData.entries.Count() > dayslimit, "There wasn't " + dayslimit + " days worth of data for project " + projectId + ". Entries = " + projectData.entries.Count());
            }
            else
            {
                Assert.Fail("There wasn't any data for project " + projectId);
            }
        }

        [When(@"adding a \(weight (.*) tonnes\) for project \(Project (.*)\) five days ago")]
        public void WhenAddingAWeightTonnesForProjectProjectFiveDaysAgo(int weight, int projectId)
        {
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.ServiceUrl + "projects/" + projectId + "/weights"), Method = HttpMethod.Post };
            request.Headers.Add("SessionID", sessionId);
            request.Content = new StringContent(JsonConvert.SerializeObject(weightForYesterday), Encoding.UTF8, "application/json");
            response = httpClient.SendAsync(request).Result;
            System.Diagnostics.Debug.WriteLine(response.ToString());
        }

        [Then(@"check the \(weight (.*) tonnes\) has been added to the project \(Project (.*)\) for five days ago")]
        public void ThenCheckTheWeightTonnesHasBeenAddedToTheProjectProjectForFiveDaysAgo(int weight, int projectId)
        {
            System.Diagnostics.Debug.WriteLine(response.Content.ReadAsStringAsync().Result);
            var projectData = JsonConvert.DeserializeObject<ProjectData>(response.Content.ReadAsStringAsync().Result);

            foreach (var weightEntry in projectData.entries)
            {
                
            }
        }
    }
}
