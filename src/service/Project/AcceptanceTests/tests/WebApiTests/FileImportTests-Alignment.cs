using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class FileImportTests_Alignment
  {
    private readonly Msg msg = new Msg();
    private const string PROJECT_DB_SCHEMA_NAME = "VSS-MasterData-Project-Only";

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void TestImportSvlFile(string uriRoot)
    {
      const string testName = "File Import 2";
      msg.Title(testName, "Create standard project and customer then upload svl file");
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
       "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
      $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);
      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name                                 | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void TestImport2SvlFiles(string uriRoot)
    {
      const string testName = "File Import 3";
      msg.Title(testName, "Create standard project and customer then upload two alignment files");
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
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name                                 | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment2.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      var filesResult2 = importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment2}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);

      var importFileList = importFile.GetImportedFilesFromWebApiV4(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
    }

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void TestImportANewFileThenUpdateTheAlignmentFile(string uriRoot)
    {
      const string testName = "File Import 9";
      msg.Title(testName, "Create standard project then upload a new alignment file. Then update alignment file");
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
        "| EventType              | ProjectUid   | CustomerUid   | Name                                 | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      _ = importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApiV4(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);

      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
    }

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void TestImportANewFileThenDeleteTheAlignmentFile(string uriRoot)
    {
      const string testName = "File Import 14";
      msg.Title(testName, "Create standard project then upload a new alignment file. Then delete alignment file");
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
        "| EventType           | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFileArray = new[]
      {
        "| EventType              | ProjectUid   | CustomerUid   | Name                                 | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"
      };
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      importFile.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;

      _ = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      _ = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApiV4(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);

      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 0, "Expected 0 imported files but got " + importFileList.ImportedFileDescriptors.Count);
    }

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void KafkaConsumeTestImportANewAlignmentFile(string uriRoot)
    {
      const string testName = "File Import 15";
      msg.Title(testName, "Kafka Consume Test Create standard project then upload a new alignment file.");
      var ts = new TestSupport();
      var projectConsumerMysql = new MySqlHelper();
      projectConsumerMysql.UpdateDbSchemaName(PROJECT_DB_SCHEMA_NAME);
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
        "| EventType           | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        |IsArchived | CoordinateSystem      | Description |",
       $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {geometryWkt}   | {customerUid} | {legacyProjectId} |false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFileArray = new[]
      {
        "| EventType              | ProjectUid   | CustomerUid   | Name                                 | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"
      };
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      importFile.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;

      projectConsumerMysql.VerifyTestResultDatabaseRecordCount("ImportedFile", "fk_ProjectUID", 1, new Guid(projectUid));
    }

    [TestMethod]
    [DataRow("api/v4/importedfile")]
    [DataRow("api/v4/importedfile/direct")]
    public void TestImportTheSameFileTwice(string uriRoot)
    {
      const string testName = "File Import 8";
      msg.Title(testName, "Create standard project then upload two alignment files that are the same name and content");
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
       "| EventType              | ProjectUid   | CustomerUid   | Name                                 | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1.FullPath()} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Assert.AreEqual(1, filesResult.ImportedFileDescriptor.ImportedFileHistory.Count, "Expected 1 imported file History but got " + filesResult.ImportedFileDescriptor.ImportedFileHistory.Count);

      var filesResult2 = importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={TestFile.TestAlignment1}" }));
      Assert.IsTrue(filesResult2.Message == "CreateImportedFileV4. The file has already been created.", "Expecting a message: CreateImportedFileV4. The file has already been created.");
      var importFileList = importFile.GetImportedFilesFromWebApiV4(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      Assert.AreEqual(1, filesResult.ImportedFileDescriptor.ImportedFileHistory.Count, "Expected 1 imported file History but got " + filesResult.ImportedFileDescriptor.ImportedFileHistory.Count);
    }

  }
}
