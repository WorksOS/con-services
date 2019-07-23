using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Requests;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.TAGFiles.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(ProcessTAGFileRequest))]
  public class ProcessTAGFileRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    private void InjectTAGFileProcessorMock()
    {
      DIBuilder
        .Continue()
        .Complete();

      IgniteMock.AddApplicationGridRouting<IComputeFunc<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>, ProcessTAGFileRequestArgument, ProcessTAGFileResponse>();
    }

    [Fact]
    public void Creation()
    {
      var req = new ProcessTAGFileRequest();

      req.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessSingleTAGFile_FailWithInvalidTAGFile()
    {
      InjectTAGFileProcessorMock();

      var req = new ProcessTAGFileRequest();
      var arg = new ProcessTAGFileRequestArgument
      {
        TAGFiles = new List<ProcessTAGFileRequestFileItem>
        {
          new ProcessTAGFileRequestFileItem
          {
            IsJohnDoe = false, FileName = "ATAGFileName", TagFileContent = new byte[] {1, 2, 3, 4, 5}
          }
        }
      };

      var response = await req.ExecuteAsync(arg);
      response.Results.Count.Should().Be(1);
      response.Results[0].Success.Should().BeFalse();
      response.Results[0].Exception.Should().BeNull();
    }

    [Fact]
    public async Task ProcessSingleTAGFile_Success()
    {
      InjectTAGFileProcessorMock();

      var fileName = Path.Combine("TestData", "TAGFiles", "TestTAGFile-TAGFile-Read-Stream.tag");

      var req = new ProcessTAGFileRequest();
      var arg = new ProcessTAGFileRequestArgument
      {
        TAGFiles = new List<ProcessTAGFileRequestFileItem>
        {
          new ProcessTAGFileRequestFileItem
          {
            IsJohnDoe = false,
            FileName = fileName,
            TagFileContent = File.ReadAllBytes(fileName)
          }
        }
      };

      var response = await req.ExecuteAsync(arg);
      response.Results.Count.Should().Be(1);
      response.Results[0].Success.Should().BeTrue();
      response.Results[0].Exception.Should().BeNull();
    }
  }
}
