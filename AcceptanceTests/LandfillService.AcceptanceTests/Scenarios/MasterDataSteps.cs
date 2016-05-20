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
        Guid geofenceUID = Guid.NewGuid();
        MasterDataSupport mdSupport = new MasterDataSupport();
        List<Project> projects;
        List<Geofence> geofences;
        List<WGSPoint> boundary;

        #region Given/When
        [Given(@"I inject '(.*)' into Kafka")]
        [When(@"I inject '(.*)' into Kafka")]
        public void GivenIInjectIntoKafka(string eventType)
        {
            string messageStr = "";
            string topic = "";

            switch (eventType)
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
                case "CreateProjectSubscriptionEvent":
                    messageStr = mdSupport.CreateProjectSubscription(projSubscripUID, Config.MasterDataCustomerUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "AssociateProjectSubscriptionEvent":
                    messageStr = mdSupport.AssociateProjectSubscription(projectUID, projSubscripUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "UpdateProjectSubscriptionEvent":
                    messageStr = mdSupport.UpdateProjectSubscription(projSubscripUID, Config.MasterDataCustomerUID);
                    topic = Config.SubscriptionTopic;
                    break;
                case "CreateGeofenceEvent":
                    messageStr = mdSupport.CreateGeofence(geofenceUID, Config.MasterDataCustomerUID, Config.MasterDataUserUID);
                    topic = Config.GeofenceTopic;
                    break;
                case "UpdateGeofenceEvent":
                    messageStr = mdSupport.UpdateGeofence(geofenceUID, Config.MasterDataUserUID);
                    topic = Config.GeofenceTopic;
                    break;
                case "DeleteGeofenceEvent":
                    messageStr = mdSupport.DeleteGeofence(geofenceUID, Config.MasterDataUserUID);
                    topic = Config.GeofenceTopic;
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

        [Given(@"I make a Web API request for a list of geofences")]
        [When(@"I make a Web API request for a list of geofences")]
        public void WhenIMakeAWebAPIRequestForAListOfGeofences()
        {
            uint projectId = projects.First(p => p.name == mdSupport.CreateProjectEvt.ProjectName).id;
            string response = RestClientUtil.DoHttpRequest(Config.ConstructGetGeofencesUri(projectId), "GET",
                RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
            geofences = JsonConvert.DeserializeObject<List<Geofence>>(response);
        }

        [When(@"I make a Web API request for the boundary of the geofence")]
        public void WhenIMakeAWebAPIRequestForTheBoundaryOfTheGeofence()
        {
            uint projectId = projects.First(p => p.name == mdSupport.CreateProjectEvt.ProjectName).id;
            string uri = Config.ConstructGetGeofencesBoundaryUri(projectId, geofenceUID);

            string response = RestClientUtil.DoHttpRequest(uri, "GET", RestClientConfig.JsonMediaType, null, Config.JwtToken, HttpStatusCode.OK);
            boundary = JsonConvert.DeserializeObject<List<WGSPoint>>(response);
        } 
        #endregion

        #region Then
        [Then(@"the created project is in the list")]
        [Given(@"the created project is in the list")]
        public void ThenTheCreatedProjectIsInTheList()
        {
            Assert.IsTrue(projects.Exists(p => p.name == mdSupport.CreateProjectEvt.ProjectName), "Project not found.");
        }

        [Then(@"the created project is not in the list")]
        public void ThenTheCreatedProjectIsNotInTheList()
        {
            Assert.IsFalse(projects.Exists(p => p.name == mdSupport.CreateProjectEvt.ProjectName), "Project not deleted.");
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

        [Then(@"the created geofence is in the list")]
        [Given(@"the created geofence is in the list")]
        public void ThenTheCreatedGeofenceIsInTheList()
        {
            Assert.IsTrue(geofences.Exists(g => g.uid == mdSupport.CreateGeofenceEvt.GeofenceUID &&
                g.name == mdSupport.CreateGeofenceEvt.GeofenceName &&
                g.type == (int)Enum.Parse(typeof(GeofenceType), mdSupport.CreateGeofenceEvt.GeofenceType)), "Geofence not found.");
        }

        [Then(@"the geofence details are updated")]
        public void ThenTheGeofenceDetailsAreUpdated()
        {
            Assert.AreEqual(mdSupport.UpdateGeofenceEvt.GeofenceName, geofences.FirstOrDefault(g => g.uid == geofenceUID).name,
                "Geofence details not updated.");
        }

        [Then(@"the created geofence is not in the list")]
        public void ThenTheCreatedGeofenceIsNotInTheList()
        {
            Assert.IsFalse(geofences.Exists(g => g.name == mdSupport.CreateGeofenceEvt.GeofenceName), "Geofence not deleted.");
        }

        [Then(@"the geofence boundary points are correct")]
        public void ThenTheGeofenceBoundaryPointsAreCorrect()
        {
            const double DEGREES_TO_RADIANS = Math.PI / 180;

            List<WGSPoint> expectedBoundary = new List<WGSPoint>();
            string geometry = mdSupport.CreateGeofenceEvt.GeometryWKT;
            //Trim off the "POLYGON((" and "))"
            geometry = mdSupport.CreateGeofenceEvt.GeometryWKT.Substring(9, geometry.Length - 11);
            var points = geometry.Split(',');
            foreach (var point in points)
            {
                var parts = point.Split(' ');
                var lat = double.Parse(parts[0]);
                var lng = double.Parse(parts[0]);
                expectedBoundary.Add(new WGSPoint { Lat = lat * DEGREES_TO_RADIANS, Lon = lng * DEGREES_TO_RADIANS });
            }

            for (int i = 0; i < expectedBoundary.Count; ++i)
            {
                Assert.IsTrue(Math.Round(expectedBoundary[i].Lat) == Math.Round(boundary[i].Lat) &&
                    Math.Round(expectedBoundary[i].Lon) == Math.Round(boundary[i].Lon),
                    "Incorrect geofence boundary.");
            }
        } 
        #endregion
    }
}
