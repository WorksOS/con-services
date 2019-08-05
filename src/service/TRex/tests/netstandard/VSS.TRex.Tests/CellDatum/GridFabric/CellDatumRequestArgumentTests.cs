using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests.CellDatum.GridFabric
{
  public class CellDatumRequestArgumentTests
  {
    [Fact]
    public void Test_CellDatumRequestArgument_ApplicationService_Creation()
    {
      var arg = new CellDatumRequestArgument_ApplicationService();
      Assert.NotNull(arg);
    }

    [Fact]
    public void Test_CellDatumRequestArgument_ApplicationService_CreationWithArgs()
    {
      Guid siteModelID = Guid.NewGuid();
      DisplayMode mode = DisplayMode.MachineSpeed;
      bool coordsAreGrid = false;
      XYZ latLngPoint = new XYZ(12345.6789, 98765.4321);
      IFilterSet filters = new FilterSet();
      var referenceDesign = new DesignOffset(Guid.NewGuid(), 12.34);
      var overrides = new OverrideParameters {OverrideMachineMDP = true, OverridingMachineMDP = 321};
      var arg = new CellDatumRequestArgument_ApplicationService(siteModelID, mode, coordsAreGrid, latLngPoint, filters, referenceDesign, overrides);
      Assert.NotNull(arg);
      Assert.Equal(siteModelID, arg.ProjectID);
      Assert.Equal(mode, arg.Mode);
      Assert.Equal(coordsAreGrid, arg.CoordsAreGrid);
      Assert.Equal(latLngPoint, arg.Point);
      Assert.Equal(filters, arg.Filters);
      Assert.Equal(referenceDesign.DesignID, arg.ReferenceDesign.DesignID);
      Assert.Equal(referenceDesign.Offset, arg.ReferenceDesign.Offset);
      Assert.Equal(overrides.OverrideMachineMDP, arg.Overrides.OverrideMachineMDP);
      Assert.Equal(overrides.OverridingMachineMDP, arg.Overrides.OverridingMachineMDP);
    }

    [Fact]
    public void Test_CellDatumRequestArgument_ClusterCompute_Creation()
    {
      var arg = new CellDatumRequestArgument_ClusterCompute();
      Assert.NotNull(arg);
    }

    [Fact]
    public void Test_CellDatumRequestArgument_ClusterCompute_CreationWithArgs()
    {
      Guid siteModelID = Guid.NewGuid();
      DisplayMode mode = DisplayMode.MachineSpeed;
      XYZ neeCoords = new XYZ(12345.6789, 98765.4321);
      int otgCellX = 16234;
      int otgCellY = 55236;
      IFilterSet filters = new FilterSet();
      var referenceDesign = new DesignOffset(Guid.NewGuid(), 12.34);
      var overrides = new OverrideParameters { OverrideMachineMDP = true, OverridingMachineMDP = 321 };
      var arg = new CellDatumRequestArgument_ClusterCompute(siteModelID, mode, neeCoords, otgCellX, otgCellY, filters, referenceDesign, overrides, null);
      Assert.NotNull(arg);
      Assert.Equal(siteModelID, arg.ProjectID);
      Assert.Equal(mode, arg.Mode);
      Assert.Equal(neeCoords, arg.NEECoords);
      Assert.Equal(otgCellX, arg.OTGCellX);
      Assert.Equal(otgCellY, arg.OTGCellY);
      Assert.Equal(filters, arg.Filters);
      Assert.Equal(referenceDesign.DesignID, arg.ReferenceDesign.DesignID);
      Assert.Equal(referenceDesign.Offset, arg.ReferenceDesign.Offset);
      Assert.Equal(overrides.OverrideMachineMDP, arg.Overrides.OverrideMachineMDP);
      Assert.Equal(overrides.OverridingMachineMDP, arg.Overrides.OverridingMachineMDP);
    }
  }
}
