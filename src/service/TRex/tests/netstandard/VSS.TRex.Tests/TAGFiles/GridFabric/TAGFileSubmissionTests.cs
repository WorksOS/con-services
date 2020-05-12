using System;
using System.IO;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  [UnitTestCoveredRequest(RequestType = typeof(SubmitTAGFileRequest))]
  public class TAGFileSubmissionTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Test_TAGFileSubmission_Creation()
    {
      SubmitTAGFileRequest submission = new SubmitTAGFileRequest();

      Assert.True(null != submission, "Failed to create SubmitTAGFileRequest instance");
    }

    private byte[] TagFileContent(string tagFileName)
    {
      byte[] tagContent;
      using (FileStream tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", tagFileName),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
      }

      return tagContent;
    }

    private void InjectTAGFileBufferQueueMock(bool result)
    {
      var mockTagFileBufferQueue = new Mock<ITAGFileBufferQueue>();
      mockTagFileBufferQueue.Setup(x => x.Add(It.IsAny<ITAGFileBufferQueueKey>(), It.IsAny<TAGFileBufferQueueItem>())).Returns(result);

      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<ITAGFileBufferQueue>(mockTagFileBufferQueue.Object))
        .Complete();

      IgniteMock.Mutable.AddApplicationGridRouting<IComputeFunc<SubmitTAGFileRequestArgument, SubmitTAGFileResponse>, SubmitTAGFileRequestArgument, SubmitTAGFileResponse>();
    }

    private SubmitTAGFileRequestArgument RequestArgument(Guid assetID, string tagFileName)
    {
      return new SubmitTAGFileRequestArgument
      {
        ProjectID = Guid.NewGuid(),
        AssetID = assetID,
        TagFileContent = TagFileContent(tagFileName),
        TAGFileName = tagFileName,
        TCCOrgID = "",
        TreatAsJohnDoe = false
      };
    }

    [Fact]
    public async Task Test_TAGFileSubmission_SubmitTAGFile_Success()
    {
      InjectTAGFileBufferQueueMock(true);

      var submission = new SubmitTAGFileRequest();
      Assert.True(null != submission, "Failed to create SubmitTAGFileRequest instance");

      string tagFileName = "TestTAGFile-TAGFile-Read-Stream.tag";
      var assetID = Guid.NewGuid();

      var response = await submission.ExecuteAsync(RequestArgument(assetID, tagFileName));

      response.Success.Should().BeTrue($"Response is not successful. Filename={response.FileName}, exception={response.Message}");
      response.Code.Should().Be((int) TRexTagFileResultCode.Valid);
      response.Message.Should().BeEmpty();
    }

    [Fact]
    public async Task Test_TAGFileSubmission_SubmitTAGFile_Failure()
    {
      InjectTAGFileBufferQueueMock(false);

      var submission = new SubmitTAGFileRequest();

      Assert.True(null != submission, "Failed to create SubmitTAGFileRequest instance");

      string tagFileName = "TestTAGFile-TAGFile-Read-Stream.tag";
      var assetID = Guid.NewGuid();

      var response = await submission.ExecuteAsync(RequestArgument(assetID, tagFileName));

      response.Success.Should().BeFalse($"Response is successful!!! [When it should not be]. Filename={response.FileName}, exception={response.Message}");
      response.Code.Should().Be((int)TRexTagFileResultCode.TRexQueueSubmissionError);
      response.Message.Should().Be("SubmitTAGFileResponse. Failed to submit tag file to processing queue. Request already exists");
    }
  }
}
