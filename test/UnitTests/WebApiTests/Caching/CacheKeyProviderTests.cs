using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Caching;

namespace VSS.Productivity3D.WebApiTests.Caching
{
  public class FakeResponseCacheOptions : ResponseCachingOptions, IOptions<ResponseCachingOptions>
  {
    public ResponseCachingOptions Value => new ResponseCachingOptions();
  }

  [TestClass]
  public class CacheKeyProviderTests
  {

    public IServiceProvider ServiceProvider;

    [TestInitialize]
    public void InitTest()
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.TryAdd(ServiceDescriptor.Singleton<ObjectPoolProvider, DefaultObjectPoolProvider>());
      serviceCollection.AddTransient<IOptions<ResponseCachingOptions>, FakeResponseCacheOptions>();
      serviceCollection.AddTransient<IFilterServiceProxy, FakeFilterProxy>();
      serviceCollection.TryAdd(ServiceDescriptor.Singleton<IResponseCachingKeyProvider, CustomResponseCachingKeyProvider>());
      this.ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateKeyProvider()
    {
      Assert.IsNotNull(this.ServiceProvider.GetRequiredService<IResponseCachingKeyProvider>());
    }

    [TestMethod]
    public void ShouldNotAppendProjectUidToBaseKey()
    {
      var defaultContext = new DefaultHttpContext();
      var defaultRequest = new DefaultHttpRequest(defaultContext)
      {
        Method = "GET",
        Path = "/MYPATH"
      };
      var keyProvider = new CustomResponseCachingKeyProvider(new DefaultObjectPoolProvider(), new FakeFilterProxy(), null, new FakeResponseCacheOptions());
      var key = keyProvider.GenerateBaseKeyFromRequest(defaultRequest);

      Assert.IsFalse(key.ToLowerInvariant().Contains("projectuid"));
    }

    [TestMethod]
    public void ShouldAppendProjectUidToBaseKey()
    {
      var defaultContext = new DefaultHttpContext();
      var defaultRequest = new DefaultHttpRequest(defaultContext) { Method = "GET" };
      var projectGuid = Guid.NewGuid();
      defaultRequest.Path = $"/MYPATH";
      defaultRequest.QueryString = new QueryString($"?projectuid={projectGuid}");
      var keyProvider = new CustomResponseCachingKeyProvider(new DefaultObjectPoolProvider(), new FakeFilterProxy(), null, new FakeResponseCacheOptions());
      var key = keyProvider.GenerateBaseKeyFromRequest(defaultRequest);

      Assert.IsTrue(key.ToLowerInvariant().Contains(projectGuid.ToString()));
    }

    [TestMethod]
    public void CanFindFilterUidInBaseKeyCompareHash()
    {
      var defaultContext = new DefaultHttpContext();
      var defaultRequest = new DefaultHttpRequest(defaultContext) { Method = "GET" };
      var projectGuid = Guid.NewGuid();
      defaultRequest.Path = $"/MYPATH";
      defaultRequest.QueryString = new QueryString($"?projectuid={projectGuid}&filteruid={projectGuid}");

      var keyProvider = new CustomResponseCachingKeyProvider(new DefaultObjectPoolProvider(), new FakeFilterProxy(), null, new FakeResponseCacheOptions());
      var key = keyProvider.GenerateBaseKeyFromRequest(defaultRequest);

      var keyProvider1 = new CustomResponseCachingKeyProvider(new DefaultObjectPoolProvider(), new FakeFilterProxy(), null, new FakeResponseCacheOptions());
      var key1 = keyProvider.GenerateBaseKeyFromRequest(defaultRequest);

      var parsedHash = this.ServiceProvider.GetRequiredService<IResponseCachingKeyProvider>().ExtractFilterHashFromKey(key);
      var parsedHash1 = this.ServiceProvider.GetRequiredService<IResponseCachingKeyProvider>().ExtractFilterHashFromKey(key1);

      Assert.AreEqual(parsedHash, parsedHash1);
    }
  }

  public class FakeFilterProxy : IFilterServiceProxy
  {
    public void ClearCacheItem(string uid, string userId = null)
    { }

    public async Task<FilterDescriptor> GetFilter(string projectUid, string filterUid, IDictionary<string, string> customHeaders = null)
    {
      return new FilterDescriptor() { FilterJson = "{\"designUID\":\"testDesign\"}", FilterUid = Guid.NewGuid().ToString() };
    }

    public async Task<List<FilterDescriptor>> GetFilters(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      return new List<FilterDescriptor>() { new FilterDescriptor() { FilterJson = "{\"designUID\":\"testDesign\"}", FilterUid = Guid.NewGuid().ToString() } };
    }

    public void ClearCacheListItem(string projectUid, string userId = null)
    { }
  }
}