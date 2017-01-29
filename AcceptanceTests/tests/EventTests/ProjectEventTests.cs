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
      DateTime startDate = testSupport.ConvertVSSDateString("-1d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("2d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID | ProjectUID     | ProjectName   | ProjectType       | ProjectTimezone            | ProjectStartDate | ProjectEndDate |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject1  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject1, 1, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void CreateInvalidProjectEvent_StartAfterEnd()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("-2d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID | ProjectUID     | ProjectName   | ProjectType       | ProjectTimezone            | ProjectStartDate | ProjectEndDate |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject2  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 0, projectGuid); //no records should be inserted
      //mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
      //  "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
      //  $"testProject1, 1, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
      //  projectGuid);
    }

    [TestMethod]
    public void CreateProjectWithSameGuidAgain()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("900d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType          | EventDate   | ProjectID | ProjectUID     | ProjectName   | ProjectType       | ProjectTimezone            | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject3  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      |",
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject4  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      |"};

    testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject3, 1, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

        [TestMethod]
    public void CreateStandardProjectWithProjectType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType          | EventDate   | ProjectID | ProjectUID     | ProjectName     | ProjectType       | ProjectTimezone            | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject5  | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      |"  };

    testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject5, 1, {(int)ProjectType.Standard}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

    /// <summary>
    /// This test calls update with a new Porject Guid, as the new one does not exist it should be
    /// created.
    /// </summary>
    [TestMethod]
    public void Update_project_Guid() 
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var newGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType          | EventDate    | ProjectID | ProjectUID       | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
       $"| CreateProjectEvent | 1d+09:00:00  | 1         | { projectGuid }  | testProject6  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |" ,
       $"| UpdateProjectEvent | 0d+09:00:00  | 1         | { newGuid }      | testProject7  | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject6, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, newGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject7, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        newGuid);
    }


    [TestMethod]
    public void UpdateProject_Change_ProjectType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject8";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType           | EventDate   | ProjectID | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |",
        $"| UpdateProjectEvent | 0d+09:01:00 | 1         | {projectGuid} | {projectName} | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.Standard}, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void UpdateProject_Change_ProjectEndDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject10";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("42d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType           | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |",
        $"| UpdateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate.AddYears(10)}  |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate.AddYears(10)}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void UpdateProject_Change_ProjectEndDateBeforeStartDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject11";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("42d+00:00:00");
      msg.Title("Create Project test 10", "Create one project");
      var eventArray = new[] {
        "| EventType           | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      |",
        $"| UpdateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {startDate.AddDays(-1)}  |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
    }



    [TestMethod]
    public void UpdateProject_Change_ProjectName()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      string projectName = $"Test Project 12";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType           | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | testProject11  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}      |",
        $"| UpdateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}      |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Create_Then_Delete_Project ()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      string projectName = $"Test Project 13";
      DateTime startDate = testSupport.ConvertVSSDateString("0d+00:00:00");
      DateTime endDate = testSupport.ConvertVSSDateString("10000d+00:00:00");
      msg.Title("Create Project test 13", "Create one project, then delete it");
      var eventArray = new[] {
        "| EventType           | EventDate   | ProjectID | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}      |",
        $"| DeleteProjectEvent | 0d+09:00:00 | 1         | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}      |"};

      testSupport.InjectEventsIntoKafka(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, IsDeleted, StartDate, EndDate", //Fields
        $"{projectName}, 1, {(int)ProjectType.LandFill}, 1, {startDate}, {endDate}", //Expected
        projectGuid);
    }

  }
}
