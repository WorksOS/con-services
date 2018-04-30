using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;

namespace WebApiTests
{
  [TestClass]
  public class FileActivationTests
  {
    private readonly Msg _msg = new Msg();

    private const string GeometryWkt = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";

    [TestMethod]
    public void GetImportedFiles_should_return_activation_state()
    {
      const string testName = "Get Activated Import Files";
      _msg.Title(testName, "Get all activated import files");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var legacyProjectId = ts.SetLegacyProjectId();
      var projectUid = Guid.NewGuid();
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
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {GeometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name                       | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {TestFile.TestAlignment2} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        |"};

      var filesResult = importFile.SendImportedFilesToWebApiV4(ts, importFileArray, 1);
      var expectedResult = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      expectedResult.IsActivated = true;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult, true);
      filesResult = importFile.SendImportedFilesToWebApiV4(ts, importFileArray, 2);
      expectedResult = importFile.expectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      expectedResult.IsActivated = true;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult, true);

      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.IsTrue(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpected(importFileList.ImportedFileDescriptors[1], expectedResult, true);

      var activatedFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.AreEqual(2, activatedFileList.ImportedFileDescriptors.Count);
    }

    [TestMethod]
    public void Set_activation_should_fail_when_projectId_is_invalid()
    {
      const string testName = "Set ImportFile::IsActivated with invalid project id";
      _msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFile = new ImportFile();
      var customerUid = Guid.NewGuid();

      var requestBody = JsonConvert.SerializeObject(new ActivatedImportFilesRequest
      {
        ImportedFileDescriptors = new List<ActivatedFileDescriptor>{
          new ActivatedFileDescriptor { ImportedFileUid = "1c9e3a93-2bb0-461b-a74e-8091b895f71c", IsActivated = false }}
      });

      var jsonResponse = importFile.DoHttpRequest(
          ts.GetBaseUri() + "api/v4/importedfiles?projectUid=INVALID_PROJECT_ID",
          "PUT",
          requestBody,
          customerUid.ToString(),
          "application/json");

      var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

      Assert.AreEqual(2001, response.Code.Value);
      Assert.AreEqual("No access to the project for a customer or the project does not exist.", response.Message.Value);
    }

    [TestMethod]
    public void Set_activation_should_handle_project_with_no_files()
    {
      const string testName = "Set ImportFile::IsActivated with no loaded project files";
      _msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFile = new ImportFile();
      var projectUid = Guid.NewGuid().ToString();
      var legacyProjectId = ts.SetLegacyProjectId();
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
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {GeometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var requestBody = JsonConvert.SerializeObject(new ActivatedImportFilesRequest
      {
        ImportedFileDescriptors = new List<ActivatedFileDescriptor>
        {
          new ActivatedFileDescriptor { ImportedFileUid = "id", IsActivated = false }
        }
      });

      var jsonResponse = importFile.DoHttpRequest(
        ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}",
        "PUT",
        requestBody,
        customerUid.ToString(),
        "application/json");

      var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

      Assert.AreEqual((int)HttpStatusCode.BadRequest, response.code.Value);
      Assert.AreEqual("Project contains no imported files.", response.message.Value);
    }

    [TestMethod]
    public void Set_activation_should_handle_empty_file_list()
    {
      const string testName = "Set ImportFile::IsActivated with empty file list";
      _msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFile = new ImportFile();
      var projectUid = Guid.NewGuid();
      var legacyProjectId = ts.SetLegacyProjectId();
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
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {GeometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var fileResult = ImportFiles(importFile, ts, projectUid, customerUid, startDateTime, TestFile.TestAlignment1);
      var requestBody = JsonConvert.SerializeObject(new ActivatedImportFilesRequest
      {
        ImportedFileDescriptors = new List<ActivatedFileDescriptor>()
      });

      var jsonResponse = importFile.DoHttpRequest(
        ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}",
        "PUT",
        requestBody,
        customerUid.ToString(),
        "application/json");

      var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

      Assert.AreEqual((int)HttpStatusCode.BadRequest, response.code.Value);
      Assert.AreEqual("Request contains no imported file IDs.", response.message.Value);
    }

    [TestMethod]
    public void Set_activation_should_handle_no_eligible_files()
    {
      const string testName = "Set ImportFile::IsActivated with no valid files";
      _msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFile = new ImportFile();
      var projectUid = Guid.NewGuid();
      var legacyProjectId = ts.SetLegacyProjectId();
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
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {GeometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var fileResult = ImportFiles(importFile, ts, projectUid, customerUid, startDateTime, TestFile.TestAlignment2);
      var requestBody = JsonConvert.SerializeObject(new ActivatedImportFilesRequest
      {
        ImportedFileDescriptors = new List<ActivatedFileDescriptor>
        {
          new ActivatedFileDescriptor { ImportedFileUid = "BAD_ID", IsActivated = false }
        }
      });

      var jsonResponse = importFile.DoHttpRequest(
        ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}",
        "PUT",
        requestBody,
        customerUid.ToString(),
        "application/json");

      var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

      Assert.AreEqual((int)HttpStatusCode.OK, response.code.Value);
      Assert.AreEqual("Success", response.message.Value);
    }

    [TestMethod]
    public void Set_activation_should_set_state_on_eligible_files()
    {
      const string testName = "Set ImportFile::IsActivated";
      _msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFile = new ImportFile();
      var projectUid = Guid.NewGuid();
      var legacyProjectId = ts.SetLegacyProjectId();
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
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {legacyProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {GeometryWkt}   | {customerUid} | {legacyProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      ts.PublishEventCollection(projectEventArray);

      var fileResult = ImportFiles(importFile, ts, projectUid, customerUid, startDateTime, TestFile.TestAlignment2);
      
      var requestBody = JsonConvert.SerializeObject(new ActivatedImportFilesRequest
      {
        ImportedFileDescriptors = new List<ActivatedFileDescriptor>
        {
          new ActivatedFileDescriptor { ImportedFileUid = fileResult.ImportedFileDescriptor.ImportedFileUid, IsActivated = false }
        }
      });

      var jsonResponse = importFile.DoHttpRequest(
        ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}",
        "PUT",
        requestBody,
        customerUid.ToString(),
        "application/json");

      var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

      Assert.AreEqual((int)HttpStatusCode.OK, response.code.Value);
      Assert.AreEqual("Success", response.message.Value);

      //Confirm it's deactivated for this user
      var importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.AreEqual(1, importFileList.ImportedFileDescriptors.Count, "Wrong number of imported files 1");
      Assert.IsFalse(importFileList.ImportedFileDescriptors[0].IsActivated, "Should be deactivated for user 1");

      //and activated for another user
      importFileList = importFile.GetImportedFilesFromWebApi(ts.GetBaseUri() + $"api/v4/importedfiles?projectUid={projectUid}", customerUid, RestClientUtil.ANOTHER_JWT);
      Assert.AreEqual(1, importFileList.ImportedFileDescriptors.Count, "Wrong number of imported files 2");
      Assert.IsTrue(importFileList.ImportedFileDescriptors[0].IsActivated, "Should be activated for user 2");
    }

    private static ImportedFileDescriptorSingleResult ImportFiles(ImportFile importFile, TestSupport testSupport, Guid projectUid, Guid customerUid, DateTime startDateTime, string testFile)
    {
      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name                       | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {testFile} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        |"};

      return importFile.SendImportedFilesToWebApiV4(testSupport, importFileArray, 1);
    }
  }
}