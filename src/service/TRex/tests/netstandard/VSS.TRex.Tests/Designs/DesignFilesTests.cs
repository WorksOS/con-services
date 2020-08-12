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

      // Modify the DI Configuration store to specify
    }

    [Fact]
    public void Creation()
    {
      var files = new DesignFiles();
      files.Should().NotBeNull();
    }

    [Fact]
    public void Creation2()
    {
      const long MAX_SIZE = 1000000;

      var files = new DesignFiles(MAX_SIZE);
      files.Should().NotBeNull();
      files.MaxDesignsCacheSize.Should().Be(MAX_SIZE);
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

    [Fact]
    public void Lock_WithDesignEviction_NoContention()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid1 = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, testFileName, true);
      var designUid2 = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, testFileName, true);

      // Work out how big this design is in terms of design cache memory use
      var files = new DesignFiles();
      var design1 = files.Lock(designUid1, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);

      var spaceUsed = files.DesignsCacheSize;
      spaceUsed.Should().BeGreaterThan(1000);

      // Create a new design files cache  just a little bit bigger, and reload the design into it
      files = new DesignFiles((int)Math.Truncate(spaceUsed * 1.2));
      design1 = files.Lock(designUid1, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);
      design1.Should().NotBeNull();
      design1.DesignUid.Should().Be(designUid1);

      // Load the second design and verify the first design is evicted and space used is as expected
      var design2 = files.Lock(designUid2, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);
      design2.Should().NotBeNull();
      design2.DesignUid.Should().Be(designUid2);

      files.DesignsCacheSize.Should().Be(spaceUsed);
    }
  }
}
