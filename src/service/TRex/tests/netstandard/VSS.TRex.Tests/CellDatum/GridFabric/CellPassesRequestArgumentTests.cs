using System;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellPassesRequestArgumentTests
  {
    [Fact]
    public void Test_CellPassesRequestArgument_ApplicationService_Creation()
    {
      var arg = new CellPassesRequestArgument_ApplicationService();
      Assert.NotNull(arg);
    }

    [Fact]
    public void Test_CellPassesRequestArgument_ApplicationService_CreationWithArgs()
    {
      Guid siteModelID = Guid.NewGuid();
      bool coordsAreGrid = false;
      XYZ latLngPoint = new XYZ(12345.6789, 98765.4321);
      IFilterSet filters = new FilterSet();
      var arg = new CellPassesRequestArgument_ApplicationService(siteModelID, coordsAreGrid, latLngPoint, filters);
      arg.Should().NotBeNull();
      arg.ProjectID.Should().Be(siteModelID);
      arg.CoordsAreGrid.Should().Be(coordsAreGrid);
      arg.Point.Should().Be(latLngPoint);
      arg.Filters.Should().Be(filters);
    }

    [Fact]
    public void Test_CellPassesRequestArgument_ClusterCompute_Creation()
    {
      var arg = new CellPassesRequestArgument_ClusterCompute();
      arg.Should().NotBeNull();
    }

    [Fact]
    public void Test_CellPassesRequestArgument_ClusterCompute_CreationWithArgs()
    {
      Guid siteModelID = Guid.NewGuid();
      XYZ neeCoords = new XYZ(12345.6789, 98765.4321);
      int otgCellX = 16234;
      int otgCellY = 55236;
      IFilterSet filters = new FilterSet();
      var arg = new CellPassesRequestArgument_ClusterCompute(siteModelID, neeCoords, otgCellX, otgCellY, filters);
      arg.Should().NotBeNull();
      arg.ProjectID.Should().Be(siteModelID);
      arg.NEECoords.Should().Be(neeCoords);
      arg.OTGCellX.Should().Be(otgCellX);
      arg.OTGCellY.Should().Be(otgCellY);
      arg.Filters.Should().Be(filters);
    }
  }
}
