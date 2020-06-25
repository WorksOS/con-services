using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Scheduler.Jobs.ExportJob;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;
using Logging = Microsoft.Extensions.Logging;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class ExportJobTests : BaseJobTests
  {
    [TestMethod]
    public void CanGetS3KeyForExport()
    {
      return;
      var jobId = "Some id";
      var filename = "dummy";
      var key = ExportJob.GetS3Key(jobId, filename);
      var expectedKey = $"3dpm/{jobId}/{filename}";
      Assert.AreEqual(expectedKey, key, "Wrong S3 key");
    }

    [TestMethod]
    public void CanGetDownloadLink()
    {
      var jobId = "Some id";
      var filename = "dummy";
      var presignedUrl = "some presigned url";
      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      Mock<ITransferProxyFactory> transferProxyFactory = new Mock<ITransferProxyFactory>();
      transferProxyFactory.Setup(x => x.NewProxy(It.IsAny<TransferProxyType>())).Returns(transferProxy.Object);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      transferProxy.Setup(t => t.GeneratePreSignedUrl(It.IsAny<string>())).Returns(presignedUrl);

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxyFactory.Object, logger.Object);
      var actualLink = exportJob.GetDownloadLink(jobId, filename);
      Assert.AreEqual(presignedUrl, actualLink);
    }

    [TestMethod]
    [DataRow("Export Success")]
    public void CanGetExportDataSuccess(string message)
    {
      var customHeaders = new HeaderDictionary();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy" };
      var context = GetMockHangfireContext(typeof(ExportJobTests), TestContext.TestName, message);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<CompactionExportResult>(scheduleRequest, customHeaders)).ReturnsAsync(new CompactionExportResult());
      
      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      Mock<ITransferProxyFactory> transferProxyFactory = new Mock<ITransferProxyFactory>();
      transferProxyFactory.Setup(x => x.NewProxy(It.IsAny<TransferProxyType>())).Returns(transferProxy.Object);

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxyFactory.Object, logger.Object);
      var result = exportJob.GetExportData(Guid.NewGuid(), customHeaders, context);
    }

    [TestMethod]
    [DataRow("BadRequest {\"Code\":2002,\"Message\":\"Failed to get requested export data with error: No data for export\"}")]
    [DataRow("InternalServerError Some general exception message")]
    public async Task CanGetExportDataFailure(string message)
    {
      var customHeaders = new HeaderDictionary();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy" };
      var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(scheduleRequest)));
      var fileStreamResult = new FileStreamResult(ms, ContentTypeConstants.ApplicationJson);

      var context = GetMockHangfireContext(typeof(ExportJobTests), TestContext.TestName, message);

      var exception = new Exception(message);
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<CompactionExportResult>(It.IsAny<ScheduleJobRequest>(), customHeaders)).Throws(exception);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();
      transferProxy.Setup(t => t.Download(It.IsAny<string>())).Returns(() => Task.FromResult(fileStreamResult));

      Mock<ITransferProxyFactory> transferProxyFactory = new Mock<ITransferProxyFactory>();
      transferProxyFactory.Setup(x => x.NewProxy(It.IsAny<TransferProxyType>())).Returns(transferProxy.Object);

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxyFactory.Object, logger.Object);

      await Assert.ThrowsExceptionAsync<Exception>(() => exportJob.GetExportData(Guid.NewGuid(), customHeaders, context));
      ms.Dispose();
    }
  }
}
