using System;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces.GridFabric
{
  public class SurfaceElevationPatchArgumentTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      Guid id = Guid.NewGuid();
      var arg = new SurfaceElevationPatchArgument(id, 1, 2, 3.0, SurveyedSurfacePatchType.EarliestSingleElevation,
        SubGridTreeBitmapSubGridBits.FullMask, new TRex.SurveyedSurfaces.SurveyedSurfaces
        {
          new SurveyedSurface(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "Folder", "FileName"), DateTime.UtcNow, BoundingWorldExtent3D.Full())
        });

      arg.Should().NotBeNull();
      arg.CellSize.Should().Be(3.0);
      arg.IncludedSurveyedSurfaces.Length.Should().Be(1);
      arg.OTGCellBottomLeftX.Should().Be(1);
      arg.OTGCellBottomLeftY.Should().Be(2);
      arg.ProcessingMap.Should().BeEquivalentTo(SubGridTreeBitmapSubGridBits.FullMask);
      arg.SiteModelID.Should().NotBeEmpty();
      arg.SurveyedSurfacePatchType.Should().Be(SurveyedSurfacePatchType.EarliestSingleElevation);
    }

    [Fact]
    public void SetOTGBottomLeftLocation()
    {
      var arg = new SurfaceElevationPatchArgument();
      arg.OTGCellBottomLeftX.Should().Be(0);
      arg.OTGCellBottomLeftY.Should().Be(0);

      arg.SetOTGBottomLeftLocation(10, 11);
      arg.OTGCellBottomLeftX.Should().Be(10);
      arg.OTGCellBottomLeftY.Should().Be(11);
    }

    [Fact]
    public void Test_ToString()
    {
      var arg = new SurfaceElevationPatchArgument();
      arg.ToString().Should().Match("*SiteModel:*OTGOriginBL*CellSize*SurfacePatchType*");
    }

    [Fact]
    public void FromToBinary()
    {
      Guid id = Guid.NewGuid();
      var arg = new SurfaceElevationPatchArgument(id, 1, 2, 3.0, SurveyedSurfacePatchType.EarliestSingleElevation,
        SubGridTreeBitmapSubGridBits.FullMask, new TRex.SurveyedSurfaces.SurveyedSurfaces
        {
          new SurveyedSurface(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "Folder", "FileName"), DateTime.UtcNow, BoundingWorldExtent3D.Full())
        });

      TestBinarizable_ReaderWriterHelper.RoundTripSerialise(arg);
    }

    [Fact]
    public void CacheFingerprint_EmptyArgument()
    {
      var arg = new SurfaceElevationPatchArgument();
      Action act = () => arg.CacheFingerprint();
      act.Should().Throw<Exception>();
    }

    [Fact]
    public void CacheFingerprint_PopulatedArgument()
    {
      Guid id = Guid.NewGuid();
      var arg = new SurfaceElevationPatchArgument(id, 1, 2, 3.0, SurveyedSurfacePatchType.EarliestSingleElevation,
        SubGridTreeBitmapSubGridBits.FullMask, new TRex.SurveyedSurfaces.SurveyedSurfaces
        {
          new SurveyedSurface(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "Folder", "FileName"), DateTime.UtcNow, BoundingWorldExtent3D.Full())
        });

      arg.CacheFingerprint().Should().NotBeNullOrEmpty();
    }
  }
}
