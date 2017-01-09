using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
    [TestClass]
    public class ProjectTests
    {
        //private const string PRODFAM = "&productFamily=";


        //[TestMethod]
        //public void Asset_Count_More_Than_10_Assets_LoadOnlyConfig()
        //{
        //    var msg = new Msg(); 
        //    msg.Title("Asset count 1","Asset count more than 10 assets. Load only asset config");

        //    var assetType = Guid.NewGuid().ToString();  // Random asset type
            
        //    var eventArray = new[] {                                                     
        //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
        //    "| CreateAssetEvent | 0         | 03:00:00  | ASSETCNT1 | ASSET1       | CAT  | D1E   | 21     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:01:00  | ASSETCNT1 | ASSET2       | CAT  | D2E   | 22     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:05:00  | ASSETCNT1 | ASSET3       | CAT  | D3E   | 23     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:09:00  | ASSETCNT1 | ASSET4       | CAT  | D4E   | 24     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:15:00  | ASSETCNT1 | ASSET5       | CAT  | D5E   | 25     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:20:00  | ASSETCNT1 | ASSET6       | CAT  | D6E   | 26     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:30:00  | ASSETCNT1 | ASSET7       | CAT  | D7E   | 27     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:35:00  | ASSETCNT1 | ASSET8       | CAT  | D8E   | 28     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:40:00  | ASSETCNT1 | ASSET9       | CAT  | D9E   | 29     | " + assetType + "|",
        //    "| CreateAssetEvent | 0         | 03:45:00  | ASSETCNT1 | ASSET10      | CAT  | E1E   | 30     | " + assetType + "|"
        //    };
        //    var testSupport = new TestSupport();
        //    testSupport.InjectEventsIntoMySqlDatabase(eventArray,false);  // Creates a new asset with different asset id for event event.
        //    var allProductFamilies = PRODFAM + assetType;
        //    testSupport.VerifyAssetCountInWebApi(allProductFamilies, 10);
        //}

        //[TestMethod]
        //public void Asset_Count_For_Three_Product_families_LoadOnlyConfig()
        //{
        //    var msg = new Msg(); 
        //    msg.Title("Asset count 2","Asset count for five product families.");

        //    var assetType1 = Guid.NewGuid().ToString();              
        //    var eventArray = new[] {                                                     
        //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
        //    "| CreateAssetEvent | 0         | 03:00:00  | ASSETCNT2 | ASSET1       | CAT  | D1E   | 21     | " + assetType1 + "|",
        //    "| CreateAssetEvent | 0         | 03:01:00  | ASSETCNT2 | ASSET2       | CAT  | D2E   | 22     | " + assetType1 + "|"
        //    };
        //    var testSupport = new TestSupport();
        //    testSupport.InjectEventsIntoMySqlDatabase(eventArray,false);  

        //    var assetType2 = Guid.NewGuid().ToString();      
        //    eventArray = new[] {                                                     
        //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
        //    "| CreateAssetEvent | 0         | 03:00:00  | ASSETCNT2 | ASSET3       | CAT  | D1E   | 21     | " + assetType2 + "|",
        //    "| CreateAssetEvent | 0         | 03:01:00  | ASSETCNT2 | ASSET4       | CAT  | D2E   | 22     | " + assetType2 + "|"
        //    };
        //    testSupport.InjectEventsIntoMySqlDatabase(eventArray,false);  

        //    var assetType3 = Guid.NewGuid().ToString();      
        //    eventArray = new[] {                                                     
        //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
        //    "| CreateAssetEvent | 0         | 03:00:00  | ASSETCNT2 | ASSET5       | CAT  | D1E   | 21     | " + assetType3 + "|",
        //    "| CreateAssetEvent | 0         | 03:01:00  | ASSETCNT2 | ASSET6       | CAT  | D2E   | 22     | " + assetType3 + "|"
        //    };
        //    testSupport.InjectEventsIntoMySqlDatabase(eventArray,false);  

        //    var assetType4 = Guid.NewGuid().ToString();      
        //    eventArray = new[] {                                                     
        //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
        //    "| CreateAssetEvent | 0         | 03:00:00  | ASSETCNT2 | ASSET7       | CAT  | D1E   | 21     | " + assetType4 + "|",
        //    "| CreateAssetEvent | 0         | 03:01:00  | ASSETCNT2 | ASSET8       | CAT  | D2E   | 22     | " + assetType4 + "|"
        //    };
        //    testSupport.InjectEventsIntoMySqlDatabase(eventArray,false); 

        //    var assetType5 = Guid.NewGuid().ToString();      
        //    eventArray = new[] {                                                     
        //    "| EventType        | DayOffset | Timestamp | AssetName | SerialNumber | Make | Model | IconId | AssetType  |",
        //    "| CreateAssetEvent | 0         | 03:00:00  | ASSETCNT2 | ASSET9       | CAT  | D1E   | 21     | " + assetType5 + "|",
        //    "| CreateAssetEvent | 0         | 03:01:00  | ASSETCNT2 | ASSET10      | CAT  | D2E   | 22     | " + assetType5 + "|"
        //    };
        //    testSupport.InjectEventsIntoMySqlDatabase(eventArray,false);

        //    var allProductFamilies = PRODFAM + assetType1 + 
        //                             PRODFAM + assetType2 + 
        //                             PRODFAM + assetType3 + 
        //                             PRODFAM + assetType4 + 
        //                             PRODFAM + assetType5;

        //    testSupport.VerifyAssetCountInWebApi(allProductFamilies, 10);
        //}
    }
}