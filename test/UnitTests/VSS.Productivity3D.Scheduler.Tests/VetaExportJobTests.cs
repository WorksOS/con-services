using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.WebAPI.ExportJobs;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class VetaExportJobTests
  {
    [TestMethod]
    public void CanGetS3Key()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid();
      var jobId = "Some id";
      var key = VetaExportJob.GetS3Key(customerUid, projectUid, jobId);
      var expectedKey = $"3dpm/{customerUid}/{projectUid}/{jobId}.zip";
      Assert.AreEqual(expectedKey, key, "Wrong S3 key");
    }

    [TestMethod]
    public void CanExportDataToVeta()
    {
      var projectUId = Guid.NewGuid();
      var customerUid = Guid.NewGuid().ToString();
      var fileName = "some file";
      var customHeaders = new Dictionary<string, string>();
      customHeaders.Add("X-VisionLink-CustomerUid", customerUid);

      //Unfortunately Hangfire doesn't have interfaces for everything so we need to
      //explicitly create some objects rather than letting Moq do it.
      Mock<IStorageConnection> connection = new Mock<IStorageConnection>();
      Mock<IJobCancellationToken> token = new Mock<IJobCancellationToken>();
      var jobId = "Some id";
      var methodInfo = typeof(VetaExportJobTests).GetMethod("CanExportDataToVeta");
      var job = new Job(methodInfo);
      var backJob = new BackgroundJob(jobId, job, DateTime.Now);
      var context = new PerformContext(connection.Object, backJob, token.Object);

      Mock<IRaptorProxy> raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(r => r.GetVetaExportData(projectUId, fileName, string.Empty, null, customHeaders)).ReturnsAsync(new ExportResult{ExportData = new byte[0]});

      Mock<ITransferProxy> transferProxy = new Mock<ITransferProxy>();
      transferProxy.Setup(t => t.Upload(It.IsAny<Stream>(), It.IsAny<string>())).Verifiable();

      var vetaExportJob = new VetaExportJob(raptorProxy.Object, transferProxy.Object);
      vetaExportJob.ExportDataToVeta(projectUId, fileName, string.Empty, null, customHeaders, context);
    }
  }
}
