using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using WebApiTests.UtilityClasses;
using Xunit;
using ImportedFileType = VSS.VisionLink.Interfaces.Events.MasterData.Models.ImportedFileType;

namespace WebApiTests
{
  public class FileImportV2forTBCTests
  {
    [Fact]
    public void TestImportV2ForTbcSvlFile_AlignmentType_OK()
    {
      const string testName = "File Import 13";
      Msg.Title(testName, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyCustomerId = MySqlHelper.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

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
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyCustomerId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var project = ts.GetProjectDetailsViaWebApiV4(customerUid, projectUid);
      Assert.NotNull(project);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {importFilename} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = importFile.SendImportedFilesToWebApiV2(ts, project.LegacyProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);

      Assert.Equal(HttpStatusCode.OK, importFileV2Result.Code);
      Assert.NotEqual(-1, importFileV2Result.Id);

      var importFileList = importFile.GetImportedFilesFromWebApi<ImmutableList<DesignDetailV2Result>>(ts.BaseUri + $"api/v2/projects/{project.LegacyProjectId}/importedfiles", customerUid);
      Assert.True(importFileList.Count == 1, "Expected 1 imported files but got " + importFileList.Count);
      Assert.Equal(importFileV2Result.Id, importFileList[0].id);
      Assert.Equal(Path.GetFileName(importFilename), importFileList[0].name);
      Assert.Equal((int)ImportedFileType.Alignment, importFileList[0].fileType);
      //Cannot compare insertUTC as we don't know it here
    }

    [Fact]
    public void TestImportV2ForTbcSvlFile_MobileLineworkType_Ignore()
    {
      const string testName = "File Import 13";
      Msg.Title(testName, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyCustomerId = MySqlHelper.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

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
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyCustomerId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var project = ts.GetProjectDetailsViaWebApiV4(customerUid, projectUid);
      Assert.NotNull(project);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {importFilename} | 4                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = importFile.SendImportedFilesToWebApiV2(ts, project.LegacyProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);

      Assert.Equal(HttpStatusCode.OK, importFileV2Result.Code);
      Assert.Equal(-1, importFileV2Result.Id);
    }

    [Fact]
    public void TestImportV2ForTbcSvlFile_MasshaulType_Exception()
    {
      const string testName = "File Import 13";
      Msg.Title(testName, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyCustomerId = MySqlHelper.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

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
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyCustomerId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var project = ts.GetProjectDetailsViaWebApiV4(customerUid, projectUid);
      Assert.NotNull(project);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {importFilename} | 7               | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = importFile.SendImportedFilesToWebApiV2(ts, project.LegacyProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);

      Assert.NotEqual(HttpStatusCode.OK, importFileV2Result.Code);
      Assert.Equal(0, importFileV2Result.Id);
    }
  }
}
