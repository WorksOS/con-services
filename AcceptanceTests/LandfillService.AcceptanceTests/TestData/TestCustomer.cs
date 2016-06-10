using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Models;
using LandfillService.AcceptanceTests.Utils;
using LandfillService.AcceptanceTests.Auth;
using AutomationCore.API.Framework.Library;
using LandfillService.AcceptanceTests.Models.Landfill;

namespace LandfillService.AcceptanceTests.TestData
{
    public class TestCustomer
    {
        public Guid CustomerUid;
        public string CustomerName;
        public List<Guid> Users;
        public Dictionary<Guid, TestSubscription> Subscriptions;
        public Dictionary<Guid, TestProject> Projects;

        public TestCustomer(Guid uid = default(Guid), string name = null)
        {
            CustomerUid = uid == default(Guid) ? Guid.NewGuid() : uid;
            CustomerName = name == null ? "AT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss") : name;
            Users = new List<Guid>();
            Subscriptions = new Dictionary<Guid, TestSubscription>();
            Projects = new Dictionary<Guid,TestProject>();

            CreateCustomerEvent CreateCustomerEvt = new CreateCustomerEvent
            {
                CustomerUID = CustomerUid,
                CustomerName = CustomerName,
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { CreateCustomerEvent = CreateCustomerEvt }));
        }

        public void AddUser(Guid userUid)
        {
            Users.Add(userUid);
            AssociateCustomerUserEvent AssociateCustomerUserEvt = new AssociateCustomerUserEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = CustomerUid,
                UserUID = userUid
            };
            Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = AssociateCustomerUserEvt }));
        }

        public void UpdateCustomer(string name)
        {
            CustomerName = name;
            UpdateCustomerEvent UpdateCustomerEvt = new UpdateCustomerEvent
            {
                CustomerUID = CustomerUid,
                CustomerName = name,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { UpdateCustomerEvent = UpdateCustomerEvt }));
        }
        public void DeleteCustomer()
        {
            DeleteCustomerEvent DeleteCustomerEvt = new DeleteCustomerEvent
            {
                CustomerUID = CustomerUid,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { DeleteCustomerEvent = DeleteCustomerEvt }));
        }

        public void AddSubscription(TestSubscription sub)
        {
            Subscriptions.Add(sub.SubscriptionUID, sub);
            CreateProjectSubscriptionEvent CreateProjectSubscriptionEvt = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = CustomerUid,
                SubscriptionType = "Landfill",
                SubscriptionUID = sub.SubscriptionUID,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate
            };
            Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, 
                JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = CreateProjectSubscriptionEvt }));
        }
        public void AssociateSubscriptionWithProject(Guid subUid, Guid projUid)
        {
            Subscriptions[subUid].ProjectUid = projUid;
            Projects[projUid].SubscriptionUid = subUid;

            AssociateProjectSubscriptionEvent AssociateProjectSubscriptionEvt = new AssociateProjectSubscriptionEvent
            {
                SubscriptionUID = subUid,
                ProjectUID = projUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,               
            };
            Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, 
                JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = AssociateProjectSubscriptionEvt }));
        }
        public void UpdateSubscription(Guid subUid, DateTime start, DateTime end)
        {
            Subscriptions[subUid].StartDate = start;
            Subscriptions[subUid].EndDate = end;
            UpdateProjectSubscriptionEvent UpdateProjectSubscriptionEvt = new UpdateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = CustomerUid,
                SubscriptionType = "Landfill",
                SubscriptionUID = subUid,
                StartDate = start,
                EndDate = end
            };
            Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, 
                JsonConvert.SerializeObject(new { UpdateProjectSubscriptionEvent = UpdateProjectSubscriptionEvt }));
        }

        public void AddProject(TestProject proj)
        {
            Projects.Add(proj.ProjectUID, proj);

            int projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            CreateProjectEvent CreateProjectEvt = new CreateProjectEvent
            {
                ProjectUID = proj.ProjectUID,
                ProjectID = projectId == 1 ? LandfillCommonUtils.Random.Next(2000, 3000) : projectId,
                ProjectName = proj.ProjectName,
                ProjectStartDate = proj.ProjectStartDate,
                ProjectEndDate = proj.ProjectEndDate,
                ProjectType = ProjectType.LandFill,
                ProjectTimezone = "New Zealand Standard Time",
                ProjectBoundary = " ",
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { CreateProjectEvent = CreateProjectEvt }));
        }
        public void UpdateProject(Guid projUid, string projName)
        {
            Projects[projUid].ProjectName = projName;
            UpdateProjectEvent UpdateProjectEvt = new UpdateProjectEvent
            {
                ProjectUID = projUid,
                ProjectName = projName,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { UpdateProjectEvent = UpdateProjectEvt }));
        }
        public void DeleteProject(Guid projUid)
        {
            Projects.Remove(projUid);
            DeleteProjectEvent DeleteProjectEvt = new DeleteProjectEvent
            {
                ProjectUID = projUid,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
            };
            Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { DeleteProjectEvent = DeleteProjectEvt }));
        }

        public void AddGeofence(TestGeofence geofence, Guid userUid = default(Guid))
        {
            if(geofence.GeofenceType == GeofenceType.Project)
            {
                bool foundProjectWithSameName = false;
                foreach(Guid projUid in Projects.Keys)
                {
                    if (Projects[projUid].ProjectName == geofence.GeofenceName)
                    {
                        // TODO: what if the project already has a project geofence?
                        Projects[projUid].Geofences.Add(geofence.GeofenceUID, geofence);
                        foundProjectWithSameName = true;
                        break;
                    }
                }
                if(!foundProjectWithSameName)
                {
                    return;
                }
            }
            if(geofence.GeofenceType == GeofenceType.Landfill)
            {
                // TODO: add landfill geofence to project if if falls within project boundary, otherwise return immediately
            }
            CreateGeofenceEvent CreateProjectGeofenceEvt = new CreateGeofenceEvent
            {
                CustomerUID = CustomerUid,
                UserUID = userUid == default(Guid) ? Users[0] : userUid,
                GeofenceUID = geofence.GeofenceUID,
                GeofenceName = geofence.GeofenceName,
                Description = geofence.GeofenceName,
                GeofenceType = ((Enum)geofence.GeofenceType).ToString(),
                GeometryWKT = geofence.GeometryWKT,
                FillColor = 0x00FF00,
                IsTransparent = true,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
            };
            Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { CreateGeofenceEvent = CreateProjectGeofenceEvt }));

        }
        public void UpdateGeofence(Guid geoUid, string geoName, Guid userUid = default(Guid))
        {
            foreach (Guid projUid in Projects.Keys)
            {
                if(Projects[projUid].Geofences.ContainsKey(geoUid))
                {
                    Projects[projUid].Geofences[geoUid].GeofenceName = geoName;

                    UpdateGeofenceEvent UpdateGeofenceEvt = new UpdateGeofenceEvent
                    {
                        GeofenceUID = geoUid,
                        UserUID = userUid == default(Guid) ? Users[0] : userUid,
                        GeofenceName = geoName,
                        Description = geoName,
                        FillColor = 0x00FF00,
                        IsTransparent = true,
                        GeofenceType = ((Enum)Projects[projUid].Geofences[geoUid].GeofenceType).ToString(),
                        GeometryWKT = Projects[projUid].Geofences[geoUid].GeometryWKT,
                        ActionUTC = DateTime.UtcNow,
                        ReceivedUTC = DateTime.UtcNow,
                    };
                    Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { UpdateGeofenceEvent = UpdateGeofenceEvt }));
                    break;
                }
            }
        }
        public void DeleteGeofence(Guid geoUid, Guid userUid = default(Guid))
        {
            foreach (Guid projUid in Projects.Keys)
            {
                if (Projects[projUid].Geofences.ContainsKey(geoUid))
                {
                    Projects[projUid].Geofences.Remove(geoUid);

                    DeleteGeofenceEvent DeleteProjectGeofenceEvt = new DeleteGeofenceEvent
                    {
                        GeofenceUID = geoUid,
                        UserUID = userUid == default(Guid) ? Users[0] : userUid,
                        ActionUTC = DateTime.UtcNow,
                        ReceivedUTC = DateTime.UtcNow
                    };
                    Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { DeleteGeofenceEvent = DeleteProjectGeofenceEvt }));
                    break;
                }
            }
        }
    }

    public class TestProject
    {
        public Guid ProjectUID;
        public string ProjectName;
        public DateTime ProjectStartDate;
        public DateTime ProjectEndDate;
        public Guid? SubscriptionUid;
        public Dictionary<Guid, TestGeofence> Geofences;
        
        public TestProject(Guid uid = default(Guid), string name = null, DateTime start = default(DateTime), DateTime end = default(DateTime))
        {
            ProjectUID = uid == default(Guid) ? Guid.NewGuid() : uid;
            ProjectName = name == null ? "AT_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss") : name;
            ProjectStartDate = start == default(DateTime) ? DateTime.UtcNow.AddMonths(-3) : start;
            ProjectEndDate = end == default(DateTime) ? DateTime.UtcNow.AddMonths(10) : end;
            Geofences = new Dictionary<Guid, TestGeofence>();
        }
    }
    public class TestSubscription
    {
        public Guid SubscriptionUID;
        public DateTime StartDate;
        public DateTime EndDate;
        public Guid? ProjectUid;

        public TestSubscription(Guid uid = default(Guid), DateTime start = default(DateTime), DateTime end = default(DateTime))
        {
            SubscriptionUID = uid == default(Guid) ? Guid.NewGuid() : uid;
            StartDate = start == default(DateTime) ? DateTime.UtcNow.AddMonths(-1) : start;
            EndDate = end == default(DateTime) ? DateTime.UtcNow.AddMonths(11) : end; 
        }
    }
    public class TestGeofence
    {
        public static string MiddletonProjectGeofenceBoundary = "POLYGON((-43.541648 172.582726,-43.541586 172.594485,-43.547870 172.594185,-43.548305 172.583370))";
        public static string AddingtonProjectGeofenceBoundary = "POLYGON((-43.541213 172.598863,-43.541306 172.607961,-43.546905 172.607789,-43.546532 172.598348))";
        public static string MarylandsLandfillGeofenceBoundary = "POLYGON((-43.544106 172.587190,-43.544199 172.591009,-43.545817 172.590880,-43.545723 172.587447))";
        public static string AmiStadiumLandfillGeofenceBoundary = "POLYGON((-43.543266 172.602725,-43.542986 172.604699,-43.544697 172.605300,-43.544915 172.603025))";

        public Guid GeofenceUID;
        public string GeofenceName;
        public GeofenceType GeofenceType;       
        public string GeometryWKT;

        public TestGeofence(Guid uid = default(Guid), string name = null, GeofenceType type = GeofenceType.Landfill, string boundary = null)
        {
            GeofenceUID = uid == default(Guid) ? Guid.NewGuid() : uid;
            GeofenceName = name == null ? "AT_Geo-" + DateTime.Now.ToString("yyyyMMddhhmmss") : name;
            GeofenceType = type;
            GeometryWKT = boundary != null ? boundary :
                "POLYGON((-77.100849 42.836199,-77.110119 42.863635,-77.061367 42.866025,-77.050896 42.836451,-77.100849 42.836199,-77.100849 42.836199))";
        }
    }
}
