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
        public static string CreateProject(Guid projectUid)
        {
            var projectName = "AT_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss");
            var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            CreateProjectEvent createProjectEvent = new CreateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundary = " ",
                ProjectEndDate = DateTime.Today.AddMonths(10),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = projectName,
                ProjectTimezone = "New Zealand Standard Time",
                ProjectType = ProjectType.LandFill,
                ProjectID = projectId == 1 ? LandfillCommonUtils.Random.Next(3000, 4000) : projectId,
                ProjectUID = Guid.NewGuid(),                
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { CreateProjectEvent = createProjectEvent },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public static string CreateCustomer(Guid customerUid)
        {
           CreateCustomerEvent evt = new CreateCustomerEvent
           {
               CustomerUID = customerUid,
                CustomerName = "AT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
           };
           return JsonConvert.SerializeObject(new { CreateCustomerEvent = evt },
               new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public static string CreateProjectSubscription(Guid subscriptionUid, Guid customerUid)
        {
            CreateProjectSubscriptionEvent evt = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                SubscriptionType = "Landfill",
                SubscriptionUID = subscriptionUid,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(11)
            };

            return JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = evt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public static string AssociateProjectSubscription(Guid projectUid, Guid subscriptionUid)
        {
            AssociateProjectSubscriptionEvent evt = new AssociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectUid
            };
            return JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = evt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public static string AssociateProjectCustomer(Guid projectUid, Guid customerUid)
        {
            AssociateProjectCustomer evt = new AssociateProjectCustomer
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                ProjectUID = projectUid,
                CustomerUID = customerUid
            };

            return JsonConvert.SerializeObject(new { AssociateProjectCustomerEvent = evt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }

        public static string UpdateProjectSubscription(Guid subscriptionUid, Guid customerUid)
        {
            UpdateProjectSubscriptionEvent evt = new UpdateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                SubscriptionType = "LandFill",
                SubscriptionUID = subscriptionUid,
                StartDate = DateTime.Now.AddMonths(-5),
                EndDate = DateTime.Now.AddMonths(7)
            };
            return JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = evt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
        public static string DissociateProjectSubscription(Guid projectUid, Guid subscriptionUid)
        {
            DissociateProjectSubscriptionEvent evt = new DissociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectUid,
            };
            return JsonConvert.SerializeObject(new { DissociateProjectSubscriptionEvent = evt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }       
        public static string AssociateCustomerUser(Guid customerUid, Guid userUid)
        {
            AssociateCustomerUserEvent evt = new AssociateCustomerUserEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                UserUID = userUid
            };

            return JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = evt },
                new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });
        }
    }
}
