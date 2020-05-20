using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.TagFileGateway.Common.Executors;
using VSS.Productivity3D.TagFileGateway.Common.Models.Sns;
using Xunit;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  public class TagFileSnsExecutorTests : ExecutorBaseFixture
  {
    private static CompactionTagFileRequest MockRequest =
      new CompactionTagFileRequest
      {
        ProjectId = 554,
        ProjectUid = Guid.NewGuid(),
        FileName = "Machine Name--whatever--161230235959.tag",
        Data = new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9 },
        OrgId = string.Empty
      };

    [Fact]
    public void ShouldBeCorrectType()
    {
      var e = CreateExecutor<TagFileSnsProcessExecutor>();

      e.Should().NotBeNull();
      e.Should().BeOfType<TagFileSnsProcessExecutor>();
    }

    [Fact]
    public void ShouldFailOnIncorrectArg()
    {
      var e = CreateExecutor<TagFileSnsProcessExecutor>();

      var result = e.ProcessAsync(new object()).Result;

      result.Should().NotBeNull();
      result.Code.Should().NotBe(0);
    }

    [Fact]
    public void ShouldHandleSubscription()
    {
      var e = CreateExecutor<TagFileSnsProcessExecutor>();

      // Subscription payloads are only logged, and only sent when we first subscribe to the SNS topic
      // There is no Tag File included
      var payLoad = new SnsPayload()
      {
        Type = SnsPayload.SubscriptionType,
        TopicArn = "TestArn"
        
      };

      var result = e.ProcessAsync(payLoad).Result;

      result.Should().NotBeNull();
      result.Code.Should().Be(0);
    }

    [Fact]
    public void ShouldDownloadDataWhenNeeded()
    {
      // When a SNS message comes in, it may contain a URL to download the file content
      // This test checks that, and ensures the data is downloaded and sent as a tag file correctly
      var e = CreateExecutor<TagFileSnsProcessExecutor>();
      var testUrl = "http://not-a-real-host/tag";
      CompactionTagFileRequest receivedTagFile = null;

      var payLoad = new SnsPayload()
      {
        Type = SnsPayload.NotificationType,
        TopicArn = "TestArn",
        Message = JsonConvert.SerializeObject(new SnsTagFile()
        {
          DownloadUrl = testUrl,
          FileName = "test-filename",
          FileSize = MockRequest.Data.Length
        })
      };

      WebRequest.Setup(m => m.ExecuteRequestAsStreamContent(It.IsAny<string>(),
          It.IsAny<HttpMethod>(),
          It.IsAny<IHeaderDictionary>(),
          It.IsAny<Stream>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()))
        .Returns(Task.FromResult<HttpContent>(new ByteArrayContent(MockRequest.Data.ToArray())));

      // Ensure the tag file will be upload and save the response
      TagFileForwarder
        .Setup(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
          It.IsAny<IHeaderDictionary>()))
        .Callback<CompactionTagFileRequest, IHeaderDictionary>((tagFileRequest, _) => receivedTagFile = tagFileRequest)
        .Returns(Task.FromResult(new ContractExecutionResult()));

      // Handle the upload
      TransferProxy.Setup(m => m.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      var result = e.ProcessAsync(payLoad).Result;

      // Validate
      WebRequest.Verify(m => m.ExecuteRequestAsStreamContent(It.Is<string>(m => m == testUrl),
          It.Is<HttpMethod>(m => m == HttpMethod.Get),
          It.IsAny<IHeaderDictionary>(),
          It.IsAny<Stream>(),
          It.IsAny<int?>(),
          It.IsAny<int>(),
          It.IsAny<bool>()), 
        Times.Once);

      TagFileForwarder
        .Verify(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
            It.IsAny<IHeaderDictionary>()),
          Times.Once);
      

      // Validate result and data
      result.Should().NotBeNull();
      result.Code.Should().Be(0);

      receivedTagFile.Should().NotBeNull();
      receivedTagFile.Data.Should().BeEquivalentTo(MockRequest.Data);
      receivedTagFile.FileName.Should().Be("test-filename");

    }

     [Fact]
    public void ShouldForwardTagfile()
    {
      // When a SNS message comes in with data, it should be mapped to a Tag File Request and processed
      var e = CreateExecutor<TagFileSnsProcessExecutor>();
      CompactionTagFileRequest receivedTagFile = null;

      var payLoad = new SnsPayload()
      {
        Type = SnsPayload.NotificationType,
        TopicArn = "TestArn",
        Message = JsonConvert.SerializeObject(new SnsTagFile()
        {
          Data = MockRequest.Data,
          FileName = "test-filename-no-download",
          FileSize = MockRequest.Data.Length
        })
      };

    // Ensure the tag file will be upload and save the response
      TagFileForwarder
        .Setup(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
          It.IsAny<IHeaderDictionary>()))
        .Callback<CompactionTagFileRequest, IHeaderDictionary>((tagFileRequest, _) => receivedTagFile = tagFileRequest)
        .Returns(Task.FromResult(new ContractExecutionResult()));

      // Handle the upload
      TransferProxy.Setup(m => m.Upload(It.IsAny<Stream>(), It.IsAny<string>()));

      var result = e.ProcessAsync(payLoad).Result;

      // Validate
      TagFileForwarder
        .Verify(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
            It.IsAny<IHeaderDictionary>()),
          Times.Once);
      

      // Validate result and data
      result.Should().NotBeNull();
      result.Code.Should().Be(0);

      receivedTagFile.Should().NotBeNull();
      receivedTagFile.Data.Should().BeEquivalentTo(MockRequest.Data);
      receivedTagFile.FileName.Should().Be("test-filename-no-download");

    }
  }
}
