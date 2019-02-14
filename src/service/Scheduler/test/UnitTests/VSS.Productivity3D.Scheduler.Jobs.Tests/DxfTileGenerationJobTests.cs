using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob;
using VSS.Productivity3D.Scheduler.Jobs.DxfTileJob.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Scheduler.Jobs.Tests
{
  [TestClass]
  public class DxfTileGenerationJobTests
  {
    [TestMethod]
    public void CanSetupJob()
    {
      var job = new DxfTileGenerationJob();
      job.Setup(null);
    }

    [TestMethod]
    public void CanTearDownJob()
    {
      var job = new DxfTileGenerationJob();
      job.TearDown(null);
    }

    [TestMethod]
    public void CanRunJobSuccess()
    {
      var request = new DxfTileGenerationRequest
      {
        CustomerUid = Guid.NewGuid(), ProjectUid = Guid.NewGuid(), ImportedFiletUid = Guid.NewGuid(),
        DataOceanRootFolder = "some folder", DxfFileName = "a dxf file", DcFileName = "a dc file", DxfUnitsType = DxfUnitsType.Meters
      };
      var job = new DxfTileGenerationJob();
      job.Run(request);
    }

    [TestMethod]
    public void CanRunJobFailureWrongRequest()
    {
      var job = new DxfTileGenerationJob();
      Assert.ThrowsException<ServiceException>(() => job.Run(new object()));
    }
  }
}
