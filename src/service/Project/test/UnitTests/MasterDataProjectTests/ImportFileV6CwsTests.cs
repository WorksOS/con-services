using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.FlowJSHandler;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Controllers;
using VSS.Pegasus.Client;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class ImportFileV6CwsTests : UnitTestsDIFixture<ImportFileV6CwsTests>
  {
    [Fact]
    public async Task GetCwsImportedFiles_HappyPath()
    {
      var projectUid = Guid.NewGuid();
      var customHeaders = new Dictionary<string, string>();

      var projectConfigurationFileListResponse = new ProjectConfigurationFileListResponseModel
      {
        ProjectConfigurationFiles = new List<ProjectConfigurationFileResponseModel>()
        {
          new ProjectConfigurationFileResponseModel()
          {
            FileName = "MyTestFilename.dc",
            FileDownloadLink = "http//whatever",
            FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
            CreatedAt = DateTime.UtcNow.ToString(),
            UpdatedAt = DateTime.UtcNow.ToString(),
            Size = "66"
          },
          new ProjectConfigurationFileResponseModel()
          {
            FileName = "MyTestFilename.avoid.dxf",
            FileDownloadLink = "http//whateverElse",
            FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString(),
            CreatedAt = DateTime.UtcNow.ToString(),
            UpdatedAt = DateTime.UtcNow.ToString(),
            Size = "99"
          }
        }
      };
      var mockCwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      mockCwsProfileSettingsClient.Setup(ps => ps.GetProjectConfigurations(projectUid, customHeaders))
        .ReturnsAsync(projectConfigurationFileListResponse);
      ServiceCollection
        .AddSingleton(mockCwsProfileSettingsClient.Object);
      ServiceProvider = ServiceCollection.BuildServiceProvider();

      var controller = CreateFileImportV6Controller();
      var result = await controller.GetImportedFilesV6(projectUid.ToString(), true);
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Null(result.ImportedFileDescriptors);
      Assert.NotNull(result.ProjectConfigFileDescriptors);
      Assert.Equal(projectConfigurationFileListResponse.ProjectConfigurationFiles, result.ProjectConfigFileDescriptors);
    }

    [Fact]
    public async Task DeleteCwsImportedFile_HappyPath_OneFile()
    {
      var projectUid = Guid.NewGuid();
      var customHeaders = new Dictionary<string, string>();

      var project = new Productivity3D.Project.Abstractions.Models.DatabaseModels.Project() { CustomerUID = Guid.NewGuid().ToString(), ProjectUID = projectUid.ToString(), ShortRaptorProjectId = 999 };
      var projectList = new List<Productivity3D.Project.Abstractions.Models.DatabaseModels.Project> { project };
      var mockProjectRepo = new Mock<IProjectRepository>();
      mockProjectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      //Remove the real project repo set up in base class and replace with mock
      var serviceDescriptor = ServiceCollection.First(s => s.ServiceType == typeof(IProjectRepository));
      ServiceCollection.Remove(serviceDescriptor);
      ServiceCollection.AddSingleton(mockProjectRepo.Object);

      var mockCwsDesignClient = new Mock<ICwsDesignClient>();
      ServiceCollection
        .AddSingleton(mockCwsDesignClient.Object);

      var filename = "MyTestFilename.avoid.svl";
      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = filename,
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };
      var mockCwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      mockCwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(projectUid, ProjectConfigurationFileType.AVOIDANCE_ZONE, customHeaders))
        .ReturnsAsync(projectConfigurationFileResponseModel);
      mockCwsProfileSettingsClient.Setup(ps => ps.DeleteProjectConfiguration(projectUid, ProjectConfigurationFileType.AVOIDANCE_ZONE, customHeaders))
        .Returns(Task.CompletedTask);
      ServiceCollection
        .AddSingleton(mockCwsProfileSettingsClient.Object);
      ServiceProvider = ServiceCollection.BuildServiceProvider();

      var mockPegasusClient = new Mock<IPegasusClient>();
      var mockWebClient = new Mock<IWebClientWrapper>();
      var controller = CreateFileImportV6Controller();
      var result = await controller.DeleteImportedFileV6(projectUid, null, ImportedFileType.AvoidanceZone, filename, mockPegasusClient.Object, mockWebClient.Object);
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Fact]
    public async Task DeleteCwsImportedFile_HappyPath_OneOfTwoFiles()
    {
      var projectUid = Guid.NewGuid();
      var customHeaders = new Dictionary<string, string>();

      var project = new Productivity3D.Project.Abstractions.Models.DatabaseModels.Project() { CustomerUID = Guid.NewGuid().ToString(), ProjectUID = projectUid.ToString(), ShortRaptorProjectId = 999 };
      var projectList = new List<Productivity3D.Project.Abstractions.Models.DatabaseModels.Project> { project };
      var mockProjectRepo = new Mock<IProjectRepository>();
      mockProjectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      //Remove the real project repo set up in base class and replace with mock
      var serviceDescriptor = ServiceCollection.First(s => s.ServiceType == typeof(IProjectRepository));
      ServiceCollection.Remove(serviceDescriptor);
      ServiceCollection.AddSingleton(mockProjectRepo.Object);

      var createFileResponseModel = new CreateFileResponseModel
        { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl = "an upload url" };
      var mockCwsDesignClient = new Mock<ICwsDesignClient>();
      mockCwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), customHeaders))
        .ReturnsAsync(createFileResponseModel);
      ServiceCollection
        .AddSingleton(mockCwsDesignClient.Object);

      var filename1 = "MyTestFilename.avoid.svl";
      var filename2 = "MyTestFilename.avoid.dxf";
      var projectConfigurationFileResponseModel1 = new ProjectConfigurationFileResponseModel
      {
        FileName = filename1,
        FileDownloadLink = "http//whatever",
        SiteCollectorFileName = filename2,
        SiteCollectorFileDownloadLink = "http//whateverelse",
        FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };
      var projectConfigurationFileResponseModel2 = new ProjectConfigurationFileResponseModel
      {
        SiteCollectorFileName = filename2,
        SiteCollectorFileDownloadLink = "http//whateverelse",
        FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "99"
      };
      var mockCwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      mockCwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(projectUid, ProjectConfigurationFileType.AVOIDANCE_ZONE, customHeaders))
        .ReturnsAsync(projectConfigurationFileResponseModel1);
      mockCwsProfileSettingsClient.Setup(ps => ps.DeleteProjectConfiguration(projectUid, ProjectConfigurationFileType.AVOIDANCE_ZONE, customHeaders))
        .Returns(Task.CompletedTask);
      var request = new ProjectConfigurationFileRequestModel{SiteCollectorFilespaceId = Guid.NewGuid().ToString()};
      mockCwsProfileSettingsClient.Setup(ps => ps.SaveProjectConfiguration(projectUid, ProjectConfigurationFileType.AVOIDANCE_ZONE, request, customHeaders))
        .ReturnsAsync(projectConfigurationFileResponseModel2);
      ServiceCollection
        .AddSingleton(mockCwsProfileSettingsClient.Object);
      ServiceProvider = ServiceCollection.BuildServiceProvider();

      var mockPegasusClient = new Mock<IPegasusClient>();
      var mockWebClient = new Mock<IWebClientWrapper>();
      mockWebClient.Setup(w => w.DownloadData(It.IsAny<string>())).Returns(new byte[] {1, 2, 3, 4});
      var controller = CreateFileImportV6Controller();
      var result = await controller.DeleteImportedFileV6(projectUid, null, ImportedFileType.AvoidanceZone, filename1, mockPegasusClient.Object, mockWebClient.Object);
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
    }

    [Theory]
    [InlineData(ImportedFileType.Calibration, "MyTestCalibration.dc", ProjectConfigurationFileType.CALIBRATION)]
    [InlineData(ImportedFileType.Calibration, "MyTestCalibration.cal", ProjectConfigurationFileType.CALIBRATION)]
    [InlineData(ImportedFileType.AvoidanceZone, "MyTestAvoidanceZone.svl", ProjectConfigurationFileType.AVOIDANCE_ZONE)]
    [InlineData(ImportedFileType.AvoidanceZone, "MyTestAvoidanceZone.dxf", ProjectConfigurationFileType.AVOIDANCE_ZONE)]
    [InlineData(ImportedFileType.ControlPoints, "MyTestControlPoints.cpz", ProjectConfigurationFileType.CONTROL_POINTS)]
    [InlineData(ImportedFileType.ControlPoints, "MyTestControlPoints.csv", ProjectConfigurationFileType.CONTROL_POINTS)]
    [InlineData(ImportedFileType.Geoid, "MyTestGeoid.ggf", ProjectConfigurationFileType.GEOID)]
    [InlineData(ImportedFileType.FeatureCode, "MyTestFeatureCode.fxl", ProjectConfigurationFileType.FEATURE_CODE)]
    [InlineData(ImportedFileType.SiteConfiguration, "MyTestSiteConfiguration.xml", ProjectConfigurationFileType.SITE_CONFIGURATION)]
    [InlineData(ImportedFileType.GcsCalibration, "MyTestGcsCalibration.cfg", ProjectConfigurationFileType.GCS_CALIBRATION)]
    public async Task CreateCwsImportedFile_HappyPath_SyncUpload(ImportedFileType importedFileType, string filename, ProjectConfigurationFileType fileType)
    {
      var projectUid = Guid.NewGuid();
      var customHeaders = new Dictionary<string, string>();

      var createFileResponseModel = new CreateFileResponseModel
        { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl = "an upload url" };
      var mockCwsDesignClient = new Mock<ICwsDesignClient>();
      mockCwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), customHeaders))
        .ReturnsAsync(createFileResponseModel);
      ServiceCollection
        .AddSingleton(mockCwsDesignClient.Object);

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ?null :  filename,
        FileDownloadLink = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? null : "http//whatever",
        SiteCollectorFileName = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? filename : null,
        SiteCollectorFileDownloadLink = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? "http://whateverelse" :null,
        FileType = fileType.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };
      var mockCwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      mockCwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(projectUid, fileType, customHeaders))
        .ReturnsAsync((ProjectConfigurationFileResponseModel)null);
      mockCwsProfileSettingsClient.Setup(ps => ps.SaveProjectConfiguration(projectUid, fileType, It.IsAny<ProjectConfigurationFileRequestModel>(), customHeaders))
        .ReturnsAsync(projectConfigurationFileResponseModel);
      ServiceCollection
        .AddSingleton(mockCwsProfileSettingsClient.Object);
      ServiceProvider = ServiceCollection.BuildServiceProvider();

      var flowFile = new FlowFile();
      flowFile.flowFilename = filename;
      flowFile.path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Resources/{filename}";

      var controller = CreateFileImportV6Controller();
      var result = await controller.SyncUpload(null, flowFile, projectUid, importedFileType, DxfUnitsType.Meters, DateTime.UtcNow, DateTime.UtcNow, null);
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Null(result.ImportedFileDescriptor);
      Assert.NotNull(result.ProjectConfigFileDescriptor);
      Assert.Equal(projectConfigurationFileResponseModel, result.ProjectConfigFileDescriptor);
    }

    [Theory]
    [InlineData(ImportedFileType.Calibration, "MyTestCalibration.dc", ProjectConfigurationFileType.CALIBRATION)]
    [InlineData(ImportedFileType.Calibration, "MyTestCalibration.cal", ProjectConfigurationFileType.CALIBRATION)]
    [InlineData(ImportedFileType.AvoidanceZone, "MyTestAvoidanceZone.svl", ProjectConfigurationFileType.AVOIDANCE_ZONE)]
    [InlineData(ImportedFileType.AvoidanceZone, "MyTestAvoidanceZone.dxf", ProjectConfigurationFileType.AVOIDANCE_ZONE)]
    [InlineData(ImportedFileType.ControlPoints, "MyTestControlPoints.cpz", ProjectConfigurationFileType.CONTROL_POINTS)]
    [InlineData(ImportedFileType.ControlPoints, "MyTestControlPoints.csv", ProjectConfigurationFileType.CONTROL_POINTS)]
    [InlineData(ImportedFileType.Geoid, "MyTestGeoid.ggf", ProjectConfigurationFileType.GEOID)]
    [InlineData(ImportedFileType.FeatureCode, "MyTestFeatureCode.fxl", ProjectConfigurationFileType.FEATURE_CODE)]
    [InlineData(ImportedFileType.SiteConfiguration, "MyTestSiteConfiguration.xml", ProjectConfigurationFileType.SITE_CONFIGURATION)]
    [InlineData(ImportedFileType.GcsCalibration, "MyTestGcsCalibration.cfg", ProjectConfigurationFileType.GCS_CALIBRATION)]
    public async Task UpdateCwsImportedFile_HappyPath_UpsertImportedFileV6(ImportedFileType importedFileType, string filename, ProjectConfigurationFileType fileType)
    {
      var projectUid = Guid.NewGuid();
      var customHeaders = new Dictionary<string, string>();

      var createFileResponseModel = new CreateFileResponseModel
      { FileSpaceId = "2c171c20-ca7a-45d9-a6d6-744ac39adf9b", UploadUrl = "an upload url" };
      var mockCwsDesignClient = new Mock<ICwsDesignClient>();
      mockCwsDesignClient.Setup(d => d.CreateAndUploadFile(It.IsAny<Guid>(), It.IsAny<CreateFileRequestModel>(), It.IsAny<Stream>(), customHeaders))
        .ReturnsAsync(createFileResponseModel);
      ServiceCollection
        .AddSingleton(mockCwsDesignClient.Object);

      var createProjectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? null : $"{filename}-original",
        FileDownloadLink = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? null : "http//whatever",
        SiteCollectorFileName = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? $"{filename}-original" : null,
        SiteCollectorFileDownloadLink = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? "http://whateverelse" : null,
        FileType = fileType.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };
      var updateProjectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? null : filename,
        FileDownloadLink = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? null : "http//whatever/updated",
        SiteCollectorFileName = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? filename : null,
        SiteCollectorFileDownloadLink = CwsConfigFileHelper.isSiteCollectorType(importedFileType, filename) ? "http://whateverelse/updated" : null,
        FileType = fileType.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "99"
      };
      var mockCwsProfileSettingsClient = new Mock<ICwsProfileSettingsClient>();
      mockCwsProfileSettingsClient.Setup(ps => ps.GetProjectConfiguration(projectUid, fileType, customHeaders))
        .ReturnsAsync(createProjectConfigurationFileResponseModel);
      mockCwsProfileSettingsClient.Setup(ps => ps.UpdateProjectConfiguration(projectUid, fileType, It.IsAny<ProjectConfigurationFileRequestModel>(), customHeaders))
        .ReturnsAsync(updateProjectConfigurationFileResponseModel);
      ServiceCollection
        .AddSingleton(mockCwsProfileSettingsClient.Object);
      ServiceProvider = ServiceCollection.BuildServiceProvider();

      var flowFile = new FlowFile();
      flowFile.flowFilename = filename;
      flowFile.path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Resources/{filename}";

      var controller = CreateFileImportV6Controller();
      var result = await controller.UpsertImportedFileV6(null, flowFile, projectUid, importedFileType, DxfUnitsType.Meters, DateTime.UtcNow, DateTime.UtcNow);
      Assert.NotNull(result);
      Assert.Equal(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code);
      Assert.Equal(ContractExecutionResult.DefaultMessage, result.Message);
      Assert.Null(result.ImportedFileDescriptor);
      Assert.NotNull(result.ProjectConfigFileDescriptor);
      Assert.Equal(updateProjectConfigurationFileResponseModel, result.ProjectConfigFileDescriptor);
    }


    private FileImportV6Controller CreateFileImportV6Controller()
    {
      ServiceCollection.AddSingleton<Func<TransferProxyType, ITransferProxy>>(transfer => TransferProxyMethod);
      var mockNotificationHubClient = new Mock<INotificationHubClient>();
      mockNotificationHubClient.Setup(n => n.Notify(It.IsAny<ProjectChangedNotification>())).Returns(Task.CompletedTask);
      ServiceProvider = ServiceCollection.BuildServiceProvider();

      var httpContext = new DefaultHttpContext();
      httpContext.RequestServices = ServiceProvider;
      httpContext.User = new TIDCustomPrincipal(new GenericIdentity(Guid.NewGuid().ToString()), null, null, "Joe Bloggs", false, null);
      var controllerContext = new ControllerContext();
      controllerContext.HttpContext = httpContext;
      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var persistentTransferProxy = ServiceProvider.GetRequiredService<Func<TransferProxyType, ITransferProxy>>();
      var controller = new FileImportV6Controller(configStore, persistentTransferProxy, null,null,null,mockNotificationHubClient.Object);
      controller.ControllerContext = controllerContext;
      return controller;
    }

    private ITransferProxy TransferProxyMethod(TransferProxyType type)
    {
      return mockTransferProxy;
    }

    private static ITransferProxy mockTransferProxy = new Mock<ITransferProxy>().Object;
  }
}
