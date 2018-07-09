using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestUtility;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace EventTests
{
  [TestClass]
  public class ProjectEventTests
  {
    private const string GEOMETRY_WKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
    private const string GEOMETRY_WKT2 = "'POLYGON((-77.0740531243794 42.8482755151629,-77.0812927509093 42.8470654333548,-77.0881228590397 42.8463941030527,-77.0940464342951 42.8508641955719,-77.0947275746861 42.8576235270907,-77.0905709567355 42.861567039969,-77.0795818211823 42.8641102732199,-77.0697542276039 42.8641987499805,-77.0650585590246 42.8535441075047,-77.0740531243794 42.8482755151629,-77.0740531243794 42.8482755151629))'";

    [TestMethod]
    [Ignore]
    public void CreateProjectEvent()
    {
      var msg = new Msg();
      msg.Title("Project test 1", "Create one project");
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var legacyProjectId = ts.SetLegacyProjectId();
      var startDate = ts.FirstEventDate.Date.ToString("yyyy-MM-dd");
      var endDate = new DateTime(9999, 12, 31).Date.ToString("yyyy-MM-dd");
      var eventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {legacyProjectId} | {projectGuid} | testProject1  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {GEOMETRY_WKT} |" };
      ts.PublishEventCollection(eventArray);
      var startDt = ts.FirstEventDate.Date.ToString("M/d/yy hh:mm:ss tt"); 
      var endDt = new DateTime(9999, 12, 31).Date.ToString("M/d/yy hh:mm:ss tt");
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project","ProjectUID","Name,LegacyProjectID,fk_ProjectTypeID,StartDate,EndDate,GeometryWKT",$"testProject1,{legacyProjectId},{(int)ProjectType.ProjectMonitoring},{startDt},{endDt},{GEOMETRY_WKT}",projectGuid);
    }

    [TestMethod]
    [Ignore]
    public void UpdateProject_Change_ProjectType()
    {
      var msg = new Msg();
      msg.Title("Project test 2", "UpdateProject_Change_ProjectType");
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var legacyProjectId = ts.SetLegacyProjectId();
      string projectName = "testProject2";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate).ToString("MM/dd/yyyy hh:mm:ss tt"); 
      var eDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);
      var endDate = eDate.ToString("MM/dd/yyyy hh:mm:ss tt"); 
      var eventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT    |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {legacyProjectId} | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |{GEOMETRY_WKT}  |" ,
      };

      ts.PublishEventCollection(eventArray);
      var updateEventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT    |" ,
      $"| UpdateProjectEvent | 0d+09:01:00 | {legacyProjectId} | {projectGuid} |               | {ProjectType.Standard} | Atlantic Standard Time    |                  | {endDate}      |{GEOMETRY_WKT2} |"
      };

      ts.PublishEventCollection(updateEventArray);
      var startDt = ts.FirstEventDate.ToString("M/d/yy hh:mm:ss tt");
      var endDt = eDate.ToString("M/d/yy hh:mm:ss tt");
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID","Name,fk_ProjectTypeID,StartDate,EndDate",$"{projectName},{(int)ProjectType.Standard},{startDt},{endDt},{GEOMETRY_WKT2}", projectGuid);
    }



    [TestMethod]
    public void Associate_Customer_With_Project()
    {
      var msg = new Msg();
      msg.Title("Project test 3", "Associate customer with project");
      var ts = new TestSupport {IsPublishToKafka = true};
      var mysql = new MySqlHelper();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Test Project 3";
      var startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate).ToString("M/d/yyyy hh:mm:ss tt");
      var endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",ts.FirstEventDate).ToString("M/d/yyyy hh:mm:ss tt");

      var customerEventArray = new[] {
      "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID   |",
     $"| CreateCustomerEvent | 0d+09:00:00 | CustName     | Customer     | {customerGuid} |"};

      ts.PublishEventCollection(customerEventArray); 
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectID         | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
      $"| CreateProjectEvent | 0d+09:00:00 | {legacyProjectId} | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |{GEOMETRY_WKT} |" };
      ts.PublishEventCollection(projectEventArray);

      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      var associateEventArray = new[] {
       "| EventType                | EventDate   | ProjectUID    | CustomerUID    | ",
      $"| AssociateProjectCustomer | 0d+09:00:00 | {projectGuid} | {customerGuid} | "};
      
      ts.PublishEventCollection(associateEventArray);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID","fk_CustomerUID, fk_ProjectUID",$"{customerGuid}, {projectGuid}",projectGuid);
    }


  }
}
