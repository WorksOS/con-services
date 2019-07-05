using System;
using System.Net.Http;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTestsGeotiff
  {
    public class FileImportTestsDxf
    {
      [Theory]
      [InlineData("api/v4/importedfile")]
      [InlineData("api/v4/importedfile/direct")]
      public void TestImportGeotiffFile(string uriRoot)
      {
        const string testName = "File Import 30";
        Msg.Title(testName, "Create standard project and customer then upload geotiff file");
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

        var importFilename = TestFileResolver.File(TestFile.TestGeotiffFile);
        var fullFilePath = TestFileResolver.GetFullPath(importFilename);

        var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc  | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath} | 8                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime} |true        | 15           | 19           |"};
        var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
        ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      }

      [Theory]
      [InlineData("api/v4/importedfile")]
      public void TestImportANewFileThenUpdateTheGeotiffFile(string uriRoot)
      {
        const string testName = "File Import 31";
        Msg.Title(testName, "Create standard project then upload a new geotiff file. Then update geotiff file");
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
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"};
        ts.PublishEventCollection(projectEventArray);

        var importFilename = TestFileResolver.File(TestFile.TestGeotiffFile);

        var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name                                           | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 8                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 8                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
        var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
        var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
        ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

        _ = importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));
        var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
        var importFileList = importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);

        Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
        ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
      }

    }
  }
}
