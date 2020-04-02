using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using IntegrationTests.UtilityClasses;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class FileActivationTests : WebApiTestsBase
  {
    /*
    // todoMaverick
    [Fact]
    public async Task GetImportedFiles_should_return_activation_state()
    {
      const string testName = "Get Activated Import Files";
      Msg.Title(testName, "Get all activated import files");
      var ts = new TestSupport();
      var importFile = new ImportFile();
      var shortRaptorProjectID = TestSupport.GenerateShortRaptorProjectID();
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
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectID} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectID} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);

      var importFilename1 = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath1 = TestFileResolver.GetFullPath(importFilename1);
      var importFilename2 = TestFileResolver.File(TestFile.TestAlignment2);
      var fullFilePath2 = TestFileResolver.GetFullPath(importFilename2);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name            | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {fullFilePath2} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        |"};

      var filesResult = await importFile.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post));
      var expectedResult = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      expectedResult.IsActivated = true;

      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult, true);
      filesResult = await importFile.SendRequestToFileImportV4(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post));
      expectedResult = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;

      expectedResult.IsActivated = true;
      ts.CompareTheActualImportFileWithExpectedV4(filesResult.ImportedFileDescriptor, expectedResult, true);

      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV4(importFileList.ImportedFileDescriptors[1], expectedResult, true);

      var activatedFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.Equal(2, activatedFileList.ImportedFileDescriptors.Count);
    }

    [Fact]
    public async Task Set_activation_should_fail_when_projectId_is_invalid()
    {
      Msg.Title("Set ImportFile::IsActivated with invalid project id", string.Empty);

      await DoActivationRequest(Guid.NewGuid(), "INVALID_PROJECT_ID", "1c9e3a93-2bb0-461b-a74e-8091b895f71c", false, HttpStatusCode.BadRequest, 2001, "No access to the project for a customer or the project does not exist.", true);
    }

    [Fact]
    public async Task Set_activation_should_handle_project_with_no_files()
    {
      const string testName = "Set ImportFile::IsActivated with no loaded project files";
      Msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var projectUid = Guid.NewGuid().ToString();
      var shortRaptorProjectId = TestSupport.GenerateShortRaptorProjectID();
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
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);

      await DoActivationRequest(customerUid, projectUid, "id", false, HttpStatusCode.OK, 400, "Project contains no imported files.");
    }

    [Fact]
    public async Task Set_activation_should_handle_empty_file_list()
    {
      const string testName = "Set ImportFile::IsActivated with empty file list";
      Msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var projectUid = Guid.NewGuid();
      var shortRaptorProjectId = TestSupport.GenerateShortRaptorProjectID();
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
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      await ImportFiles(ts, projectUid, customerUid, startDateTime, fullFilePath);

      await DoActivationRequest(customerUid, projectUid.ToString(), null,
        false, HttpStatusCode.OK, 400, "Request contains no imported file IDs.");
    }

    [Fact]
    public async Task Set_activation_should_handle_no_eligible_files()
    {
      const string testName = "Set ImportFile::IsActivated with no valid files";
      Msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var projectUid = Guid.NewGuid();
      var shortRaptorProjectId = TestSupport.GenerateShortRaptorProjectID();
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
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment2);

      await ImportFiles(ts, projectUid, customerUid, startDateTime, TestFileResolver.GetFullPath(importFilename));

      await DoActivationRequest(customerUid, projectUid.ToString(), "BAD_ID", false, HttpStatusCode.OK, 200, "Success");
    }

    [Fact]
    public async Task Set_activation_should_set_state_on_eligible_files()
    {
      const string testName = "Set ImportFile::IsActivated";
      Msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var projectUid = Guid.NewGuid();
      var shortRaptorProjectId = TestSupport.GenerateShortRaptorProjectID();
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
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment2);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var fileResult = await ImportFiles(ts, projectUid, customerUid, startDateTime, fullFilePath);

      await DoActivationRequest(customerUid, projectUid.ToString(), fileResult.ImportedFileDescriptor.ImportedFileUid, false, HttpStatusCode.OK, 200, "Success");

      //Confirm it's deactivated for this user
      var importFile = new ImportFile();
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.Single(importFileList.ImportedFileDescriptors);
      Assert.False(importFileList.ImportedFileDescriptors[0].IsActivated, "Should be deactivated for user 1");

      //and activated for another user
      importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid, RestClient.ANOTHER_JWT);
      Assert.Single(importFileList.ImportedFileDescriptors);
      Assert.True(importFileList.ImportedFileDescriptors[0].IsActivated, "Should be activated for user 2");
    }

    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    public async Task Set_activation_should_set_state_on_design_and_reference_surfaces(string uriRoot1, string uriRoot2)
    {
      const string testName = "Set ImportFile::IsActivated";
      Msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);

      var projectUid = Guid.NewGuid();
      var shortRaptorProjectId = TestSupport.GenerateShortRaptorProjectID();
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
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);
      //Import parent design and reference surface
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      await importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));

      //Deactivate the parent design
      await DoActivationRequest(customerUid, projectUid.ToString(), filesResult.ImportedFileDescriptor.ImportedFileUid, false, HttpStatusCode.OK, 200, "Success");

      //Confirm both design and ref surface have been deactivated
      var importFileList = await importFileParent.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      Assert.False(importFileList.ImportedFileDescriptors[0].IsActivated, "First file should be deactivated");
      Assert.False(importFileList.ImportedFileDescriptors[1].IsActivated, "Second file should be deactivated");
    }

    [Theory]
    [InlineData("api/v4/importedfile", "api/v4/importedfile/referencesurface")]
    public async Task Set_activation_should_ignore_reference_surface(string uriRoot1, string uriRoot2)
    {
      const string testName = "Set ImportFile::IsActivated";
      Msg.Title(testName, string.Empty);

      var ts = new TestSupport();
      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);

      var projectUid = Guid.NewGuid();
      var shortRaptorProjectId = TestSupport.GenerateShortRaptorProjectID();
      var customerUid = Guid.NewGuid();
      var tccOrg = Guid.NewGuid();
      var subscriptionUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      var startDate = startDateTime.ToString("yyyy-MM-dd");
      var endDate = endDateTime.ToString("yyyy-MM-dd");

      var eventsArray = new[] {
        "| TableName            | EventDate   | CustomerUID   | Name       | fk_CustomerTypeID | SubscriptionUID   | fk_CustomerUID | fk_ServiceTypeID | StartDate   | EndDate        | fk_ProjectUID | TCCOrgID | fk_SubscriptionUID |",
        $"| Customer            | 0d+09:00:00 | {customerUid} | {testName} | 1                 |                   |                |                  |             |                |               |          |                    |",
        $"| CustomerTccOrg      | 0d+09:00:00 | {customerUid} |            |                   |                   |                |                  |             |                |               | {tccOrg} |                    |",
        $"| Subscription        | 0d+09:10:00 |               |            |                   | {subscriptionUid} | {customerUid}  | 19               | {startDate} | {endDate}      |               |          |                    |",
        $"| ProjectSubscription | 0d+09:20:00 |               |            |                   |                   |                |                  | {startDate} |                | {projectUid}  |          | {subscriptionUid}  |"};
      await ts.PublishEventCollection(eventsArray);

      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
        "| EventType          | EventDate   | ProjectUID   | ProjectID         | ProjectName | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary | CustomerUID   | CustomerID        | IsArchived | CoordinateSystem      | Description |",
        $"| CreateProjectEvent | 0d+09:00:00 | {projectUid} | {shortRaptorProjectId} | {testName}  | Standard    | New Zealand Standard Time | {startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff} | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} | {shortRaptorProjectId} | false      | BootCampDimensions.dc | {testName}  |"};
      await ts.PublishEventCollection(projectEventArray);

      //Import parent design and reference surface
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
        "| EventType               | ProjectUid   | CustomerUid   | Name         | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFileParent.SendRequestToFileImportV4(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = await importFileChild.SendRequestToFileImportV4(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));

      //Try and deactivate the reference surface
      await DoActivationRequest(customerUid, projectUid.ToString(), filesResult2.ImportedFileDescriptor.ImportedFileUid, false, HttpStatusCode.OK, 200, "Success");

      //Confirm it has not changed state
      var importFileList = await importFileParent.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v4/importedfiles?projectUid={projectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      var refSurf = importFileList.ImportedFileDescriptors.SingleOrDefault(i =>
        i.ImportedFileUid == filesResult2.ImportedFileDescriptor.ImportedFileUid);

      Assert.NotNull(refSurf);
      Assert.True(refSurf.IsActivated, "Reference surface should be activated");
    }

    private Task<ImportedFileDescriptorSingleResult> ImportFiles(TestSupport testSupport, Guid projectUid, Guid customerUid, DateTime startDateTime, string testFile)
    {
      var importFile = new ImportFile();

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name       | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated |",
        $"| ImportedFileDescriptor | {projectUid} | {customerUid} | {testFile} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        |"};

      return importFile.SendRequestToFileImportV4(testSupport, importFileArray, 1, new ImportOptions(HttpMethod.Post));
    }

    internal static async Task DoActivationRequest(Guid customerUid, string projectUid, string importedFileUid, bool activated, HttpStatusCode statusCode, int errorCode, string expectedMessage, bool uppercase = false)
    {
      var descrList = string.IsNullOrEmpty(importedFileUid)
        ? new List<ActivatedFileDescriptor>()
        : new List<ActivatedFileDescriptor>
        {
          new ActivatedFileDescriptor {ImportedFileUid = importedFileUid, IsActivated = activated}
        };

      var requestBody = JsonConvert.SerializeObject(new ActivatedImportFilesRequest
      {
        ImportedFileDescriptors = descrList
      });

      var jsonResponse = await RestClient.SendHttpClientRequest($"api/v4/importedfiles?projectUid={projectUid}", HttpMethod.Put, MediaTypes.JSON, MediaTypes.JSON,
        customerUid.ToString(),
        requestBody,
        expectedHttpCode: statusCode);

      var response = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

      Assert.Equal(errorCode, uppercase ? response.Code.Value : response.code.Value);
      Assert.Equal(expectedMessage, uppercase ? response.Message.Value : response.message.Value);
    }
    */
  }
}
