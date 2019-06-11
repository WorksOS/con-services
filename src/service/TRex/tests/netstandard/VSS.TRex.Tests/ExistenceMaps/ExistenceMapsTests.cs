using System;
using FluentAssertions;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
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

    [Fact]
    public void SetExistenceMapViaKey()
    {
      var ProjectUid = Guid.NewGuid();
      var bitMask = new SubGridTreeSubGridExistenceBitMask();

      var em = new TRex.ExistenceMaps.ExistenceMaps();
      em.SetExistenceMap(new NonSpatialAffinityKey(ProjectUid, "SetExistenceMapViaKey"), bitMask);
    }

    [Fact]
    public void SetExistenceMapViaDescriptor()
    {
      var ProjectUid = Guid.NewGuid();
      var DesignUid = Guid.NewGuid();
      var bitMask = new SubGridTreeSubGridExistenceBitMask();

      var em = new TRex.ExistenceMaps.ExistenceMaps();
      em.SetExistenceMap(ProjectUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, DesignUid, bitMask);
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
    public void GetSingleExistenceMapViaKey_EmptyBitMask()
    {
      var ProjectUid = Guid.NewGuid();
      var setBitMask = new SubGridTreeSubGridExistenceBitMask();

      var em = new TRex.ExistenceMaps.ExistenceMaps();
      em.SetExistenceMap(new NonSpatialAffinityKey(ProjectUid, "GetExistenceMapViaKey"), setBitMask);

      var getBitMask = em.GetSingleExistenceMap(new NonSpatialAffinityKey(ProjectUid, "GetExistenceMapViaKey"));
      TestBitMasksAreTheSame(setBitMask, getBitMask);
    }

    [Fact]
    public void GetSingleExistenceMapViaDescriptor_EmptyBitMask()
    {
      var ProjectUid = Guid.NewGuid();
      var DesignUid = Guid.NewGuid();
      var setBitMask = new SubGridTreeSubGridExistenceBitMask();

      var em = new TRex.ExistenceMaps.ExistenceMaps();
      em.SetExistenceMap(ProjectUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, DesignUid, setBitMask);

      var getBitMask = em.GetSingleExistenceMap(ProjectUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, DesignUid);
      TestBitMasksAreTheSame(setBitMask, getBitMask);
    }

    public void Test_GetCombinedExistenceMap_ViaKeys(SubGridTreeSubGridExistenceBitMask setBitMask1,
      SubGridTreeSubGridExistenceBitMask setBitMask2)
    {
      var ProjectUid = Guid.NewGuid();

      var em = new TRex.ExistenceMaps.ExistenceMaps();

      em.SetExistenceMap(new NonSpatialAffinityKey(ProjectUid, "GetCombinedExistenceMapViaKeys1"), setBitMask1);
      em.SetExistenceMap(new NonSpatialAffinityKey(ProjectUid, "GetCombinedExistenceMapViaKeys2"), setBitMask2);

      var getBitMask = em.GetCombinedExistenceMap(new INonSpatialAffinityKey[]
      {
        new NonSpatialAffinityKey(ProjectUid, "GetCombinedExistenceMapViaKeys1"),
        new NonSpatialAffinityKey(ProjectUid, "GetCombinedExistenceMapViaKeys2")
      });

      setBitMask1.SetOp_OR(setBitMask2);
      TestBitMasksAreTheSame(setBitMask1, getBitMask);
    }

    [Fact]
    public void GetCombinedExistenceMapViaKeys_EmptyBitMask()
    {
      Test_GetCombinedExistenceMap_ViaKeys(new SubGridTreeSubGridExistenceBitMask(), new SubGridTreeSubGridExistenceBitMask());
    }

    [Fact]
    public void GetCombinedExistenceMapViaKeys_NonEmptyBitMask()
    {
      Test_GetCombinedExistenceMap_ViaKeys(MakeSprinkledBitMask(3), MakeSprinkledBitMask(5));
    }

    private void Test_GetCombinedExistenceMap_ViaDescriptor(SubGridTreeSubGridExistenceBitMask setBitMask1,
      SubGridTreeSubGridExistenceBitMask setBitMask2)
    {
      var ProjectUid = Guid.NewGuid();
      var DesignUid1 = Guid.NewGuid();
      var DesignUid2 = Guid.NewGuid();

      var em = new TRex.ExistenceMaps.ExistenceMaps();

      em.SetExistenceMap(ProjectUid, Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, DesignUid1, setBitMask1);
      em.SetExistenceMap(ProjectUid, Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, DesignUid2, setBitMask2);

      var getBitMask = em.GetCombinedExistenceMap(ProjectUid, new Tuple<long, Guid>[]
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
