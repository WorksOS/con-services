using System.Net.Http;
using System.Net.Http.Headers;
using LandfillService.AcceptanceTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using LandfillService.AcceptanceTests;
using LandfillService.AcceptanceTests.Utils;

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
                LandfillCommonUtils.UpdateAppSetting("StagingTPaaSTokenUsername", Config.MasterDataUserName);
                LandfillCommonUtils.UpdateAppSetting("StagingTPaaSTokenPassWord", Config.MasterDataUserPassword);
            }
            else
            {
                LandfillCommonUtils.UpdateAppSetting("StagingTPaaSTokenUsername", Config.LandfillUserName);
                LandfillCommonUtils.UpdateAppSetting("StagingTPaaSTokenPassWord", Config.LandfillUserPassword);
            }
        }
    }
}
