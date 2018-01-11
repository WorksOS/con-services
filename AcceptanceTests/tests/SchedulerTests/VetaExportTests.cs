using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Scheduler.Common.Utilities;

namespace SchedulerTests
{
  [TestClass]
  public class VetaExportTests :  TestControllerBase
  {
    protected ILogger _log;


    [TestInitialize]
    public void Init()
    {
      SetupDi();
      _log = LoggerFactory.CreateLogger<VetaExportTests>();
    }

    [TestMethod]
    public async void CanScheduleVetaExportJob()
    {
      var jobId = await ServiceProvider.GetRequiredService<ISchedulerProxy>()
        .ScheduleVetaExportJob(Guid.NewGuid(), "testfile", "allmachines", new Guid(), null);
      Assert.IsFalse(string.IsNullOrEmpty(jobId));
    }
  }
}
