using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;
using VSS.Productivity3D.AssetMgmt3D.Controllers;
using VSS.Productivity3D.AssetMgmt3D.Helpers;
using VSS.Productivity3D.AssetMgmt3D.Models;
using Xunit;

namespace AssetMgmt.IntegrationTests.Controllers
{
  public class AssetMgmtControllerFixture : TestBase
  {
    private readonly ServiceProvider _serviceProvider;

    public AssetMgmtControllerFixture()
    {
      _serviceProvider = new ServiceCollection()
                         .AddSingleton(Log)
                         .AddSingleton<IConfigurationStore, GenericConfiguration>()
                         .AddSingleton<IAssetRepository, AssetRepository>()
                         .AddSingleton<AssetExtensions>()
                         .BuildServiceProvider();
    }

    [Fact]
    public async Task GetMatchingAssets_Should_return_correct_Assets_When_given_valid_UIDs()
    {
      var assetUids = new List<Guid> { Guid.NewGuid() };

      var assets = new List<Asset>
                   {
                     new Asset { AssetUID = Guid.NewGuid().ToString() }
                   };

      var assetRepository = new Mock<IAssetRepository>();
      assetRepository.Setup(x => x.GetAssets(assetUids))
                     .Returns(Task.FromResult<IEnumerable<Asset>>(assets));

      var assetExtensions = _serviceProvider.GetService<AssetExtensions>();

      var controller = new AssetMgmtController(assetRepository.Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
      };

      var result = await controller.GetMatchingAssets(assetExtensions, assetUids);

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<AssetDisplayModel>(jsonResult.Value);
    }

    [Fact]
    public async Task GetMatchingAssets_Should_return_correct_Assets_When_given_valid_Ids()
    {
      var assetIds = new List<long> { 987654321 };

      var assets = new List<Asset>
                   {
                     new Asset { AssetUID = Guid.NewGuid().ToString() }
                   };

      var assetRepository = new Mock<IAssetRepository>();
      assetRepository.Setup(x => x.GetAssets(assetIds))
                     .Returns(Task.FromResult<IEnumerable<Asset>>(assets));

      var assetExtensions = _serviceProvider.GetService<AssetExtensions>();

      var controller = new AssetMgmtController(assetRepository.Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
      };

      var result = await controller.GetMatchingAssets(assetExtensions, assetIds);

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<AssetDisplayModel>(jsonResult.Value);
    }

    [Fact]
    public async Task GetAssetsLocationData_Should_return_correct_Assets_When_given_valid_Ids()
    {
      var assetUids = new List<Guid> { Guid.NewGuid() };

      var assets = new List<Asset>
                   {
                     new Asset
                     {
                       AssetUID = Guid.NewGuid().ToString(),
                       EquipmentVIN = "5678AB",
                       SerialNumber = "34251",
                       AssetType = "Tough Bulldozer",
                       LastActionedUtc = new DateTime(2020, 3, 26, 9, 10, 0),
                       Name = "Tonka Tough"
                     }
                   };

      var assetRepository = new Mock<IAssetRepository>();
      assetRepository.Setup(x => x.GetAssets(assetUids))
                     .Returns(Task.FromResult<IEnumerable<Asset>>(assets));

      var controller = new AssetMgmtController(assetRepository.Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
      };

      var result = await controller.GetAssetLocationData(assetUids);

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<List<AssetLocationData>>(jsonResult.Value);

      var asset = assets[0];
      var assetLocationData = jsonResult.Value as List<AssetLocationData>;
      Assert.NotNull(assetLocationData);

      var assetData = assetLocationData[0];
      Assert.Equal(asset.AssetUID, assetData.AssetUid.ToString());
      Assert.Equal(asset.EquipmentVIN, assetData.AssetIdentifier);
      Assert.Equal(asset.AssetType, assetData.AssetType);
      Assert.Equal(asset.LastActionedUtc, assetData.LocationLastUpdatedUtc);
      Assert.Equal(asset.SerialNumber, assetData.AssetSerialNumber);
      Assert.Equal(asset.Name, assetData.MachineName);
      Assert.Equal(0, assetData.Latitude);
      Assert.Equal(0, assetData.Longitude);
    }
  }
}
