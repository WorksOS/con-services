using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
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

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object);
      var actualLink = exportJob.GetDownloadLink(jobId, filename);
      Assert.AreEqual(presignedUrl, actualLink);
    }

    [TestMethod]
    public void CanGetExportDataSuccess()
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy"};

      var context = GetMockHangfireContext("CanGetExportDataSuccess");

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<ExportResult>(scheduleRequest, customHeaders, null)).ReturnsAsync(new ExportResult { ExportData = new byte[0] });

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object);
      var result = exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }

    [TestMethod]
    [ExpectedException(typeof (ServiceException))]//Assert.ThrowsException doesn't work so use this instead
    public async Task CanGetExportDataFailure()
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy" };

      var context = GetMockHangfireContext("CanGetExportDataFailure");

      var resultJson = @"{
          ""exportData"": null,
          ""resultCode"": 0,
          ""Code"": 2002,
          ""Message"": ""Failed to get requested export data with error: No data for export""
          }";
      var exportResult = JsonConvert.DeserializeObject<ExportResult>(resultJson);
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest<ExportResult>(scheduleRequest, customHeaders, null)).ReturnsAsync(exportResult);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object);
 
      await exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }

    private PerformContext GetMockHangfireContext(string testName)
    {
      //Unfortunately Hangfire doesn't have interfaces for everything so we need to
      //explicitly create some objects rather than letting Moq do it.
      Mock<IStorageConnection> connection = new Mock<IStorageConnection>();
      Mock<IJobCancellationToken> token = new Mock<IJobCancellationToken>();
      var jobId = "Some id";
      var methodInfo = typeof(ExportJobTests).GetMethod(testName);
      var job = new Job(methodInfo);
      var backJob = new BackgroundJob(jobId, job, DateTime.Now);
      var context = new PerformContext(connection.Object, backJob, token.Object);
      return context;
    }
  }
}
