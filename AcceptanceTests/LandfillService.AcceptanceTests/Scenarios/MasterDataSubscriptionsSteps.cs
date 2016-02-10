using System;
using LandfillService.AcceptanceTests.Helpers;
using LandfillService.AcceptanceTests.Models.KafkaTopics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TechTalk.SpecFlow;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding, Scope(Feature = "MasterDataSubscriptions")]
    [TestClass]
    public class MasterDataSubscriptionsSteps
    {
        private CreateProjectSubscriptionEvent expectedProjectSubscriptionEvent;
        private CreateCustomerSubscriptionEvent expectedCustomerSubscriptionEvent;
        private CreateAssetSubscriptionEvent expectedAssetSubscriptionEvent;
        private UpdateProjectSubscriptionEvent expectedUpdateProjectSubscriptionEvent;
        private UpdateCustomerSubscriptionEvent expectedUpdateCustomerSubscriptionEvent;
        private UpdateAssetSubscriptionEvent expectedUpdateAssetSubscriptionEvent;
        private AssociateProjectSubscriptionEvent expectedAssociatedProjectSubscriptionEvent;
        private DissociateProjectSubscriptionEvent expectedDissociatedProjectSubscriptionEvent;

        [Given(@"I inject the following master data subscription event ""(.*)"" into kafka")]
        public void GivenIInjectTheFollowingMasterDataSubscriptionEventIntoKafka(string eventType)
        {
            var messageStr = string.Empty;
            var messageType = new MessageType();
            switch (eventType)
            {
                case "CreateProjectSubscriptionEvent":
                    messageType = MessageType.CreateProjectSubscriptionEvent;
                    messageStr = CreateProjectSubscription();
                    break;
                case "CreateCustomerSubscriptionEvent":
                    messageType = MessageType.CreateCustomerSubscriptionEvent;
                    messageStr = CreateCustomerSubscription();
                    break;
                case "CreateAssetSubscriptionEvent":
                    messageType = MessageType.CreateAssetSubscriptionEvent;
                    messageStr = CreateAssetSubscription();
                    break;
                case "UpdateAssetSubscriptionEvent":
                    messageType = MessageType.UpdateAssetSubscriptionEvent;
                    messageStr = UpdateAssetSubscription();
                    break;
                case "UpdateProjectSubscriptionEvent":
                    messageType = MessageType.UpdateProjectSubscriptionEvent;
                    messageStr = UpdateProjectSubscription();
                    break;
                case "UpdateCustomerSubscriptionEvent":
                    messageType = MessageType.UpdateCustomerSubscriptionEvent;
                    messageStr = UpdateCustomerSubscription();
                    break;
                case "AssociateProjectSubscriptionEvent":
                    messageType = MessageType.AssociateProjectSubscriptionEvent;
                    messageStr = AssociateProjectSubscription();
                    break;
                case "DissociateProjectSubscriptionEvent":
                    messageType = MessageType.DissociateProjectSubscriptionEvent;
                    messageStr = DissociateProjectSubscription();
                    break;                   
            }
            var message = MessageFactory.Instance.CreateMessage(messageStr, messageType);
            message.Send();
        }

        /// <summary>
        /// Set up the Create project subscription
        /// </summary>
        /// <returns></returns>
        private string CreateProjectSubscription()
        {
            expectedProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = new Guid(),
                SubscriptionType = "project",
                SubscriptionUID = new Guid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate  = DateTime.Now.AddMonths(11),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = expectedProjectSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        /// <summary>
        /// Update project subscription 
        /// </summary>
        /// <returns></returns>
        private string UpdateProjectSubscription()
        {
            expectedUpdateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = new Guid(),
                SubscriptionType = "project",
                SubscriptionUID = new Guid(),
                StartDate = DateTime.Now.AddMonths(-5),
                EndDate = DateTime.Now.AddMonths(7),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = expectedUpdateProjectSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Create Asset subscription
        /// </summary>
        /// <returns></returns>
        private string CreateAssetSubscription()
        {
            expectedAssetSubscriptionEvent = new CreateAssetSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                AssetUID = new Guid(),
                DeviceUID = new Guid(),
                CustomerUID = new Guid(),
                SubscriptionType = "asset",
                SubscriptionUID = new Guid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(11),                
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { CreateAssetSubscriptionEvent = expectedAssetSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Update Asset Subscription
        /// </summary>
        /// <returns></returns>
        private string UpdateAssetSubscription()
        {
            expectedUpdateAssetSubscriptionEvent = new UpdateAssetSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                AssetUID = new Guid(),
                DeviceUID = new Guid(),
                CustomerUID = new Guid(),
                SubscriptionType = "asset",
                SubscriptionUID = new Guid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(11),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { UpdateAssetSubscriptionEvent = expectedUpdateAssetSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Create a customer subscription
        /// </summary>
        /// <returns></returns>
        private string CreateCustomerSubscription()
        {
            expectedCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,                
                CustomerUID = new Guid(),
                SubscriptionType = "customer",
                SubscriptionUID = new Guid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(11),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { CreateCustomerSubscriptionEvent = expectedCustomerSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Update a customer subscription
        /// </summary>
        /// <returns></returns>
        private string UpdateCustomerSubscription()
        {
            expectedUpdateCustomerSubscriptionEvent = new UpdateCustomerSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                SubscriptionUID = new Guid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(11),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { UpdateCustomerSubscriptionEvent = expectedUpdateCustomerSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Dissociate a project subscription
        /// </summary>
        /// <returns></returns>
        private string DissociateProjectSubscription()
        {
            expectedDissociatedProjectSubscriptionEvent = new DissociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                SubscriptionUID = new Guid(),
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID =  new Guid(),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { DissociateProjectSubscriptionEvent = expectedDissociatedProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Associate a project subscription
        /// </summary>
        /// <returns></returns>
        private string AssociateProjectSubscription()
        {
            expectedAssociatedProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                SubscriptionUID = new Guid(),
                EffectiveDate = DateTime.Now.AddMonths(-1), 
                ProjectUID =  new Guid(),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = expectedAssociatedProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        [Then(@"I verify the correct subscription event in the database")]
        public void ThenIVerifyTheCorrectSubscriptionEventInTheDatabase()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
