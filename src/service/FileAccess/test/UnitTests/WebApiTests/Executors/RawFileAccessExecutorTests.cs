using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Executors;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using VSS.Productivity3D.FileAccess.WebAPI.Models.ResultHandling;
using VSS.TCCFileAccess;
using Xunit;

namespace WebApiTests.Executors
{
  public class RawFileAccessExecutorTests : IClassFixture<ExecutorBaseTests>
  {
    private readonly ExecutorBaseTests _testFixture;
    private IServiceProvider _serviceProvider => _testFixture.serviceProvider;
    private IConfigurationStore _configStore => _testFixture.configStore;

    public RawFileAccessExecutorTests(ExecutorBaseTests testFixture)
    {
      _testFixture = testFixture;
    }

    [Fact]
    public void GetConfigurationStore() => Assert.NotNull(_serviceProvider.GetRequiredService<IConfigurationStore>());

    [Fact]
    public void GetLogger() => Assert.NotNull(_serviceProvider.GetRequiredService<ILoggerFactory>());

    [Fact]
    public void RawFileAccess_NoValidInput()
    {
      var filespaceId = Guid.NewGuid().ToString();
      var path = "/132465/55644";
      var fileName = "file.ext";
      var request = FileDescriptor.CreateFileDescriptor(filespaceId, path, fileName);
      var logger = _serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, _configStore);
      var ex = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.NotEqual(-1, ex.GetContent.IndexOf("Failed to download file from TCC", StringComparison.Ordinal));
    }

    [Fact]
    public void RawFileAccess_FileFound()
    {
      var filespaceId = Guid.NewGuid().ToString();
      var path = "/132465/55644";
      var fileName = "file.ext";
      var request = FileDescriptor.CreateFileDescriptor(filespaceId, path, fileName);
      var logger = _serviceProvider.GetRequiredService<ILoggerFactory>();

      var fileRepo = new Mock<IFileRepository>();
      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream(buffer));

      var executor = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, _configStore,
        fileRepo.Object);
      var result = executor.Process(request) as RawFileAccessResult;

      Assert.NotNull(result);
      Assert.Equal(0, result.Code);
      Assert.True(buffer.SequenceEqual(result.fileContents), "File content not read correctly.");
    }

    [Fact]
    public void RawFileAccess_NoFileFound()
    {
      var filespaceId = Guid.NewGuid().ToString();
      var path = "/132465/55644";
      var fileName = "file.ext";
      var request = FileDescriptor.CreateFileDescriptor(filespaceId, path, fileName);
      var logger = _serviceProvider.GetRequiredService<ILoggerFactory>();

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((MemoryStream)null);

      var executor = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, _configStore,
        fileRepo.Object);
      var ex = Assert.Throws<ServiceException>(() => executor.Process(request));
      Assert.Equal(HttpStatusCode.BadRequest, ex.Code);
      Assert.NotEqual(-1, ex.GetContent.IndexOf("Failed to download file from TCC", StringComparison.Ordinal));
    }
  }
}
