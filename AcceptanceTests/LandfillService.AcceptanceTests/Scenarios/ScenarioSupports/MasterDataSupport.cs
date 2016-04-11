using System;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Models.KafkaTopics;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using LandfillService.AcceptanceTests.LandFillKafka;
using LandfillService.AcceptanceTests.Utils;

namespace LandfillService.AcceptanceTests.Scenarios.ScenarioSupports
{
    public class MasterDataSupport
    {
        private CreateProjectSubscriptionEvent expectedProjectSubscriptionEvent;
     //   private CreateCustomerSubscriptionEvent expectedCustomerSubscriptionEvent;
        private UpdateProjectSubscriptionEvent expectedUpdateProjectSubscriptionEvent;
      //  private UpdateCustomerSubscriptionEvent expectedUpdateCustomerSubscriptionEvent;
        private AssociateProjectSubscriptionEvent expectedAssociatedProjectSubscriptionEvent;
        private AssociateCustomerUserEvent expectedAssociateCustomerUserEvent;
        private DissociateProjectSubscriptionEvent expectedDissociatedProjectSubscriptionEvent;
        private CreateCustomerEvent expectedCreateCustomerEvent;
        private CreateProjectEvent expectedCreateProjectEvent;

        public Guid masterSubscriptionUid;
        public Guid masterProjectUid;
        public Guid masterCustomerUid;


        public string CreateProjectEvent()
        {
            var projectName = "AT-Test" + DateTime.Now;
            var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            expectedCreateProjectEvent = new CreateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundary = " ",
                ProjectEndDate = DateTime.Today.AddMonths(10),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = projectName,
                ProjectTimezone = "America/Chicago",
                ProjectType = ProjectType.LandFill, // : ProjectType.Full3D,
                ProjectID = projectId,
                ProjectUID = Guid.NewGuid(),                
                ReceivedUTC = DateTime.UtcNow
            };
            masterProjectUid = expectedCreateProjectEvent.ProjectUID;
            return JsonConvert.SerializeObject(new { CreateProjectEvent = expectedCreateProjectEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <returns></returns>
        public string CreateCustomer()
        {
           expectedCreateCustomerEvent = new CreateCustomerEvent
            {
                CustomerUID = Guid.NewGuid(),
                CustomerName = "AT-" + DateTime.Now,
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
            };
           masterCustomerUid = expectedCreateCustomerEvent.CustomerUID;
           return JsonConvert.SerializeObject(new { CreateCustomerEvent = expectedCreateCustomerEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Set up the Create project subscription
        /// </summary>
        /// <returns></returns>
        public string CreateProjectSubscription()
        {
            expectedProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = Guid.NewGuid(),
                SubscriptionType = "LandFill",
                SubscriptionUID = Guid.NewGuid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(11),
                ReceivedUTC = DateTime.UtcNow
            };
            masterSubscriptionUid = expectedProjectSubscriptionEvent.SubscriptionUID;
            masterCustomerUid = expectedProjectSubscriptionEvent.CustomerUID;

            return JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = expectedProjectSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        /// <summary>
        /// Update project subscription 
        /// </summary>
        /// <returns></returns>
        public string UpdateProjectSubscription(Guid subscriptionUid, Guid customerUid)
        {
            expectedUpdateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                SubscriptionType = "LandFill",
                SubscriptionUID = subscriptionUid,
                StartDate = DateTime.Now.AddMonths(-5),
                EndDate = DateTime.Now.AddMonths(7),
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = expectedUpdateProjectSubscriptionEvent },
                            new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Create a customer subscription
        /// </summary>
        /// <returns></returns>
        //public string CreateCustomerSubscription()
        //{
        //    expectedCustomerSubscriptionEvent = new CreateCustomerSubscriptionEvent
        //    {
        //        ActionUTC = DateTime.UtcNow,
        //        CustomerUID = Guid.NewGuid(),
        //        SubscriptionType = "customer",
        //        SubscriptionUID = Guid.NewGuid(),
        //        StartDate = DateTime.Now.AddMonths(-1),
        //        EndDate = DateTime.Now.AddMonths(11),
        //        ReceivedUTC = DateTime.UtcNow
        //    };
        //    masterSubscriptionUid = expectedCustomerSubscriptionEvent.SubscriptionUID;
        //    masterCustomerUid = expectedCustomerSubscriptionEvent.CustomerUID;
        //    return JsonConvert.SerializeObject(new { CreateCustomerSubscriptionEvent = expectedCustomerSubscriptionEvent },
        //                    new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        //}

        /// <summary>
        /// Update a customer subscription
        /// </summary>
        /// <returns></returns>
        //public string UpdateCustomerSubscription(Guid subscriptionUid)
        //{
        //    expectedUpdateCustomerSubscriptionEvent = new UpdateCustomerSubscriptionEvent
        //    {
        //        ActionUTC = DateTime.UtcNow,
        //        SubscriptionUID = subscriptionUid,
        //        StartDate = DateTime.Now.AddMonths(-1),
        //        EndDate = DateTime.Now.AddMonths(11),
        //        ReceivedUTC = DateTime.UtcNow
        //    };
        //    return JsonConvert.SerializeObject(new { UpdateCustomerSubscriptionEvent = expectedUpdateCustomerSubscriptionEvent },
        //                    new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        //}

        /// <summary>
        /// Dissociate a project subscription
        /// </summary>
        /// <returns></returns>
        public string DissociateProjectSubscription(Guid subscriptionUid, Guid projectUid)
        {
            expectedDissociatedProjectSubscriptionEvent = new DissociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectUid,
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { DissociateProjectSubscriptionEvent = expectedDissociatedProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Associate a project subscription
        /// </summary>
        /// <returns></returns>
        public string AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid)
        {
            expectedAssociatedProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectUid,
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = expectedAssociatedProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        /// <summary>
        /// Associate a project subscription
        /// </summary>
        /// <returns></returns>
        public string AssociateCustomerUser(Guid customerUid, Guid userUid)
        {
            expectedAssociateCustomerUserEvent = new AssociateCustomerUserEvent
            {
                ActionUTC= DateTime.UtcNow,
                CustomerUID = customerUid,
                ReceivedUTC = DateTime.UtcNow,
                UserUID = userUid
            };

            return JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = expectedAssociateCustomerUserEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

    }
}
