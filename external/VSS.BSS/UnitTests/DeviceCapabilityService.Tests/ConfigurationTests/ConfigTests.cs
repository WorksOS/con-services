using System;
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Nighthawk.DeviceCapabilityService.Configuration;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.ConfigurationTests
{
  [TestClass]
  public class ConfigTests
  {
    [TestMethod]
    public void DeviceHandlerNoEndpoints_Success()
    {
      var deviceHandlerConfig = ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      Assert.IsNotNull(deviceHandlerConfig);

      EndpointConfigCollection endpointCollection = deviceHandlerConfig.HandlerConfigs["TestHandler_NoEndpoints"].OutboundEndpoints;
      Assert.IsTrue(endpointCollection.Count == 0);
    }

    [TestMethod]
    public void DeviceHandlerSingleEndpoint_Success()
    {
      var deviceHandlerConfig = ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      Assert.IsNotNull(deviceHandlerConfig);

      EndpointConfigCollection endpointCollection = deviceHandlerConfig.HandlerConfigs["TestHandler_SingleEndpoint"].OutboundEndpoints;
      Assert.IsTrue(1 == endpointCollection.Count);
      Assert.IsTrue(endpointCollection.ContainsKey("TestEndpoint1"));
      Assert.IsTrue("TestEndpoint1" == endpointCollection[0].Name);
      Assert.IsTrue("TestEndpoint1" == endpointCollection["TestEndpoint1"].Name);
      Assert.IsFalse(endpointCollection.ContainsKey("???"));
    }

    [TestMethod]
    public void DeviceHandlerMultipleEndpoints_Success()
    {
      var deviceHandlerConfig = ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      Assert.IsNotNull(deviceHandlerConfig);

      EndpointConfigCollection endpointCollection = deviceHandlerConfig.HandlerConfigs["TestHandler_MultipleEndpoints"].OutboundEndpoints;
      Assert.IsTrue(3 == endpointCollection.Count);
      Assert.IsTrue(endpointCollection.ContainsKey("TestEndpoint1"));
      Assert.IsTrue("TestEndpoint1" == endpointCollection[0].Name);
      Assert.IsTrue("TestEndpoint1" == endpointCollection["TestEndpoint1"].Name);
      Assert.IsTrue(endpointCollection.ContainsKey("TestEndpoint3"));
      Assert.IsTrue("TestEndpoint3" == endpointCollection[2].Name);
      Assert.IsTrue("TestEndpoint3" == endpointCollection["TestEndpoint3"].Name);
      Assert.IsFalse(endpointCollection.ContainsKey("???"));
    }

    [TestMethod]
    public void HandlerConfigAccessors_Success()
    {
      var deviceHandlerConfig = ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      Assert.IsNotNull(deviceHandlerConfig);

      Assert.IsTrue(deviceHandlerConfig.HandlerConfigs.ContainsKey("TestHandler_NoEndpoints"));
      Assert.IsTrue("TestHandler_NoEndpoints" == deviceHandlerConfig.HandlerConfigs[0].Name);
      Assert.IsTrue("TestHandler_NoEndpoints" == deviceHandlerConfig.HandlerConfigs["TestHandler_NoEndpoints"].Name);
      Assert.IsFalse(deviceHandlerConfig.HandlerConfigs.ContainsKey("???"));
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void HandlerConfigBadHandlerName_Success()
    {
      var deviceHandlerConfig = ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      Assert.IsNotNull(deviceHandlerConfig);
     
      var endpointConfigCollection = deviceHandlerConfig.HandlerConfigs["???"].OutboundEndpoints;
    }

    [TestMethod]
    public void EndPointConfigBadEndpointName_Success()
    {
      var deviceHandlerConfig = ConfigurationManager.GetSection("deviceHandlerConfig") as HandlerConfigSection;
      Assert.IsNotNull(deviceHandlerConfig);

      EndpointConfigCollection endpointCollection = deviceHandlerConfig.HandlerConfigs["TestHandler_SingleEndpoint"].OutboundEndpoints;
      Assert.IsTrue(1 == endpointCollection.Count);
      Assert.IsTrue(endpointCollection.ContainsKey("TestEndpoint1"));
      Assert.IsNull(endpointCollection["???"]);
    }
  }
}
