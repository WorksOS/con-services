using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class FileImportTests
  {
    private readonly Msg msg = new Msg();
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void TestNoFileUploads(string uriRoot)
    {
      const string testName = "File Import 1";
      msg.Title(testName, "Create standard project and customer then get imported files - There should be none.");
      var ts = new TestSupport();
      var legacyProjectId = ts.SetLegacyProjectId();
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

      var projectsArray = new[] {
         "| TableName           | EventDate   | ProjectUID   | LegacyProjectID   | Name       | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone          | StartDate   | EndDate   | GeometryWKT   |",
        $"| Project             | 0d+09:10:00 | {projectUid} | {legacyProjectId} | {testName} | 0                | New Zealand Standard Time | New Zealand Standard Time | {startDate} | {endDate} | {geometryWkt} |"};
      ts.PublishEventCollection(projectsArray);
      var importFile = new ImportFile(uriRoot);
      var expectedResults = importFile.ExpectedImportFileDescriptorsListResult;
      var uri = ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}";

      var filesResult = importFile.GetImportedFilesFromWebApiV4(uri, customerUid);
      Assert.IsTrue(filesResult.ImportedFileDescriptors.Count == expectedResults.ImportedFileDescriptors.Count, " Expected number of fields does not match actual");
      CollectionAssert.AreEqual(expectedResults.ImportedFileDescriptors, filesResult.ImportedFileDescriptors);
    }

    [TestMethod]
    [Ignore] 
    // the reason this is ignored, is that it requires a Trex system be stood up.
    //       Belongs in under-construction (Aaron) system-wide tests.
    [DataRow("api/v4/importedfile")]
    public void ManualTRexTest_CreateImportedFile(string uriRoot)
    {
      const string testName = "File Import 11";
      msg.Title(testName, "Create standard project then upload a new design surface file.");
      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var trexService = new TRex();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid().ToString();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");
      const string geometryWkt =
        "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";
      var eventsArray = new[]
      {
        "| TableName           | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"
      };
      ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[]
      {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"
      };
      ts.PublishEventCollection(projectEventArray);

      var importFileArray = new[]
      {
        "| EventType              | ProjectUid   | CustomerUid   | Name                                                  | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc              | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestDesignSurface3_GoodContent.FullPath()} | 1                | {startDateTime} | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | true        | 15           | 19           |"
      };
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1,
        new ImportOptions(HttpMethod.Post, new[] {"filename=TestDesignSurfaceTestDesignSurface3_GoodContent.TTM"}));
      Assert.IsNotNull(filesResult, "no result returned from ProjectSvc");
      Assert.AreEqual(0, filesResult.Code, "should be success result returned from ProjectSvc");
      Assert.AreEqual(projectUid, filesResult.ImportedFileDescriptor.ProjectUid, "invalid ProjectUIid returned");

      var designsResult = trexService.GetDesignsFromTrex(customerUid.ToString(), projectUid);
      Assert.AreEqual(0, designsResult.Code);
      Assert.AreEqual(1, designsResult.DesignFileDescriptors.Count, "should be 1 design");
      Assert.AreEqual(filesResult.ImportedFileDescriptor.ImportedFileUid, designsResult.DesignFileDescriptors[0].DesignUid, "should be correct designUid");
      Assert.AreEqual("TestDesignSurface3_GoodContent.TTM", designsResult.DesignFileDescriptors[0].Name, $"expected name: TestDesignSurfaceTestDesignSurface3_GoodContent.TTM received: {designsResult.DesignFileDescriptors[0].Name}");
    }

    [TestMethod]
    [Description("The goal of this method is to validate that the FileDescriptor of an upsert ImportedFile object can be deserialized successfully" +
                 "and doesn't contain invalid JSON; i.e. the JSON is not escaped.")]
    [DataRow("api/v4/importedfile")]
    public void TestImportANewFileThenUpdateTheFileThenDeleteTheFile(string uriRoot)
    {
      const string testName = "File insert, upsert & delete";
      msg.Title(testName, "Inset a new file, upsert a new version and then delete the file");

      var ts = new TestSupport();
      var importFile = new ImportFile(uriRoot);
      var legacyProjectId = ts.SetLegacyProjectId();
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
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestDesignSurface1.FullPath()} | 2                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestDesignSurface1.FullPath()} | 2                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestDesignSurface1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      _ = importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={TestFile.TestDesignSurface1}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApiV4(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);

      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult2, true);

      importFile.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;

      var deleteResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      Assert.AreEqual(0, deleteResult.Code);
      Assert.AreEqual("success", deleteResult.Message);
    }

  }
}
