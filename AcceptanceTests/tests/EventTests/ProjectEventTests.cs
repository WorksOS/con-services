using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace EventTests
{
  [TestClass]
  public class ProjectEventTests
  {

    [TestMethod]
    public void CreateProjectEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      string startDate = testSupport.ConvertVSSDateString("-1d+01:00:00").ToString();
      string endDate = testSupport.ConvertVSSDateString("2d+12:00:00").ToString();
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID | ProjectUID     | ProjectName   | ProjectType       | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject1  | ProjectMonitoring | New Zealand Standard Time | {startDate}     | {endDate}   |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID", 
        "Name, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject1, , {ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

  }
}
