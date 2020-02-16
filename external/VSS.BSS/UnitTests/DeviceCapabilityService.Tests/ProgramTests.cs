using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Topshelf.Runtime;
using Topshelf.Runtime.Windows;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Service;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  [TestClass]
  public class ProgramTests
  {
    [TestMethod]
    public void ServiceFactoryReturnsValidRestServiceTest()
    {
      HostSettings settings = new WindowsHostSettings("UnitTest", "UnitTest");
      RestService restService = Program.ServiceFactory(settings);
      //Start might throw an exception if port 8001 is not enabled
      try
      {
        restService.Start();
      }
      catch
      {
      }
      //make sure stop doesn't throw an error
      restService.Stop();
      //make sure Error doesn't throw an error
      restService.Error();
    }

    [TestMethod]
    public void DeviceHandlerParametersSetupCorrectlyTest()
    {
      PrivateType obj = new PrivateType(typeof(Program));
      HostSettings settings = new WindowsHostSettings("UnitTest", "UnitTest");
      obj.InvokeStatic("ServiceFactory", new object[] { settings });

      var container = obj.GetStaticFieldOrProperty("_container") as IContainer;
      var DeviceHandlerParameters = container.Resolve<IDeviceHandlerParameters>();
      Assert.IsNotNull(DeviceHandlerParameters.UnknownDeviceHandler);
      Assert.AreEqual(10, DeviceHandlerParameters.DeviceHandlers.Count);
    }
  }
}