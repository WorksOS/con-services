using System;
using FluentAssertions;
using Moq;
using VSS.Productivity3D.TagFileGateway.Common.Executors;
using Xunit;

namespace VSS.Productivity3D.TagFileGateway.UnitTests
{
  public class TagFileProcessExecutorTests : ExecutorBaseFixture
  {
    private TagFileProcessExecutor CreateExecutor()
    {
      ConfigStore.Reset();
      DataCache.Reset();
      TagFileForwarder.Reset();
      TransferProxy.Reset();
      WebRequest.Reset();
      return RequestExecutorContainer.Build<TagFileProcessExecutor>(LoggerFactory, ConfigStore.Object, DataCache.Object, TagFileForwarder.Object, TransferProxy.Object, WebRequest.Object);
    }


    [Fact]
    public void ShouldBeCorrectType()
    {
      var e = CreateExecutor();

      e.Should().NotBeNull();
      e.Should().BeOfType<TagFileProcessExecutor>();
    }

    [Fact]
    public void ShouldFailOnIncorrectArg()
    {
      var e = CreateExecutor();

      var result = e.ProcessAsync(new object()).Result;

      result.Should().NotBeNull();
      result.Code.Should().NotBe(0);

    }
  }
}
