using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logging=Microsoft.Extensions.Logging;
using Moq;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class ExportJobTests 
  {
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void CanGetS3KeyForExport()
    {
      var jobId = "Some id";
      var filename = "dummy";
      var key = ExportJob.GetS3Key(jobId, filename);
      var expectedKey = $"3dpm/{jobId}/{filename}.zip";
      Assert.AreEqual(expectedKey, key, "Wrong S3 key");
    }

    [TestMethod]
    public void CanGetDownloadLink()
    {
      var jobId = "Some id";
      var filename = "dummy";
      var presignedUrl = "some presigned url";
      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      transferProxy.Setup(t => t.GeneratePreSignedUrl(It.IsAny<string>())).Returns(presignedUrl);

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
      var actualLink = exportJob.GetDownloadLink(jobId, filename);
      Assert.AreEqual(presignedUrl, actualLink);
    }

    [TestMethod]
    public void CanGetExportDataSuccess()
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy"};

      var context = GetMockHangfireContext(TestContext.TestName);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<ExportResult>(scheduleRequest, customHeaders, null)).ReturnsAsync(new ExportResult { ExportData = new byte[0] });

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
      var result = exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }

    [TestMethod]
    [ExpectedException(typeof (Exception))]//Assert.ThrowsException doesn't work so use this instead
    [DataRow("BadRequest {\"Code\":2002,\"Message\":\"Failed to get requested export data with error: No data for export\"}")]
    [DataRow("InternalServerError Some general exception message")]
    public async Task CanGetExportDataFailure(string message)
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy" };

      var context = GetMockHangfireContext(TestContext.TestName, message);

      var exception = new Exception(message);
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<ExportResult>(scheduleRequest, customHeaders, null)).Throws(exception);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
 
      await exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }

    private PerformContext GetMockHangfireContext(string testName, string message = null)
    {
      //Unfortunately Hangfire doesn't have interfaces for everything so we need to
      //explicitly create some objects rather than letting Moq do it.
      Mock<IStorageConnection> connection = new Mock<IStorageConnection>();
      Mock<IJobCancellationToken> token = new Mock<IJobCancellationToken>();
      var jobId = "Some id";
      var methodInfo = typeof(ExportJobTests).GetMethod(testName);
      var args = string.IsNullOrEmpty(message) ? null : new object[] {message};
      var job = new Job(methodInfo, args);
      var backJob = new BackgroundJob(jobId, job, DateTime.Now);
      var context = new PerformContext(connection.Object, backJob, token.Object);
      return context;
    }
  }
}
