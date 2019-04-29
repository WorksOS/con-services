using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
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
      Guid referenceDesignUid = Guid.NewGuid();
      var referenceOffset = 12.34;
      var arg = new CellDatumRequestArgument_ApplicationService(siteModelID, mode, coordsAreGrid, latLngPoint, filters, referenceDesignUid, referenceOffset);
      Assert.NotNull(arg);
      Assert.Equal(siteModelID, arg.ProjectID);
      Assert.Equal(mode, arg.Mode);
      Assert.Equal(coordsAreGrid, arg.CoordsAreGrid);
      Assert.Equal(latLngPoint, arg.Point);
      Assert.Equal(filters, arg.Filters);
      Assert.Equal(referenceDesignUid, arg.ReferenceDesignUID);
      Assert.Equal(referenceOffset, arg.ReferenceOffset);
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
      uint otgCellX = 16234;
      uint otgCellY = 55236;
      IFilterSet filters = new FilterSet();
      Guid referenceDesignUid = Guid.NewGuid();
      var referenceOffset = 12.34;
      var arg = new CellDatumRequestArgument_ClusterCompute(siteModelID, mode, neeCoords, otgCellX, otgCellY, filters, referenceDesignUid, referenceOffset);
      Assert.NotNull(arg);
      Assert.Equal(siteModelID, arg.ProjectID);
      Assert.Equal(mode, arg.Mode);
      Assert.Equal(neeCoords, arg.NEECoords);
      Assert.Equal(otgCellX, arg.OTGCellX);
      Assert.Equal(otgCellY, arg.OTGCellY);
      Assert.Equal(filters, arg.Filters);
      Assert.Equal(referenceDesignUid, arg.ReferenceDesignUID);
      Assert.Equal(referenceOffset, arg.ReferenceOffset);
    }
  }
}
