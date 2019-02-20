using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Caching;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;

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
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void CanCreateKeyProvider()
    {
      Assert.IsNotNull(ServiceProvider.GetRequiredService<IResponseCachingKeyProvider>());
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
      defaultRequest.Path = "/MYPATH";
      defaultRequest.QueryString = new QueryString($"?projectuid={projectGuid}");
      var keyProvider = new CustomResponseCachingKeyProvider(new DefaultObjectPoolProvider(), new FakeFilterProxy(), null, new FakeResponseCacheOptions());
      var key = keyProvider.GenerateBaseKeyFromRequest(defaultRequest);

      Assert.IsTrue(key.ToLowerInvariant().Contains(projectGuid.ToString()));
    }
  }

  public class FakeFilterProxy : IFilterServiceProxy
  {
    public void ClearCacheItem(string uid, string userId = null)
    { }

    Task<FilterDescriptorSingleResult> IFilterServiceProxy.CreateFilter(string projectUid, FilterRequest request, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }


#pragma warning disable 1998
    public async Task<FilterDescriptor> GetFilter(string projectUid, string filterUid, IDictionary<string, string> customHeaders = null)
#pragma warning restore 1998
    {
      return new FilterDescriptor { FilterJson = "{\"designUID\":\"testDesign\"}", FilterUid = Guid.NewGuid().ToString() };
    }

    public Task<FilterDescriptorSingleResult> CreateFilter(string projectUid, FilterRequest request, IDictionary<string, string> customHeaders = null)
    {
      throw new NotImplementedException();
    }

#pragma warning disable 1998
    public async Task<List<FilterDescriptor>> GetFilters(string projectUid, IDictionary<string, string> customHeaders = null)
#pragma warning restore 1998
    {
      return new List<FilterDescriptor> { new FilterDescriptor { FilterJson = "{\"designUID\":\"testDesign\"}", FilterUid = Guid.NewGuid().ToString() } };
    }

    public void ClearCacheListItem(string projectUid, string userId = null)
    { }
  }
}
