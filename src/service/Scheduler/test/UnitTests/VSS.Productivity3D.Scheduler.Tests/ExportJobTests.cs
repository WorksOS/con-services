using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logging = Microsoft.Extensions.Logging;
using Moq;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Http;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class ExportJobTests : BaseJobTests
  {
    [TestMethod]
    public void CanGetS3KeyForExport()
    {
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
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      transferProxy.Setup(t => t.GeneratePreSignedUrl(It.IsAny<string>())).Returns(presignedUrl);

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
      var actualLink = exportJob.GetDownloadLink(jobId, filename);
      Assert.AreEqual(presignedUrl, actualLink);
    }

    [TestMethod]
    [DataRow("Export Success")]
    public void CanGetExportDataSuccess(string message)
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy" };
      var context = GetMockHangfireContext(typeof(ExportJobTests), TestContext.TestName, message);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest(scheduleRequest, customHeaders)).ReturnsAsync(new StreamContent(new MemoryStream()));

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
      var result = exportJob.GetExportData(Guid.NewGuid(), customHeaders, context);
    }

    [TestMethod]
    [DataRow("Export Success")]
    public void CanGetUpdatedFilename(string message)
    {
      // We will say our filename is named something different to the content type, and make sure the export function gives us back the correct file name

      const string extension = ".json";
      const string contentType = ContentTypeConstants.ApplicationJson;
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy.mp3" };
      var expectedFilename = scheduleRequest.Filename + extension;

      var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(scheduleRequest)));
      var fileStreamResult = new FileStreamResult(ms, ContentTypeConstants.ApplicationJson);

      var context = GetMockHangfireContext(typeof(ExportJobTests), TestContext.TestName, message);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();

      var apiresult = new StringContent("some content", Encoding.UTF8, contentType);


      apiClient.Setup(a => a.SendRequest(It.IsAny<ScheduleJobRequest>(), customHeaders)).ReturnsAsync(apiresult);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns((Stream stream, string filename, string ct) => filename + ".json"); // Match the filename to the content type for testings
      transferProxy.Setup(t => t.Download(It.IsAny<string>())).Returns(() => Task.FromResult(fileStreamResult));

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);

      exportJob.GetExportData(Guid.NewGuid(), customHeaders, context).Wait();

      var key = JobStorage.Current.GetConnection().GetJobParameter(context.BackgroundJob.Id, ExportJob.S3KeyStateKey);
      Assert.AreEqual(key, ExportJob.GetS3Key(context.BackgroundJob.Id, expectedFilename));
      ms.Dispose();
    }

    [TestMethod]
    [DataRow("BadRequest {\"Code\":2002,\"Message\":\"Failed to get requested export data with error: No data for export\"}")]
    [DataRow("InternalServerError Some general exception message")]
    public async Task CanGetExportDataFailure(string message)
    {
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy" };
      var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(scheduleRequest)));
      var fileStreamResult = new FileStreamResult(ms, ContentTypeConstants.ApplicationJson);

      var context = GetMockHangfireContext(typeof(ExportJobTests), TestContext.TestName, message);

      var exception = new Exception(message);
      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest(It.IsAny<ScheduleJobRequest>(), customHeaders)).Throws(exception);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();
      transferProxy.Setup(t => t.Download(It.IsAny<string>())).Returns(() => Task.FromResult(fileStreamResult));

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);

      await Assert.ThrowsExceptionAsync<Exception>(() => exportJob.GetExportData(Guid.NewGuid(), customHeaders, context));
      ms.Dispose();
    }
  }
}
