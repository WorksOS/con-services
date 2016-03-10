using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using LandfillService.AcceptanceTests.Helpers;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Models.KafkaTopics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataProjectsSteps
    {
        private readonly StepSupport stepSupport = new StepSupport();
        private HttpResponseMessage response;
        private ProjectEvent projEvent = new ProjectEvent();

        /// <summary>
        /// Create the kafka message. This creates a Project Event and is available to be used by other messages.
        /// </summary>
        /// <param name="eventRow"></param>
        /// <returns>A ProjectEvent object</returns>
        private ProjectEvent CreateAProjectEvent(TableRow eventRow)
        {
            var projectName = eventRow["ProjectName"] + stepSupport.GetRandomNumber();
            var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            var createProjectEvent = new ProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundaries = eventRow.Keys.Contains("Boundaries") ? eventRow["Boundaries"] : " ",
                ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(eventRow["DaysToExpire"])),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = projectName,
                ProjectTimezone = eventRow.Keys.Contains("TimeZone") ? eventRow["TimeZone"] : " ",
                ProjectType = eventRow["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D,
                ProjectID = projectId,
                ProjectUID = Guid.NewGuid(),
                ReceivedUTC = DateTime.UtcNow
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
                        messageStr = JsonConvert.SerializeObject(new {CreateProjectEvent = projEvent},
                            new JsonSerializerSettings {DateTimeZoneHandling = DateTimeZoneHandling.Unspecified});
                        break;
                    case "UpdateProjectEvent":
                        projEvent.ProjectEndDate = DateTime.Today.AddDays(Convert.ToInt32(row["DaysToExpire"]));
                        projEvent.ProjectName = row["ProjectName"] + stepSupport.GetRandomNumber();
                        projEvent.ProjectType = row["Type"] == "LandFill" ? ProjectType.LandFill : ProjectType.Full3D;
                        messageStr = JsonConvert.SerializeObject(new {UpdateProjectEvent = projEvent},
                            new JsonSerializerSettings {DateTimeZoneHandling = DateTimeZoneHandling.Unspecified});
                        break;
                    case "DeleteProjectEvent":
                        messageStr = JsonConvert.SerializeObject(new {DeleteProjectEvent = projEvent},
                            new JsonSerializerSettings {DateTimeZoneHandling = DateTimeZoneHandling.Unspecified});
                        break;
                }
                topic = ConfigurationManager.AppSettings["AssetMasterDataTopic"];
                uniqueId = projEvent.ProjectUID.ToString();
                //var message = MessageFactory.Instance.CreateMessage(messageStr, messageType);
                //message.Send();
                KafkaResolver.SendMessage(topic, KafkaResolver.ResolveTopic(topic), messageStr, uniqueId);

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
            var request = new HttpRequestMessage() { RequestUri = new Uri(Config.serviceUrl + "projects/NG"), Method = HttpMethod.Get };
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
