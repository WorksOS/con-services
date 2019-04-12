﻿using System;
using System.Linq;
using Castle.Core.Internal;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.Machines;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers
{
  public class AutoMapperTests : IClassFixture<AutoMapperFixture>
  {
    [Fact]
    public void MapPointToFencePoint()
    {
      var point = new Point
      {
        x = 10,
        y = 15
      };
      var fencePoint = AutoMapperUtility.Automapper.Map<FencePoint>(point);
      Assert.Equal(point.x, fencePoint.X);
      Assert.Equal(point.y, fencePoint.Y);
      Assert.Equal(0, fencePoint.Z);
    }

    [Fact]
    public void MapWGSPointToFencePoint()
    {
      var point = new WGSPoint(123.4, 567.8);
      var fencePoint = AutoMapperUtility.Automapper.Map<FencePoint>(point);
      Assert.Equal(point.Lon, fencePoint.X);
      Assert.Equal(point.Lat, fencePoint.Y);
      Assert.Equal(0, fencePoint.Z);
    }

    [Fact]
    public void MapBoundingBox2DGridToBoundingWorldExtent3D()
    {
      var box = new BoundingBox2DGrid(10, 12, 35, 27);
      var box3d = AutoMapperUtility.Automapper.Map<BoundingWorldExtent3D>(box);
      Assert.Equal(box.BottomLeftX, box3d.MinX);
      Assert.Equal(box.BottomleftY, box3d.MinY);
      Assert.Equal(box.TopRightX, box3d.MaxX);
      Assert.Equal(box.TopRightY, box3d.MaxY);
    }

    [Fact]
    public void MapBoundingBox2DLatLonToBoundingWorldExtent3D()
    {
      var box = new BoundingBox2DLatLon(10, 12, 35, 27);
      var box3d = AutoMapperUtility.Automapper.Map<BoundingWorldExtent3D>(box);
      Assert.Equal(box.BottomLeftLon, box3d.MinX);
      Assert.Equal(box.BottomLeftLat, box3d.MinY);
      Assert.Equal(box.TopRightLon, box3d.MaxX);
      Assert.Equal(box.TopRightLat, box3d.MaxY);
    }
    
    [Fact]
    public void MapPointToXYZ()
    {
      var gridPoint = new Point(1.234, 5.678);
      var xyz = AutoMapperUtility.Automapper.Map<XYZ>(gridPoint);
      Assert.Equal(gridPoint.Longitude, xyz.X);
      Assert.Equal(gridPoint.Latitude, xyz.Y);
      Assert.Equal(0, xyz.Z);
    }

    [Fact]
    public void MapWGSPointToXYZ()
    {
      var llPoint = new WGSPoint(1.234, 5.678);
      var xyz = AutoMapperUtility.Automapper.Map<XYZ>(llPoint);
      Assert.Equal(llPoint.Lat, xyz.Y);
      Assert.Equal(llPoint.Lon, xyz.X);
      Assert.Equal(0, xyz.Z);
    }

    [Fact]
    public void MapMachineToMachineStatus()
    {
      var machineUid1 = Guid.NewGuid();
      var machineUid2 = Guid.NewGuid();
      var machineUid3 = Guid.Empty;
      var machines = new MachinesList {DataModelID = Guid.NewGuid()};
      machines.CreateNew("MachineName1", "hardwareID444", MachineType.ConcretePaver, DeviceTypeEnum.SNM940, false,
        machineUid1);
      machines[0].InternalSiteModelMachineIndex = 0;
      machines[0].LastKnownX = 34.34;
      machines[0].LastKnownY = 77.77;
      machines[0].LastKnownPositionTimeStamp = DateTime.UtcNow.AddMonths(-2);
      machines[0].LastKnownDesignName = "design1";
      machines[0].LastKnownLayerId = 11;

      machines.CreateNew("MachineName2", "hardwareID555", MachineType.AsphaltCompactor, DeviceTypeEnum.SNM940, false,
        machineUid2);
      machines.CreateNew("MachineName3", "hardwareID666", MachineType.Generic, DeviceTypeEnum.MANUALDEVICE, true,
        machineUid3);

      var machineStatuses = machines.Select(machine =>
        AutoMapperUtility.Automapper.Map<MachineStatus>(machine)).ToArray();
      machineStatuses.Length.Equals(3);
      machineStatuses[0].AssetUid.HasValue.Equals(true);
      machineStatuses[0].AssetUid?.Equals(machines[0].ID);
      machineStatuses[0].AssetId.Equals(-1);
      machineStatuses[0].MachineName.IsNullOrEmpty().Equals(false);
      machineStatuses[0].MachineName.Equals(machines[0].Name);
      machineStatuses[0].IsJohnDoe.Equals(machines[0].IsJohnDoeMachine);
      machineStatuses[0].lastKnownDesignName.IsNullOrEmpty().Equals(false);
      machineStatuses[0].lastKnownDesignName.Equals(machines[0].LastKnownDesignName);
      machineStatuses[0].lastKnownLayerId.HasValue.Equals(true);
      machineStatuses[0].lastKnownLayerId?.Equals(machines[0].LastKnownLayerId);
      machineStatuses[0].lastKnownTimeStamp.HasValue.Equals(true);
      machineStatuses[0].lastKnownTimeStamp?.Equals(machines[0].LastKnownPositionTimeStamp);
      machineStatuses[0].lastKnownLatitude.HasValue.Equals(true);
      machineStatuses[0].lastKnownLatitude?.Equals(Double.MaxValue);
      machineStatuses[0].lastKnownLongitude.HasValue.Equals(true);
      machineStatuses[0].lastKnownLongitude?.Equals(Double.MaxValue);
      machineStatuses[0].lastKnownX.HasValue.Equals(true);
      machineStatuses[0].lastKnownX?.Equals(machines[0].LastKnownX);
      machineStatuses[0].lastKnownY.HasValue.Equals(true);
      machineStatuses[0].lastKnownY?.Equals(machines[0].LastKnownY);

      machineStatuses[1].AssetUid.HasValue.Equals(true);
      machineStatuses[1].AssetUid?.Equals(machineUid2);
      machineStatuses[1].lastKnownX.HasValue.Equals(false);
      machineStatuses[1].lastKnownY.HasValue.Equals(false);

      machineStatuses[2].AssetUid.HasValue.Equals(true);
      machineStatuses[2].AssetUid?.Equals(machineUid3);
    }
  }
}