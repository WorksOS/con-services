using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;
using System.Net;
using ProjectWebApiCommon.Models;

namespace WebApiTests
{
  [TestClass]
  public class FileImportTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
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

      var filesResult = importFile.GetImportedFilesFromWebApi(uri, customerUid, projectUid);
      Assert.IsTrue(filesResult.ImportedFileDescriptors.Count == expectedResults.ImportedFileDescriptors.Count, " Expected number of fields does not match actual");
      CollectionAssert.AreEqual(expectedResults.ImportedFileDescriptors, filesResult.ImportedFileDescriptors);
    }

    [TestMethod] 
    public void TestImportFileUpload()
    {
      const string testName = "File Import 2";
      msg.Title(testName, "Create standard project and customer then upload file");
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
       "| TableName | EventDate   | ProjectUID   | LegacyProjectID   | Name       | fk_ProjectTypeID | ProjectTimeZone           | LandfillTimeZone          | StartDate   | EndDate   | GeometryWKT   |",
      $"| Project   | 0d+09:10:00 | {projectUid} | {legacyProjectId} | {testName} | 0                | New Zealand Standard Time | New Zealand Standard Time | {startDate} | {endDate} | {geometryWkt} |"};
      ts.PublishEventCollection(projectsArray);

      ImportFile importFile = new ImportFile();
      var expectedResults = importFile.expectedImportFileDescriptorSingleResult;
      var uri = ts.GetBaseUri() + $"api/v4/importedfile?projectUid={projectUid}&importedFileType=Alignment&fileCreatedUtc=2017-05-01T23:24:26.222Z&fileUpdatedUtc=2017-05-03T01:02:03.111Z";
     // var uri =  $"http://localhost:20979/api/v4/importedfile?projectUid={projectUid}&importedFileType=Alignment&fileCreatedUtc=2017-05-01T23:24:26.222Z&fileUpdatedUtc=2017-05-03T01:02:03.111Z";
      var filesResult = importFile.PostImportedFilesToWebApi(uri, customerUid, projectUid,"FileImportFiles\\Boundarys.txt");
      Assert.AreEqual(filesResult.ImportedFileDescriptor, expectedResults.ImportedFileDescriptor, " Expected number of fields does not match actual");
    }
  }
}
