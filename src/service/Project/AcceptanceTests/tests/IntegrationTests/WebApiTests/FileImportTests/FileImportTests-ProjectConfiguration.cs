﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IntegrationTests.UtilityClasses;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.WebApiTests.FileImportTests
{
  public class FileImportTests_ProjectConfiguration
  {
    [Theory]
    [InlineData("api/v6/importedfile")]
    [InlineData("api/v6/importedfile/direct")]
    public async Task TestNoFileUploads(string uriRoot)
    {
      const string testText = "File Import Project Configuration 1";
      Msg.Title(testText, "Create standard project then get imported files - There should be one file, the coord system file.");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
         "| EventType            | EventDate   | ProjectName   | ProjectType | CoordinateSystem      | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
        $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | BootCampDimensions.dc | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile(uriRoot);
      var expectedResults = ts.ExpectedProjectConfigFileDescriptorsListResult;
      var filesResult = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}&getProjectCalibrationFiles=true", customerUid);

      Assert.True(filesResult.ProjectConfigFileDescriptors.Count == 1, $"Expected 1 config file but got {filesResult.ProjectConfigFileDescriptors.Count}");
      Assert.Equal(expectedResults.ProjectConfigFileDescriptors[0].FileName, filesResult.ProjectConfigFileDescriptors[0].FileName);
    }

    [Theory]
    [InlineData(ImportedFileType.AvoidanceZone)]
    [InlineData(ImportedFileType.ControlPoints)]
    [InlineData(ImportedFileType.Geoid)]
    [InlineData(ImportedFileType.FeatureCode)]
    [InlineData(ImportedFileType.SiteConfiguration)]
    [InlineData(ImportedFileType.GcsCalibration)]
    public async Task TestCreateProjectConfigurationFile(ImportedFileType importedFileType)
    {
      const string testText = "File Import Project Configuration 2";
      Msg.Title(testText, "Create standard project then upload project configuration file");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
         "| EventType            | EventDate   | ProjectName   | ProjectType  | CoordinateSystem      | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
        $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | BootCampDimensions.dc | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile("api/v6/importedfile");
      var importFilename = TestFileResolver.File(GetTestFileNameForImportedFileType(importedFileType));

      var importFileArray = new[] {
         "| EventType                             | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType   |", 
        $"| ProjectConfigurationFileResponseModel | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | {importedFileType} |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" })) ;
      Assert.Equal(importFile.ExpectedImportFileDescriptorSingleResult.ProjectConfigFileDescriptor.FileName, filesResult.ProjectConfigFileDescriptor.FileName);

      if (importedFileType == ImportedFileType.AvoidanceZone || importedFileType == ImportedFileType.ControlPoints)
      {
        // Upload the second file
        var firstFilename = importFile.ExpectedImportFileDescriptorSingleResult.ProjectConfigFileDescriptor.FileName;
        importFile = new ImportFile("api/v6/importedfile");
        importFilename = TestFileResolver.File(GetTestFileNameForImportedFileType(importedFileType, false));

        filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
        Assert.Equal(firstFilename, filesResult.ProjectConfigFileDescriptor.FileName);
        Assert.Equal(importFile.ExpectedImportFileDescriptorSingleResult.ProjectConfigFileDescriptor.FileName, filesResult.ProjectConfigFileDescriptor.SiteCollectorFileName);
      }
    }

    [Theory]
    [InlineData(ImportedFileType.AvoidanceZone)]
    [InlineData(ImportedFileType.ControlPoints)]
    [InlineData(ImportedFileType.Geoid)]
    [InlineData(ImportedFileType.FeatureCode)]
    [InlineData(ImportedFileType.SiteConfiguration)]
    [InlineData(ImportedFileType.GcsCalibration)]
    public async Task TestUpdateProjectConfigurationFile(ImportedFileType importedFileType)
    {
      const string testText = "File Import Project Configuration 3";
      Msg.Title(testText, "Create standard project then upload project configuration file then update");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      var coordSysFileName = "BootCampDimensions.dc";
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
         "| EventType            | EventDate   | ProjectName   | ProjectType  | CoordinateSystem      | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
        $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | {coordSysFileName} | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile("api/v6/importedfile");
      var importFilename = TestFileResolver.File(GetTestFileNameForImportedFileType(importedFileType));

      var importFileArray = new[] {
         "| EventType                             | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType   |",
        $"| ProjectConfigurationFileResponseModel | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | {importedFileType} |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      Assert.Equal(importFile.ExpectedImportFileDescriptorSingleResult.ProjectConfigFileDescriptor.FileName, filesResult.ProjectConfigFileDescriptor.FileName);

      _ = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Put, new[] { $"filename={importFilename}" }));
      var expectedResult2 = importFile.ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor;
      var importFileList = await importFile.GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>($"api/v6/importedfiles?projectUid={ts.ProjectUid}", customerUid);

      Assert.True(importFileList.ProjectConfigFileDescriptors.Count == 2, "Expected 2 config files but got " + importFileList.ImportedFileDescriptors.Count);
      Assert.Equal(coordSysFileName, importFileList.ProjectConfigFileDescriptors[0].FileName);
      Assert.Equal(importFilename, importFileList.ProjectConfigFileDescriptors[1].FileName);
    }

    [Theory]
    [InlineData(ImportedFileType.AvoidanceZone)]
    [InlineData(ImportedFileType.ControlPoints)]
    [InlineData(ImportedFileType.Geoid)]
    [InlineData(ImportedFileType.FeatureCode)]
    [InlineData(ImportedFileType.SiteConfiguration)]
    [InlineData(ImportedFileType.GcsCalibration)]
    public async Task TestDeleteProjectConfigurationFile(ImportedFileType importedFileType)
    {
      const string testText = "File Import Project Configuration 4";
      Msg.Title(testText, "Create standard project, upload project configuration file then delete");
      var ts = new TestSupport();
      var customerUid = Guid.NewGuid();
      ts.IsPublishToWebApi = true;
      var projectEventArray = new[] {
         "| EventType            | EventDate   | ProjectName   | ProjectType  | CoordinateSystem      | ProjectTimezone           | ProjectBoundary          | CustomerUID   |",
        $"| CreateProjectRequest | 0d+09:00:00 | {testText}    | Standard    | BootCampDimensions.dc | New Zealand Standard Time | {Boundaries.Boundary1}   | {customerUid} |" };
      await ts.PublishEventCollection(projectEventArray);

      var importFile = new ImportFile("api/v6/importedfile");
      var importFilename = TestFileResolver.File(GetTestFileNameForImportedFileType(importedFileType));

      var importFileArray = new[] {
         "| EventType                             | ProjectUid      | CustomerUid   | Name                                           | ImportedFileType   |",
        $"| ProjectConfigurationFileResponseModel | {ts.ProjectUid} | {customerUid} | {TestFileResolver.GetFullPath(importFilename)} | {importedFileType} |"};
      var filesResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Post, new[] { $"filename={importFilename}" }));
      Assert.Equal(importFile.ExpectedImportFileDescriptorSingleResult.ProjectConfigFileDescriptor.FileName, filesResult.ProjectConfigFileDescriptor.FileName);

      var deleteResult = await importFile.SendRequestToFileImportV6(ts, importFileArray, 1, new ImportOptions(HttpMethod.Delete));
      Assert.Equal(0, deleteResult.Code);
      Assert.Equal("success", deleteResult.Message);
    }


    private string GetTestFileNameForImportedFileType(ImportedFileType importedFileType, bool first = true)
    {
      string filename = null;
      switch (importedFileType)
      {
        case ImportedFileType.AvoidanceZone:
          filename = first? TestFile.TestAvoidanceZone1: TestFile.TestAvoidanceZone2;
          break;
        case ImportedFileType.ControlPoints:
          filename = first ? TestFile.TestControlPoints1: TestFile.TestControlPoints2;
          break;
        case ImportedFileType.Geoid:
          filename = TestFile.TestGeoid;
          break;
        case ImportedFileType.FeatureCode:
          filename = TestFile.TestFeatureCode;
          break;
        case ImportedFileType.SiteConfiguration:
          filename = TestFile.TestSiteConfiguration;
          break;
        case ImportedFileType.GcsCalibration:
          filename = TestFile.TestGcsCalibration;
          break;
      }
      return filename;
    }
  }

  
}
