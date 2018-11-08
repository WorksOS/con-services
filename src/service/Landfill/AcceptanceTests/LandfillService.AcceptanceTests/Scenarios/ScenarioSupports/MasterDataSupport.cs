using System;
using LandfillService.AcceptanceTests.Models;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using LandfillService.AcceptanceTests.LandFillKafka;
using LandfillService.AcceptanceTests.Utils;

namespace LandfillService.AcceptanceTests.Scenarios.ScenarioSupports
{
    public class MasterDataSupport
    {
        public CreateProjectEvent CreateProjectEvt;
        public UpdateProjectEvent UpdateProjectEvt;
        public DeleteProjectEvent DeleteProjectEvt;

        public CreateCustomerEvent CreateCustomerEvt;
        public UpdateCustomerEvent UpdateCustomerEvt;
        public DeleteCustomerEvent DeleteCustomerEvt;

        public AssociateCustomerUserEvent AssociateCustomerUserEvt;
        public CreateProjectSubscriptionEvent CreateProjectSubscriptionEvt;
        public AssociateProjectSubscriptionEvent AssociateProjectSubscriptionEvt;
        public AssociateProjectCustomer AssociateProjectCustomerEvt;
        public UpdateProjectSubscriptionEvent UpdateProjectSubscriptionEvt;

        public CreateGeofenceEvent CreateProjectGeofenceEvt;
        public UpdateGeofenceEvent UpdateProjectGeofenceEvt;
        public DeleteGeofenceEvent DeleteProjectGeofenceEvt;

        public CreateGeofenceEvent CreateInBoundaryLandfillGeofenceEvt;
        public CreateGeofenceEvent CreateOutBoundaryLandfillGeofenceEvt;
        public AssociateProjectGeofence AssociateProjectGeofenceEvt;

