using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestUtility;

namespace WebApiTests.FileImport
{
  [TestClass]
  public class FileActivationTests
  {
    private readonly Msg msg = new Msg();

    [TestMethod]
    [Ignore]
    public void TestGetActivatedImportFiles()
    {
      const string testName = "Set Import File activated state";
      msg.Title(testName, "");
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

      var importFileArray = new[]
      {
        "| EventType              | ProjectUid   | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com |"
      };

      var filesResult = importFile.SendToImportedFilesToWebApi(ts, importFileArray, 1);
      var expectedResult = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      expectedResult.IsActivated = true;
      //ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult, true);
      Thread.Sleep(3000);

      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[1], expectedResult, true);
      
    }
  }
}