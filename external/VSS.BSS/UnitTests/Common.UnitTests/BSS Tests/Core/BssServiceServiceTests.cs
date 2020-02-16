using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssServiceServiceTests : BssUnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void GetServiceTypeByPartNumber()
    {
      var serviceType = Services.ServiceViews().GetServiceTypeByPartNumber("89500-00");
      AssertServiceType(ServiceTypeEnum.Essentials, serviceType);
    }

    [DatabaseTest]
    [TestMethod]
    public void GetServiceTypeByPartNumber_PartNumberDoesNotExists()
    {
      var serviceType = Services.ServiceViews().GetServiceTypeByPartNumber("89500-11");
      AssertServiceType(ServiceTypeEnum.Unknown, serviceType);
    }

    [TestMethod]
    [DatabaseTest]
    public void GetServiceByPlanLineID()
    {
      var device = Entity.Device.MTS521.IbKey(IdGen.GetId().ToString()).Save();
      var service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.GetId().ToString()).Save();

      var serviceDto = Services.ServiceViews().GetServiceByPlanLineID(service.BSSLineID);

      Assert.IsTrue(serviceDto.ServiceExists, "Service should exist.");
      Assert.AreEqual(device.ID, serviceDto.DeviceID, "Device IDs are expected to match.");
      Assert.AreEqual(service.BSSLineID, serviceDto.PlanLineID, "Service Plan Line IDs are expected to match.");
    }

    [TestMethod]
    [DatabaseTest]
    public void GetServiceByPlanLineID_PlanLineIDDoesNotExists()
    {
      var serviceDto = Services.ServiceViews().GetServiceByPlanLineID(IdGen.GetId().ToString());
      Assert.IsNull(serviceDto);
    }

    private void AssertServiceType(ServiceTypeEnum expected, ServiceTypeEnum? actual)
    {
      Assert.IsNotNull(actual);
      Assert.AreEqual(expected, actual, "Service Types are expected to match.");
    }
  }
}
