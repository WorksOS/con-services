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
    public const string TESTFILENAME = "Bug36372.ttm";

    private const int SAMPLE_DESIGN_SPACE_USED_IN_CACHE = 2214624;

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
      const int MAX_EVICTION_ITERATIONS = 10;

      var files = new DesignFiles(MAX_SIZE, MAX_EVICTION_ITERATIONS);
      files.Should().NotBeNull();
      files.MaxDesignsCacheSize.Should().Be(MAX_SIZE);
      files.MaxWaitIterationsDuringDesignEviction.Should().Be(MAX_EVICTION_ITERATIONS);
    }

    [Fact]
    public void NumDesignsInCache()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      var files = new DesignFiles();
      files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      files.NumDesignsInCache().Should().Be(1);
    }

    [Fact]
    public void SpaceUsed()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      var files = new DesignFiles();
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);
      files.DesignsCacheSize.Should().Be(SAMPLE_DESIGN_SPACE_USED_IN_CACHE);

      design.Should().NotBeNull();
    }

    [Fact]
    public void FreeSpace()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      var files = new DesignFiles(SAMPLE_DESIGN_SPACE_USED_IN_CACHE + 1000, 10);
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      design.Should().NotBeNull();
      loadResult.Should().Be(DesignLoadResult.Success);
      files.FreeSpaceInCache.Should().Be(1000);
    }

    [Fact]
    public void Lock_SingleDesign_NoContention()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      var files = new DesignFiles();
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      design.Should().NotBeNull();
      design.Locked.Should().BeTrue();
    }

    [Fact]
    public void Unlock_SingleDesign_NoContention()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      var files = new DesignFiles();
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      design.Should().NotBeNull();

      files.UnLock(designUid, design).Should().BeTrue();
      design.Locked.Should().BeFalse(); 
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
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      var files = new DesignFiles();
      var design = files.Lock(designUid, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);

      var result = files.RemoveDesignFromCache(designUid, mockDesign.Object, siteModel.ID, removeFromStorage);
      result.Should().BeTrue();
      files.NumDesignsInCache().Should().Be(0);

      mockDesign.Verify(x => x.RemoveFromStorage(It.IsAny<Guid>(), It.IsAny<string>()), removeFromStorage ? Times.Once() : Times.Never());
    }

    [Fact]
    public void Lock_WithDesignEviction_FailWithUnresolvableContention()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid1 = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);
      var designUid2 = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      // Create a new design files cache  just a little bit bigger that the size it requires, and reload the design into it
      var files = new DesignFiles((int)Math.Truncate(SAMPLE_DESIGN_SPACE_USED_IN_CACHE * 1.2), 10);
      var design1 = files.Lock(designUid1, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);
      design1.Should().NotBeNull();
      design1.DesignUid.Should().Be(designUid1);
      files.DesignsCacheSize.Should().Be(SAMPLE_DESIGN_SPACE_USED_IN_CACHE);

      // Load the second design and verify the first design is evicted and space used is as expected
      // leave the first design locked so that it cannot be evicted
      var design2 = files.Lock(designUid2, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out loadResult);
      loadResult.Should().Be(DesignLoadResult.InsufficientMemory);
      design2.Should().BeNull();

      files.DesignsCacheSize.Should().Be(SAMPLE_DESIGN_SPACE_USED_IN_CACHE);
    }

    [Fact]
    public void Lock_WithDesignEviction_SucceedWithDesignRemoval()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid1 = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);
      var designUid2 = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, TESTFILENAME, true);

      // Create a new design files cache  just a little bit bigger that the size it requires, and reload the design into it
      var files = new DesignFiles((int)Math.Truncate(SAMPLE_DESIGN_SPACE_USED_IN_CACHE * 1.2), 10);
      var design1 = files.Lock(designUid1, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out var loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);
      design1.Should().NotBeNull();
      design1.DesignUid.Should().Be(designUid1);
      files.DesignsCacheSize.Should().Be(SAMPLE_DESIGN_SPACE_USED_IN_CACHE);

      // Load the second design and verify the first design is evicted and space used is as expected
      // Unlock the first design to allow it to be evicted from the cache
      files.UnLock(designUid1, design1).Should().BeTrue();

      var design2 = files.Lock(designUid2, siteModel.ID, SubGridTreeConsts.DefaultCellSize, out loadResult);
      loadResult.Should().Be(DesignLoadResult.Success);
      design2.Should().NotBeNull();
      design2.DesignUid.Should().Be(designUid2);

      files.DesignsCacheSize.Should().Be(SAMPLE_DESIGN_SPACE_USED_IN_CACHE);
    }

  }
}
