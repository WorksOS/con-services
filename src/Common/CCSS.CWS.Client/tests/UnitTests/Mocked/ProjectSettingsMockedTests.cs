using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class ProjectSettingsMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsProfileSettingsClient, CwsProfileSettingsClient>();

      return services;
    }

    [Fact]
    public void GetProjectConfiguration()
    {
      var projectUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };
      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/configuration/CALIBRATION";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get a project calibration file", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectConfigurationFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProfileSettingsClient>();
        var result = await client.GetProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION);

        Assert.NotNull(result);
        Assert.Equal(projectConfigurationFileResponseModel.FileName, result.FileName);
        return true;
      });
    }

    [Fact]
    public void GetProjectConfigurations()
    {
      var projectUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
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
            Size = "66"
          }
        }
      };
      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/configuration";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get a project calibration file", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectConfigurationFileListResponse, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProfileSettingsClient>();
        var result = await client.GetProjectConfigurations(projectUid);

        Assert.NotNull(result);
        Assert.Equal(2, projectConfigurationFileListResponse.ProjectConfigurationFiles.Count);
        Assert.Equal(projectConfigurationFileListResponse.ProjectConfigurationFiles[0].FileName, result.ProjectConfigurationFiles[0].FileName);
        Assert.Equal(projectConfigurationFileListResponse.ProjectConfigurationFiles[0].FileType, result.ProjectConfigurationFiles[0].FileType);
        Assert.Equal(projectConfigurationFileListResponse.ProjectConfigurationFiles[1].FileType, result.ProjectConfigurationFiles[1].FileType);
        return true;
      });
    }


    [Fact]
    public void SaveProjectConfiguration()
    {
      var projectUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var projectConfigurationFileRequestModel = new ProjectConfigurationFileRequestModel
      {
        MachineControlFilespaceId = Guid.NewGuid().ToString()
      };

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };

      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/configuration/CALIBRATION";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Create a project calibration file", mockWebRequest, null, expectedUrl, HttpMethod.Post, projectConfigurationFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProfileSettingsClient>();
        var result = await client.SaveProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION, projectConfigurationFileRequestModel);

        Assert.NotNull(result);
        Assert.Equal(projectConfigurationFileResponseModel.FileName, result.FileName);
        return true;
      });
    }

    [Fact]
    public void UpdateProjectConfiguration()
    {
      var projectUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var projectConfigurationFileRequestModel = new ProjectConfigurationFileRequestModel
      {
        MachineControlFilespaceId = Guid.NewGuid().ToString()
      };

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };

      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/configuration/CALIBRATION";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("update a project calibration file", mockWebRequest, null, expectedUrl, HttpMethod.Put, projectConfigurationFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProfileSettingsClient>();
        var result = await client.UpdateProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION, projectConfigurationFileRequestModel);

        Assert.NotNull(result);
        Assert.Equal(projectConfigurationFileResponseModel.FileName, result.FileName);
        return true;
      });
    }

    [Fact]
    public void DeleteProjectConfiguration()
    {
      var projectUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var projectConfigurationFileRequestModel = new ProjectConfigurationFileRequestModel
      {
        MachineControlFilespaceId = Guid.NewGuid().ToString()
      };     

      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/configuration/CALIBRATION";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Delete a project calibration file", mockWebRequest, null, expectedUrl, HttpMethod.Delete, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProfileSettingsClient>();
        await client.DeleteProjectConfiguration(projectUid, ProjectConfigurationFileType.CALIBRATION);
        return true;
      });
    }

  }
}
