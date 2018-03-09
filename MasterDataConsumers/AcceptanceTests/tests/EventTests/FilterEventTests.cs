using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace EventTests
{
  [TestClass]
  public class FilterEventTests
  {

    [TestMethod]
    public void CreateFilterEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var filterGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      var userId = Guid.NewGuid().ToString();
      var name = "Test SS type.ttm";
      var filterJson = "{\"startUTC\":\"2012-11-05\",\"endUTC\":\"2012-11-06\"}";
      var filterType = (int) FilterType.Persistent;

      msg.Title("Create Project filter test 1", "Create project filter");
      var eventArray = new[] {
        " | EventType         | EventDate   | ProjectUID    | FilterUID    | CustomerUID    | UserID   | Name   | FilterJson   | FilterType   |" ,
        $"| CreateFilterEvent | 0d+09:00:00 | {projectGuid} | {filterGuid} | {customerGuid} | {userId} | {name} | {filterJson} | {filterType} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Filter", "FilterUID", 1, filterGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Filter", "FilterUID",
        "fk_ProjectUID, FilterUID, fk_CustomerUID,  UserID, Name, FilterJson, fk_FilterTypeID", //Fields
        $"{projectGuid}, {filterGuid}, {customerGuid}, {userId}, {name}, {filterJson}, {filterType}", //Expected
        filterGuid);
    }

  }
}
