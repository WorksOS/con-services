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

        [Given(@"I create and associate a dummy project via Web API as the user for the customer")]
        public void GivenICreateAndAssociateADummyProjectViaWebAPIAsTheUserForTheCustomer()
        {
            Guid dummyProjUid = Guid.NewGuid();
            string jwtToken = Jwt.GetJwtToken(userUID);

            RestClientUtil.DoHttpRequest(Config.ProjectCrudUri, "POST", RestClientConfig.JsonMediaType,
                JsonConvert.SerializeObject(apiSupport.CreateProject(dummyProjUid)), jwtToken, HttpStatusCode.OK);

            RestClientUtil.DoHttpRequest(Config.AssociateProjectCustomerUri, "POST", RestClientConfig.JsonMediaType,
                JsonConvert.SerializeObject(apiSupport.AssociateProjectCustomer(dummyProjUid, customerUID)), jwtToken, HttpStatusCode.OK);
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
                    request = JsonConvert.SerializeObject(apiSupport.UpdateProject(projectUID));
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

        [When(@"I associate the project with the customer via Web API")]
        public void WhenIAssociateTheProjectWithTheCustomerViaWebAPI()
        {
            string jwtToken = Jwt.GetJwtToken(userUID);
            string request = JsonConvert.SerializeObject(apiSupport.AssociateProjectCustomer(projectUID, customerUID));
            string response = RestClientUtil.DoHttpRequest(Config.AssociateProjectCustomerUri, "POST", RestClientConfig.JsonMediaType,
                request, jwtToken, HttpStatusCode.OK);
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
                string.Format("SELECT ProjectID FROM {0}.Project WHERE ProjectUID = '{1}'", Config.MySqlDbName, projectUID)));

            switch(action)
            {
                case "Created":
                    Assert.IsTrue(getAllProjectsResult.ContainsKey(projectId), "Created project not in the list.");
                    break;
                case "Updated":
                    ProjectDescriptor updatedProj = getAllProjectsResult[projectId];
                    Assert.AreEqual(apiSupport.UpdateProjectRequest.ProjectType == ProjectType.LandFill, updatedProj.isLandFill,
                        "Project not updated.");
                    break;
            }
        }

        [Then(@"deleted project is not in the list returned by the Web API")]
        public void ThenDeletedProjectIsNotInTheListReturnedByTheWebAPI()
        {
            long projectId = Convert.ToInt64(DatabaseUtils.ExecuteMySqlQueryResult(Config.MySqlConnString,
                string.Format("SELECT ProjectID FROM {0}.Project WHERE ProjectUID = '{1}'", Config.MySqlDbName, projectUID)));

            Assert.IsFalse(getAllProjectsResult.ContainsKey(projectId), "Project still in the list.");
        }
    }
}
