using System;
using FluentAssertions;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.ExistenceMaps
{
  public class ExistenceMapsTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private SubGridTreeSubGridExistenceBitMask MakeSprinkledBitMask(int sprinkleFactor)
    {
      var result = new SubGridTreeSubGridExistenceBitMask();

      SubGridUtilities.SubGridDimensionalIterator((x, y) => { result[x * sprinkleFactor, y * sprinkleFactor] = true; });

      return result;
    }

    [Fact]
    public void Creation()
    {
      var em = new TRex.ExistenceMaps.ExistenceMaps();
      em.Should().NotBeNull();
    }

    /// <summary>
    /// Note: This method destructive modifies bitmask 'one'
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    private void TestBitMasksAreTheSame(ISubGridTreeBitMask one, ISubGridTreeBitMask two)
    {
      // Ensure bit counts are the same
      one.CountBits().Should().Be(two.CountBits());

      // XOR the two bit masks together. This should clear all set bits resulting in a BitCount of zero
      one.SetOp_XOR(two);
      one.CountBits().Should().Be(0);
    }

    [Fact]
    public void GetSingleExistenceMapViaDescriptor_EmptyBitMask()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var DesignUid = Guid.NewGuid();
      var setBitMask = new SubGridTreeSubGridExistenceBitMask();

      siteModel.PrimaryStorageProxy.WriteStreamToPersistentStore
        (siteModel.ID,
        BaseExistenceMapRequest.CacheKeyString(Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, DesignUid),
        FileSystemStreamType.DesignTopologyExistenceMap,
        (new SubGridTreeSubGridExistenceBitMask()).ToStream(), null)
      .Should().Be(FileSystemErrorStatus.OK);

      var em = new TRex.ExistenceMaps.ExistenceMaps();

      var getBitMask = em.GetSingleExistenceMap(siteModel.ID, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, DesignUid);
      TestBitMasksAreTheSame(setBitMask, getBitMask);
    }

    private void Test_GetCombinedExistenceMap_ViaDescriptor(SubGridTreeSubGridExistenceBitMask setBitMask1,
      SubGridTreeSubGridExistenceBitMask setBitMask2)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var DesignUid1 = Guid.NewGuid();
      var DesignUid2 = Guid.NewGuid();

      var em = new TRex.ExistenceMaps.ExistenceMaps();

      siteModel.PrimaryStorageProxy.WriteStreamToPersistentStore
        (siteModel.ID,
        BaseExistenceMapRequest.CacheKeyString(Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, DesignUid1),
        FileSystemStreamType.DesignTopologyExistenceMap,
        setBitMask1.ToStream(), setBitMask1.ToStream())
      .Should().Be(FileSystemErrorStatus.OK);

      siteModel.PrimaryStorageProxy.WriteStreamToPersistentStore
        (siteModel.ID,
        BaseExistenceMapRequest.CacheKeyString(Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, DesignUid2),
        FileSystemStreamType.DesignTopologyExistenceMap,
        setBitMask2.ToStream(), setBitMask2.ToStream())
      .Should().Be(FileSystemErrorStatus.OK);

      var getBitMask = em.GetCombinedExistenceMap(siteModel.ID, new Tuple<long, Guid>[]
      {
        new Tuple<long, Guid> (Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, DesignUid1),
        new Tuple<long, Guid> (Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, DesignUid2)
      });

      setBitMask1.SetOp_OR(setBitMask2);
      TestBitMasksAreTheSame(setBitMask1, getBitMask);
    }

    [Fact]
    public void GetCombinedExistenceMapViaDescriptor_EmptyBitMask()
    {
      Test_GetCombinedExistenceMap_ViaDescriptor(new SubGridTreeSubGridExistenceBitMask(), new SubGridTreeSubGridExistenceBitMask());
    }

    [Fact]
    public void GetCombinedExistenceMapViaDescriptor_NonEmptyBitMask()
    {
      Test_GetCombinedExistenceMap_ViaDescriptor(MakeSprinkledBitMask(3), MakeSprinkledBitMask(5));
    }
  }
}
