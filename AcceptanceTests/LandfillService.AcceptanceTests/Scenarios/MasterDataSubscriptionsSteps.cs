using System;
using System.Configuration;
using LandfillService.AcceptanceTests.Scenarios.ScenarioSupports;
using LandfillService.AcceptanceTests.LandFillKafka;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding, Scope(Feature = "MasterDataSubscriptions")]
    [TestClass]
    public class MasterDataSubscriptionsSteps
    {
        private readonly MasterDataSupport masterDataSupport = new MasterDataSupport();      

        [Given(@"I inject the following master data event ""(.*)"" into kafka")]
        public void GivenIInjectTheFollowingMasterDataEventIntoKafka(string eventTypeStr)
        {
            string messageStr = string.Empty;
            string topic = string.Empty;
            EventType eventType = (EventType)Enum.Parse(typeof(EventType), eventTypeStr);

            switch (eventType)
            {
                case EventType.CreateCustomerEvent:
                    messageStr = masterDataSupport.CreateCustomer(); 
                    topic = Config.CustomerMasterDataTopic;
                    break;
                case EventType.CreateProjectEvent:
                    messageStr = masterDataSupport.CreateProjectEvent();
                    topic = Config.ProjectMasterDataTopic;
                    break;
                case EventType.CreateProjectSubscriptionEvent:
                    messageStr = masterDataSupport.CreateProjectSubscription();
                    topic = Config.SubscriptionTopic;
                    break;
                case EventType.UpdateProjectSubscriptionEvent:
                    messageStr = masterDataSupport.UpdateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterCustomerUid);
                    topic = Config.SubscriptionTopic;
                    break;
                case EventType.AssociateProjectSubscriptionEvent:
                    messageStr = masterDataSupport.AssociateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterProjectUid);
                    topic = Config.SubscriptionTopic;
                    break;
                case EventType.AssociateCustomerUserEvent:
                    messageStr = masterDataSupport.AssociateCustomerUser(masterDataSupport.masterCustomerUid, Guid.NewGuid());
                    topic = Config.CustomerUserMasterDataTopic;
                    break;
                case EventType.DissociateProjectSubscriptionEvent:
                    messageStr = masterDataSupport.DissociateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterProjectUid);
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

        [Then(@"I verify the correct subscription event in the database")]
        public void ThenIVerifyTheCorrectSubscriptionEventInTheDatabase()
        {
            Console.WriteLine("Now verify it is in the database");
        }
    }
}
