using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logging=Microsoft.Extensions.Logging;
using Moq;
using Hangfire;
using Hangfire.Common;
using Hangfire.MySql;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
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

      var scheduleRequest = new ScheduleJobRequest { Url = "some url", Filename = "dummy"};
      var context = GetMockHangfireContext(TestContext.TestName, message);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      apiClient.Setup(a => a.SendRequest(scheduleRequest, customHeaders)).ReturnsAsync(new StreamContent(new MemoryStream()));

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
      var result = exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }

    [TestMethod]
    [Ignore]
    [DataRow("Export Success")]
    public void CanGetUpdatedFilename(string message)
    {
      // We will say our filename is named something different to the content type, and make sure the export function gives us back the correct file name

      const string extension = ".json";
      const string contentType = "application/json";
      var customHeaders = new Dictionary<string, string>();

      var scheduleRequest = new ScheduleJobRequest {Url = "some url", Filename = "dummy.mp3"};
      var expectedFilename = scheduleRequest.Filename + extension; 

      var context = GetMockHangfireContext(TestContext.TestName, message);

      Mock<IApiClient> apiClient = new Mock<IApiClient>();
      var apiresult = new StreamContent(new MemoryStream());
      apiresult.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
      apiClient.Setup(a => a.SendRequest(scheduleRequest, customHeaders)).ReturnsAsync(apiresult);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
        .Returns((Stream stream, string filename, string ct) => filename + ".json"); // Match the filename to the content type for testings

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);

      exportJob.GetExportData(scheduleRequest, customHeaders, context).Wait();

      var key = JobStorage.Current.GetConnection().GetJobParameter(context.BackgroundJob.Id, ExportJob.S3KeyStateKey);
      Assert.AreEqual(key, ExportJob.GetS3Key(context.BackgroundJob.Id, expectedFilename));
    }

    [TestMethod]
    [Ignore]
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
      apiClient.Setup(a => a.SendRequest(scheduleRequest, customHeaders)).Throws(exception);

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      Mock<Logging.ILoggerFactory> logger = new Mock<Logging.ILoggerFactory>();

      var exportJob = new ExportJob(apiClient.Object, transferProxy.Object, logger.Object);
 
      await exportJob.GetExportData(scheduleRequest, customHeaders, context);
    }

    private PerformContext GetMockHangfireContext(string testName, string message = "")
    {
      var jobParameters = new Dictionary<string, string>();
      //Unfortunately Hangfire doesn't have interfaces for everything so we need to
      //explicitly create some objects rather than letting Moq do it.
      var connection = new Mock<IStorageConnection>();

      // Mock the job parameters to a dictionary, but ignore the Job ID as it is constant
      connection.Setup(x => x.SetJobParameter(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .Callback((string id, string key, string value) => { jobParameters[key] = value; });

      connection.Setup(x => x.GetJobParameter(It.IsAny<string>(), It.IsAny<string>()))
        .Returns((string id, string key) => jobParameters[key]);

      var token = new Mock<IJobCancellationToken>();
      var storage = new Mock<JobStorage>();

      storage.Setup(x => x.GetConnection()).Returns(connection.Object);
      // Assigned the job storage to our mocked object, as it's used by some implementations
      JobStorage.Current = storage.Object;

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
