using System;
using FluentAssertions;
using Microsoft.AspNetCore.Identity.UI.Pages.Internal.Account.Manage;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Machines;
using VSS.TRex.Tests.BinaryReaderWriter;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Machines
{
  public class MachineTests
  {
    private Machine TestMachine()
    {
      var l = new MachinesList();
      Guid newGuid = Guid.NewGuid();

      var m = new Machine("Machine", "HardwareID", MachineType.AsphaltPaver, DeviceTypeEnum.SNM940, newGuid, 12, true);
      m.LastKnownLayerId = 10;
      m.LastKnownDesignName = "layer";
      m.LastKnownPositionTimeStamp = DateTime.UtcNow;
      m.LastKnownX = 11;
      m.LastKnownY = 12;

      return m;
    }

    [Fact]
    public void Creation()
    {
      var m = new Machine();

      m.CompactionDataReported.Should().BeFalse();
      m.CompactionSensorType.Should().Be(CompactionSensorType.NoSensor);
      m.DeviceType.Should().Be(DeviceTypeEnum.MANUALDEVICE);
      m.ID.Should().Be(Guid.Empty);
      m.InternalSiteModelMachineIndex.Should().Be(0);
      m.IsJohnDoeMachine.Should().BeFalse();
      m.LastKnownDesignName.Should().BeNullOrEmpty();
      m.LastKnownLayerId.Should().Be(0);
      m.LastKnownPositionTimeStamp.Should().Be(TRex.Common.Consts.MIN_DATETIME_AS_UTC);
      m.LastKnownX.Should().Be(Consts.NullDouble);
      m.LastKnownY.Should().Be(Consts.NullDouble);
      m.CompactionDataReported.Should().BeFalse();
      m.MachineHardwareID.Should().BeNullOrEmpty();
      m.Name.Should().BeNullOrEmpty();
      m.MachineIsCompactorType().Should().BeFalse();
    }

    [Fact]
    public void Creation3()
    {
      Guid newGuid = Guid.NewGuid();

      var l = new MachinesList();
      var m = new Machine("Machine", "HardwareID", MachineType.AsphaltPaver, DeviceTypeEnum.SNM940, newGuid, 12, true);

      m.ID.Should().Be(newGuid);
      m.Name.Should().Be("Machine");
      m.MachineHardwareID.Should().Be("HardwareID");
      m.MachineType.Should().Be(MachineType.AsphaltPaver);
      m.DeviceType.Should().Be((int) DeviceTypeEnum.SNM940);
      m.InternalSiteModelMachineIndex.Should().Be(12);
      m.IsJohnDoeMachine.Should().BeTrue();
    }

    [Fact]
    public void MachineIsCompactoryType()
    {
      var m = new Machine();
      m.MachineIsCompactorType().Should().BeFalse();

      foreach (MachineType e in Enum.GetValues(typeof(MachineType)))
      {
        m.MachineType = e;

        var isCompactor = e == MachineType.SoilCompactor ||
               e == MachineType.AsphaltCompactor ||
               e == MachineType.FourDrumLandfillCompactor;

        m.MachineIsCompactorType().Should().Be(isCompactor);
      }
    }

    [Fact]
    public void MachineGearIsForwardGear()
    {
      foreach (MachineGear gear in Enum.GetValues(typeof(MachineGear)))
      {
        var isForward = gear == MachineGear.Forward || gear == MachineGear.Forward2 || gear == MachineGear.Forward3 || gear == MachineGear.Forward4 || gear == MachineGear.Forward5;

        Machine.MachineGearIsForwardGear(gear).Should().Be(isForward);
      }
    }

    [Fact]
    public void MachineGearIsReverseGear()
    {
      foreach (MachineGear gear in Enum.GetValues(typeof(MachineGear)))
      {
        var isReverse = gear == MachineGear.Reverse || gear == MachineGear.Reverse2 || gear == MachineGear.Reverse3 || gear == MachineGear.Reverse4 || gear == MachineGear.Reverse5;

        Machine.MachineGearIsReverseGear(gear).Should().Be(isReverse);
      }
    }

    [Fact]
    public void Assign()
    {
      var m = TestMachine();

      var m2 = new Machine();
      m2.Assign(m);

      m.ID.Should().NotBe(m2.ID);
      m.InternalSiteModelMachineIndex.Should().NotBe(m2.InternalSiteModelMachineIndex);

      m.Name.Should().Be(m2.Name);
      m.MachineHardwareID.Should().Be(m2.MachineHardwareID);
      m.CompactionSensorType.Should().Be(m2.CompactionSensorType);
      // todo CompactionRMVJumpThreshold 
      // todo UseMachineRMVThreshold 
      // todo OverrideRMVJumpThreshold 
      m.DeviceType.Should().Be(m2.DeviceType);
      m.CompactionDataReported.Should().Be(m2.CompactionDataReported);
      m.MachineType.Should().Be(m2.MachineType);
      m.IsJohnDoeMachine.Should().Be(m2.IsJohnDoeMachine);
      m.LastKnownX.Should().Be(m2.LastKnownX);
      m.LastKnownY.Should().Be(m2.LastKnownY);
      m.LastKnownLayerId.Should().Be(m2.LastKnownLayerId);
      m.LastKnownDesignName.Should().Be(m2.LastKnownDesignName);
      m.LastKnownPositionTimeStamp.Should().Be(m2.LastKnownPositionTimeStamp);
    }

    [Fact]
    public void Test_ReadWriterBinary()
    {
      var m = TestMachine();

      TestBinary_ReaderWriterHelper.RoundTripSerialise(m);
    }
  }
}
