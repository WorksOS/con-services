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
  public class FileImportTests_DesignSurface : WebApiTestsBase, IClassFixture<ExecutorTestFixture>
  {
    private readonly ExecutorTestFixture _fixture;
    public FileImportTests_DesignSurface(ExecutorTestFixture fixture)
    {
      _fixture = fixture;
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportDesignSurfaceFile(string uriRoot)
    {
      const string testText = "File Import ds test 1";
      Msg.Title(testText, "Create standard project and customer then upload design surface file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);


      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var importFileArray = new[] {
         "| EventType              | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImport2DesignSurfaceFiles(string uriRoot)
    {
      const string testText = "File Import ds test 2";
      Msg.Title(testText, "Create standard project and customer then upload two Design surface files");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);


      var importFile = new ImportFile(uriRoot);
      var importFilename1 = TestFileResolver.File(TestFile.TestDesignSurface1);
      var importFilename2 = TestFileResolver.File(TestFile.TestDesignSurface2);

      var importFileArray = new[] {
         "| EventType              | ProjectUid      | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename1)} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename2)} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
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
    public async Task TestImportANewFileThenUpdateTheDesignSurfaceFile(string uriRoot)
    {
      const string testText = "File Import ds test 3";
      Msg.Title(testText, "Create standard project then upload a new design surface file. Then update design surface file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var fullPath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name       | ImportedFileType | FileCreatedUtc              | FileUpdatedUtc              | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullPath} | 1                | {startDateTime}             | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com | true        | 15           | 19           |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullPath} | 1                | {startDateTime.AddDays(10)} | {startDateTime.AddDays(10)} | testProjectMDM@trimble.com | true        | 15           | 19           |" };
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, expectedResult1, true);
      Assert.Single(filesResult.ImportedFileDescriptor.ImportedFileHistory);

      var filesResult2 = await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);

      Assert.True(importFileList.ImportedFileDescriptors.Count == 1, "Expected 1 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult2, true);
      Assert.Equal(2, filesResult2.ImportedFileDescriptor.ImportedFileHistory.Count);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    public async Task TestImportANewFileThenUpdateTheDesignSurfaceFile_SameFileDates(string uriRoot)
    {
      const string testText = "File Import ds test 4";
      Msg.Title(testText, "Create standard project then upload a new design surface file. Then update design surface file however leave same FileDates");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var fullPath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
        "| EventType              | ProjectUid       | CustomerUid   | Name        | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc              | ImportedBy                        | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullPath} | 1                | {startDateTime} | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com        | true        | 15           | 19           |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullPath} | 1                | {startDateTime} | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.com        | true        | 15           | 19           |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullPath} | 1                | {startDateTime} | {startDateTime.AddDays(5)}  | testProjectMDM@trimble.comChanged | true        | 15           | 19           |" };

      await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));

      var filesResult3 = await importFile.SendRequestToFileImportV6(ts, importFileArray, 2, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));

      Assert.Single(filesResult3.ImportedFileDescriptor.ImportedFileHistory);
    }
  }
}
