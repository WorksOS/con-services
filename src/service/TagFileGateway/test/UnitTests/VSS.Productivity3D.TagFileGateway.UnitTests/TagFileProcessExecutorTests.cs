using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.TagFileGateway.Common.Executors;
using Xunit;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  public class TagFileProcessExecutorTests : ExecutorBaseFixture
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
      var e = CreateExecutor<TagFileProcessExecutor>();

      e.Should().NotBeNull();
      e.Should().BeOfType<TagFileProcessExecutor>();
    }

    [Fact]
    public void ShouldFailOnIncorrectArg()
    {
      var e = CreateExecutor<TagFileProcessExecutor>();

      var result = e.ProcessAsync(new object()).Result;

      result.Should().NotBeNull();
      result.Code.Should().NotBe(0);
    }

    [Fact]
    public void ShouldUploadWhenTagFileForwarderThrowsException()
    {
      // This simulates a situation when TagFileForwarder cant connect to TRex
      // We want to upload the tag file to S3, but return an error to the caller
      var executor = CreateExecutor<TagFileProcessExecutor>();

      var key = TagFileProcessExecutor.GetS3Key(MockRequest.FileName);
      var expectedS3Path = $"{TagFileProcessExecutor.CONNECTION_ERROR_FOLDER}/{key}";
      var uploadedData = new List<byte>();

      // Setup a failed connection
      TagFileForwarder
        .Setup(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
          It.IsAny<IHeaderDictionary>()))
        .Throws<HttpRequestException>();

      // Handle the upload, and save the data for validation
      TransferProxy.Setup(m => m.Upload(It.IsAny<Stream>(), It.IsAny<string>()))
        .Callback<Stream, string>((stream, path) => { uploadedData.AddRange(((MemoryStream)stream).ToArray()); });

      // Run the test
      var result = executor.ProcessAsync(MockRequest).Result;

      // Validate we tried to upload
      TagFileForwarder
        .Verify(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
            It.IsAny<IHeaderDictionary>()),
          Times.Exactly(1));

      // Validate that the path was correct (we check the data separately)
      TransferProxy.Verify(m => m.Upload(It.IsAny<MemoryStream>(), It.Is<string>(s => s == expectedS3Path)), Times.Once);

      // Validate the data
      uploadedData.Should().BeEquivalentTo(MockRequest.Data);

      // Validate we got a non-zero result
      result.Code.Should().NotBe(0);
    }

    [Fact]
    public void ShouldUploadWhenTagFileForwarderFails()
    {
      var executor = CreateExecutor<TagFileProcessExecutor>();

      var key = TagFileProcessExecutor.GetS3Key(MockRequest.FileName);
      var expectedS3Path = $"{key}";
      var uploadedData = new List<byte>();
      var expectedErrorCode = 55; // Executor should forward on the error code when tag file forwarder returns an error
      // Setup a failed connection
      TagFileForwarder
        .Setup(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
          It.IsAny<IHeaderDictionary>()))
        .Returns(Task.FromResult(new ContractExecutionResult(expectedErrorCode)));

      // Handle the upload, and save the data for validation
      TransferProxy.Setup(m => m.Upload(It.IsAny<Stream>(), It.IsAny<string>()))
        .Callback<Stream, string>((stream, path) => { uploadedData.AddRange(((MemoryStream)stream).ToArray()); });

      // Run the test
      var result = executor.ProcessAsync(MockRequest).Result;

      // Validate we tried to upload
      TagFileForwarder
        .Verify(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
            It.IsAny<IHeaderDictionary>()),
          Times.Exactly(1));

      // Validate that the path was correct (we check the data separately)
      TransferProxy.Verify(m => m.Upload(It.IsAny<MemoryStream>(), It.Is<string>(s => s == expectedS3Path)), Times.Once);

      // Validate the data
      uploadedData.Should().BeEquivalentTo(MockRequest.Data);

      // Validate we got a non-zero result
      result.Code.Should().Be(expectedErrorCode);
    }

    [Fact]
    public void ShouldUploadWhenTagFileForwarderPasses()
    {
      var executor = CreateExecutor<TagFileProcessExecutor>();

      var key = TagFileProcessExecutor.GetS3Key(MockRequest.FileName);
      var expectedS3Path = $"{key}";
      var uploadedData = new List<byte>();

      // Setup a failed connection
      TagFileForwarder
        .Setup(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
          It.IsAny<IHeaderDictionary>()))
        .Returns(Task.FromResult(new ContractExecutionResult(0)));

      // Handle the upload, and save the data for validation
      TransferProxy.Setup(m => m.Upload(It.IsAny<Stream>(), It.IsAny<string>()))
        .Callback<Stream, string>((stream, path) => { uploadedData.AddRange(((MemoryStream)stream).ToArray()); });

      // Run the test
      var result = executor.ProcessAsync(MockRequest).Result;

      // Validate we tried to upload
      TagFileForwarder
        .Verify(m => m.SendTagFileDirect(It.IsAny<CompactionTagFileRequest>(),
            It.IsAny<IHeaderDictionary>()),
          Times.Exactly(1));

      // Validate that the path was correct (we check the data separately)
      TransferProxy.Verify(m => m.Upload(It.IsAny<MemoryStream>(), It.Is<string>(s => s == expectedS3Path)), Times.Once);

      // Validate the data
      uploadedData.Should().BeEquivalentTo(MockRequest.Data);

      // Validate we got a non-zero result
      result.Code.Should().Be(0);
    }

  }
}
