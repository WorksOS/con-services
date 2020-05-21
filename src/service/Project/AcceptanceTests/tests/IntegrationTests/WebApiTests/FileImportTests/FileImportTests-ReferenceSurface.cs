using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests_ReferenceSurface : WebApiTestsBase
  {
    [Theory]
    [InlineData("api/v6/importedfile", "api/v6/importedfile/referencesurface")]
    [InlineData("api/v6/importedfile/direct", "api/v6/importedfile/referencesurface")]
    public async Task TestImportReferenceSurfaceFile(string uriRoot1, string uriRoot2)
    {
      const string testText = "File Import ref test 1";
      Msg.Title(testText, "Create standard project and customer then upload reference surface file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid      | CustomerUid   | Name         | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFileParent.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid      | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile", "api/v6/importedfile/referencesurface")]
    [InlineData("api/v6/importedfile/direct", "api/v6/importedfile/referencesurface")]
    public async Task TestImportReferenceSurfaceFileFromDeactivatedDesign(string uriRoot1, string uriRoot2)
    {
      const string testText = "File Import ref test 2";
      Msg.Title(testText, "Create standard project and customer then upload reference surface file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name            | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFileParent.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Deactivate the parent design
      await FileActivationTests.DoActivationRequest(customerUid, ts.ProjectUid.ToString(), filesResult.ImportedFileDescriptor.ImportedFileUid, false, HttpStatusCode.OK, 200, "Success");

      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      //Expect the reference surface to be deactivated
      importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor.IsActivated = false;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile", "api/v6/importedfile/referencesurface")]
    [InlineData("api/v6/importedfile/direct", "api/v6/importedfile/referencesurface")]
    public async Task TestImport2ReferenceSurfaceFiles(string uriRoot1, string uriRoot2)
    {
      const string testText = "File Import ref test 3";
      Msg.Title(testText, "Create standard project and customer then upload two Reference surface files");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name          | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult1 = await importFileParent.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      var expectedResult1 = importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult1.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surfaces
      var parentUid = filesResult1.ImportedFileDescriptor.ImportedFileUid;
      var offset1 = 1.5;
      var offset2 = -2.5;
      parentName = Path.GetFileNameWithoutExtension(parentName);
      var name1 = $"{parentName} +{offset1}m";
      var name2 = $"{parentName} {offset2}m";
      var importFileArray2 = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name    | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset    |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name1} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset1} |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name2} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset2} |"};
      var filesResult2 = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name1)}" }));
      var expectedResult2 = importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, expectedResult2, true);

      var filesResult3 = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 2, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name2)}" }));
      var expectedResult3 = importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      ts.CompareTheActualImportFileWithExpected(filesResult3.ImportedFileDescriptor, expectedResult3, true);

      var importFileList = await importFileParent.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);
      Assert.True(importFileList.ImportedFileDescriptors.Count == 3, "Expected 3 imported files but got " + importFileList.ImportedFileDescriptors.Count);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[0], expectedResult1, true);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[1], expectedResult2, true);
      ts.CompareTheActualImportFileWithExpectedV6(importFileList.ImportedFileDescriptors[2], expectedResult3, true);
    }

    [Theory]
    [InlineData("api/v6/importedfile/referencesurface")]
    public async Task TestImportReferenceSurfaceFileWithoutParentShouldFail(string uriRoot)
    {
      const string testText = "File Import ref test 4";
      Msg.Title(testText, "Create standard project and customer then upload reference surface file without parent design uploaded");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      //Reference Surface
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var parentUid = Guid.NewGuid();
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};

      var errorResultObj = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }), HttpStatusCode.BadRequest);
      Assert.Equal("Missing parent design for reference surface", errorResultObj.Message);
    }

    [Theory]
    [InlineData("api/v6/importedfile", "api/v6/importedfile/referencesurface")]
    [InlineData("api/v6/importedfile/direct", "api/v6/importedfile/referencesurface")]
    public async Task TestImportReferenceSurfaceThenDeleteTheParentDesignSurface(string uriRoot1, string uriRoot2)
    {
      const string testText = "File Import ref test 5";
      Msg.Title(testText, "Create standard project then upload a new reference file. Then delete parent design surface file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFileParent.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Delete parent design
      importFileParent.ImportedFileUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var errorResultObj = await importFileParent.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete), HttpStatusCode.BadRequest);
      Assert.Equal("Cannot delete a design that has reference surfaces", errorResultObj.Message);
    }

    [Theory]
    [InlineData("api/v6/importedfile", "api/v6/importedfile/referencesurface")]
    [InlineData("api/v6/importedfile/direct", "api/v6/importedfile/referencesurface")]
    public async Task TestImportReferenceSurfaceFileTwice(string uriRoot1, string uriRoot2)
    {
      const string testText = "File Import ref test 6";
      Msg.Title(testText, "Create standard project and customer then upload reference surface file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var startDateTime = ts.FirstEventDate;
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
       "| EventType            | EventDate   | ProjectName   | ProjectType | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
      $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFileParent = new ImportFile(uriRoot1);
      var importFileChild = new ImportFile(uriRoot2);
      //Parent Design
      var importFilename = TestFileResolver.File(TestFile.TestDesignSurface1);
      var parentName = TestFileResolver.GetFullPath(importFilename);

      var importFileArray = new[] {
         "| EventType              | ProjectUid   | CustomerUid   | Name                                     | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel |",
        $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {parentName} | 1                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           |"};
      var filesResult = await importFileParent.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult.ImportedFileDescriptor, importFileParent.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);
      //Reference Surface
      var parentUid = filesResult.ImportedFileDescriptor.ImportedFileUid;
      var offset = 1.5;
      var name = $"{Path.GetFileNameWithoutExtension(parentName)} +{offset}m";
      var importFileArray2 = new[] {
        "| EventType              | ProjectUid   | CustomerUid   | Name   | ImportedFileType | FileCreatedUtc  | FileUpdatedUtc             | ImportedBy                 | IsActivated | MinZoomLevel | MaxZoomLevel | ParentUid   | Offset   |",
       $"| ImportedFileDescriptor | {ts.ProjectUid} | {customerUid} | {name} | 6                | {startDateTime} | {startDateTime.AddDays(5)} | testProjectMDM@trimble.com | true        | 15           | 19           | {parentUid} | {offset} |"};
      var filesResult2 = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }));
      ts.CompareTheActualImportFileWithExpected(filesResult2.ImportedFileDescriptor, importFileChild.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor, true);

      //Import again
      var errorResultObj = await importFileChild.SendRequestToFileImportV6(ts, importFileArray2, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={HttpUtility.UrlEncode(name)}" }), HttpStatusCode.BadRequest);
      Assert.Equal("Reference surface already exists", errorResultObj.Message);
    }
  }
}
