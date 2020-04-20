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
  public class DesignMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsDesignClient, CwsDesignClient>();

      return services;
    }

    [Fact]
    public void CreateFileTest()
    {
      string projectUid = Guid.NewGuid().ToString();

      var createFileRequestModel = new CreateFileRequestModel
      {
        FileName = "myFirstProject.dc"
      };

      var createFileResponseModel = new CreateFileResponseModel
      {
        FileSpaceId = Guid.NewGuid().ToString(),
        UploadUrl = $"{createFileRequestModel.FileName} {projectUid}"
      };
      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/file";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Create a project", mockWebRequest, null, expectedUrl, HttpMethod.Post, createFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDesignClient>();
        var result = await client.CreateFile(new Guid(projectUid), createFileRequestModel);

        Assert.NotNull(result);
        Assert.Equal(createFileResponseModel.FileSpaceId, result.FileSpaceId);
        return true;
      });
    }
  }
}
