using System;
using TechTalk.SpecFlow;
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
        
        [When(@"I try to '(.*)' a project via Web API as the user for the customer")]
        public void WhenITryToAProjectViaWebAPIAsTheUserForTheCustomer(string action)
        {
            switch (action)
            {
                case "Create":
                    // TODO
                    break;
            }
        }
        
        [When(@"I try to get all projects for the customer via Web API")]
        public void WhenITryToGetAllProjectsForTheCustomerViaWebAPI()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the created project is in the list returned by the Web API")]
        public void ThenTheCreatedProjectIsInTheListReturnedByTheWebAPI()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
