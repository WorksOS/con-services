using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Models
{
  [TestClass]
  public class ProfileProductionDataRequestTests
  {
    [TestMethod]
    public void IsAlignmentProfile_Should_return_false_when_AlignmentDesign_is_null()
    {
      var request = ProfileProductionDataRequest.CreateProfileProductionData(0, Guid.NewGuid(), ProductionDataType.All, null, 0, null, null, null, 0, 0, null, false);
      Assert.IsFalse(request.IsAlignmentDesign);
    }

    [TestMethod]
    public void IsAlignmentProfile_Should_return_true_when_AlignmentDesign_file_is_set()
    {
      var request = ProfileProductionDataRequest.CreateProfileProductionData(0, Guid.NewGuid(), ProductionDataType.All, null, 0, new DesignDescriptor(1, FileDescriptor.CreateFileDescriptor("1", "path", "filename"), 0), null, null, 0, 0, null, false);
      Assert.IsTrue(request.IsAlignmentDesign);
    }
  }
}
