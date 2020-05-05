using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests_Alignment : WebApiTestsBase
  {
    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportSvlFile(string uriRoot)
    {
      const string testText = "File Import align test 1";
      Msg.Title(testText, "Create standard project then upload svl file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};

      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImport2SvlFiles(string uriRoot)
    {
      const string testText = "File Import align test 2";
      Msg.Title(testText, "Create standard project then upload two alignment files");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename1 = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath1 = TestFileResolver.GetFullPath(importFilename1);
      var importFilename2 = TestFileResolver.File(TestFile.TestAlignment2);
      var fullFilePath2 = TestFileResolver.GetFullPath(importFilename2);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name            | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath1} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath2} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
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
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportANewFileThenUpdateTheAlignmentFile(string uriRoot)
    {
      const string testText = "File Import align test 3";
      Msg.Title(testText, "Create standard project then upload a new alignment file. Then update alignment file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);

      await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }), HttpStatusCode.BadRequest);
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);

      Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportANewFileThenDeleteTheAlignmentFile(string uriRoot)
    {
      const string testText = "File Import align test 4";
      Msg.Title(testText, "Create standard project then upload a new alignment file. Then delete alignment file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid      | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      importFile.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;

      await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);

      Assert.True(importFileList.ImportedFileDescriptors.Count == 0, "Expected 0 imported files but got " + importFileList.ImportedFileDescriptors.Count);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportTheSameFileTwice(string uriRoot)
    {
      const string testText = "File Import align test 5";
      Msg.Title(testText, "Create standard project then upload two alignment files that are the same name and content");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestAlignment1);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 3                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Assert.Single(filesResult.ImportedFileDescriptor.ImportedFileHistory);

      var filesResult2 = await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }), HttpStatusCode.BadRequest);
      Assert.True(filesResult2.Message == "CreateImportedFile. The file has already been created.", "Expecting a message: CreateImportedFile. The file has already been created.");
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      Assert.Single(filesResult.ImportedFileDescriptor.ImportedFileHistory);
    }
  }
}
