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
            if (FeatureContext.Current.FeatureInfo.Title.Contains("MasterData"))
            {
                Config.JwtToken = Jwt.GetJwtToken(Config.MasterDataUserUid);
            }
            else
            {
                Config.JwtToken = Jwt.GetJwtToken(Config.LandfillUserUid);
            }
        }
    }
}
