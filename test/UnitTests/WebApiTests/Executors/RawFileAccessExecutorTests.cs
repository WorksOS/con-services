using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;

namespace WebApiTests.Executors
{
  [TestClass]
  public class RawFileAccessExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public void GetConfigurationStore()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      Assert.IsNotNull(configStore, "Unable to retrieve configStore from DI");
    }

    [TestMethod]
    public void GetLogger()
    {
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public async Task RawFileAccess_NoValidInput()
    {
      throw new NotImplementedException();
      //var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      //var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      //var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<RawFileAccessExecutorTests>(), configStore,
      //  assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      //var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      //Assert.IsNotNull(result, "executor returned nothing");
      //Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      //Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task RawFileAccess_ValidInput()
    {
      throw new NotImplementedException();
    }
  }
}
