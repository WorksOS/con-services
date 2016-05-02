using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using AutomationCore.API.Framework.Library;
using VSP.MasterData.Project.AcceptanceTests.Utils;
using VSP.MasterData.Project.AcceptanceTests.Models.Project;
using VSP.MasterData.Project.AcceptanceTests.Auth;
using VSP.MasterData.Project.AcceptanceTests.Kafka;
using VSP.MasterData.Project.AcceptanceTests.Scenarios.ScenarioSupports;

namespace VSP.MasterData.Project.AcceptanceTests.Scenarios
{
    [Binding]
    public class ProjectCRUDSteps
    {
        Guid customerUID = Guid.NewGuid();
        Guid userUID = Guid.NewGuid();
        Guid projectUID = Guid.NewGuid();
        MasterDataSupport mdSupport = new MasterDataSupport();
        WebApiSupport apiSupport = new WebApiSupport();

        Dictionary<long, ProjectDescriptor> getAllProjectsResult;

        [Given(@"I inject '(.*)' into Kafka")]
        public void GivenIInjectIntoKafka(string eventType)
        {
            string messageStr = "";
            string topic = "";

            switch (eventType)
            {
                case "CreateCustomerEvent":
                    messageStr = mdSupport.CreateCustomer(customerUID);
                    topic = Config.CustomerMasterDataTopic;
                    break;
                case "AssociateCustomerUserEvent":
                    messageStr = mdSupport.AssociateCustomerUser(customerUID, userUID);
                    topic = Config.CustomerUserMasterDataTopic;
                    break;
            }
           KafkaDotNet.SendMessage(topic, messageStr);
        }

        [When(@"I '(.*)' a project via Web API as the user for the customer")]
        public void WhenIAProjectViaWebAPIAsTheUserForTheCustomer(string action)
        {
            string request;
            string response;
            string jwtToken = Jwt.GetJwtToken(userUID);

            switch (action)
            {
                case "Create":
                    request = JsonConvert.SerializeObject(apiSupport.CreateProject(projectUID));
                    response = RestClientUtil.DoHttpRequest(Config.ProjectCrudUri, "POST", RestClientConfig.JsonMediaType,
                        request, jwtToken, HttpStatusCode.OK);                    
                    break;
                case "Update":
                    request = JsonConvert.SerializeObject(apiSupport.CreateProject(projectUID));
                    response = RestClientUtil.DoHttpRequest(Config.ProjectCrudUri, "PUT", RestClientConfig.JsonMediaType,
                        request, jwtToken, HttpStatusCode.OK);  
                    break;
                case "Delete":
                    string deleteProjUri = string.Format("{0}?projectUID={1}&actionUTC={2}", Config.ProjectCrudUri, projectUID, DateTime.UtcNow);
                    response = RestClientUtil.DoHttpRequest(deleteProjUri, "DELETE", RestClientConfig.JsonMediaType,
                        null, jwtToken, HttpStatusCode.OK);
                    break;
            }
        }

        [When(@"I '(.*)' the project with the customer via Web API")]
        public void WhenITheProjectWithTheCustomerViaWebAPI(string action)
        {
            string request;
            string response;
            string jwtToken = Jwt.GetJwtToken(userUID);

            switch(action)
            {
                case "Associate":
                    request = JsonConvert.SerializeObject(apiSupport.AssociateProjectCustomer(projectUID, customerUID));
                    response = RestClientUtil.DoHttpRequest(Config.AssociateProjectCustomerUri, "POST", RestClientConfig.JsonMediaType,
                        request, jwtToken, HttpStatusCode.OK);
                    break;
                case "Dissociate":
                    request = JsonConvert.SerializeObject(apiSupport.DissociateProjectCustomer(projectUID, customerUID));
                    response = RestClientUtil.DoHttpRequest(Config.DissociateProjectCustomerUri, "POST", RestClientConfig.JsonMediaType,
                        request, jwtToken, HttpStatusCode.OK);
                    break;
            }
        }


        [When(@"I try to get all projects for the customer via Web API")]
        public void WhenITryToGetAllProjectsForTheCustomerViaWebAPI()
        {
            string jwtToken = Jwt.GetJwtToken(userUID);
            string response = RestClientUtil.DoHttpRequest(Config.ProjectCrudUri, "GET", RestClientConfig.JsonMediaType,
                null, jwtToken, HttpStatusCode.OK);

            getAllProjectsResult = JsonConvert.DeserializeObject<Dictionary<long, ProjectDescriptor>>(response);
        }

        [When(@"the '(.*)' project is in the list returned by the Web API")]
        [Then(@"the '(.*)' project is in the list returned by the Web API")]
        public void ThenTheProjectIsInTheListReturnedByTheWebAPI(string action)
        {
            long projectId = Convert.ToInt64(DatabaseUtils.ExecuteMySqlQueryResult(Config.MySqlConnString,
                string.Format("SELECT ProjectID FROM {0}.Project WHERE ProjectUID = {1}", Config.MySqlDbName, projectUID)));

            switch(action)
            {
                case "Created":
                    Assert.IsTrue(getAllProjectsResult.ContainsKey(projectId), "Created project not in the list.");
                    break;
                case "Updated":
                    ProjectDescriptor updatedProj = getAllProjectsResult[projectId];
                    ScenarioContext.Current.Pending();
                    // TODO: check updatedProj has updated project name
                    break;
            }
        }

        [Then(@"project is not in the list returned by the Web API")]
        public void ThenProjectIsNotInTheListReturnedByTheWebAPI()
        {
            long projectId = Convert.ToInt64(DatabaseUtils.ExecuteMySqlQueryResult(Config.MySqlConnString,
                string.Format("SELECT ProjectID FROM {0}.Project WHERE ProjectUID = {1}", Config.MySqlDbName, projectUID)));

            Assert.IsFalse(getAllProjectsResult.ContainsKey(projectId), "Project still in the list.");
        }
    }
}
