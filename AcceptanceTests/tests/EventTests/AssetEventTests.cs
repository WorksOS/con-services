using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace EventTests
{
  [TestClass]
  public class AssetEventTests
  {
    [TestMethod]
    public void Inject_A_Single_Create_Asset_Event()
    {
      var msg = new Msg();
      var ts = new TestSupport();
      msg.Title("Asset test 1","Create one asset event and check that it has been consume by seeing it in the database");
      ;

      var eventArray = new[] {
        $"| TableName | EventDate   | AssetUID      | Name      | MakeCode | SerialNumber | Model | IconKey | AssetType  | LastActionedUTC |",
        $"| Asset     | 0d+09:00:00 | {ts.AssetUid} | AssetT1   | CAT      | XAT1         | 345D  | 10      | Excavators | 0d+09:00:00     |"
        };

        ts.PublishEventCollection(eventArray);                                           // Inject events into kafka             
        //mysql.VerifyTestResultDatabaseRecordCount("Asset", 1,testSupport.AssetUid);                                         // Verify the the result in the database  
        //mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "Name,SerialNumber,MakeCode,Model,IconKey,AssetType", "YT669,8JG89719,CAT,345DL,10,Unassigned",testSupport.AssetUid);  
    }
  }
}
