using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using LandfillService.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests;
using LandfillService.AcceptanceTests.Utils;
using LandfillService.AcceptanceTests.Auth;

namespace LandfillService.AcceptanceTests.Scenarios.ScenarioSupports
{
    [Binding]
    public class BeforeAndAfter
    {
        [BeforeFeature]
        public static void BeforeFeature()
        {
            // create a new customer and new user before master data tests
            if (FeatureContext.Current.FeatureInfo.Title.Contains("MasterData"))
            {
                Config.MasterDataCustomerUID = Guid.NewGuid();
                Config.MasterDataUserUID = Guid.NewGuid();
                Config.JwtToken = Jwt.GetJwtToken(Config.MasterDataUserUID);

                CreateCustomerEvent CreateCustomerEvt = new CreateCustomerEvent
                {
                    CustomerUID = Config.MasterDataCustomerUID,
                    CustomerName = "MDM_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    CustomerType = CustomerType.Corporate,
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow
                };
                AssociateCustomerUserEvent AssociateCustomerUserEvt = new AssociateCustomerUserEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    CustomerUID = Config.MasterDataCustomerUID,
                    UserUID = Config.MasterDataUserUID
                };
                string createCustomer = JsonConvert.SerializeObject(new { CreateCustomerEvent = CreateCustomerEvt });
                string associateUserCustomer = JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = AssociateCustomerUserEvt });

                Config.KafkaDriver.SendMessage(Config.CustomerTopic, createCustomer);
                Config.KafkaDriver.SendMessage(Config.CustomerTopic, associateUserCustomer); 
            }
            else
            {
                Config.JwtToken = Jwt.GetJwtToken(Config.LandfillUserUID);
            }
        }

        [BeforeScenario]
        public static void BeforeScenario()
        {
            List<string> tags = new List<string>(ScenarioContext.Current.ScenarioInfo.Tags);
            if(tags.Contains("require2ndCustomer"))
            {
                // create customer
                CreateCustomerEvent Create2ndCustomerEvt = new CreateCustomerEvent
                {
                    CustomerUID = Config.MasterData2ndUserUID,
                    CustomerName = "2nd_CUS-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    CustomerType = CustomerType.Corporate,
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow
                };
                Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { CreateCustomerEvent = Create2ndCustomerEvt }));

                // associate customer user
                AssociateCustomerUserEvent Associate2ndCustomerUserEvt = new AssociateCustomerUserEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    CustomerUID = Config.MasterData2ndCustomerUID,
                    UserUID = Config.MasterData2ndUserUID
                };
                Config.KafkaDriver.SendMessage(Config.CustomerTopic, JsonConvert.SerializeObject(new { AssociateCustomerUserEvent = Associate2ndCustomerUserEvt }));

                // create project
                var projectId = LandFillMySqlDb.GetTheHighestProjectId() + 1;
                CreateProjectEvent Create2ndProjectEvt = new CreateProjectEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ProjectBoundary = " ",
                    ProjectEndDate = DateTime.Today.AddMonths(10),
                    ProjectStartDate = DateTime.Today.AddMonths(-3),
                    ProjectName = "2nd_PRO-" + DateTime.Now.ToString("yyyyMMddhhmmss"),
                    ProjectTimezone = "New Zealand Standard Time",
                    ProjectType = ProjectType.LandFill,
                    ProjectID = projectId == 1 ? LandfillCommonUtils.Random.Next(2000, 3000) : projectId,
                    ProjectUID = Config.MasterData2ndProjectUID,
                    ReceivedUTC = DateTime.UtcNow
                };
                Config.KafkaDriver.SendMessage(Config.ProjectTopic, JsonConvert.SerializeObject(new { CreateProjectEvent = Create2ndProjectEvt }));

                // create subscription
                CreateProjectSubscriptionEvent Create2ndProjectSubscriptionEvt = new CreateProjectSubscriptionEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    CustomerUID = Config.MasterData2ndCustomerUID,
                    SubscriptionType = "Landfill",
                    SubscriptionUID = Config.MasterData2ndProjectSubscriptionUID,
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    EndDate = DateTime.UtcNow.AddMonths(11)
                };
                Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, 
                    JsonConvert.SerializeObject(new { CreateProjectSubscriptionEvent = Create2ndProjectSubscriptionEvt }));

                // associate project customer
                AssociateProjectCustomer Associate2ndProjectCustomerEvt = new AssociateProjectCustomer
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    ProjectUID = Config.MasterData2ndProjectUID,
                    CustomerUID = Config.MasterData2ndCustomerUID
                };
                Config.KafkaDriver.SendMessage(Config.ProjectTopic, 
                    JsonConvert.SerializeObject(new { AssociateProjectCustomer = Associate2ndProjectCustomerEvt }));

                // associate project subscription
                AssociateProjectSubscriptionEvent Associate2ndProjectSubscriptionEvt = new AssociateProjectSubscriptionEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    SubscriptionUID = Config.MasterData2ndProjectSubscriptionUID,
                    EffectiveDate = DateTime.Now.AddMonths(-1),
                    ProjectUID = Config.MasterData2ndProjectUID
                };
                Config.KafkaDriver.SendMessage(Config.SubscriptionTopic, 
                    JsonConvert.SerializeObject(new { AssociateProjectSubscriptionEvent = Associate2ndProjectSubscriptionEvt }));

                // create project geofence
                CreateGeofenceEvent Create2ndProjectGeofenceEvt = new CreateGeofenceEvent
                {
                    ActionUTC = DateTime.UtcNow,
                    ReceivedUTC = DateTime.UtcNow,
                    GeofenceUID = Config.MasterData2ndProjectGeofenceUID,
                    CustomerUID = Config.MasterData2ndCustomerUID,
                    UserUID = Config.MasterData2ndUserUID,
                    GeofenceName = Create2ndProjectEvt.ProjectName,
                    Description = "Created project geofence",
                    FillColor = 0x00FF00,
                    IsTransparent = true,
                    GeofenceType = "Project",
                    GeometryWKT = Config.MasterData2ndProjectBoundary
                };
                Config.KafkaDriver.SendMessage(Config.GeofenceTopic, JsonConvert.SerializeObject(new { CreateGeofenceEvent = Create2ndProjectGeofenceEvt }));
            }
        }
    }
}
