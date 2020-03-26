using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using ILogger = Serilog.ILogger;

namespace CCSS.CWS.Client.UnitTests
{
  [TestClass]
  public abstract class BaseTestClass
  {
    protected string baseUrl = "http://nowhere.really";

    protected Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();
    protected Mock<IServiceResolution> mockServiceResolution = new Mock<IServiceResolution>();

    private IServiceCollection serviceCollection;

    protected IServiceProvider ServiceProvider { get; private set; }

    protected ILogger Log { get; set; }

    protected string userId;

    [TestInitialize]
    public void SetupServices()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure($"Tests::{ GetType().Name}.log"));
      serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddSingleton<IMemoryCache, MemoryCache>();
      serviceCollection.AddSingleton<IDataCache, InMemoryDataCache>();
      
      SetupTestServices(serviceCollection);

      ServiceProvider = serviceCollection.BuildServiceProvider();

      var pretest = PretestChecks();
      if (!pretest)
      {
        Assert.Inconclusive("Pretest checks for test failed");
      }
    }

    protected abstract IServiceCollection SetupTestServices(IServiceCollection services);

    protected virtual bool PretestChecks() => true;

    protected Dictionary<string, string> CustomHeaders() =>
      new Dictionary<string, string>
      {
        {"Content-Type", ContentTypeConstants.ApplicationJson},
        {"Authorization", $"Bearer {ServiceProvider.GetService<ITPaaSApplicationAuthentication>().GetApplicationBearerToken()}"}
      };

    protected bool CheckTPaaS()
    {
      // Get Bearer Token
      try
      {
        var token = ServiceProvider.GetService<ITPaaSApplicationAuthentication>().GetApplicationBearerToken();
        //todomaverick
        //TODO: set userId from JWT
        //string authorization = context.Request.Headers["X-Jwt-Assertion"];
        //var jwtToken = new TPaaSJWT(authorization);
        //userId = jwtToken.IsApplicationToken ? jwtToken.ApplicationId : jwtToken.UserUid.ToString();
        //TEMPORARY use Elspeth's userId
        userId = "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32";
        //NOTE: TPaaS should return Id_token (JWT) as well as Access_token for GetApplicationBearerToken but no longer appears to support this
        return !string.IsNullOrEmpty(token);
      }
      catch (Exception e)
      {
        // No point running the tests if tpass is offline or not authenticating
        return false;
      }
    }
  }
}
