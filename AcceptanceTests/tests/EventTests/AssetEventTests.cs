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
    public void InjectASingleCreateAssetEvent()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 1","Inject A Single Create Asset Event");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  | ",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent1 | CAT  | XAT1         | 374D  | 10      | Excavators | "};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent1,CAT,XAT1,374D,10,Excavators", new Guid(ts.AssetUid));
    }

    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventMake()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent2 | CAT  | XAT1         | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent2,CAT,XAT1,374D,10,Excavators", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | AssetEvent2 | DEE  | XAT1         | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent2,DEE,XAT1,374D,10,Excavators", new Guid(ts.AssetUid));
    }

    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventSerialNumber()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent2 | CAT  | XAT1         | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent2,CAT,XAT1,374D,10,Excavators", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | SerialNumber | ",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | AssetEvent2 | UpdateSerial | "};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent2,CAT,UpdateSerial,374D,10,Excavators", new Guid(ts.AssetUid));
    }

  }
}
