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
    public class MDMTestCustomer
    {
        public Guid UserUid;
        public Guid CustomerUid;
        public Guid ProjectUid;
        public string ProjectName;
        public Guid SubscriptionUid;
        public Guid ProjectGeofenceUid;
        public string ProjectGeofenceBoundary;
        public List<Site> LandfillGeofences;

        private bool IsCreated;
        
        public static MDMTestCustomer Middleton
        {
            get
            {
                return new MDMTestCustomer()
                {
                    UserUid = Guid.NewGuid(),
                    CustomerUid = Guid.NewGuid(),
                    ProjectUid = Guid.NewGuid(),
                    ProjectName = "Middleton-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    SubscriptionUid = Guid.NewGuid(),
                    ProjectGeofenceUid = Guid.NewGuid(),
                    ProjectGeofenceBoundary = "POLYGON((-43.541648 172.582726,-43.541586 172.594485,-43.547870 172.594185,-43.548305 172.583370))",
                    LandfillGeofences = new List<Site>(),
                    IsCreated = false
                };
            }
        }
        public static MDMTestCustomer Addington
        {
            get
            {
                return new MDMTestCustomer()
                {
                    UserUid = Guid.NewGuid(),
                    CustomerUid = Guid.NewGuid(),
                    ProjectUid = Guid.NewGuid(),
                    ProjectName = "Addington-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    SubscriptionUid = Guid.NewGuid(),
                    ProjectGeofenceUid = Guid.NewGuid(),
                    ProjectGeofenceBoundary = "POLYGON((-43.541213 172.598863,-43.541306 172.607961,-43.546905 172.607789,-43.546532 172.598348))",
                    LandfillGeofences = new List<Site>(),
                    IsCreated = false
                };
            }
        }
        public static MDMTestCustomer Maddington
        {
            get
            {
                return new MDMTestCustomer()
                {
                    UserUid = Guid.NewGuid(),
                    CustomerUid = Guid.NewGuid(),
                    ProjectUid = Guid.NewGuid(),
                    ProjectName = "Maddington-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    SubscriptionUid = Guid.NewGuid(),
                    ProjectGeofenceUid = Guid.NewGuid(),
                    ProjectGeofenceBoundary = "POLYGON((-43.539849 172.582082,-43.539663 172.608604,-43.549866 172.608346,-43.549990 172.582511))",
                    LandfillGeofences = new List<Site>(),
                    IsCreated = false
                };
            }
        }

        public void Create()
        {
            // create customer
            CreateCustomerEvent CreateCustomerEvt = new CreateCustomerEvent
            {
                CustomerUID = CustomerUid,
                CustomerName = "AT_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                CustomerType = CustomerType.Corporate,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { CreateCustomerEvent = CreateCustomerEvt }));

            // associate customer user
            AssociateCustomerUserEvent AssociateCustomerUserEvt = new AssociateCustomerUserEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = CustomerUid,
                UserUID = UserUid
            };
            Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = AssociateCustomerUserEvt }));

            // create project
            var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
            CreateProjectEvent CreateProjectEvt = new CreateProjectEvent
            {
                ActionUTC = DateTime.UtcNow,
                ProjectBoundary = " ",
                ProjectEndDate = DateTime.Today.AddMonths(10),
                ProjectStartDate = DateTime.Today.AddMonths(-3),
                ProjectName = ProjectName,
                ProjectTimezone = "New Zealand Standard Time",
                ProjectType = ProjectType.LandFill,
                ProjectID = projectId == 1 ? LandfillCommonUtils.Random.Next(2000, 3000) : projectId,
                ProjectUID = ProjectUid,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { CreateProjectEvent = CreateProjectEvt }));

            // create subscription
            CreateProjectSubscriptionEvent CreateProjectSubscriptionEvt = new CreateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                CustomerUID = CustomerUid,
                SubscriptionType = "Landfill",
                SubscriptionUID = SubscriptionUid,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(11)
            };
            Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = CreateProjectSubscriptionEvt }));

            // associate project customer
            AssociateProjectCustomer AssociateProjectCustomerEvt = new AssociateProjectCustomer
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                ProjectUID = ProjectUid,
                CustomerUID = CustomerUid
            };
            Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { AssociateProjectCustomer = AssociateProjectCustomerEvt }));

            // associate project subscription
            AssociateProjectSubscriptionEvent AssociateProjectSubscriptionEvt = new AssociateProjectSubscriptionEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                SubscriptionUID = SubscriptionUid,
                EffectiveDate = DateTime.Now.AddMonths(-1),
                ProjectUID = ProjectUid
            };
            Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = AssociateProjectSubscriptionEvt }));

            // create project geofence
            CreateGeofenceEvent CreateProjectGeofenceEvt = new CreateGeofenceEvent
            {
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow,
                GeofenceUID = ProjectGeofenceUid,
                CustomerUID = CustomerUid,
                UserUID = UserUid,
                GeofenceName = ProjectName,
                Description = "Project geofence",
                FillColor = 0x00FF00,
                IsTransparent = true,
                GeofenceType = "Project",
                GeometryWKT = ProjectGeofenceBoundary
            };
            Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { CreateGeofenceEvent = CreateProjectGeofenceEvt }));

            AssociateProjectGeofence AssociateProjectGeofenceEvt = new AssociateProjectGeofence
            {
                ProjectUID = ProjectUid,
                GeofenceUID = ProjectGeofenceUid,
                ActionUTC = DateTime.UtcNow,
                ReceivedUTC = DateTime.UtcNow
            };
            Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { AssociateProjectGeofence = AssociateProjectGeofenceEvt }));

            // create landfill geofences
            foreach(Site landfill in LandfillGeofences)
            {
                CreateGeofenceEvent CreateLandfillGeofenceEvt = new CreateGeofenceEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    GeofenceUID = landfill.uid,
                    CustomerUID = CustomerUid,
                    UserUID = UserUid,
                    GeofenceName = landfill.name,
                    Description = "Landfill geofence",
                    FillColor = 0x00FF00,
                    IsTransparent = true,
                    GeofenceType = "Landfill",
                    GeometryWKT = landfill.boundary
                };
                Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { CreateGeofenceEvent = CreateLandfillGeofenceEvt }));

                AssociateProjectGeofence AssociateProjectLandfillGeofenceEvt = new AssociateProjectGeofence
                {
                    ProjectUID = ProjectUid,
                    GeofenceUID = landfill.uid,
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow
                };
                Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { AssociateProjectGeofence = AssociateProjectLandfillGeofenceEvt }));
            }

            IsCreated = true;
        }
        public void AddLandfillSite(Site site)
        {
            LandfillGeofences.Add(site);
            if (IsCreated)
            {
                CreateGeofenceEvent CreateLandfillGeofenceEvt = new CreateGeofenceEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    GeofenceUID = site.uid,
                    CustomerUID = CustomerUid,
                    UserUID = UserUid,
                    GeofenceName = site.name,
                    Description = "Landfill geofence",
                    FillColor = 0x00FF00,
                    IsTransparent = true,
                    GeofenceType = "Landfill",
                    GeometryWKT = site.boundary
                };
                Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { CreateGeofenceEvent = CreateLandfillGeofenceEvt }));

                AssociateProjectGeofence AssociateProjectLandfillGeofenceEvt = new AssociateProjectGeofence
                {
                    ProjectUID = ProjectUid,
                    GeofenceUID = site.uid,
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow
                };
                Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { AssociateProjectGeofence = AssociateProjectLandfillGeofenceEvt }));
            }
        }
        public bool HasSite(Site site)
        {
            if (IsCreated)
            {
                // get project id by web api request
                string response = RestClientUtil.DoHttpRequest(Config.ConstructGetProjectListUri(), "GET", 
                    RestClientConfig.JsonMediaType, null, Jwt.GetJwtToken(UserUid), HttpStatusCode.OK);
                List<Project> projects = JsonConvert.DeserializeObject<List<Project>>(response);
                uint projectId = projects.First(p => p.name == ProjectName).id;

                response = RestClientUtil.DoHttpRequest(Config.ConstructGetGeofencesUri(projectId), "GET", 
                    RestClientConfig.JsonMediaType, null, Jwt.GetJwtToken(UserUid), HttpStatusCode.OK);
                List<Geofence> geofences = JsonConvert.DeserializeObject<List<Geofence>>(response);

                return geofences.Exists(g => g.uid == site.uid && g.name == site.name && g.type == (int)site.type);
            }
            else
            {
                throw new InvalidOperationException("The customer has not been created - use MDMTestCustomer.Create() to create it first.");
            }
        }
    }

    public class Site
    {
        public Guid uid { get; set; }
        public string name { get; set; }
        public string boundary { get; set; }
        public GeofenceType type { get; set; } 

        public static Site MarylandsLandfill
        {
            get
            {
                return new Site()
                {
                    uid = Guid.NewGuid(),
                    name = "Marylands-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    boundary = "POLYGON((-43.544106 172.587190,-43.544199 172.591009,-43.545817 172.590880,-43.545723 172.587447))",
                    type = GeofenceType.Landfill
                };
            }
        }
        public static Site AmiStadiumLandfill
        {
            get
            {
                return new Site()
                {
                    uid = Guid.NewGuid(),
                    name = "AmiStadium-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    boundary = "POLYGON((-43.543266 172.602725,-43.542986 172.604699,-43.544697 172.605300,-43.544915 172.603025))",
                    type = GeofenceType.Landfill
                };
            }
        }
    }
}
