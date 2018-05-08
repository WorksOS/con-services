using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace WebApiTests.Executors
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public void GetAssetRepository()
    {
      var repo = serviceProvider.GetRequiredService<IRepository<IAssetEvent>>();
      Assert.IsNotNull(repo, "Unable to retrieve asset repo from DI");
    }

    [TestMethod]
    public void GetLogger()
    {
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Assert.IsNotNull(loggerFactory, "Unable to retrieve loggerFactory from DI");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorNoValidInput()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "");
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task CanCallAssetIDExecutorWithRadioSerialWithManualDeviceType()
    {
      var assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, 0, "3k45LK");

      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(loggerFactory.CreateLogger<AssetIdExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect AssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");      
    }
      
  }
}
