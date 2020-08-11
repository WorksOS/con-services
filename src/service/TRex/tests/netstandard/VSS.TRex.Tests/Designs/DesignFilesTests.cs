using System;
using FluentAssertions;
using Moq;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs
{
  public class DesignFilesTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    public const string testFileName = "Bug36372.ttm";

    public DesignFilesTests(DITAGFileAndSubGridRequestsWithIgniteFixture fixture)
    {
      fixture.ClearDynamicFixtureContent();
      fixture.SetupFixture();
    }

    [Fact]
    public void Creation()
    {
      var files = new DesignFiles();
      files.Should().NotBeNull();
    }

    [Fact]
    public void NumDesignsInCache()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, testFileName, true);

      var files = new DesignFiles();
      files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      files.NumDesignsInCache().Should().Be(1);
    }

    [Fact]
    public void Lock_SingleDesign_NoContention()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, testFileName, true);

      var files = new DesignFiles();
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      design.Should().NotBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RemoveDesignFromCache(bool removeFromStorage)
    {
      // Calls the method to remove the design from storage
      var mockDesign = new Mock<IDesignBase>();
      mockDesign.Setup(x => x.RemoveFromStorage(It.IsAny<Guid>(), It.IsAny<string>())).Returns(true);
      mockDesign.Setup(x => x.FileName).Returns("TheFileName");

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, testFileName, true);

      var files = new DesignFiles();
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      var result = files.RemoveDesignFromCache(designUid, mockDesign.Object, siteModel.ID, removeFromStorage);
      result.Should().BeTrue();
      files.NumDesignsInCache().Should().Be(0);

      mockDesign.Verify(x => x.RemoveFromStorage(It.IsAny<Guid>(), It.IsAny<string>()), removeFromStorage ? Times.Once() : Times.Never());
    }
  }
}
