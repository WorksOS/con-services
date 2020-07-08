using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jose.native;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class ProjectMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsProjectClient, CwsProjectClient>();

      return services;
    }

    [Fact]
    public void CreateProjectTest()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var expectedProjectUid = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      var expectedProjectTrn = TRNHelper.MakeTRN(expectedProjectUid);

      var createProjectRequestModel = new CreateProjectRequestModel
      {
        AccountId = customerUid.ToString(),
        ProjectName = "my first project",
        Timezone = "Mountain Standard Time",
        Boundary = new ProjectBoundary()
        {
          type = "Polygon",
          coordinates = new List<List<double[]>> { new List<double[]> { new double[] { 150.3, 1.2 }, new double[] { 150.4, 1.2 }, new double[] { 150.4, 1.3 }, new double[] { 150.4, 1.4 }, new double[] { 150.3, 1.2 } } }
        }
      };

      var createProjectResponseModel = new CreateProjectResponseModel
      {
        TRN = expectedProjectTrn
      };
      const string route = "/projects";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Create a project", mockWebRequest, null, expectedUrl, HttpMethod.Post, createProjectResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.CreateProject(createProjectRequestModel);

        Assert.NotNull(result);
        Assert.Equal(expectedProjectUid, result.Id);
        return true;
      });
    }

    [Fact]
    public void GetProjectsForMyAccount()
    {
      var customerUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var projectSummaryListResponseModel = new ProjectSummaryListResponseModel();
      projectSummaryListResponseModel.Projects.Add(new ProjectSummaryResponseModel() {ProjectTRN = TRNHelper.MakeTRN(projectUid), UserProjectRole = UserProjectRoleEnum.Admin}
      );
      var route = $"/accounts/{TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT)}/projects";
      var expectedUrl = $"{baseUrl}{route}?from=0&limit=20";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get projects for account", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectSummaryListResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.GetProjectsForMyCustomer(customerUid, userUid);

        Assert.NotNull(result);
        Assert.Single(result.Projects);
        Assert.Equal(projectUid.ToString(), result.Projects[0].ProjectId);
        return true;
      });
    }

    [Fact]
    public void GetMyProject()
    {
      var customerUid = Guid.NewGuid();
      var userUid = Guid.NewGuid();
      var projectUid = Guid.NewGuid();

      var projectDetailResponseModel = new ProjectDetailResponseModel()
      {
        AccountTRN = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT),
        ProjectTRN = TRNHelper.MakeTRN(projectUid),
        ProjectSettings = new ProjectSettingsModel()
        {
          ProjectTRN = TRNHelper.MakeTRN(projectUid),
          TimeZone = "Pacific/Auckland",
          Boundary = new ProjectBoundary()
          {
            type = "Polygon",
            coordinates = new List<List<double[]>> { new List<double[]> { new double[] { 150.3, 1.2 }, new double[] { 150.4, 1.2 }, new double[] { 150.4, 1.3 }, new double[] { 150.4, 1.4 }, new double[] { 150.3, 1.2 } } }
          },
          Config = new List<ProjectConfigurationModel>() { new ProjectConfigurationModel() { FileType = ProjectConfigurationFileType.CALIBRATION.ToString() } } 
        }
      };
      
      var route = $"/projects/{TRNHelper.MakeTRN(projectUid)}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get my project", mockWebRequest, null, expectedUrl, HttpMethod.Get, projectDetailResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = await client.GetMyProject(projectUid, userUid);

        Assert.NotNull(result);
        Assert.Equal(customerUid.ToString(), result.AccountId);
        Assert.Equal(projectUid.ToString(), result.ProjectId);
        Assert.Equal(projectUid.ToString(), result.ProjectSettings.ProjectId);
        Assert.Equal("Polygon", result.ProjectSettings.Boundary.type);
        Assert.Equal(ProjectConfigurationFileType.CALIBRATION.ToString(), result.ProjectSettings.Config[0].FileType);
        return true;
      });
    }


    [Fact]
    public void UpdateProjectDetailsTest()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      //var accountTrn = "trn::profilex:us-west-2:account:{customerUid}";
      string projectUid = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      string projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);

      var updateProjectDetailsRequestModel = new UpdateProjectDetailsRequestModel
      {
        projectName = "my updated project"
      };

      string route = $"/projects/{projectTrn}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Update a projects details", mockWebRequest, null, expectedUrl, HttpMethod.Post, updateProjectDetailsRequestModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        await client.UpdateProjectDetails(new Guid(projectUid), updateProjectDetailsRequestModel);

        return true;
      });
    }

    [Fact]
    public void UpdateProjectBoundaryTest()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      //var accountTrn = "trn::profilex:us-west-2:account:{customerUid}";
      string projectUid = "560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      string projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);

      var projectBoundary = new ProjectBoundary()
      {
        type = "Polygon",
        coordinates = new List<List<double[]>> { new List<double[]> { new [] { 151.3, 1.2 }, new[] { 151.4, 1.2 }, new[] { 151.4, 1.3 }, new[] { 151.4, 1.4 }, new[] { 151.3, 1.2 } } }
      };

      string route = $"/projects/{projectTrn}/boundary";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Update a projects boundary", mockWebRequest, null, expectedUrl, HttpMethod.Post, projectBoundary, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        await client.UpdateProjectBoundary(new Guid(projectUid), projectBoundary);

        return true;
      });
    }

    [Fact]
    public void GetProjectSetsCorrectQueryParameters()
    {
      var customerUid = new Guid("560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97");
      var userUid = new Guid("d5842a67-208a-40f5-b51d-f533ca740929");
      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      var route = $"/accounts/{accountTrn}/projects";
      var expectedUrl = $"{baseUrl}{route}";


      // When we pass the values to the API Client, we expect them to be passed as query params EXACTLY
      // Each of these test cases represent these values
      // Null (not provided) options leave the values off, and get the CWS Default
      var testCases = new List<(bool includeSettings, CwsProjectType? projectType, ProjectStatus? status, List<KeyValuePair<string, string>> queryParamValues)>();
      testCases.Add((true, CwsProjectType.AcceptsTagFiles, ProjectStatus.Active, new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("includeSettings", true.ToString()), new KeyValuePair<string, string>("status", "ACTIVE"), new KeyValuePair<string, string>("projectType", "1")
      }));

      testCases.Add((true, CwsProjectType.Standard, ProjectStatus.Active, new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("includeSettings", true.ToString()), new KeyValuePair<string, string>("status", "ACTIVE"), new KeyValuePair<string, string>("projectType", "0")
      }));

      testCases.Add((true, CwsProjectType.Standard, ProjectStatus.Archived, new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("includeSettings", true.ToString()), new KeyValuePair<string, string>("status", "ARCHIVED"), new KeyValuePair<string, string>("projectType", "0")
      }));

      testCases.Add((true, null, null, new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("includeSettings", true.ToString())
      }));

      foreach (var (includeSettings, projectType, status, expectedQueryParams) in testCases)
      {
        mockServiceResolution.Reset();
        mockWebRequest.Reset();
        IList<KeyValuePair<string, string>> passedQueryParams = null;
      
        mockServiceResolution
          .Setup(m => m.ResolveRemoteServiceEndpoint(It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>()))
          .Callback<string, ApiType, ApiVersion, string, IList<KeyValuePair<string, string>>>((serviceName, apiType, apiVersion, r, p) => { passedQueryParams = p; })
          .Returns(Task.FromResult(expectedUrl));

        // Setup so that the second call returns the second model and any follow up calls throw an exception
        mockWebRequest.Setup(s => s.ExecuteRequest<ProjectSummaryListResponseModel>(It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<IHeaderDictionary>(),
            It.IsAny<HttpMethod>(),
            It.IsAny<int?>(),
            It.IsAny<int>(),
            It.IsAny<bool>()))
          .Returns(Task.FromResult(new ProjectSummaryListResponseModel()));

        var client = ServiceProvider.GetRequiredService<ICwsProjectClient>();
        var result = client.GetProjectsForCustomer(customerUid, userUid, includeSettings, projectType, status).Result;

        Assert.NotNull(result);
        foreach (var (expectedKey, expectedValue) in expectedQueryParams)
        {
          // Each query param should be there with the correct value
          var (passedKey, passedValue) = passedQueryParams.SingleOrDefault(q => q.Key == expectedKey);
          Assert.Equal(expectedKey, passedKey);
          Assert.Equal(expectedValue, passedValue);
        }
      }
    }
  }
}
