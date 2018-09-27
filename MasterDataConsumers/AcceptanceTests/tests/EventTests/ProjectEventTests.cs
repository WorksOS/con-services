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
    const string GeometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";


    [TestMethod]
    public void CreateProjectEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("-1d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("2d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID   | ProjectUID    | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT    |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | {projectGuid} | testProject1  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT}  |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject1,{projectId},{(int)ProjectType.ProjectMonitoring},{startDate},{endDate}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void CreateInvalidProjectEvent_StartAfterEnd()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("-2d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
             "| EventType          | EventDate   | ProjectID  | ProjectUID      | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
            $"| CreateProjectEvent | 0d+09:00:00 | {projectId}| { projectGuid } | testProject2  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
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
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("900d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID   | ProjectUID      | ProjectName   | ProjectType                     | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | testProject3  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | testProject4  | {ProjectType.ProjectMonitoring} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject3, {projectId}, {(int)ProjectType.ProjectMonitoring}, {startDate}, {endDate}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void CreateStandardProjectWithProjectType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        "| EventType          | EventDate   | ProjectID   | ProjectUID      | ProjectName     | ProjectType            | ProjectTimezone            | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
       $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | testProject5    | {ProjectType.Standard} | New Zealand Standard Time  | {startDate}      | {endDate}      | {GeometryWKT} |"  };

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"testProject5,{projectId}, {(int)ProjectType.Standard}, {startDate}, {endDate}", //Expected
        projectGuid);
    }



    [TestMethod]
    public void UpdateProject_Change_ProjectType()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject8";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID   | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |",
        $"| UpdateProjectEvent | 0d+09:01:00 | {projectId} | {projectGuid} | {projectName} | {ProjectType.Standard} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName},{projectId}, {(int)ProjectType.Standard}, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void UpdateProject_Change_ProjectEndDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject10";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("42d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID   | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate          | GeometryWKT   |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}               | {GeometryWKT} |",
        $"| UpdateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate.AddYears(10)}  | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, {projectId}, {(int)ProjectType.LandFill}, {startDate}, {endDate.AddYears(10)}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void UpdateProject_Change_ProjectEndDateBeforeStartDate()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      string projectName = "testProject11";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("42d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 10", "Create one project");
      var eventArray = new[] {
        " | EventType          | EventDate   | ProjectID   | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate           | GeometryWKT   |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}                | {GeometryWKT} |",
        $"| UpdateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {startDate.AddDays(-1)}  | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, {projectId}, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
    }



    [TestMethod]
    public void UpdateProject_Change_ProjectName()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      string projectName = $"Test Project 12";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 1", "Create one project");
      var eventArray = new[] {
        " | EventType          | EventDate   | ProjectID   | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | {projectGuid} | testProject11 | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |",
        $"| UpdateProjectEvent | 0d+09:00:00 | {projectId} | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, StartDate, EndDate", //Fields
        $"{projectName}, {projectId}, {(int)ProjectType.LandFill}, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Create_Then_Delete_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      string projectName = $"Test Project 13";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("200d+00:00:00",testSupport.FirstEventDate);
      msg.Title("Create Project test 13", "Create one project, then delete it");
      var eventArray = new[] {
         "| EventType          | EventDate   | ProjectID   | ProjectUID      | ProjectName    | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
        $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}       | {GeometryWKT} |",
        $"| DeleteProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName}  | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}     | {endDate}       | {GeometryWKT} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("Project", "ProjectUID",
        "Name, LegacyProjectID, fk_ProjectTypeID, IsDeleted, StartDate, EndDate", //Fields
        $"{projectName}, {projectId}, {(int)ProjectType.LandFill}, 1, {startDate}, {endDate}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Associate_Customer_With_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var projectGuid = Guid.NewGuid();
      var customerGuid = Guid.NewGuid();
      string projectName = $"Test Project 14";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",testSupport.FirstEventDate);
      var customerEventArray = new[] {
             "| EventType           | EventDate   | CustomerName | CustomerType | CustomerUID   |",
            $"| CreateCustomerEvent | 0d+09:00:00 | CustName     | Customer     | {customerGuid} |"};

      testSupport.PublishEventCollection(customerEventArray); //Create customer to associate project with

      msg.Title("Create Project test 14", "Create one project");
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectID   | ProjectUID      | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
       $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | { projectGuid } | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} |"};

      testSupport.PublishEventCollection(projectEventArray);
      mysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);


      var associateEventArray = new[] {
        "| EventType                | EventDate   | ProjectUID    | CustomerUID    | ",
       $"| AssociateProjectCustomer | 0d+09:00:00 | {projectGuid} | {customerGuid} | "};


      testSupport.PublishEventCollection(associateEventArray);
      //Verify project has been associated
      mysql.VerifyTestResultDatabaseFieldsAreExpected("CustomerProject", "fk_ProjectUID",
        "fk_CustomerUID, fk_ProjectUID", //Fields
        $"{customerGuid}, {projectGuid}", //Expected
        projectGuid);
    }


    [TestMethod]
    public void Associate_Geofence_With_Project()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectMysql = new MySqlHelper();
      var projectId = testSupport.SetLegacyProjectId();
      var customerGuid = Guid.NewGuid();
      var projectGuid = Guid.NewGuid();
      var geofenceGuid = Guid.NewGuid();
      var userGuid = Guid.NewGuid();
      string projectName = $"Test Project 15";
      DateTime startDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00",testSupport.FirstEventDate);
      DateTime endDate = testSupport.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00",testSupport.FirstEventDate);


      var geofenceEventArray = new[] {
         "| EventType           | EventDate   | CustomerUID    | Description | FillColor | GeofenceName | GeofenceType | GeofenceUID    | GeometryWKT | IsTransparent | UserUID    | AreaSqMeters |",
        $"| CreateGeofenceEvent | 0d+09:00:00 | {customerGuid} | Fence       | 1         | SuperFence   | 0            | {geofenceGuid} | 1,2,3,4,5,6 | {false}       | {userGuid} | 123.456      |"};

      testSupport.PublishEventCollection(geofenceEventArray); //Create customer to associate project with
      mysql.VerifyTestResultDatabaseRecordCount("Geofence", "GeofenceUID", 1, geofenceGuid);

      msg.Title("Create Project test 15", "Create one project");
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectID   | ProjectUID    | ProjectName   | ProjectType            | ProjectTimezone           | ProjectStartDate | ProjectEndDate | GeometryWKT   |" ,
       $"| CreateProjectEvent | 0d+09:00:00 | {projectId} | {projectGuid} | {projectName} | {ProjectType.LandFill} | New Zealand Standard Time | {startDate}      | {endDate}      | {GeometryWKT} | "};

      testSupport.PublishEventCollection(projectEventArray);
      projectMysql.VerifyTestResultDatabaseRecordCount("Project", "ProjectUID", 1, projectGuid);

      var associateEventArray = new[] {
        "| EventType                | EventDate   | ProjectUID    | GeofenceUID    | ",
       $"| AssociateProjectGeofence | 0d+09:00:00 | {projectGuid} | {geofenceGuid} | "};
      
      testSupport.PublishEventCollection(associateEventArray);

      projectMysql.VerifyTestResultDatabaseRecordCount("ProjectGeofence", "fk_GeofenceUID", 1, geofenceGuid);
      projectMysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectGeofence", "fk_GeofenceUID",
        "fk_GeofenceUID, fk_ProjectUID", //Fields
        $"{geofenceGuid}, {projectGuid}", //Expected
        geofenceGuid);
    }

    [TestMethod]
    public void CreateProjectSettingsEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var userId = Guid.NewGuid().ToString();
      string settings = @"<ProjectSettings>  
        < CompactionSettings >
        < OverrideTargetCMV > false </ OverrideTargetCMV >
        </ CompactionSettings >
        < VolumeSettings >       
        < ExpiryPromptDismissed > false </ ExpiryPromptDismissed >
        </ ProjectSettings > ";

      msg.Title("Create Project Settings test 1", "Create one projectSettings");
      var eventArray = new[] {
        "| EventType                  | EventDate    | ProjectUID    | ProjectSettingsType | Settings   | UserID   |" ,
        $"| UpdateProjectSettingsEvent | 0d+09:00:00 | {projectGuid} | 1                   | {settings} | {userId} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("ProjectSettings", "fk_ProjectUID", 1, projectGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ProjectSettings", "fk_ProjectUID",
        "Settings, fk_ProjectSettingsTypeID, UserID", //Fields
        $"{settings}, 1, {userId}", //Expected
        projectGuid);
    }

    [TestMethod]
    public void CreateImportedFileEvent()
    {
      var msg = new Msg();
      var testSupport = new TestSupport();
      var mysql = new MySqlHelper();
      var projectGuid = Guid.NewGuid();
      var importedFileGuid = Guid.NewGuid();
      var importedFileId = new Random().Next(1, 1999999);
      var customerGuid = Guid.NewGuid();
      var importedFileType = ImportedFileType.SurveyedSurface;
      var name = "Test SS type.ttm";
      var fileDescriptor = "fd";
      var fileCreatedUtc = new DateTime(2017, 1, 1, 2, 30, 3);
      var fileUpdatedUtc = fileCreatedUtc;
      var importedBy = "JoeSmoe";
      var surveyedUtc = new DateTime(2016, 12, 15, 2, 30, 3);

      msg.Title("Create Imported File test 1", "Create one Imported File");
      var eventArray = new[] {
        "| EventType                | EventDate   | ProjectUID    | ImportedFileUID    | ImportedFileID   | CustomerUID    | ImportedFileType   | Name   | FileDescriptor   | FileCreatedUTC   | FileUpdatedUTC   | ImportedBy   | SurveyedUTC   |" ,
        $"| CreateImportedFileEvent | 0d+09:00:00 | {projectGuid} | {importedFileGuid} | {importedFileId} | {customerGuid} | {importedFileType} | {name} | {fileDescriptor} | {fileCreatedUtc} | {fileUpdatedUtc} | {importedBy} | {surveyedUtc} |"};

      testSupport.PublishEventCollection(eventArray);
      mysql.VerifyTestResultDatabaseRecordCount("ImportedFile", "ImportedFileUID", 1, importedFileGuid);
      mysql.VerifyTestResultDatabaseFieldsAreExpected("ImportedFile", "ImportedFileUID",
        "fk_ProjectUID, ImportedFileID, fk_CustomerUID,  fk_ImportedFileTypeID, Name, FileDescriptor, fileCreatedUtc, fileUpdatedUtc, importedBy,  surveyedUTC", //Fields
        $"{projectGuid}, {importedFileId}, {customerGuid}, {(int)ImportedFileType.SurveyedSurface}, {name}, {fileDescriptor}, {fileCreatedUtc}, {fileUpdatedUtc}, {importedBy}, {surveyedUtc}", //Expected
        importedFileGuid);
    }
  }
}
