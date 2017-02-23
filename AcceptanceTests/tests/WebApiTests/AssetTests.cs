using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class AssetTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    public void InjectAnAssetIntoDatabaseAndRetrieveAssetIdFromWebapi()
    {
      msg.Title("Asset Test 1", "Inject an asset into database and retrieve AssetId from web api");
      var ts = new TestSupport {IsPublishToKafka = false};
      var legacyAssetId = ts.SetLegacyAssetId();
      var eventArray = new[] {
       "| TableName | EventDate   | AssetUID      | LegacyAssetID   | Name      | MakeCode | SerialNumber | Model | IconKey | AssetType  | ",
      $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | {legacyAssetId} | AssetT1   | CAT      | XAT1         | 345D  | 10      | Excavators | "};
      ts.PublishEventCollection(eventArray);
      
      // Call the web api and verify the result


    }
  }
}
