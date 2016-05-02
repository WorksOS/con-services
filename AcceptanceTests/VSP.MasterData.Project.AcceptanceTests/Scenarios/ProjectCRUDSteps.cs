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
            switch (action)
            {
                case "Create":
                    string request = JsonConvert.SerializeObject(apiSupport.CreateProject(projectUID));
                    string jwtToken = Jwt.GetJwtToken(userUID);
                    string response = RestClientUtil.DoHttpRequest(Config.ProjectCrudUri, "POST", RestClientConfig.JsonMediaType,
                        request, jwtToken, HttpStatusCode.OK);                    
                    break;
                case "Update":
                    // TODO
                    break;
            }
        }

        [When(@"I associate the project with the customer via Web API")]
        public void WhenIAssociateTheProjectWithTheCustomerViaWebAPI()
        {
            string request = JsonConvert.SerializeObject(apiSupport.AssociateProjectCustomer(projectUID, customerUID));
            string jwtToken = Jwt.GetJwtToken(userUID);
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

        [Then(@"the created project is in the list returned by the Web API")]
        public void ThenTheCreatedProjectIsInTheListReturnedByTheWebAPI()
        {
            long projectId = Convert.ToInt64(DatabaseUtils.ExecuteMySqlQueryResult(Config.MySqlConnString,
                string.Format("SELECT ProjectID FROM {0}.Project WHERE ProjectUID = {1}", Config.MySqlDbName, projectUID)));

            Assert.IsTrue(getAllProjectsResult.ContainsKey(projectId), "Project not in the list.");
        }
    }
}
