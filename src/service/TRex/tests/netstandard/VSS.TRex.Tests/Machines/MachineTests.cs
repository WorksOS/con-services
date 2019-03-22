using System;
using FluentAssertions;
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
      m.LastKnownPositionTimeStamp.Should().Be(DateTime.MinValue);
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

      m.Should().BeEquivalentTo(m2);
    }

    [Fact]
    public void Test_ReadWriterBinary()
    {
      var m = TestMachine();

      TestBinary_ReaderWriterHelper.RoundTripSerialise(m);
    }
  }
}
