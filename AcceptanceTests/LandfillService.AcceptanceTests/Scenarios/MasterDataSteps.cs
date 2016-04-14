using System;
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


namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataSteps
    {
        Guid projSubscripUID = Guid.NewGuid();
        Guid projectUID = Guid.NewGuid();
        Guid customerUID = Guid.NewGuid();
        MasterDataSupport mdSupport = new MasterDataSupport();
        List<Project> projects;
        
        [Given(@"I inject '(.*)' into Kafka")]
        public void GivenIInjectIntoKafka(string eventType)
        {
            string messageStr = "";
            string topic = "";

            switch(eventType)
            {
                case "CreateCustomerEvent":
                    messageStr = mdSupport.CreateCustomer(customerUID);
                    topic = Config.CustomerMasterDataTopic;
                    break;
                case "CreateProjectEvent":
                    messageStr = mdSupport.CreateProject(projectUID);
                    topic = Config.ProjectMasterDataTopic;
                    break;
                case "UpdateProjectEvent":
                    messageStr = mdSupport.UpdateProject(projectUID);
                    topic = Config.ProjectMasterDataTopic;
                    break;
                case "CreateProjectSubscriptionEvent":
                    messageStr = mdSupport.CreateProjectSubscription(projSubscripUID, Config.GoldenCustomerUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "AssociateProjectCustomer":
                    string query = string.Format("DELETE FROM {0}.Project WHERE CustomerUID = '{1}'", Config.MySqlDbName, Config.GoldenCustomerUID);
                    LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query);
                    messageStr = mdSupport.AssociateProjectCustomer(projectUID, Config.GoldenCustomerUID);
                    topic = Config.ProjectMasterDataTopic;
                    break;
                case "AssociateProjectSubscriptionEvent":
                    messageStr = mdSupport.AssociateProjectSubscription(projectUID, projSubscripUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "UpdateProjectSubscriptionEvent":
                    messageStr = mdSupport.UpdateProjectSubscription(projSubscripUID, Config.GoldenCustomerUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "DissociateProjectSubscriptionEvent":
                    messageStr = mdSupport.DissociateProjectSubscription(projSubscripUID, projSubscripUID);
                    topic = Config.SubscriptionTopic;
                    break;
            }

            if (Config.KafkaDriver == "JAVA")
            {
                KafkaResolver.SendMessage(topic, messageStr);
            }
            if (Config.KafkaDriver == ".NET")
            {
                KafkaDotNet.SendMessage(topic, messageStr);
            }
        }

        [Given(@"I make a Web API request for a list of projects")]
        [When(@"I make a Web API request for a list of projects")]
        public void WhenIMakeAWebAPIRequestForAListOfProjects()
        {
            LandfillCommonUtils.UpdateAppSetting("StagingTPaaSTokenUsername", Config.GoldenUserName);

            string response = RestClientUtil.DoHttpRequest(Config.LandfillBaseUri, "GET", TPaaS.BearerToken,
                RestClientConfig.JsonMediaType, null, System.Net.HttpStatusCode.OK, "Bearer", null);
            projects = JsonConvert.DeserializeObject<List<Project>>(response);
        }

        [Given(@"the created project is in the list")]
        [Then(@"the created project is in the list")]
        public void ThenTheCreatedProjectIsInTheList()
        {
            Assert.IsTrue(projects.Exists(p => p.name == mdSupport.CreateProjectEvt.ProjectName), "Project not found.");
        }

        [Then(@"the created project is not in the list")]
        public void ThenTheCreatedProjectIsNotInTheList()
        {
            Assert.IsFalse(projects.Exists(p => p.name == mdSupport.CreateProjectEvt.ProjectName), "Project found.");
        }

        [Then(@"the number of days to subscription expiry is correct")]
        public void ThenTheNumberOfDaysToSubscriptionExpiryIsCorrect()
        {
            int expectedDaysToExpiry = (mdSupport.CreateProjectSubscriptionEvt.EndDate - DateTime.Today).Days;
            int actualDaysToExpiry = (int)projects.FirstOrDefault(p => p.name == mdSupport.CreateProjectEvt.ProjectName).daysToSubscriptionExpiry;

            Assert.AreEqual(expectedDaysToExpiry, actualDaysToExpiry, "daysToSubscriptionExpiry incorrect.");
        }

        [Then(@"the updated number of days to subscription expiry is correct")]
        public void ThenTheUpdatedNumberOfDaysToSubscriptionExpiryIsCorrect()
        {
            int expectedDaysToExpiry = ((DateTime)mdSupport.UpdateProjectSubscriptionEvt.EndDate - DateTime.Today).Days;
            int actualDaysToExpiry = (int)projects.FirstOrDefault(p => p.name == mdSupport.CreateProjectEvt.ProjectName).daysToSubscriptionExpiry;

            Assert.AreEqual(expectedDaysToExpiry, actualDaysToExpiry, "daysToSubscriptionExpiry incorrect.");
        }

        [Then(@"the project details are updated")]
        public void ThenTheProjectDetailsAreUpdated()
        {
            Assert.AreEqual(mdSupport.UpdateProjectEvt.ProjectName, projects.FirstOrDefault(p => p.id == mdSupport.CreateProjectEvt.ProjectID).name,
                "Project details not updated.");
        }

        [Then(@"a new customer is created")]
        public void ThenANewCustomerIsCreated()
        {
            string query = string.Format("SELECT COUNT(ID) FROM {0}.Customer WHERE CustomerUID = '{1}'", Config.MySqlDbName, customerUID);
            Assert.IsTrue(Convert.ToInt32(LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query)) == 1, "Customer not created.");
        }

    }
}
