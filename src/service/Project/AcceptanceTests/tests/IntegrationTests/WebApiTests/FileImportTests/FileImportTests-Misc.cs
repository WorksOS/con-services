using System;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests : WebApiTestsBase
  {
    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestNoFileUploads(string uriRoot)
    {
      const string testText = "File Import Misc 1";
      Msg.Title(testText, "Create standard project and customer then get imported files - There should be none.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var expectedResults = importFile.ExpectedImportFileDescriptorsListResult;
      var filesResult = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);
      
      Assert.True(filesResult.ImportedFileDescriptors.Count == expectedResults.ImportedFileDescriptors.Count, " Expected number of fields does not match actual");
      Assert.Equal(expectedResults.ImportedFileDescriptors, filesResult.ImportedFileDescriptors);
    }

    [Theory(Skip ="Cannot run until we have a a TRex system or mock.")]
    [InlineData("api/v6/importedfile")]
    public async Task ManualTRexTest_CreateImportedFile(string uriRoot)
    {
      const string testText = "File Import Misc 2";
      Msg.Title(testText, "Create standard project then upload a new design surface file.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface3_GoodContent);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc              | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 1                | {startDateTime} | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | true        | 15           | 19           |" };
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1,
        new ImportOptions(HttpMethod.Post, new[] {"filename=TestDesignSurfaceTestDesignSurface3_GoodContent.TTM"}));
      Assert.NotNull(filesResult);
      Assert.Equal(0, filesResult.Code);
      Assert.Equal(ts.ProjectUid.ToString(), filesResult.ImportedFileDescriptor.ProjectUid);

      var trexService = new TRex();
      var designsResult = await trexService.GetDesignsFromTrex(customerUid.ToString(), ts.ProjectUid.ToString());
      Assert.Equal(0, designsResult.Code);
      Assert.Single(designsResult.DesignFileDescriptors);
      Assert.Equal(filesResult.ImportedFileDescriptor.ImportedFileUid, designsResult.DesignFileDescriptors[0].DesignUid);
      Assert.Equal("TestDesignSurface3_GoodContent.TTM", designsResult.DesignFileDescriptors[0].Name);
    }

    /// <summary>
    /// The goal of this method is to validate that the FileDescriptor of an upsert ImportedFile object can be deserialized successfully
    /// "and doesn't contain invalid JSON; i.e. the JSON is not escaped.
    /// </summary>
    [Theory]
    [InlineData("api/v6/importedfile")]
    public async Task TestImportANewFileThenUpdateTheFileThenDeleteTheFile(string uriRoot)
    {
      const string testText = "File Import Misc 3";
      Msg.Title(testText, "Inset a new file, upsert a new version and then delete the file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var endDateTime = new DateTime(9999, 12, 31);
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectStartDate                            | ProjectEndDate                             | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time |{startDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {endDateTime:yyyy-MM-ddTHH:mm:ss.fffffff}  | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 2                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 2                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      _ = await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);

      Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult2, true);

      importFile.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;

      var deleteResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      Assert.Equal(0, deleteResult.Code);
      Assert.Equal("success", deleteResult.Message);
    }
  }
}
