using System;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.ExecutorTests;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests_SurveyedSurface : WebApiTestsBase, IClassFixture<ExecutorTestFixture>
  {
    private readonly ExecutorTestFixture _fixture;
    public FileImportTests_SurveyedSurface(ExecutorTestFixture fixture)
    {
      _fixture = fixture;
    }

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
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

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
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

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
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

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
