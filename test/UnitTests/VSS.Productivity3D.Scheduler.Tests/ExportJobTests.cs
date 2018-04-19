using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class ExportJobTests
  {
    [TestMethod]
    public void CanGetS3KeyForExport()
    {
      var jobId = "Some id";
      var key = ExportJob.GetS3Key(jobId);
      var expectedKey = $"3dpm/{jobId}.zip";
      Assert.AreEqual(expectedKey, key, "Wrong S3 key");
    }

    [TestMethod]
    public void CanGetDownloadLink()
    {
      var jobId = "Some id";
      var presignedUrl = "some presigned url";
      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      transferProxy.Setup(t => t.GeneratePreSignedUrl(It.IsAny<string>())).Returns(presignedUrl);

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object);
      var actualLink = exportJob.GetDownloadLink(jobId);
      Assert.AreEqual(presignedUrl, actualLink);
    }

    [TestMethod]
    public void CanGetExportData()
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url"};

      //Unfortunately Hangfire doesn't have interfaces for everything so we need to
      //explicitly create some objects rather than letting Moq do it.
      Mock<IStorageConnection> connection = new Mock<IStorageConnection>();
      Mock<IJobCancellationToken> token = new Mock<IJobCancellationToken>();
      var jobId = "Some id";
      var methodInfo = typeof(ExportJobTests).GetMethod("CanGetExportData");
      var job = new Job(methodInfo);
      var backJob = new BackgroundJob(jobId, job, DateTime.Now);
      var context = new PerformContext(connection.Object, backJob, token.Object);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<ExportResult>(scheduleRequest, customHeaders, null, null)).ReturnsAsync(new ExportResult { ExportData = new byte[0] });

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object);
      var result = exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }
  }
}
