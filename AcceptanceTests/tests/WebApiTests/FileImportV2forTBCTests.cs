using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType;

namespace WebApiTests
{
  [TestClass]
  public class FileImportV2forTBCTests
  {
    private readonly Msg msg = new Msg();
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";
    

    [TestMethod]
    public void TestImportV2ForTbcSvlFile_AlignmentType_OK()
    {
      const string testName = "File Import 13";
      msg.Title(testName, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyCustomerId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyCustomerId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var project = ts.GetProjectDetailsViaWebApiV4(customerUid, projectUid);
      Assert.IsNotNull(project, $"unable to retrieve project. customerUid: {customerUid} projectUid: {projectUid} projectname: {testName}");

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name                      | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = importFile.SendImportedFilesToWebApiV2(ts, project.LegacyProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);

      Assert.AreEqual(HttpStatusCode.OK, importFileV2Result.Code, "Not imported ok.");
      Assert.AreNotEqual(-1, importFileV2Result.Id, "LegacyFileID invalid.");

      var importFileList = importFile.GetImportedFilesFromWebApiV2(ts.GetBaseUri() + $"api/v2/projects/{project.LegacyProjectId}/importedfiles", customerUid);
      Assert.IsTrue(importFileList.Count == 1, "Expected 1 imported files but got " + importFileList.Count);
      Assert.AreEqual(importFileV2Result.Id, importFileList[0].id, "Wrong id");
      Assert.AreEqual(TestFile.TestAlignment1, importFileList[0].name, "Wrong name");
      Assert.AreEqual((int)ImportedFileType.Alignment, importFileList[0].fileType, "Wrong filetype");
      //Cannot compare insertUTC as we don't know it here
    }

    [TestMethod]
    public void TestImportV2ForTbcSvlFile_MobileLineworkType_Ignore()
    {
      const string testName = "File Import 13";
      msg.Title(testName, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyCustomerId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyCustomerId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var project = ts.GetProjectDetailsViaWebApiV4(customerUid, projectUid);
      Assert.IsNotNull(project, $"unable to retrieve project. customerUid: {customerUid} projectUid: {projectUid} projectname: {testName}");

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name                      | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1} | 4                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = importFile.SendImportedFilesToWebApiV2(ts, project.LegacyProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);

      Assert.AreEqual(HttpStatusCode.OK, importFileV2Result.Code, "Not ignored.");
      Assert.AreEqual(-1, importFileV2Result.Id, "LegacyFileID should be unset.");
    }

    [TestMethod]
    public void TestImportV2ForTbcSvlFile_MasshaulType_Exception()
    {
      const string testName = "File Import 13";
      msg.Title(testName, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyCustomerId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[] {
       "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
      $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
      $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
      $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
      $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType          | EventDate   | ProjectUID   | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyCustomerId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var project = ts.GetProjectDetailsViaWebApiV4(customerUid, projectUid);
      Assert.IsNotNull(project, $"unable to retrieve project. customerUid: {customerUid} projectUid: {projectUid} projectname: {testName}");

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name                      | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1} | 7               | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = importFile.SendImportedFilesToWebApiV2(ts, project.LegacyProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);

      Assert.AreNotEqual(HttpStatusCode.OK, importFileV2Result.Code, "Not rejected.");
      Assert.AreEqual(0, importFileV2Result.Id, "LegacyFileID should be unset.");
    }
  }
}