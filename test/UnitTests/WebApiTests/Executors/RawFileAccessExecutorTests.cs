using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Executors;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using VSS.Productivity3D.FileAccess.WebAPI.Models.ResultHandling;
using VSS.TCCFileAccess;
using Moq;

namespace WebApiTests.Executors
{
  [TestClass]
  public class RawFileAccessExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public void GetConfigurationStore()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      Assert.IsNotNull(configStore, "Unable to retrieve configStore from DI");
    }

    [TestMethod]
    public void GetLogger()
    {
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public void RawFileAccess_NoValidInput()
    {
      string filespaceId = Guid.NewGuid().ToString();
      string path = "/132465/55644";
      string fileName = "file.ext";
      var request = FileDescriptor.CreateFileDescriptor(filespaceId, path, fileName);
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var fileRepository = serviceProvider.GetRequiredService<IFileRepository>();

      var executor = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, configStore, null);
      var ex = Assert.ThrowsException<ServiceException>(() => executor.Process(request));
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Failed to download file from TCC", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RawFileAccess_FileFound()
    {
      string filespaceId = Guid.NewGuid().ToString();
      string path = "/132465/55644";
      string fileName = "file.ext";
      var request = FileDescriptor.CreateFileDescriptor(filespaceId, path, fileName);
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      
      var fileRepo = new Mock<IFileRepository>();
      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new MemoryStream(buffer));

      var executor = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, configStore,
        fileRepo.Object);
      var result = executor.Process(request) as RawFileAccessResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(0, result.Code, "executor returned invalid status");
      Assert.IsTrue(buffer.SequenceEqual(result.fileContents), "File content not read correctly.");
    }

    [TestMethod]
    public void RawFileAccess_NoFileFound()
    {
      string filespaceId = Guid.NewGuid().ToString();
      string path = "/132465/55644";
      string fileName = "file.ext";
      var request = FileDescriptor.CreateFileDescriptor(filespaceId, path, fileName);
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      var fileRepo = new Mock<IFileRepository>();
      fileRepo.Setup(fr => fr.GetFile(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((MemoryStream) null);

      var executor = RequestExecutorContainer.Build<RawFileAccessExecutor>(logger, configStore,
        fileRepo.Object);
      var ex = Assert.ThrowsException<ServiceException>(() => executor.Process(request));
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Failed to download file from TCC", StringComparison.Ordinal));
    }
  }
}
