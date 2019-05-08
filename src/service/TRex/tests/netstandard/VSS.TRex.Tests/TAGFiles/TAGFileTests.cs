using System.IO;
using FluentAssertions;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.TAGFiles
{
  public class TAGFileTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void CompareSiteModels(ISiteModel sm1, ISiteModel sm2, int expectedExistanceMapSubGridCount, int expectedCallPassCount)
    {
      var bitCount1 = sm1.ExistenceMap.CountBits();
      var bitCount2 = sm2.ExistenceMap.CountBits();

      // Check both site models have the same number of sub grids in their existence maps
      bitCount1.Should().Be(bitCount2);
      bitCount1.Should().Be(expectedExistanceMapSubGridCount);

      // Check the content of the existence maps is identical
      var testMap = new SubGridTreeSubGridExistenceBitMask();
      testMap.SetOp_OR(sm1.ExistenceMap);
      testMap.SetOp_XOR(sm2.ExistenceMap);
      testMap.CountBits().Should().Be(0);

      // The expected distribution of cell pass counts
      long[] expectedCounts = { 93, 687, 68, 385, 57, 598, 65, 986, 52, 63, 0, 0, 0, 0, 0 };

      // Scan the leaves in each model and count cell passes
      int sm1Count = 0;
      int sm1LeafCount = 0;
      long[] actualCounts1 = new long[15];
      int segmentCount1 = 0;
      int nonNullCellCount1 = 0;
      sm1.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        sm1LeafCount++;
        var leaf = SubGridUtilities.LocateSubGridContaining(sm1.PrimaryStorageProxy, sm1.Grid, address.X, address.Y, sm1.Grid.NumLevels, false, false) as IServerLeafSubGrid;
        var iterator = new SubGridSegmentIterator(leaf, sm1.PrimaryStorageProxy);
        while (iterator.MoveNext())
        {
          segmentCount1++;
          TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) =>
          {
            var passCount = (int) iterator.CurrentSubGridSegment.PassesData.PassCount(x, y);
            sm1Count += passCount;
            if (passCount > 0)
            {
              nonNullCellCount1++;
              actualCounts1[passCount - 1]++;
            }
          });
        }
      });

      int sm2Count = 0;
      int sm2LeafCount = 0;
      long[] actualCounts2 = new long[15];
      int segmentCount2 = 0;
      int nonNullCellCount2 = 0;
      sm2.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        sm2LeafCount++;
        var leaf = SubGridUtilities.LocateSubGridContaining(sm2.PrimaryStorageProxy, sm2.Grid, address.X, address.Y, sm2.Grid.NumLevels, false, false) as IServerLeafSubGrid;
        var iterator = new SubGridSegmentIterator(leaf, sm2.PrimaryStorageProxy);
        while (iterator.MoveNext())
        {
          segmentCount2++;
          TRex.SubGridTrees.Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((x, y) =>
          {
            var passCount = (int) iterator.CurrentSubGridSegment.PassesData.PassCount(x, y);
            sm2Count += passCount;
            if (passCount > 0)
            {
              nonNullCellCount2++;
              actualCounts2[passCount - 1]++;
            }
          });
        }
      });

      segmentCount1.Should().Be(segmentCount2);
      segmentCount1.Should().Be(sm1LeafCount);

      sm1LeafCount.Should().Be(expectedExistanceMapSubGridCount);
      sm1Count.Should().Be(sm2Count);

      sm2LeafCount.Should().Be(expectedExistanceMapSubGridCount);
      sm1Count.Should().Be(expectedCallPassCount);

      actualCounts1.Should().BeEquivalentTo(actualCounts2);
      actualCounts1.Should().BeEquivalentTo(expectedCounts);

      nonNullCellCount1.Should().Be(nonNullCellCount2);
      nonNullCellCount1.Should().Be(2538);
    }

    [Fact]
    public void Test_TAGFile_DropSiteModel_WithImmutableRepresentation()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      // Build the model, assess the contents, then drop the model and verify the contents are the same when
      // re-read from the persistence layer
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, true);
      var siteModel2 = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, false);

      CompareSiteModels(siteModel, siteModel2, 12, 16525);
    }

    [Fact]
    public void Test_TAGFile_DropSiteModel_WithMutableRepresentation()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      // Build the model, assess the contents, then drop the model and verify the contents are the same when
      // re-read from the persistence layer
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, false, false);
      var siteModel2 = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, false);

      siteModel.ExistenceMap.CountBits().Should().Be(12);
      siteModel2.ExistenceMap.CountBits().Should().Be(12);

      CompareSiteModels(siteModel, siteModel2, 12, 16525);
    }

    [Fact]
    public void Test_TAGFile_DropSiteModel_WithMixedRepresentation()
    {
      var tagFiles = new[]
      {
        Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag"),
      };

      // Build the model, assess the contents, then drop the model and verify the contents are the same when
      // re-read from the persistence layer
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, false, false);
      var siteModel2 = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, false, true);

      CompareSiteModels(siteModel, siteModel2, 12, 16525);

      siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, false);
      siteModel2 = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _, true, true);

      CompareSiteModels(siteModel, siteModel2, 12, 16525);
    }
  }
}
