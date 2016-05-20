using System.Net.Http;
using System.Net.Http.Headers;
using LandfillService.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
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
                Config.JwtToken = Jwt.GetJwtToken(Config.MasterDataUserUID);
            }
            else
            {
                Config.JwtToken = Jwt.GetJwtToken(Config.LandfillUserUID);
            }
        }
    }
}
