using System;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests_SurveyedSurface : WebApiTestsBase
  {
    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportSurveyedSurfaceFile(string uriRoot)
    {
      const string testText = "File Import SS test 1";
      Msg.Title(testText, "Create standard project and customer then upload surveyed surface file");
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

      var importFileArray = new[] {
        "| EventType              | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImport2SurveyedSurfaceFiles(string uriRoot)
    {
      const string testText = "File Import v2";
      Msg.Title(testText, "Create standard project and customer then upload two Surveryed surface files");
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
      var importFilename1 = TestFileResolver.File(TestFile.TestDesignSurface1);
      var importFilename2 = TestFileResolver.File(TestFile.TestDesignSurface2);

      var importFileArray = new[] {
        "| EventType              | ProjectUid      | CustomerUid   | Name                                            | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename1)} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename2)} | 2                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename1}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;

      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      var filesResult2 = await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename2}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);

      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 2, "Expected 2 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    public async Task TestImportANewFileThenUpdateTheSurveyedSurfaceFile(string uriRoot)
    {
      const string testText = "File Import SS test 3";
      Msg.Title(testText, "Create standard project then upload a new surveyed surface file. Then update surveyed surface file");
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

      var importFileArray = new[] {
        "| EventType              | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | SurveyedUtc     | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 2                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 2                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | {startDateTime} | true        | 0            | 0            |"};
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
