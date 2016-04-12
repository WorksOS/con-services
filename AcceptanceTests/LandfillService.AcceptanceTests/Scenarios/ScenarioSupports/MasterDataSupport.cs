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
        private CreateProjectSubscriptionEvent createProjectSubscriptionEvent;
        private UpdateProjectSubscriptionEvent updateProjectSubscriptionEvent;
        private AssociateProjectSubscriptionEvent associateProjectSubscriptionEvent;
        private AssociateCustomerUserEvent associateCustomerUserEvent;
        private DissociateProjectSubscriptionEvent dissociateProjectSubscriptionEvent;
        private CreateCustomerEvent createCustomerEvent;
        private CreateProjectEvent createProjectEvent;

        public Guid MasterSubscriptionGuid;
        public Guid MasterProjectGuid;
        public Guid MasterCustomerGuid;

        public string CreateProjectEvent()
        {
            var projectName = "AT_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss");
            var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            createProjectEvent = new CreateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundary = " ",
                ProjectEndDate = DateTime.Today.AddMonths(10),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = projectName,
                ProjectTimezone = "New Zealand Standard Time",
                ProjectType = ProjectType.LandFill,
                ProjectID = projectId,
                ProjectUID = Guid.NewGuid(),                
                ReceivedUTC = DateTime.UtcNow
            };
            MasterProjectGuid = createProjectEvent.ProjectUID;
            return JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string CreateCustomer()
        {
           createCustomerEvent = new CreateCustomerEvent
           {
                CustomerUID = Guid.NewGuid(),
                CustomerName = "AT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
           };
           MasterCustomerGuid = createCustomerEvent.CustomerUID;
           return JsonConvert.SerializeObject(new { CreateCustomerEvent = createCustomerEvent },
               new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string CreateProjectSubscription()
        {
            createProjectSubscriptionEvent = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = Guid.NewGuid(),
                SubscriptionType = "LandFill",
                SubscriptionUID = Guid.NewGuid(),
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now.AddMonths(11)
            };
            MasterSubscriptionGuid = createProjectSubscriptionEvent.SubscriptionUID;
            MasterCustomerGuid = createProjectSubscriptionEvent.CustomerUID;

            return JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = createProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string UpdateProjectSubscription(Guid subscriptionGuid, Guid customerGuid)
        {
            updateProjectSubscriptionEvent = new UpdateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerGuid,
                SubscriptionType = "LandFill",
                SubscriptionUID = subscriptionGuid,
                StartDate = DateTime.Now.AddMonths(-5),
                EndDate = DateTime.Now.AddMonths(7)
            };
            return JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = updateProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string DissociateProjectSubscription(Guid subscriptionGuid, Guid projectGuid)
        {
            dissociateProjectSubscriptionEvent = new DissociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionGuid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectGuid,
            };
            return JsonConvert.SerializeObject(new { DissociateProjectSubscriptionEvent = dissociateProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string AssociateProjectSubscription(Guid subscriptionGuid, Guid projectGuid)
        {
            associateProjectSubscriptionEvent = new AssociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionGuid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectGuid
            };
            return JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = associateProjectSubscriptionEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public string AssociateCustomerUser(Guid customerGuid, Guid userGuid)
        {
            associateCustomerUserEvent = new AssociateCustomerUserEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerGuid,
                UserUID = userGuid
            };

            return JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = associateCustomerUserEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
    }
}
