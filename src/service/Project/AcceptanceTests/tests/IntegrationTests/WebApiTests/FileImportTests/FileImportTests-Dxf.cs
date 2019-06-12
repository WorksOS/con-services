using System;
using System.Net.Http;
using IntegrationTests.UtilityClasses;
using TestUtility;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTestsDxf
  {
    [Theory]
    [InlineData("api/v4/importedfile")]
    [InlineData("api/v4/importedfile/direct")]
    public void TestImportDxfFileUsSurveyFeet(string uriRoot)
    {
      const string testName = "File Import 17";
      Msg.Title(testName, "Create standard project and customer then upload dxf file");
      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestDxFfile);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | DxfUnitsType | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 0                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 2            | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v4/importedfile")]
    [InlineData("api/v4/importedfile/direct")]
    public void TestImportDxfFileImperial(string uriRoot)
    {
      const string testName = "File Import 18";
      Msg.Title(testName, "Create standard project and customer then upload dxf file in imperial");
      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestDxFfile);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | DxfUnitsType | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 0                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 1            | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v4/importedfile")]
    [InlineData("api/v4/importedfile/direct")]
    public void TestImportDxfFileMetric(string uriRoot)
    {
      const string testName = "File Import 19";
      Msg.Title(testName, "Create standard project and customer then upload dxf file in metric");
      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestDxFfile);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | DxfUnitsType | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 0                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 0            | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }
  }
}
