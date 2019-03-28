using System;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces
{
  public class SurveyedSurfaceManagerTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var manager = new SurveyedSurfaceManager(StorageMutability.Mutable);
      manager.Should().NotBeNull();
    }

    [Fact]
    public void Add()
    {
      var manager = new SurveyedSurfaceManager(StorageMutability.Mutable);

      var siteModelUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();
      var newSS = manager.Add(siteModelUid, new DesignDescriptor(designUid, "", "", 0.0), DateTime.UtcNow, BoundingWorldExtent3D.Null());

      var results = manager.List(siteModelUid);

      results.Should().NotBeNull();
      results.Count.Should().Be(1);
      results[0].ID.Should().Be(designUid);
    }

    [Fact]
    public void List_Empty()
    {
      var manager = new SurveyedSurfaceManager(StorageMutability.Mutable);

      var results = manager.List(Guid.Empty);

      results.Should().NotBeNull();
      results.Count.Should().Be(0);
    }

    [Fact]
    public void Remove()
    {
      var manager = new SurveyedSurfaceManager(StorageMutability.Mutable);

      var siteModelUid = Guid.NewGuid();
      manager.Add(siteModelUid, DesignDescriptor.Null(), DateTime.UtcNow, BoundingWorldExtent3D.Null());

      var results = manager.List(siteModelUid);

      results.Should().NotBeNull();
      results.Count.Should().Be(1);

      manager.Remove(siteModelUid, results[0].ID).Should().BeTrue();
      manager.List(siteModelUid).Count.Should().Be(0);
    }

    [Fact]
    public void Remove_Empty()
    {
      var manager = new SurveyedSurfaceManager(StorageMutability.Mutable);
      manager.Remove(Guid.NewGuid(), Guid.NewGuid()).Should().BeFalse();
    }
  }
}