        public string CreateProject(Guid projectUid)
        {
            var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            CreateProjectEvt = new CreateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundary = " ",
                ProjectEndDate = DateTime.Today.AddMonths(10),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = "AT_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                ProjectTimezone = "New Zealand Standard Time",
                ProjectType = ProjectType.LandFill,
                ProjectID = projectId == 1 ? LandfillCommonUtils.Random.Next(2000, 3000) : projectId,
                ProjectUID = projectUid,                
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { CreateProjectEvent = CreateProjectEvt });
        }
        public string UpdateProject(Guid projectUid)
        {
            var projectName = "upAT_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss");
            UpdateProjectEvt = new UpdateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectEndDate = DateTime.Today.AddMonths(12),
                ProjectName = projectName,
                ProjectType = ProjectType.LandFill,
                ProjectUID = projectUid,
            };
            return JsonConvert.SerializeObject(new { UpdateProjectEvent = UpdateProjectEvt });
        }
        public string DeleteProject(Guid projectUid)
        {
            DeleteProjectEvt = new DeleteProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                ProjectUID = projectUid
            };
            return JsonConvert.SerializeObject(new { DeleteProjectEvent = DeleteProjectEvt });
        }

        public string CreateCustomer(Guid customerUid)
        {
            CreateCustomerEvt = new CreateCustomerEvent
           {
                CustomerUID = customerUid,
                CustomerName = "AT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
           };
            return JsonConvert.SerializeObject(new { CreateCustomerEvent = CreateCustomerEvt });
        }
        public string UpdateCustomer(Guid customerUid)
        {
            UpdateCustomerEvt = new UpdateCustomerEvent
            {
                CustomerUID = customerUid,
                CustomerName = "upAT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { UpdateCustomerEvent = UpdateCustomerEvt });
        }
        public string DeleteCustomer(Guid customerUid)
        {
            DeleteCustomerEvt = new DeleteCustomerEvent
            {
                CustomerUID = customerUid,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            return JsonConvert.SerializeObject(new { DeleteCustomerEvent = DeleteCustomerEvt });
        }
        public string AssociateCustomerUser(Guid customerUid, Guid userUid)
        {
            AssociateCustomerUserEvt = new AssociateCustomerUserEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                UserUID = userUid
            };

            return JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = AssociateCustomerUserEvt });
        }

        public string CreateProjectSubscription(Guid subscriptionUid, Guid customerUid)
        {
            CreateProjectSubscriptionEvt = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                SubscriptionType = "Landfill",
                SubscriptionUID = subscriptionUid,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(11)
            };

            return JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = CreateProjectSubscriptionEvt });
        }
        public string AssociateProjectSubscription(Guid projectUid, Guid subscriptionUid)
        {
            AssociateProjectSubscriptionEvt = new AssociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                SubscriptionUID = subscriptionUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = projectUid
            };
            return JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = AssociateProjectSubscriptionEvt });
        }
        public string AssociateProjectCustomer(Guid projectUid, Guid customerUid)
        {
            AssociateProjectCustomerEvt = new AssociateProjectCustomer
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                ProjectUID = projectUid,
                CustomerUID = customerUid
            };

            return JsonConvert.SerializeObject(new { AssociateProjectCustomer = AssociateProjectCustomerEvt });
        }
        public string UpdateProjectSubscription(Guid subscriptionUid, Guid customerUid)
        {
            UpdateProjectSubscriptionEvt = new UpdateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = customerUid,
                SubscriptionType = "Landfill",
                SubscriptionUID = subscriptionUid,
                StartDate = DateTime.Now.AddMonths(-5),
                EndDate = DateTime.Now.AddMonths(7)
            };
            return JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = UpdateProjectSubscriptionEvt });
        }

        public string CreateProjectGeofence(Guid geofenceUid, Guid customerUid, Guid userUid, string projectName)
        {
            CreateProjectGeofenceEvt = new CreateGeofenceEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                GeofenceUID = geofenceUid,
                CustomerUID = customerUid,
                UserUID = userUid,
                GeofenceName = projectName,
                Description = "Created project geofence",
                FillColor = 0x00FF00,
                IsTransparent = true,
                GeofenceType = "Project",
                GeometryWKT = "POLYGON((-77.100849 42.836199,-77.110119 42.863635,-77.061367 42.866025,-77.050896 42.836451,-77.100849 42.836199,-77.100849 42.836199))"
            };

            return JsonConvert.SerializeObject(new { CreateGeofenceEvent = CreateProjectGeofenceEvt });
        }
        public string AssociateProjectGeofence(Guid projectUID, Guid geofenceUID)
        {
            AssociateProjectGeofenceEvt = new AssociateProjectGeofence
            {
                ProjectUID = projectUID,
                GeofenceUID = geofenceUID,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };

            return JsonConvert.SerializeObject(new { AssociateProjectGeofence = AssociateProjectGeofenceEvt });
        }
        public string UpdateProjectGeofence(Guid geofenceUid, Guid userUid)
        {
            UpdateProjectGeofenceEvt = new UpdateGeofenceEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                GeofenceUID = geofenceUid,
                UserUID = userUid,
                GeofenceName = "upAT_Geo-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                Description = "Updated project geofence",
                FillColor = 0x00FF00,
                IsTransparent = true,
                GeofenceType = "Project",
                GeometryWKT = "POLYGON((-77.100849 42.836199,-77.110119 42.863635,-77.061367 42.866025,-77.050896 42.836451,-77.100849 42.836199,-77.100849 42.836199))"
            };

            return JsonConvert.SerializeObject(new { UpdateGeofenceEvent = UpdateProjectGeofenceEvt });
        }
        public string DeleteProjectGeofence(Guid geofenceUid, Guid userUid)
        {
            DeleteProjectGeofenceEvt = new DeleteGeofenceEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                GeofenceUID = geofenceUid,
                UserUID = userUid
            };

            return JsonConvert.SerializeObject(new { DeleteGeofenceEvent = DeleteProjectGeofenceEvt });
        }
    }
}
