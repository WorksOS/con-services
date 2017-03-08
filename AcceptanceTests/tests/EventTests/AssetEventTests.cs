using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

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
    public void InjectACreateAssetEventAndUpdateAssetEventAssetName()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 1","Inject A Create Asset Event And Update Asset Event different AssetName");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent1 | CAT  | 11111        | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent1,CAT,11111,374D,10,Excavators", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName       | Model | IconKey | AssetType  |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | UpdateAssetName | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,Model,IconKey,AssetType", "UpdateAssetName,374D,10,Excavators", new Guid(ts.AssetUid));
    }

    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventModel()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event different Model");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent2 | CAT  | 22222        | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent2,CAT,22222,374D,10,Excavators", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Model | IconKey | AssetType  |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | AssetEvent2 | 345   | 10      | Excavators |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,Model,IconKey,AssetType", "AssetEvent2,345,10,Excavators", new Guid(ts.AssetUid));
    }

    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventIcon()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event different Icon");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent3 | CAT  | 22222        | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent3,CAT,22222,374D,10,Excavators", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | IconKey | AssetType  |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | 22      | Excavators |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "IconKey,AssetType", "22,Excavators", new Guid(ts.AssetUid));
    }

    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventAssetType()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event different Icon");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent4 | CAT  | 22222        | 374D  | 10      | Excavators |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType", "AssetEvent4,CAT,22222,374D,10,Excavators", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetType  |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | Excavators |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "AssetType", "Excavators", new Guid(ts.AssetUid));
    }

    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventLegacyAssetId()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event different Icon");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  | LegacyAssetId |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent5 | CAT  | 22222        | 374D  | 10      | Excavators | 1234567       |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType,LegacyAssetId", "AssetEvent5,CAT,22222,374D,10,Excavators,1234567", new Guid(ts.AssetUid));

      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetType  | LegacyAssetId |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | Excavators | 987654321     |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType,LegacyAssetId", "AssetEvent5,CAT,22222,374D,10,Excavators,987654321", new Guid(ts.AssetUid));
    }


    [TestMethod]
    public void InjectACreateAssetEventAndUpdateAssetEventOwningCustomerUid()
    {
      var msg = new Msg();
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var customerUid = Guid.NewGuid();
      msg.Title("Asset event 2","Inject A Create Asset Event And Update Asset Event different Icon");
      var eventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | AssetName   | Make | SerialNumber | Model | IconKey | AssetType  | LegacyAssetId | OwningCustomerUID |",
        $"| CreateAssetEvent | 0d+09:00:00 | {ts.AssetUid} | AssetEvent5 | CAT  | 22222        | 374D  | 10      | Excavators | 1234567       | {customerUid}     |"};
      ts.PublishEventCollection(eventArray);                                          
      mysql.VerifyTestResultDatabaseRecordCount("Asset","AssetUID",1,new Guid(ts.AssetUid));
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType,LegacyAssetId,OwningCustomerUID", $"AssetEvent5,CAT,22222,374D,10,Excavators,1234567,{customerUid}", new Guid(ts.AssetUid));

      customerUid = Guid.NewGuid();
      var updEventArray = new[] {
         "| EventType        | EventDate   | AssetUID      | OwningCustomerUID |",
        $"| UpdateAssetEvent | 1d+09:00:00 | {ts.AssetUid} | {customerUid}     |"};
      ts.PublishEventCollection(updEventArray);  
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Asset", "AssetUID", "Name,MakeCode,SerialNumber,Model,IconKey,AssetType,LegacyAssetId,OwningCustomerUID", $"AssetEvent5,CAT,22222,374D,10,Excavators,987654321,{customerUid}", new Guid(ts.AssetUid));
    }

  }
}
