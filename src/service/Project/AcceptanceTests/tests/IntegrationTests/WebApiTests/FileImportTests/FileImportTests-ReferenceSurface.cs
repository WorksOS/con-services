using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests_ReferenceSurface : WebApiTestsBase
  {
    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    [InlineData("api/v4/importedfile/direct", "api/v4/importedfile/referencesurface")]
    public void TestImportReferenceSurfaceFile(string uriRoot1, string uriRoot2)
    {
      const string testName = "File Import 20";
      Msg.Title(testName, "Create standard project and customer then upload reference surface file");
      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate.ToUniversalTime();
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

      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    [InlineData("api/v4/importedfile/direct", "api/v4/importedfile/referencesurface")]
    public void TestImportReferenceSurfaceFileFromDeactivatedDesign(string uriRoot1, string uriRoot2)
    {
      const string testName = "File Import 24";
      Msg.Title(testName, "Create standard project and customer then upload reference surface file");
      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate.ToUniversalTime();
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

      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Deactivate the parent design
      FileActivationTests.DoActivationRequest(customerUid, projectUid, filesResult.ImportedFileDescriptor.ImportedFileUid, false, (int)HttpStatusCode.OK, "Success");

      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      //Expect the reference surface to be deactivated
      importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor.IsActivated = false;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    [InlineData("api/v4/importedfile/direct", "api/v4/importedfile/referencesurface")]
    public void TestImport2ReferenceSurfaceFiles(string uriRoot1, string uriRoot2)
    {
      const string testName = "File Import 21";
      Msg.Title(testName, "Create standard project and customer then upload two Reference surface files");
      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
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
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name          | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult1 = importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult1.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surfaces
      var parentUid = filesResult1.ImportedFileDescriptor.ImportedFileUid;
      var offset1 = 1.5;
      var offset2 = -2.5;
      parentName = Path.GetFileNameWithoutExtension(parentName);
      var name1 = $"{parentName} +{offset1}m";
      var name2 = $"{parentName} {offset2}m";
      var importFileArray2 = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name    | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset    |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name1} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset1} |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name2} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset2} |"};
      var filesResult2 = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name1)}" }));
      var expectedResult2 = importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);

      var filesResult3 = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name2)}" }));
      var expectedResult3 = importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult3.ImportedFileDescriptor, expectedResult3, true);

      var importFileList = importFileParent.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 3, "Expected 3 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[2], expectedResult3, true);
    }

    [Theory]
    [InlineData("api/v4/importedfile/referencesurface")]
    public void TestImportReferenceSurfaceFileWithoutParentShouldFail(string uriRoot)
    {
      const string testName = "File Import 23";
      Msg.Title(testName, "Create standard project and customer then upload reference surface file without parent design uploaded");
      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate.ToUniversalTime();
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
      //Reference Surface
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};

      var errorResultObj = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      Assert.Equal("Missing parent design for reference surface", errorResultObj.Message);
    }

    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    [InlineData("api/v4/importedfile/direct", "api/v4/importedfile/referencesurface")]
    public void TestImportReferenceSurfaceThenDeleteTheParentDesignSurface(string uriRoot1, string uriRoot2)
    {
      const string testName = "File Import 14";
      Msg.Title(testName, "Create standard project then upload a new reference file. Then delete parent design surface file");
      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
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
        "| EventType           | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Delete parent design

      var errorResultObj = importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      Assert.Equal("[{\"Key\":\"importedFileUid\",\"Value\":\"The value '' is invalid.\\r\\n\"}]", errorResultObj.Message);
    }

    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    [InlineData("api/v4/importedfile/direct", "api/v4/importedfile/referencesurface")]
    public void TestImportReferenceSurfaceFileTwice(string uriRoot1, string uriRoot2)
    {
      const string testName = "File Import 20";
      Msg.Title(testName, "Create standard project and customer then upload reference surface file");
      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      var legacyProjectId = TestSupport.GenerateLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate.ToUniversalTime();
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
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffffK} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);

      //Import again
      var errorResultObj = importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      Assert.Equal("Reference surface already exists", errorResultObj.Message);
    }
  }
}
