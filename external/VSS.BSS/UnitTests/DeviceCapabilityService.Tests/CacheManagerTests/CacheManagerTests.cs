using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Data;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;

namespace VSS.Nighthawk.DeviceCapabilityService.Tests.CacheManagerTests
{
  [TestClass]
  public class CacheManagerTests
  {
    [Ignore]
    [TestMethod]
    public void CacheManager_CachedItemExpired()
    {
      const int deviceTypeCacheLifetimeMinutes = 1;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      long deviceId = 11;
      var deviceKey = String.Format("Device.{0}", deviceId);
      DeviceTypeEnum? deviceTypeOfDevice = DeviceTypeEnum.TAP66;

      // Add device type for device (Device.11) to cache
      cacheManager.Add(deviceKey, deviceTypeOfDevice, deviceTypeCacheLifetimeMinutes);

      TimeSpan interval = new TimeSpan(0, deviceTypeCacheLifetimeMinutes, 3);
      Thread.Sleep(interval);

      // Retrieving device type for device (Device.11) from cache
      var deviceTypeOfDeviceFromCache = (DeviceTypeEnum?)cacheManager.GetData(deviceKey);

      Assert.IsNotNull(deviceTypeOfDevice);
      Assert.IsNull(deviceTypeOfDeviceFromCache);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CacheManager_CacheManagerNameIsNull()
    {
      string cacheManagerName = null;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig(cacheManagerName);
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);
    }

    [TestMethod]
    public void CacheManager_CachedItemIsNull()
    {
      const int deviceTypeCacheLifetimeMinutes = 1;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      long deviceId = 11;
      var deviceKey = String.Format("Device.{0}", deviceId);

      // Add device type for device (Device.11) to cache
      cacheManager.Add(deviceKey, null, deviceTypeCacheLifetimeMinutes);

      // Retrieving device type for device (Device.11) from cache
      var deviceTypeOfDeviceFromCache = (DeviceTypeEnum?)cacheManager.GetData(deviceKey);

      Assert.IsNull(deviceTypeOfDeviceFromCache);
    }

    [TestMethod]
    public void CacheManager_CachedItemAvailable()
    {
      const int deviceTypeCacheLifetimeMinutes = 1;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      long deviceId = 11;
      var deviceKey = String.Format("Device.{0}", deviceId);
      DeviceTypeEnum? deviceTypeOfDevice = DeviceTypeEnum.TAP66;

      // Add device type for device (Device.11) to cache
      cacheManager.Add(deviceKey, deviceTypeOfDevice, deviceTypeCacheLifetimeMinutes);

      // Retrieving device type for device (Device.11) from cache
      var deviceTypeOfDeviceFromCache = (DeviceTypeEnum?)cacheManager.GetData(deviceKey);

      Assert.AreEqual(deviceTypeOfDevice, deviceTypeOfDeviceFromCache);
    }

    [TestMethod]
    public void CacheManager_CachedItemUnvailable()
    {
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      long deviceId = 11;
      var deviceKey = String.Format("Device.{0}", deviceId);
      // Retrieving device type for device (Device.11) from cache
      var deviceTypeOfDeviceFromCache = (DeviceTypeEnum?)cacheManager.GetData(deviceKey);

      Assert.AreEqual((DeviceTypeEnum?)null, deviceTypeOfDeviceFromCache);
    }

    [TestMethod]
    public void CacheManager_Contains()
    {
      const int deviceTypeCacheLifetimeMinutes = 1;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      long deviceId = 11;
      var deviceKey = String.Format("Device.{0}", deviceId);
      DeviceTypeEnum? deviceTypeOfDevice = DeviceTypeEnum.TAP66;

      Assert.IsFalse(cacheManager.Contains(deviceKey));

      // Add device type for device (Device.11) to cache
      cacheManager.Add(deviceKey, deviceTypeOfDevice, deviceTypeCacheLifetimeMinutes);

      Assert.IsTrue(cacheManager.Contains(deviceKey));
    }

    [TestMethod]
    public void CacheManager_Remove()
    {
      const int deviceTypeCacheLifetimeMinutes = 1;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      long deviceId = 11;
      var deviceKey = String.Format("Device.{0}", deviceId);
      DeviceTypeEnum? deviceTypeOfDevice = DeviceTypeEnum.TAP66;

      // Add device type for device (Device.11) to cache
      cacheManager.Add(deviceKey, deviceTypeOfDevice, deviceTypeCacheLifetimeMinutes);

      Assert.IsTrue(cacheManager.Contains(deviceKey));

      cacheManager.Remove(deviceKey);

      Assert.IsFalse(cacheManager.Contains(deviceKey));
    }

    [TestMethod]
    public void CacheManager_AbsoluteTime()
    {
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);
      
      DateTime nowAndOneMinute = DateTime.Now.AddMinutes(1.0);
      DateTime nowAndOneMinuteInUniversalTime = nowAndOneMinute.ToUniversalTime();

      DateTimeOffset nowAndOneMinuteInUniversalTimeFromCacheManager = (cacheManager as CacheManager).AbsoluteTime(nowAndOneMinute);

      Assert.AreEqual(nowAndOneMinuteInUniversalTime, nowAndOneMinuteInUniversalTimeFromCacheManager);
    }

    [TestMethod]
    public void CacheManager_AbsoluteTimeArgumentOutOfRangeException()
    {
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);

      DateTime dtNow = DateTime.Now;
      AssertEx.Throws<ArgumentOutOfRangeException>(() => (cacheManager as CacheManager).AbsoluteTime(dtNow), "Specified argument was out of the range of valid values.");
    }

    [TestMethod]
    public void CacheManager_CacheManagerConfig()
    {
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      Assert.AreEqual("CacheManagerTest", cacheManagerConfig.CacheManagerName);
      Assert.IsNull(cacheManagerConfig.Config);
    }
  }
}
