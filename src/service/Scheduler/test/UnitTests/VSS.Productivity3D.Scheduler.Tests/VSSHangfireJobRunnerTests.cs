using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.Productivity3D.Scheduler.WebAPI.JobRunner;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Tests
{
  [TestClass]
  public class VSSHangfireJobRunnerTests : BaseJobTests
  {
    private ILoggerFactory loggerFactory;
    private IServiceProvider serviceProvider;
    private bool setupCalled;
    private bool runCalled;
    private bool tearDownCalled;

    [TestInitialize]
    public void TestInitialize()
    {
      var services = new ServiceCollection();
      services.AddSingleton<MockDxfTileGenerationJob>();
      serviceProvider = services
        .AddLogging()
        .BuildServiceProvider();

      loggerFactory = serviceProvider.GetService<ILoggerFactory>();
    }

    [TestMethod]
    public void CanRunJobSuccess()
    {
      setupCalled = false;
      runCalled = false;
      tearDownCalled = false;
      var mockJob = serviceProvider.GetRequiredService<MockDxfTileGenerationJob>();
      Assert.IsNotNull(mockJob);
      mockJob.SetupInvoked += OnSetupInvoked;
      mockJob.RunInvoked += OnRunInvoked;
      mockJob.TearDownInvoked += OnTearDownInvoked;
      var context = GetMockHangfireContext(typeof(VSSHangfireJobRunnerTests), TestContext.TestName, string.Empty);
      var errorProvider = new Mock<IErrorCodesProvider>();
      var jobRunner = new VSSHangfireJobRunner(loggerFactory, errorProvider.Object, serviceProvider);
      var vssJobUid = Guid.NewGuid();
      jobRunner.RegisterJob(vssJobUid, typeof(MockDxfTileGenerationJob));
      var request = new JobRequest { JobUid = vssJobUid, RunParameters = new DxfTileGenerationRequest() };
      var result = jobRunner.RunHangfireJob(request, context);
      Assert.IsTrue(setupCalled);
      Assert.IsTrue(runCalled);
      Assert.IsTrue(tearDownCalled);
    }

    [TestMethod]
    public void CanRunJobFailureNotRegistered()
    {
      var context = GetMockHangfireContext(typeof(VSSHangfireJobRunnerTests), TestContext.TestName, string.Empty);
      var errorProvider = new Mock<IErrorCodesProvider>();
      var jobRunner = new VSSHangfireJobRunner(loggerFactory, errorProvider.Object, serviceProvider);
      var vssJobUid = Guid.NewGuid();
      var request = new JobRequest { JobUid = vssJobUid, RunParameters = new DxfTileGenerationRequest() };
      Assert.ThrowsException<AggregateException>(() => jobRunner.RunHangfireJob(request, context).Result);
    }

    private void OnSetupInvoked(object sender, EventArgs e)
    {
      setupCalled = true;
    }
    private void OnRunInvoked(object sender, EventArgs e)
    {
      runCalled = true;
    }
    private void OnTearDownInvoked(object sender, EventArgs e)
    {
      tearDownCalled = true;
    }
  }
}
