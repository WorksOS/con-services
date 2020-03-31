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
using VSS.Productivity3D.AssetMgmt3D.Models;
using Xunit;

namespace AssetMgmt.IntegrationTests.Controllers
{
  public class AssetMgmtControllerTests : TestBase
  {
    private readonly ServiceProvider _serviceProvider;

    public AssetMgmtControllerTests()
    {
      _serviceProvider = new ServiceCollection()
                         .AddSingleton(Log)
                         .AddSingleton<IConfigurationStore, GenericConfiguration>()
                         .AddSingleton<IAssetRepository, AssetRepository>()
                         .BuildServiceProvider();
    }

    [Fact]
    public async Task GetMatchingAssetsShouldReturnAssetFromUid()
    {
      var assetUids = new List<Guid> { Guid.NewGuid() };

      var assets = new List<Asset>
                   {
                     new Asset { AssetUID = Guid.NewGuid().ToString() }
                   };

      var assetRepository = new Mock<IAssetRepository>();
      assetRepository.Setup(x => x.GetAssets(assetUids))
                     .Returns(Task.FromResult(assets));

      var controller = new AssetMgmtController(assetRepository.Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
      };

      var result = await controller.GetMatchingAssets(assetUids);

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<AssetDisplayModel>(jsonResult.Value);
    }

    [Fact]
    public async Task GetMatchingAssetsShouldReturnAssetsFromLegacyId()
    {
      var assetIds = new List<long> { 987654321 };

      var assets = new List<Asset>
                   {
                     new Asset { AssetUID = Guid.NewGuid().ToString() }
                   };

      var assetRepository = new Mock<IAssetRepository>();
      assetRepository.Setup(x => x.GetAssets(assetIds))
                     .Returns(Task.FromResult(assets));

      var controller = new AssetMgmtController(assetRepository.Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
      };

      var result = await controller.GetMatchingAssets(assetIds);

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<AssetDisplayModel>(jsonResult.Value);
    }

    [Fact]
    public async Task GetAssetsLocationDataShouldReturnFullyHydratedAssetDto()
    {
      var assetRepository = new Mock<IAssetRepository>();

      var controller = new AssetMgmtController(assetRepository.Object)
      {
        ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
      };

      var result = await controller.GetAssetLocationData(new List<Guid> { Guid.Parse("6cb6fa71-9800-4700-b7ff-c62014970deb") });

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<List<AssetLocationData>>(jsonResult.Value);

      var assetLocationData = jsonResult.Value as List<AssetLocationData>;
      Assert.NotNull(assetLocationData);

      var assetData = assetLocationData[0];
      Assert.Equal("6cb6fa71-9800-4700-b7ff-c62014970deb", assetData.AssetUid.ToString());
      Assert.Equal("970DEB", assetData.AssetIdentifier);
      Assert.Equal("C62014", assetData.AssetSerialNumber);
      Assert.Equal("30/03/2020 2:45:04 PM", assetData.LocationLastUpdatedUtc.ToString());
      Assert.Equal("Dump Truck", assetData.AssetType);
      Assert.Equal("Tonka Dump Truck", assetData.MachineName);
      Assert.Equal(0, assetData.Latitude);
      Assert.Equal(0, assetData.Longitude);
    }

    [Fact]
    public async Task GetAssetsLocationDataShouldReturnMultipleAssets()
    {
      var assetRepository = new Mock<IAssetRepository>();

      var controller = new AssetMgmtController(assetRepository.Object)
                       {
                         ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = _serviceProvider } }
                       };

      var result = await controller.GetAssetLocationData(new List<Guid> { Guid.Parse("6b4dc385-b517-4baa-9419-d9dc58f808c5"),
                                                                          Guid.Parse("6cb6fa71-9800-4700-b7ff-c62014970deb")});

      Assert.IsType<JsonResult>(result);

      var jsonResult = result as JsonResult;

      Assert.NotNull(jsonResult);
      Assert.IsType<List<AssetLocationData>>(jsonResult.Value);

      var assetLocationData = jsonResult.Value as List<AssetLocationData>;
      Assert.NotNull(assetLocationData);

      Assert.Equal(2, assetLocationData.Count);
      Assert.Equal("6cb6fa71-9800-4700-b7ff-c62014970deb", assetLocationData[0].AssetUid.ToString());
      Assert.Equal("6b4dc385-b517-4baa-9419-d9dc58f808c5", assetLocationData[1].AssetUid.ToString());
    }
  }
}
