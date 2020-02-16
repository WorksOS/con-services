using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Nighthawk.ReferenceIdentifierService.Data;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Tests.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.CacheManagerTests
{
  [TestClass]
  public class CacheManagerTests
  {
    private const int _cacheLifetimeMinutes = 1;
    private static CustomerReference _store;
    private static string _cacheKey;
    

    public class CustomerReference
    {
      public long Id { get; set; }
      public long StoreId { get; set; }
      public Guid UID { get; set; }
      public string Alias { get; set; }
      public string Value { get; set; }
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
      _store = new CustomerReference()
      {
        Id = 1234,
        StoreId = 56,
        UID = Guid.Empty,
        Alias = "CustomerCode",
        Value = "42"
      };

      _cacheKey = String.Format("CacheManagerTestsCacheKey.{0}", _store.StoreId);
    }

    [Ignore]
    [TestMethod]
    public void CacheManagerTests_CachedItemExpired()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      _cacheManager.Add(_cacheKey, _store, _cacheLifetimeMinutes);
      TimeSpan interval = new TimeSpan(0, _cacheLifetimeMinutes, 3);
      Thread.Sleep(interval);
      var objectFromCache = (CustomerReference)_cacheManager.GetData(_cacheKey);

      Assert.IsNotNull(_store);
      Assert.IsNull(objectFromCache);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CacheManagerTests_CacheManagerNameIsNull()
    {
      string cacheManagerName = null;
      ICacheManagerConfig cacheManagerConfig = new CacheManagerConfig(cacheManagerName);
      ICacheManager cacheManager = new CacheManager(cacheManagerConfig);
    }

    [TestMethod]
    public void CacheManagerTests_CachedItemIsNull()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      _cacheManager.Add(_cacheKey, null, _cacheLifetimeMinutes);
      var objectFromCache = (CustomerReference)_cacheManager.GetData(_cacheKey);

      Assert.IsNull(objectFromCache);
    }

    [TestMethod]
    public void CacheManagerTests_GetsClosestCacheItemTest()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      _cacheManager.Add("http://Test", new Credentials {  EncryptedPassword = "Password", UserName = "username" }, _cacheLifetimeMinutes);
      _cacheManager.Add("http://Test/a", new Credentials { EncryptedPassword = "Passworda", UserName = "username" }, _cacheLifetimeMinutes);
      _cacheManager.Add("http://Test/b", new Credentials { EncryptedPassword = "Passwordb", UserName = "username" }, _cacheLifetimeMinutes);
      _cacheManager.Add("http://Test/c", new Credentials { EncryptedPassword = "Passwordc", UserName = "username" }, _cacheLifetimeMinutes);
      _cacheManager.Add("http://Test/a/a/a", new Credentials { EncryptedPassword = "Passwordd", UserName = "username" }, _cacheLifetimeMinutes);

      var objectfromCache = (Credentials)_cacheManager.GetClosestData("http://Test/a/b");
      Assert.IsNotNull(objectfromCache);
      Assert.AreEqual("Passworda", objectfromCache.EncryptedPassword);
    }

    [TestMethod]
    public void CacheManagerTests_CachedItemAvailable()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      _cacheManager.Add(_cacheKey, _store, _cacheLifetimeMinutes);
      var objectFromCache = (CustomerReference)_cacheManager.GetData(_cacheKey);

      Assert.AreEqual(_store, objectFromCache);
    }

    [TestMethod]
    public void CacheManagerTests_CachedItemUnvailable()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      var objectFromCache = (CustomerReference)_cacheManager.GetData(_cacheKey);

      Assert.AreEqual((CustomerReference)null, objectFromCache);
    }

    [TestMethod]
    public void CacheManagerTests_Contains()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      Assert.IsFalse(_cacheManager.Contains(_cacheKey));

      _cacheManager.Add(_cacheKey, _store, _cacheLifetimeMinutes);
      Assert.IsTrue(_cacheManager.Contains(_cacheKey));
    }

    [TestMethod]
    public void CacheManagerTests_Remove()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      _cacheManager.Add(_cacheKey, _store, _cacheLifetimeMinutes);
      Assert.IsTrue(_cacheManager.Contains(_cacheKey));

      _cacheManager.Remove(_cacheKey);
      Assert.IsFalse(_cacheManager.Contains(_cacheKey));
    }

    [TestMethod]
    public void CacheManagerTests_AbsoluteTime()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      DateTime nowAndOneMinute = DateTime.Now.AddMinutes(1.0);
      DateTime nowAndOneMinuteInUniversalTime = nowAndOneMinute.ToUniversalTime();
      DateTimeOffset nowAndOneMinuteInUniversalTimeFromCacheManager = (_cacheManager as CacheManager).AbsoluteTime(nowAndOneMinute);

      Assert.AreEqual(nowAndOneMinuteInUniversalTime, nowAndOneMinuteInUniversalTimeFromCacheManager);
    }

    [TestMethod]
    public void CacheManagerTests_AbsoluteTimeArgumentOutOfRangeException()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      DateTime dtNow = DateTime.Now;
      AssertEx.Throws<ArgumentOutOfRangeException>(() => (_cacheManager as CacheManager).AbsoluteTime(dtNow), "Specified argument was out of the range of valid values.");
    }

    [TestMethod]
    public void CacheManagerTests_CacheManagerConfig()
    {
      ICacheManagerConfig _cacheManagerConfig = new CacheManagerConfig("CacheManagerTest");
      ICacheManager _cacheManager = new CacheManager(_cacheManagerConfig);

      Assert.IsNull(_cacheManagerConfig.Config);
    }
  }
}
