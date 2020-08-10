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
    [Fact(Skip = "Waiting for CCSSSCON-396")]
    public async Task TestImportV5ForTbcSvlFile_AlignmentType_OK()
    {
      var testText = "File Import V5TBC test 1";
      Msg.Title(testText, "Create project then upload Alignment file via TBC V5 API");
      var ts = new TestSupport();

      var startDateTime = ts.FirstEventDate;
      var projectName = $"project {testText}";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      var importFile = new ImportFile();
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid      | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {ts.CustomerUid} | {importFilename} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = await importFile.SendImportedFilesToWebApiV5TBC(ts, returnLongV5Result.Id, importFileArray, 1);
      var importFileV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.OK, importFileV5Result.Code);
      Assert.NotEqual(-1, importFileV5Result.Id);

      var importFileList = await importFile.GetImportedFilesFromWebApi<ImmutableList<DesignDetailV5Result>>($"api/v5/projects/{returnLongV5Result.Id}/importedfiles", ts.CustomerUid);
      Assert.True(importFileList.Count == 1, "Expected 1 imported files but got " + importFileList.Count);
      Assert.Equal(importFileV5Result.Id, importFileList[0].id);
      Assert.Equal(Path.GetFileName(importFilename), importFileList[0].name);
      Assert.Equal((int)ImportedFileType.Alignment, importFileList[0].fileType);
    }

    [Fact(Skip = "Waiting for CCSSSCON-396")]
    public async Task TestImportV5ForTbcSvlFile_MobileLineworkType_Ignore()
    {
      var testText = "File Import V5TBC test 2";
      Msg.Title(testText, "Create project then upload (should fail) mobileLinework file via TBC V5 API");
      var ts = new TestSupport();

      var startDateTime = ts.FirstEventDate;
      var projectName = $"project {testText}";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      var importFile = new ImportFile();
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid      | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {ts.CustomerUid} | {importFilename} | 4                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = await importFile.SendImportedFilesToWebApiV5TBC(ts, returnLongV5Result.Id, importFileArray, 1);
      var importFileV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.OK, importFileV5Result.Code);
      Assert.Equal(-1, importFileV5Result.Id);
    }

    [Fact(Skip = "Waiting for CCSSSCON-396")]
    public async Task TestImportV5ForTbcSvlFile_MassHaulType_Exception()
    {
      var testText = "File Import V5TBC test 3";
      Msg.Title(testText, "Create project then upload (exception) MassHaul file via TBC V5 API");
      var ts = new TestSupport();

      var startDateTime = ts.FirstEventDate;
      var projectName = $"project {testText}";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      var importFile = new ImportFile();
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid      | Name             | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {ts.CustomerUid} | {importFilename} | 7                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var response = await importFile.SendImportedFilesToWebApiV5TBC(ts, returnLongV5Result.Id, importFileArray, 1, HttpStatusCode.BadRequest);
      var importFileV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.NotEqual(HttpStatusCode.OK, importFileV5Result.Code);
      Assert.Equal(0, importFileV5Result.Id);
    }
  }
}
