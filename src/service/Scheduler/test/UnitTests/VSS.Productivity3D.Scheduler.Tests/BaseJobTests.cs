using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace VSS.Productivity3D.Scheduler.Tests
{
  public class BaseJobTests
  {
    public TestContext TestContext { get; set; }
    protected PerformContext GetMockHangfireContext(Type testType, string testName, string message = "")
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
      var methodInfo = testType.GetMethod(testName);
      var args = string.IsNullOrEmpty(message) ? new object[0] : new object[] { message };
      var job = new Job(methodInfo, args);
      var backJob = new BackgroundJob(jobId, job, DateTime.Now);
      var context = new PerformContext(connection.Object, backJob, token.Object);

      return context;
    }
  }
}
