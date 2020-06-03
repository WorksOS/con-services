using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS;
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
      mockWebRequest.Setup(s => s.ExecuteRequestAsStreamContent(uploadUrl, HttpMethod.Put, It.IsAny<IHeaderDictionary>(), It.IsAny<Stream>(), null, 3, false))
        .Returns(Task.FromResult(expectedContent as HttpContent));
     
      MockUtilities.TestRequestSendsCorrectJson("Create and upload a file", mockWebRequest, null, expectedUrl, HttpMethod.Post, createFileResponseModel, async () =>
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

    [Fact]
    public void GetAndDownloadFileTest()
    {
      string projectUid = Guid.NewGuid().ToString();
      string filespaceId = Guid.NewGuid().ToString();

      var fileName = "trn::profilex:us-west-2:project:2092b1a9-e4d6-41e5-b210-b8fff3e922da||2020-04-20 23:30:28.253||BootCamp 2012.dc";
      var downloadUrl = "https://fs-ro-us1.staging-tdata-cdn.com/r/af390a82-8cc2-4486-aba8-e66a2dcfa3f8?Signature=eVLwMzTwyAlUg~ClgMu2V1BD0QqtwiNDHD~323QfKZw5bEYHs329k2E2fwbarld3HhhoV9xuBFuom6YHGfd7Tlj4j9nFC~8vl4bh0oFsuZF0DsVG0PBKWeQmOnWGvw-HbyRYqstJa5QybeGT1B8JnJG9ApMmBUkC0Myb2nTTbirCgz1mHZ2~kSPe8gqY5WNH~1pRXhB7NeEdYr76~rVr5zlwMcesKoSxPhKVuwBDy5P7rtY-NfbHg5-bSB703bvDCdANrZAw4zTItg0Z9fsa~YiSdKyaaaetPc9PkY7Wkbo048VWXiyM3yRAM0jamN4txTTQjPs3WcpTBqRWxz-mEw__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvYWYzOTBhODItOGNjMi00NDg2LWFiYTgtZTY2YTJkY2ZhM2Y4IiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTg3NTA3NzY1fX19XX0_&Key-Pair-Id=APKAJ4FHA7WZOWHG4EOQ";
      var getFileResponseModel = new GetFileResponseModel
      {
        FileSpaceId = filespaceId,
        FileName = fileName,
        DownloadUrl = downloadUrl
      };
      var fileContents = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

      string route = $"/projects/{TRNHelper.MakeTRN(projectUid)}/file/{filespaceId}";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));
      var expectedContent = new ByteArrayContent(fileContents);
      mockWebRequest.Setup(s => s.ExecuteRequestAsStreamContent(downloadUrl, HttpMethod.Get, It.IsAny<IHeaderDictionary>(), null, null, 3, false))
        .Returns(Task.FromResult(expectedContent as HttpContent));

      MockUtilities.TestRequestSendsCorrectJson("Get and download a file", mockWebRequest, null, expectedUrl, HttpMethod.Get, getFileResponseModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsDesignClient>();
        using (var ms = new MemoryStream(fileContents))
        {
          var result = await client.GetAndDownloadFile(new Guid(projectUid), filespaceId);
          Assert.NotNull(result);
          Assert.Equal(getFileResponseModel.FileSpaceId, result.FileSpaceId);
          Assert.Equal(getFileResponseModel.FileName, result.FileName);
          Assert.Equal(getFileResponseModel.FileSpaceId, result.FileSpaceId);
          Assert.Equal(fileContents, result.FileContents);
        }
        return true;
      });
    }
  }
}
