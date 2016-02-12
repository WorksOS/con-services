using System;
using LandfillService.AcceptanceTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding, Scope(Feature = "MasterDataSubscriptions")]
    [TestClass]
    public class MasterDataSubscriptionsSteps
    {
        private readonly MasterDataSupport masterDataSupport = new MasterDataSupport();

        public MasterDataSubscriptionsSteps()
        {

            KafkaRpl.InitialiseKafkaRpl();
        }

        [Given(@"I inject the following master data event ""(.*)"" into kafka")]
        public void GivenIInjectTheFollowingMasterDataEventIntoKafka(string eventType)
        {
            var messageStr = string.Empty;
            var topic = string.Empty;
            var messageType = new MessageType();
            switch (eventType)
            {
                case "CreateCustomerEvent":
                    messageType = MessageType.CreateCustomerEvent;
                    messageStr = masterDataSupport.CreateCustomer();
                    break;
                case "CreateProjectEvent":
                    messageType = MessageType.CreateProjectEvent;
                    messageStr = masterDataSupport.CreateProjectEvent();
                    break;
                case "CreateProjectSubscriptionEvent":
                    messageType = MessageType.CreateProjectSubscriptionEvent;
                    messageStr = masterDataSupport.CreateProjectSubscription();
                    break;
                //case "CreateCustomerSubscriptionEvent":
                //    messageType = MessageType.CreateCustomerSubscriptionEvent;
                //    messageStr = masterDataSupport.CreateCustomerSubscription();
                //    break;
                case "UpdateProjectSubscriptionEvent":
                    messageType = MessageType.UpdateProjectSubscriptionEvent;
                    messageStr = masterDataSupport.UpdateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterCustomerUid);
                    break;
                //case "UpdateCustomerSubscriptionEvent":
                //    messageType = MessageType.UpdateCustomerSubscriptionEvent;
                //    messageStr = masterDataSupport.UpdateCustomerSubscription(masterDataSupport.masterSubscriptionUid);
                //    break;
                case "AssociateProjectSubscriptionEvent":
                    messageType = MessageType.AssociateProjectSubscriptionEvent;
                    messageStr = masterDataSupport.AssociateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterProjectUid);
                    break;
                case "AssociateCustomerUserEvent":
                    messageType = MessageType.AssociateCustomerUserEvent;
                    messageStr = masterDataSupport.AssociateCustomerUser(masterDataSupport.masterCustomerUid, Guid.NewGuid());
                    break;
                case "DissociateProjectSubscriptionEvent":
                    messageType = MessageType.DissociateProjectSubscriptionEvent;
                    messageStr = masterDataSupport.DissociateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterProjectUid);
                    break;                   
            }
            //var message = MessageFactory.Instance.CreateMessage(messageStr, messageType);
            //message.Send();
            topic = KafkaRpl.GetMyTopic(messageType);
            KafkaRpl.SendToKafka(topic, messageStr);
            Console.WriteLine();
        }

        [Then(@"I verify the correct subscription event in the database")]
        public void ThenIVerifyTheCorrectSubscriptionEventInTheDatabase()
        {
            Console.WriteLine("Now verify it is in the database");
        }
    }
}
