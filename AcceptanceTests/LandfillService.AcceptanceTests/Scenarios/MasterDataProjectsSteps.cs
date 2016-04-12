using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Models.KafkaTopics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using LandfillService.AcceptanceTests.LandFillKafka;
using LandfillService.AcceptanceTests.Utils;
using LandfillService.AcceptanceTests.Scenarios.ScenarioSupports;
using LandfillService.AcceptanceTests.Utils;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataProjectsSteps
    {
        private HttpResponseMessage response;
        private CreateProjectEvent projEvent = new CreateProjectEvent();

        private CreateProjectEvent CreateAProjectEvent(TableRow eventRow)
        {
            var projectName = eventRow["ProjectName"] + LandfillCommonUtils.Random.Next(1, 100000).ToString("D6");
            //var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            var projectId = 1600;
            var createProjectEvent = new CreateProjectEvent
            {
                ProjectUID = Guid.NewGuid(),
                ProjectID = projectId,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                ProjectName = projectName,
                ProjectTimezone = eventRow.Keys.Contains("TimeZone") ? eventRow["TimeZone"] : " ",
                ProjectBoundary = eventRow.Keys.Contains("Boundaries") ? eventRow["Boundaries"] : " ",
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(eventRow["DaysToExpire"])),
                ProjectType = eventRow["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D
            };
            return createProjectEvent;
        }

        [Given(@"I inject the following master data events")]
        public void GivenIInjectTheFollowingMasterDataEvents(Table table)
        {            
            var messageStr = string.Empty;
            var topic = string.Empty;
            var uniqueId = string.Empty;
            foreach (var row in table.Rows)
            {
                switch (row["Event"])
                {
                    case "CreateProjectEvent":
                        projEvent = CreateAProjectEvent(row);
                        messageStr = JsonConvert.SerializeObject(new { CreateProjectEvent = projEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
                        break;
                    case "UpdateProjectEvent":
                        projEvent.ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(row["DaysToExpire"]));
                        projEvent.ProjectName = row["ProjectName"] + LandfillCommonUtils.Random.Next(1, 1000000).ToString("D7");
                        projEvent.ProjectType = row["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D;
                        messageStr = JsonConvert.SerializeObject(new {UpdateProjectEvent = projEvent},
                            new JsonSerializerSettings {DateTimeZoneHandling = DateTimeZoneHandling.Unspecified});
                        break;
                    case "DeleteProjectEvent":
                        messageStr = JsonConvert.SerializeObject(new {DeleteProjectEvent = projEvent},
                            new JsonSerializerSettings {DateTimeZoneHandling = DateTimeZoneHandling.Unspecified});
                        break;
                }
                topic = ConfigurationManager.AppSettings["ProjectMasterDataTopic"];
                uniqueId = projEvent.ProjectUID.ToString();
                if (Config.KafkaDriver == "JAVA")
                {
                    KafkaResolver.SendMessage(topic, messageStr);
                }
                if (Config.KafkaDriver == ".NET")
                {
                    KafkaDotNet.SendMessage(topic, messageStr);
                }

                switch (row["Event"])
                {
                    case "CreateProjectEvent":
                        Assert.IsTrue(LandFillMySqlDb.WaitForProjectToBeCreated(projEvent.ProjectName),"Failed to created a project in landfill mySql db");
                        break;
                    case "UpdateProjectEvent":
                        Assert.IsTrue(LandFillMySqlDb.WaitForProjectToBeCreated(projEvent.ProjectName), "Failed to find the updated project in landfill mySql db");
                        break;
                    case "DeleteProjectEvent":
                        Assert.IsTrue(LandFillMySqlDb.WaitForProjectToBeDeleted(projEvent.ProjectName), "Failed to find the deleted project in landfill mySql db");
                        break;
                }            
            }
        }

        [When(@"I request a list of projects from landfill web api")]
        public void WhenIRequestAListOfProjectsFromLandfillWebApi()
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.LandfillBaseUri + "projects/NG"), Method = HttpMethod.Get };
            request.Headers.Add("SessionID", SpecFlowHooks.sessionId);
            response = httpClient.SendAsync(request).Result;
        }

        [Then(@"I find the project I created in the list")]
        public void ThenIFindTheProjectICreatedInTheList()
        {
            var allProjects = JsonConvert.DeserializeObject<List<ProjectNg>>(response.Content.ReadAsStringAsync().Result);
            if (allProjects.Any(proj => proj.name == projEvent.ProjectName))
                { return; }
            Assert.Fail("FAILED: Did not find the project name in the list of projects");
        }

        [Then(@"I find update project details in the project list")]
        public void ThenIFindUpdateProjectDetailsInTheProjectList()
        {
            var allProjects = JsonConvert.DeserializeObject<List<ProjectNg>>(response.Content.ReadAsStringAsync().Result);
            foreach (var proj in allProjects)
            {
                if (proj.projectUid == projEvent.ProjectUID.ToString())
                {
                    if (proj.name == projEvent.ProjectName)
                    {
                        return;
                    }
                }
            }
            Assert.Fail("FAILED: Did not find the project name in the list of projects");
        }


        [Then(@"I dont find the project I created in the list")]
        public void ThenIDontFindTheProjectICreatedInTheList()
        {
            var allProjects = JsonConvert.DeserializeObject<List<ProjectNg>>(response.Content.ReadAsStringAsync().Result);
            if (allProjects.Any(proj => proj.name == projEvent.ProjectName))
            {
                Assert.Fail("FAILED: Found the project name in the list of projects after it was deleted");
            }
        }
    }
}
