using System;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Machines;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.BinaryReaderWriter;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Machines
{
  public class MachinesListTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var l = new MachinesList();

      l.DataModelID.Should().Be(Guid.Empty);
    }

    [Fact]
    public void CreateNew()
    {
      var newGuid = Guid.NewGuid();
      var l = new MachinesList();
      var m = l.CreateNew("Machine", "HardwareID", MachineType.Dozer, DeviceTypeEnum.SNM940, false, newGuid);

      m.Should().NotBeNull();
      m.ID.Should().Be(newGuid);
      m.Name.Should().Be("Machine");
      m.MachineHardwareID.Should().Be("HardwareID");
      m.MachineType.Should().Be(MachineType.Dozer);
      m.DeviceType.Should().Be((int)DeviceTypeEnum.SNM940);
      m.InternalSiteModelMachineIndex.Should().Be(0);
      m.IsJohnDoeMachine.Should().BeFalse();
    }

    [Fact]
    public void CreateNew_Duplicate()
    {
      var newGuid = Guid.NewGuid();
      var l = new MachinesList();
      var m = l.CreateNew("Machine", "HardwareID", MachineType.Dozer, DeviceTypeEnum.SNM940, false, newGuid);

      // Create an identical machine - should return the first machine created
      var m2 = l.CreateNew("Machine", "HardwareID", MachineType.Dozer, DeviceTypeEnum.SNM940, false, newGuid);

      m.Should().BeSameAs(m2);
    }

    [Fact]
    public void Add()
    {
      var l = new MachinesList();
      var m = new Machine();
      l.Add(m);

      l.Count.Should().Be(1);
      l.Locate(m.ID).Should().BeSameAs(m);
    }

    [Fact]
    public void Add_FailWithNull()
    {
      var l = new MachinesList();

      Action act = () => l.Add(null);
      act.Should().Throw<ArgumentException>().WithMessage("*Machine reference cannot be null*");
    }

    [Fact]
    public void Locate_NameAndJohnDoe()
    {
      var newGuid1 = Guid.NewGuid();
      var l = new MachinesList();
      var m = l.CreateNew("Machine", "HardwareID", MachineType.Dozer, DeviceTypeEnum.SNM940, false, newGuid1);

      l.Locate("Machine", false).Should().BeSameAs(m);

      var m2 = l.CreateNew("Machine2", "HardwareID2", MachineType.Dozer, DeviceTypeEnum.SNM940, true, Guid.Empty); // John Doe machines don;t have known guids at this point

      l.Locate("Machine2", true).Should().BeSameAs(m2);
    }

    [Fact]
    public void Locate_GuidAndJohnDoe()
    {
      var newGuid1 = Guid.NewGuid();
      var l = new MachinesList();
      var m = l.CreateNew("Machine", "HardwareID", MachineType.Dozer, DeviceTypeEnum.SNM940, false, newGuid1);

      l.Locate(newGuid1, false).Should().BeSameAs(m);

      var newGuid2 = Guid.NewGuid();
      var m2 = l.CreateNew("Machine2", "HardwareID2", MachineType.Dozer, DeviceTypeEnum.SNM940, true, Guid.Empty); // John Doe machines don;t have known guids at this point

      l.Locate(m2.ID, true).Should().BeSameAs(m2);
    }

    [Fact]
    public void LocateByMachineHardwareID()
    {
      var l = new MachinesList();
      var m = l.CreateNew("Machine1", "HardwareID1", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var m2 = l.CreateNew("Machine2", "HardwareID2", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      l.LocateByMachineHardwareID("HardwareID1").Should().BeSameAs(m);
      l.LocateByMachineHardwareID("HardwareID2").Should().BeSameAs(m2);
      l.LocateByMachineHardwareID("HardwareID3").Should().BeNull();
    }

    [Fact]
    public void Test_ReadWriteBinary()
    {
      var l = new MachinesList();
      var m = l.CreateNew("Machine1", "HardwareID1", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var m2 = l.CreateNew("Machine2", "HardwareID2", MachineType.Dozer, DeviceTypeEnum.SNM940, true, Guid.Empty);

      TestBinary_ReaderWriterHelper.RoundTripSerialise(l);
    }

    [Fact]
    public void ReadWritePersistentStore()
    {
      var l = new MachinesList();
      var m = l.CreateNew("Machine1", "HardwareID1", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var m2 = l.CreateNew("Machine2", "HardwareID2", MachineType.Dozer, DeviceTypeEnum.SNM940, true, Guid.Empty);

      // Save it then read it back
      l.SaveToPersistentStore(DIContext.Obtain<ISiteModels>().PrimaryMutableStorageProxy);

      // read it back
      var l2 = new MachinesList();
      l2.LoadFromPersistentStore(DIContext.Obtain<ISiteModels>().PrimaryMutableStorageProxy);

      l.Should().BeEquivalentTo(l2);
    }
  }
}
