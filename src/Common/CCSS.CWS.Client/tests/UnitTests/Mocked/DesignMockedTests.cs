using System;
using System.Collections.Generic;
using System.IO;
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

      MockUtilities.TestRequestSendsCorrectJson("Create a file", mockWebRequest, null, expectedUrl, HttpMethod.Post, createFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDesignClient>();
        var result = await client.CreateFile(new Guid(projectUid), createFileRequestModel);

        Assert.NotNull(result);
        Assert.Equal(createFileResponseModel.FileSpaceId, result.FileSpaceId);
        return true;
      });
    }

    [Fact]
    public void CreateAndUploadFileTest()
    {
      string projectUid = Guid.NewGuid().ToString();

      var createFileRequestModel = new CreateFileRequestModel
      {
        FileName = "myFirstProject.dc"
      };
      var uploadUrl = $"{createFileRequestModel.FileName} {projectUid}";
      var createFileResponseModel = new CreateFileResponseModel
      {
        FileSpaceId = Guid.NewGuid().ToString(),
        UploadUrl = uploadUrl
      };
      var fileContents = new byte[] {1,2,3,4,5,6,7,8 };
     
      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/file";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));
      var expectedContent = new StringContent("Dummy");
      mockWebRequest.Setup(s => s.ExecuteRequestAsStreamContent(uploadUrl, HttpMethod.Put, It.IsAny<IDictionary<string, string>>(), It.IsAny<Stream>(), null, 3, false))
        .Returns(Task.FromResult(expectedContent as HttpContent));
     
      MockUtilities.TestRequestSendsCorrectJson("Create and uload a file", mockWebRequest, null, expectedUrl, HttpMethod.Post, createFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDesignClient>();
        using (var ms = new MemoryStream(fileContents))
        {
          var result = await client.CreateAndUploadFile(new Guid(projectUid), createFileRequestModel, ms);
          Assert.NotNull(result);
          Assert.Equal(createFileResponseModel.FileSpaceId, result.FileSpaceId);
        }
        return true;
      });
    }
  }
}
