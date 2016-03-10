using System;
using System.Configuration;
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

        [Given(@"I inject the following master data event ""(.*)"" into kafka")]
        public void GivenIInjectTheFollowingMasterDataEventIntoKafka(string eventType)
        {
            var messageStr = string.Empty;
            var topic = string.Empty;
            var uniqueId = string.Empty;
            switch (eventType)
            {
                case "CreateCustomerEvent":
                    topic = ConfigurationManager.AppSettings["CustomerMasterDataTopic"];
                    messageStr = masterDataSupport.CreateCustomer();
                    break;
                case "CreateProjectEvent":
                    messageStr = masterDataSupport.CreateProjectEvent();
                    topic = ConfigurationManager.AppSettings["AssetMasterDataTopic"];
                    uniqueId = masterDataSupport.masterProjectUid.ToString();
                    break;
                case "CreateProjectSubscriptionEvent":
                    messageStr = masterDataSupport.CreateProjectSubscription();
                    topic = ConfigurationManager.AppSettings["SubscriptionTopic"];
                    uniqueId = masterDataSupport.masterSubscriptionUid.ToString();
                    break;
                case "UpdateProjectSubscriptionEvent":
                    messageStr = masterDataSupport.UpdateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterCustomerUid);
                    topic = ConfigurationManager.AppSettings["SubscriptionTopic"];
                    uniqueId = masterDataSupport.masterSubscriptionUid.ToString();
                    break;
                case "AssociateProjectSubscriptionEvent":
                    messageStr = masterDataSupport.AssociateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterProjectUid);
                    topic = ConfigurationManager.AppSettings["SubscriptionTopic"];
                    uniqueId = masterDataSupport.masterSubscriptionUid.ToString();
                    break;
                case "AssociateCustomerUserEvent":
                    messageStr = masterDataSupport.AssociateCustomerUser(masterDataSupport.masterCustomerUid, Guid.NewGuid());
                    topic = ConfigurationManager.AppSettings["CustomerUserMasterDataTopic"];
                    uniqueId = masterDataSupport.masterCustomerUid.ToString();
                    break;
                case "DissociateProjectSubscriptionEvent":
                    messageStr = masterDataSupport.DissociateProjectSubscription(masterDataSupport.masterSubscriptionUid, masterDataSupport.masterProjectUid);
                    topic = ConfigurationManager.AppSettings["SubscriptionTopic"];
                    uniqueId = masterDataSupport.masterSubscriptionUid.ToString();
                    break;                   
            }

            KafkaResolver.SendMessage(topic, KafkaResolver.ResolveTopic(topic), messageStr, uniqueId);
        }

        [Then(@"I verify the correct subscription event in the database")]
        public void ThenIVerifyTheCorrectSubscriptionEventInTheDatabase()
        {
            Console.WriteLine("Now verify it is in the database");
        }
    }
}
