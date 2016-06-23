using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using LandfillService.AcceptanceTests.Scenarios.ScenarioSupports;
using LandfillService.AcceptanceTests.LandFillKafka;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using AutomationCore.API.Framework.Library;
using LandfillService.AcceptanceTests.Models.Landfill;
using Newtonsoft.Json;
using LandfillService.AcceptanceTests.Auth;
using LandfillService.AcceptanceTests.Utils;
using LandfillService.AcceptanceTests.TestData;

namespace LandfillService.AcceptanceTests.Scenarios
{
    [Binding]
    public class CCASteps
    {
        MDMTestCustomer customer;
        List<MachineDetails> machines;

        [Given(@"I have a landfill project '(.*)' with landfill sites '(.*)'")]
        public void GivenIHaveALandfillProjectWithLandfillSites(string project, string sites)
        {
            switch(project)
            {
                case "Middleton":
                    customer = MDMTestCustomer.Middleton;
                    break;
                case "Addington":
                    customer = MDMTestCustomer.Addington;
                    break;
                case "Maddington":
                    customer = MDMTestCustomer.Maddington;
                    break;
            }
            
            foreach(string site in sites.Split(','))
            {
                switch(site)
                {
                    case "MarylandsLandfill":
                        customer.AddLandfillSite(Site.MarylandsLandfill);
                        break;
                    case "AmiStadiumLandfill":
                        customer.AddLandfillSite(Site.AmiStadiumLandfill);
                        break;
                }
            }

            customer.Create();
        }
        
        [Given(@"I have the following machines")]
        public void GivenIHaveTheFollowingMachines(Table machineTable)
        {
            machines = GeneralSlave.CreateMachines(machineTable);
        }
        
        [Given(@"I have the following CCA data")]
        public void GivenIHaveTheFollowingCCAData(Table ccaTable)
        {
            GeneralSlave.CreateCcaData(ccaTable, customer, machines);
        }
        
        [When(@"I request CCA ratio for site '(.*)' for the past (.*) days")]
        public void WhenIRequestCCARatioForSiteForThePastDays(string p0, int p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the response contains the following CCA ration data")]
        public void ThenTheResponseContainsTheFollowingCCARationData(Table table)
        {
            ScenarioContext.Current.Pending();
        }
    }
}
