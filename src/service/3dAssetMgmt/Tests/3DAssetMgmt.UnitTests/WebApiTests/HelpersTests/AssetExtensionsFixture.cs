using System;
using System.Collections.Generic;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.AssetMgmt3D.Helpers;
using Xunit;

namespace AssetMgmt.UnitTests.WebApiTests.HelpersTests
{
  public class AssetExtensionsFixture : TestBase
  {
    public static IEnumerable<object[]> Data =>
      new List<object[]>
      {
        new object[] { new Asset{ AssetUID = Guid.NewGuid().ToString() }
        }
      };

    [Fact]
    public void Should_throw_When_given_bad_inputs()
    {
      Assert.Throws<ArgumentNullException>(()=> new AssetExtensions(Log).ConvertDbAssetToDisplayModel(null));
    }

    
    [Theory]
    [MemberData(nameof(Data))]
    public void Should_return_correct_result_When_given_good_inputs(Asset asset)
    {
      var helper = new AssetExtensions(Log);

      var result = helper.ConvertDbAssetToDisplayModel(new List<Asset>() { asset });

      Assert.Equal(0, result.Code);
      Assert.Equal("success", result.Message);
      Assert.Single(result.assetIdentifiers);
    }
  }
}
