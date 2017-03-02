using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace EventTests
{
  [TestClass]
  public class AssetEventTests
  {
    [TestMethod]
    public void Inject_A_Single_Create_Asset_Event()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 1","Create Asset event ");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName | Make | SerialNumber | Model | IconKey | AssetType  | LastActionedUTC |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetE1   | CAT  | XAT1         | 345D  | 10      | Excavators | 0d+09:00:00     |"};

        ts.PublishEventCollection(eventArray);                                          
        mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));      
        mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset","AssetUID", "Name,SerialNumber,MakeCode,Model,IconKey,AssetType", "YT669,8JG89719,CAT,345DL,10,Unassigned",new Guid(ts.AssetUid));  
    }
  }
}
