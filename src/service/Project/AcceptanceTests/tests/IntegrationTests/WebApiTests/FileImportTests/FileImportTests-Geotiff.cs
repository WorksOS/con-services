using System;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTestsGeotiff
  {
    public class FileImportTestsDxf
    {
      [Theory]
      [InlineData("api/v6/importedfile")]
      [InlineData("api/v6/importedfile/direct")]
      public async Task TestImportGeotiffFile(string uriRoot)
      {
        const string testText = "File Import geotiff 1";
        Msg.Title(testText, "Create standard project and customer then upload geotiff file");
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
        var importFilename = TestFileResolver.File(TestFile.TestGeotiffFile);
        var fullFilePath = TestFileResolver.GetFullPath(importFilename);

        var importFileArray = new[] {
          "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc  | IsActivated | MinZoomLevel | MaxZoomLevel |",
         $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 8                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime} |true        | 15           | 19           |"};
        var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
        ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      }

      [Theory]
      [InlineData("api/v6/importedfile")]
      public async Task TestImportANewFileThenUpdateTheGeotiffFile(string uriRoot)
      {
        const string testText = "File Import geotiff 2";
        Msg.Title(testText, "Create standard project then upload a new geotiff file. Then update geotiff file");
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
        var importFilename = TestFileResolver.File(TestFile.TestGeotiffFile);

        var importFileArray = new[] {
          "| EventType              | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
         $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 8                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
         $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 8                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
        var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
        var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
        ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

        _ = await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));
        var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
        var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);

        Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
        ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
      }
    }
  }
}
