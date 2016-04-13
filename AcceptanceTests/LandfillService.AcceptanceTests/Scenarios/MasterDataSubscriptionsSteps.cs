//using System;
//using System.Configuration;
//using LandfillService.AcceptanceTests.Scenarios.ScenarioSupports;
//using LandfillService.AcceptanceTests.LandFillKafka;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using TechTalk.SpecFlow;

//namespace LandfillService.AcceptanceTests.Scenarios
//{
//    [Binding, Scope(Feature = "MasterDataSubscriptions")]
//    [TestClass]
//    public class MasterDataSubscriptionsSteps
//    {
//        private readonly MasterDataSupport masterDataSupport = new MasterDataSupport();      

//        [Given(@"I inject the following master data event ""(.*)"" into kafka")]
//        public void GivenIInjectTheFollowingMasterDataEventIntoKafka(string eventTypeStr)
//        {
//            string messageStr = string.Empty;
//            string topic = string.Empty;

//            switch (eventTypeStr)
//            {
//                case "CreateCustomerEvent":
//                    messageStr = masterDataSupport.CreateCustomer(); 
//                    topic = Config.CustomerMasterDataTopic;
//                    break;
//                case "CreateProjectEvent":
//                    messageStr = masterDataSupport.CreateProject();
//                    topic = Config.ProjectMasterDataTopic;
//                    break;
//                case "CreateProjectSubscriptionEvent":
//                    messageStr = masterDataSupport.CreateProjectSubscription();
//                    topic = Config.SubscriptionTopic;
//                    break;
//                case "UpdateProjectSubscriptionEvent":
//                    messageStr = masterDataSupport.UpdateProjectSubscription(masterDataSupport.SubscriptionUID, masterDataSupport.CustomerUID);
//                    topic = Config.SubscriptionTopic;
//                    break;
//                case "AssociateProjectSubscriptionEvent":
//                    //messageStr = masterDataSupport.AssociateProjectSubscription(masterDataSupport.MasterSubscriptionGuid, masterDataSupport.MasterProjectGuid);
//                    messageStr = masterDataSupport.AssociateProjectSubscription(Guid.Parse("43b9f670-fad3-4896-9f79-20cdb8cc9e96"), Guid.Parse("2f71d9b6-0e57-487b-9a47-412e7242d46b"));
//                    topic = Config.SubscriptionTopic;
//                    break;
//                case "AssociateCustomerUserEvent":
//                    messageStr = masterDataSupport.AssociateCustomerUser(masterDataSupport.CustomerUID, Guid.NewGuid());
//                    topic = Config.CustomerUserMasterDataTopic;
//                    break;
//                case "DissociateProjectSubscriptionEvent":
//                    messageStr = masterDataSupport.DissociateProjectSubscription(masterDataSupport.SubscriptionUID, masterDataSupport.ProjectUID);
//                    topic = Config.SubscriptionTopic;
//                    break;    
//                case "AssociateProjectCustomer":
//                    //messagestr = masterdatasupport.associateprojectcustomer(masterdatasupport.mastercustomerguid, masterdatasupport.masterprojectguid);
//                    messageStr = masterDataSupport.AssociateProjectCustomer(Guid.Parse("8ec5b8f3-fb47-4d65-aaf4-e2276fc6f63c"), Guid.Parse("2f71d9b6-0e57-487b-9a47-412e7242d46b"));
//                    topic = Config.ProjectMasterDataTopic;
//                    break;    
//            }

//            if (Config.KafkaDriver == "JAVA")
//            {
//                KafkaResolver.SendMessage(topic, messageStr);
//            }
//            if (Config.KafkaDriver == ".NET")
//            {
//                KafkaDotNet.SendMessage(topic, messageStr);
//            }
//        }

//        [Then(@"I verify the correct subscription event in the database")]
//        public void ThenIVerifyTheCorrectSubscriptionEventInTheDatabase()
//        {
//            Console.WriteLine("Now verify it is in the database");
//        }
//    }
//}
