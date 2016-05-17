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


namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataSteps
    {
        Guid projSubscripUID = Guid.NewGuid();
        Guid projectUID = Guid.NewGuid();
        Guid customerUID = Guid.NewGuid();
        Guid userUID = Guid.NewGuid();
        MasterDataSupport mdSupport = new MasterDataSupport();
        List<Project> projects;
        
        [Given(@"I inject '(.*)' into Kafka")]
        [When(@"I inject '(.*)' into Kafka")]
        public void GivenIInjectIntoKafka(string eventType)
        {
            string messageStr = "";
            string topic = "";

            switch(eventType)
            {
                case "CreateCustomerEvent":
                    messageStr = mdSupport.CreateCustomer(customerUID);
                    topic = Config.CustomerTopic;
                    break;
                case "UpdateCustomerEvent":
                    messageStr = mdSupport.UpdateCustomer(customerUID);
                    topic = Config.CustomerTopic;
                    break;
                case "DeleteCustomerEvent":
                    messageStr = mdSupport.DeleteCustomer(customerUID);
                    topic = Config.CustomerTopic;
                    break;
                case "CreateProjectEvent":
                    messageStr = mdSupport.CreateProject(projectUID);
                    topic = Config.ProjectTopic;
                    break;
                case "UpdateProjectEvent":
                    messageStr = mdSupport.UpdateProject(projectUID);
                    topic = Config.ProjectTopic;
                    break;
                case "DeleteProjectEvent":
                    messageStr = mdSupport.DeleteProject(projectUID);
                    topic = Config.ProjectTopic;
                    break;
                case "CreateProjectSubscriptionEvent":
                    messageStr = mdSupport.CreateProjectSubscription(projSubscripUID, Config.MasterDataCustomerUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "AssociateCustomerUserEvent":
                    messageStr = mdSupport.AssociateCustomerUser(customerUID, userUID);
                    topic = Config.CustomerTopic;
                    break;
                case "AssociateProjectCustomer":
                    string query = string.Format("DELETE FROM {0}.Project WHERE CustomerUID = '{1}'", Config.MySqlDbName, Config.MasterDataCustomerUID);
                    LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query);
                    messageStr = mdSupport.AssociateProjectCustomer(projectUID, Config.MasterDataCustomerUID);
                    topic = Config.ProjectTopic;
                    break;
                case "AssociateProjectSubscriptionEvent":
                    messageStr = mdSupport.AssociateProjectSubscription(projectUID, projSubscripUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "UpdateProjectSubscriptionEvent":
                    messageStr = mdSupport.UpdateProjectSubscription(projSubscripUID, Config.MasterDataCustomerUID);
                    topic = Config.SubscriptionTopic;
                    break;
            }

            Config.KafkaDriver.SendMessage(topic, messageStr);
        }

        [Given(@"I make a Web API request for a list of projects")]
        [When(@"I make a Web API request for a list of projects")]
        public void WhenIMakeAWebAPIRequestForAListOfProjects()
        {
            string response = RestClientUtil.DoHttpRequest(Config.ConstructGetProjectListUri(), "GET",
                RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
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
            int expected = (mdSupport.CreateProjectSubscriptionEvt.EndDate - DateTime.Today).Days;
            int actual = (int)projects.FirstOrDefault(p => p.name == mdSupport.CreateProjectEvt.ProjectName).daysToSubscriptionExpiry;
            
            int diff = expected - actual;
            Assert.IsFalse(diff < -1 || diff > 1, "daysToSubscriptionExpiry incorrect.");
        }

        [Then(@"the updated number of days to subscription expiry is correct")]
        public void ThenTheUpdatedNumberOfDaysToSubscriptionExpiryIsCorrect()
        {
            int expected = ((DateTime)mdSupport.UpdateProjectSubscriptionEvt.EndDate - DateTime.Today).Days;
            int actual = (int)projects.FirstOrDefault(p => p.name == mdSupport.CreateProjectEvt.ProjectName).daysToSubscriptionExpiry;

            int diff = expected - actual;
            Assert.IsFalse(diff < -1 || diff > 1, "Updated daysToSubscriptionExpiry incorrect.");
        }

        [Then(@"the project details are updated")]
        public void ThenTheProjectDetailsAreUpdated()
        {
            Assert.AreEqual(mdSupport.UpdateProjectEvt.ProjectName, projects.FirstOrDefault(p => p.id == mdSupport.CreateProjectEvt.ProjectID).name,
                "Project details not updated.");
        }

        [Then(@"a new '(.*)' is created")]
        [Given(@"a new '(.*)' is created")]
        public void ThenANewIsCreated(string thing)
        {
            string query = "";

            switch (thing)
            {
                case "Customer":
                    query = string.Format("SELECT COUNT(ID) FROM {0}.Customer WHERE CustomerUID = '{1}'", Config.MySqlDbName, customerUID);
                    break;
                case "Project":
                    query = string.Format("SELECT COUNT(ID) FROM {0}.Project WHERE ProjectUID = '{1}'", Config.MySqlDbName, projectUID);
                    break;
            }
            Assert.IsTrue(Convert.ToInt32(LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query)) == 1,
                thing + " not created.");
        }

        [Then(@"the new '(.*)' is updated")]
        public void ThenTheNewIsUpdated(string thing)
        {
            string query = "";

            switch (thing)
            {
                case "Customer":
                    query = string.Format("SELECT CustomerName FROM {0}.Customer WHERE CustomerUID = '{1}'", Config.MySqlDbName, customerUID);
                    Assert.AreEqual(mdSupport.UpdateCustomerEvt.CustomerName, LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query),
                        "Customer not updated.");
                    break;
                case "Project":
                    query = string.Format("SELECT Name FROM {0}.Project WHERE ProjectUID = '{1}'", Config.MySqlDbName, projectUID);
                    Assert.AreEqual(mdSupport.UpdateProjectEvt.ProjectName, LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query),
                        "Project not updated.");
                    break;
            }  
        }

        [Then(@"the new '(.*)' is deleted")]
        public void ThenTheNewIsDeleted(string thing)
        {
            string query = "";

            switch (thing)
            {
                case "Customer":
                    query = string.Format("SELECT COUNT(ID) FROM {0}.Customer WHERE CustomerUID = '{1}'", Config.MySqlDbName, customerUID);
                    break;
                case "Project":
                    query = string.Format("SELECT COUNT(ID) FROM {0}.Project WHERE ProjectUID = '{1}'", Config.MySqlDbName, projectUID);
                    break;
            }
            Assert.IsTrue(Convert.ToInt32(LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query)) == 0,
                thing + " not deleted.");
        }

        [Then(@"user and customer are associated")]
        public void ThenUserAndCustomerAreAssociated()
        {
            string query = string.Format("SELECT fk_UserUID FROM {0}.CustomerUser WHERE fk_CustomerUID = '{1}'", Config.MySqlDbName, customerUID);
            Guid associatedUserUid = Guid.Parse(LandFillMySqlDb.ExecuteMySqlQueryResult(Config.MySqlConnString, query));
            Assert.AreEqual(userUID, associatedUserUid, "User and customer not associated.");
        }

    }
}
