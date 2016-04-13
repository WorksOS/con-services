using System;
using System.Configuration;
using LandfillService.AcceptanceTests.Scenarios.ScenarioSupports;
using LandfillService.AcceptanceTests.LandFillKafka;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class MasterDataSteps
    {
        Guid SubscriptionUID = Guid.NewGuid();
        Guid ProjectUID = Guid.NewGuid();
        Guid CustomerUID = Guid.NewGuid();

        [Given(@"I inject '(.*)' into Kafka")]
        public void GivenIInjectIntoKafka(string eventType)
        {
            string messageStr = "";
            string topic = "";

            switch(eventType)
            {
                case "CreateProjectEvent":
                    messageStr = MasterDataSupport.CreateProject(ProjectUID);
                    topic = Config.ProjectMasterDataTopic;
                    break;
                case "CreateProjectSubscriptionEvent":
                    break;
                case "AssociateProjectCustomer":
                    break;
                case "AssociateProjectSubscriptionEvent":
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

        [When(@"I make a Web API request for a list of projects")]
        public void WhenIMakeAWebAPIRequestForAListOfProjects()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the created project is in the list")]
        public void ThenTheCreatedProjectIsInTheList()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the number of days to subscription expiry is correct")]
        public void ThenTheNumberOfDaysToSubscriptionExpiryIsCorrect()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
