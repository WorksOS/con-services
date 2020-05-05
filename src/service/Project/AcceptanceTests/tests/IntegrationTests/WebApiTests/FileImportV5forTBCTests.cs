using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using Newtonsoft.Json;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class FileImportV5forTBCTests : WebApiTestsBase
  {
    [Fact]
    public async Task TestImportV2ForTbcSvlFile_AlignmentType_OK()
    {
      const string testText = "File Import V5TBC test 1";
      Msg.Title(testText, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile();
      var project = await ts.GetProjectDetailsViaWebApiV6(customerUid, ts.ProjectUid.ToString(), HttpStatusCode.OK);
      Assert.NotNull(project);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {importFilename} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = await importFile.SendImportedFilesToWebApiV5TBC(ts, project.ShortRaptorProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.OK, importFileV2Result.Code);
      Assert.NotEqual(-1, importFileV2Result.Id);

      var importFileList = await importFile.GetImportedFilesFromWebApi<ImmutableList<DesignDetailV5Result>>($"api/v5/projects/{project.ShortRaptorProjectId}/importedfiles", customerUid);
      Assert.True(importFileList.Count == 1, "Expected 1 imported files but got " + importFileList.Count);
      Assert.Equal(importFileV2Result.Id, importFileList[0].id);
      Assert.Equal(Path.GetFileName(importFilename), importFileList[0].name);
      Assert.Equal((int)ImportedFileType.Alignment, importFileList[0].fileType);
      //Cannot compare insertUTC as we don't know it here
    }

    [Fact]
    public async Task TestImportV2ForTbcSvlFile_MobileLineworkType_Ignore()
    {
      const string testText = "File Import V5TBC test 2";
      Msg.Title(testText, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile();
      var project = await ts.GetProjectDetailsViaWebApiV6(customerUid, ts.ProjectUid.ToString(), HttpStatusCode.OK);
      Assert.NotNull(project);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {importFilename} | 4                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = await importFile.SendImportedFilesToWebApiV5TBC(ts, project.ShortRaptorProjectId, importFileArray, 1);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.OK, importFileV2Result.Code);
      Assert.Equal(-1, importFileV2Result.Id);
    }

    [Fact]
    public async Task TestImportV2ForTbcSvlFile_MasshaulType_Exception()
    {
      const string testText = "File Import V5TBC test 3";
      Msg.Title(testText, "Create standard project and customer then upload svl file via TBC V2 API");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile();
      var project = await ts.GetProjectDetailsViaWebApiV6(customerUid, ts.ProjectUid.ToString(), HttpStatusCode.OK);
      Assert.NotNull(project);

      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {importFilename} | 7               | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = await importFile.SendImportedFilesToWebApiV5TBC(ts, project.ShortRaptorProjectId, importFileArray, 1, HttpStatusCode.BadRequest);
      var importFileV2Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.NotEqual(HttpStatusCode.OK, importFileV2Result.Code);
      Assert.Equal(0, importFileV2Result.Id);
    }
  }
}
