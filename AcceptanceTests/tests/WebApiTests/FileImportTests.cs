using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests
{
  [TestClass]
  public class FileImportTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod] [Ignore]
    public void TestNoFileUploads()
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
      var importFile = new ImportFile();
      var expectedResults = importFile.expectedImportFileDescriptorsListResult;
      var uri = ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}";

      var filesResult = importFile.GetImportedFilesFromWebApi(uri, customerUid);
      Assert.IsTrue(filesResult.ImportedFileDescriptors.Count == expectedResults.ImportedFileDescriptors.Count, " Expected number of fields does not match actual");
      CollectionAssert.AreEqual(expectedResults.ImportedFileDescriptors, filesResult.ImportedFileDescriptors);
    }

    [TestMethod] [Ignore]
    public void TestImportSvlFile()
    {
      const string testName = "File Import 2";
      msg.Title(testName, "Create standard project and customer then upload svl file");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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
      var fileName = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";
      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name       | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor,importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }
    [TestMethod] [Ignore]
    public void TestImport2SvlFiles()
    {
      const string testName = "File Import 3";
      msg.Title(testName, "Create standard project and customer then upload two alignment files");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestAlignment2.SVL";

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName2} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 2);
      var expectedResult2 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);
      Thread.Sleep(3000);
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count ==2,"Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
    }

    [TestMethod]// [Ignore]
    public void TestImportSurfaceFile()
    {
      const string testName = "File Import 4";
      msg.Title(testName, "Create standard project and customer then upload design surface file");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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
      var fileName = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface1.ttm";
      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name       | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [TestMethod] [Ignore]
    public void TestImport2SurfaceFiles()
    {
      const string testName = "File Import 5";
      msg.Title(testName, "Create standard project and customer then upload two Design surface files");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface1.ttm";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface2.TTM";

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName2} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 2);
      var expectedResult2 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);
      Thread.Sleep(3000);
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
    }

    [TestMethod] [Ignore]
    public void TestImportSurveyedSurfaceFile()
    {
      const string testName = "File Import 4";
      msg.Title(testName, "Create standard project and customer then upload surveyed surface file");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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
      var fileName = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface1.ttm";
      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name       | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc    |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime}|"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [TestMethod] //[Ignore]
    public void TestImport2SurveyedSurfaceFiles()
    {
      const string testName = "File Import 5";
      msg.Title(testName, "Create standard project and customer then upload two Surveryed surface files");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface1.ttm";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface2.TTM";

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc    |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime}|",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName2} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime}|"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 2);
      var expectedResult2 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);
      Thread.Sleep(3000);
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
    }

    [TestMethod] [Ignore]
    public void TestImportTheSameFileTwice()
    {
      const string testName = "File Import 6";
      msg.Title(testName, "Create standard project then upload two alignment files that are the same name and content");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName2} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 2);
      Assert.IsTrue(filesResult2.Message == "CreateImportedFileV4. The file has already been created.", "Expecting a message: CreateImportedFileV4.The file has already been created.");
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
    }

    [TestMethod] [Ignore]
    public void TestImportANewFileThenUpdateTheAlignmentFile()
    {
      const string testName = "File Import 7";
      msg.Title(testName, "Create standard project then upload a new alignment file. Then update alignment file");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName2} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 2, "PUT");
      var expectedResult2 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
    }


    [TestMethod] //[Ignore]
    public void TestImportANewFileThenUpdateTheSurveyedSurfaceFile()
    {
      const string testName = "File Import 7";
      msg.Title(testName, "Create standard project then upload a new surveyed surface file. Then update surveyed surface file");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface1.ttm";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestDesignSurface1.ttm";

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc               |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime}           |",
      $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName2} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime.AddDays(5)}|"};
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 2, "PUT");
      var expectedResult2 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
    }


    [TestMethod] [Ignore]
    public void TestImportANewFileThenDeleteTheAlignmentFile()
    {
      const string testName = "File Import 7";
      msg.Title(testName, "Create standard project then upload a new alignment file. Then delete alignment file");
      var ts = new TestSupport();
      var importFile = new ImportFile();
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

      var fileName1 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";
      var fileName2 = "FileImportFiles" + ts.FileSeperator + "TestAlignment1.svl";

      var importFileArray = new[]
      {
        "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
       $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fileName1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"
      };
      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult1 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      importFile.importedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      Thread.Sleep(3000);
      var filesResult2 = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1,"DELETE");
      var expectedResult2 = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 0, "Expected 0 imported files but got " + importFileList.ImportedFileDescriptors.Count);
    }

  }
}
