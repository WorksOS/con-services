using System;
using System.Net.Http;
using System.Threading.Tasks;
using IntegrationTests.ExecutorTests;
using IntegrationTests.UtilityClasses;
using TestUtility;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTestsDxf : WebApiTestsBase, IClassFixture<ExecutorTestFixture>
  {
    private readonly ExecutorTestFixture _fixture;
    public FileImportTestsDxf(ExecutorTestFixture fixture)
    {
      _fixture = fixture;
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportDxfFileUsSurveyFeet(string uriRoot)
    {
      const string testText = "File Import DXF test 1";
      Msg.Title(testText, "Create standard project and customer then upload dxf file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDxFfile);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | DxfUnitsType | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 0                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 2            | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportDxfFileImperial(string uriRoot)
    {
      const string testText = "File Import DXF test 2";
      Msg.Title(testText, "Create standard project and customer then upload dxf file in imperial");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDxFfile);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid      | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | DxfUnitsType | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 0                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 1            | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestImportDxfFileMetric(string uriRoot)
    {
      const string testText = "File Import DXF test 3";
      Msg.Title(testText, "Create standard project and customer then upload dxf file in metric");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      var createProjectResponse = _fixture.CreateCustomerProject(customerUid.ToString(), testText, Boundaries.Boundary1);
      ts.ProjectUid = new Guid(createProjectResponse.Result.Id);

      var importFile = new ImportFile(uriRoot);
      var importFilename = TestFileResolver.File(TestFile.TestDxFfile);
      var fullFilePath = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
       "| EventType              | ProjectUid   | CustomerUid   | Name           | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | DxfUnitsType | MinZoomLevel | MaxZoomLevel |",
      $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {fullFilePath} | 0                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 0            | 15           | 19           |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }
  }
}
