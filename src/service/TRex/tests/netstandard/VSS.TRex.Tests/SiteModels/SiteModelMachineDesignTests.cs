using System;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelMachineDesignTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void GetMachineDesigns_NoMachineNoDesignsNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      var machineDesigns = siteModel.GetMachineDesigns();
      machineDesigns.Count.Should().Be(0);
    }

    [Fact]
    public void GetMachineDesigns_OneMachineNoDesignsNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      var machineDesigns = siteModel.GetMachineDesigns();
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      machineDesigns.Count.Should().Be(0);
    }

    [Fact]
    public void GetMachineDesigns_OneMachineOneDesignsNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      
      var machineDesigns = siteModel.GetMachineDesigns();
      machineDesigns.Count.Should().Be(0);
    }

    [Fact]
    public void GetMachineDesigns_OneMachineTwoDesignsSimpleEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      DateTime referenceDate = DateTime.UtcNow;
      var eventDate1 = referenceDate.AddMinutes(-60);
      var eventDate2 = referenceDate.AddMinutes(-30);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate2, design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.Count().Should().Be(2);
      
      var machineDesigns = siteModel.GetMachineDesigns();
      machineDesigns.Count.Should().Be(2);
      machineDesigns[0].Id.Should().Be(design1.Id);
      machineDesigns[0].Name.Should().Be(design1.Name);
      machineDesigns[0].MachineId.Should().Be(Consts.LEGACY_ASSETID); 
      machineDesigns[0].AssetUid.Should().Be(machine.ID);
      machineDesigns[0].StartDate.Should().Be(eventDate1);
      machineDesigns[0].EndDate.Should().Be(DateTime.MaxValue);

      machineDesigns[1].Id.Should().Be(design2.Id);
      machineDesigns[1].Name.Should().Be(design2.Name);
      machineDesigns[1].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[1].AssetUid.Should().Be(machine.ID);
      machineDesigns[1].StartDate.Should().Be(eventDate2);
      machineDesigns[1].EndDate.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void GetMachineDesigns_OneMachineTwoDesignsDuplicateEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      DateTime referenceDate = DateTime.UtcNow;
      var eventDate1 = referenceDate.AddMinutes(-60);
      var eventDate2 = referenceDate.AddMinutes(-30);
      var eventDate3 = referenceDate.AddMinutes(-15);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate2, design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate3, design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.Count().Should().Be(3);

      var machineDesigns = siteModel.GetMachineDesigns();
      machineDesigns.Count.Should().Be(2);
      machineDesigns[0].Id.Should().Be(design1.Id);
      machineDesigns[0].Name.Should().Be(design1.Name);
      machineDesigns[0].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[0].AssetUid.Should().Be(machine.ID);
      machineDesigns[0].StartDate.Should().Be(eventDate1);
      machineDesigns[0].EndDate.Should().Be(DateTime.MaxValue);

      machineDesigns[1].Id.Should().Be(design2.Id);
      machineDesigns[1].Name.Should().Be(design2.Name);
      machineDesigns[1].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[1].AssetUid.Should().Be(machine.ID);
      machineDesigns[1].StartDate.Should().Be(eventDate2);
      machineDesigns[1].EndDate.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public void GetMachineDesigns_TwoMachinesTwoDesignsDuplicateEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // a little gotcha is that machines must be added before any events
      //    to do with adding event should 'normally' generate a NewMachine event which trigger a whole series of events to the siteModel.
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine Source 2", "", MachineType.WheelLoader, 2, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      DateTime referenceDate = DateTime.UtcNow;
      var eventDate1 = referenceDate.AddMinutes(-60);
      var eventDate2 = referenceDate.AddMinutes(-30);
      var eventDate3 = referenceDate.AddMinutes(-15);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate1, design1.Id);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate2, design2.Id);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate3, design2.Id);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.Count().Should().Be(3);

      var eventDate5 = referenceDate.AddDays(-6);
      var eventDate6 = referenceDate.AddDays(-3);
      var eventDate7 = referenceDate.AddDays(-1);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate5, design2.Id);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate6, design2.Id);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate7, design1.Id);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.Count().Should().Be(3);


      var machineDesigns = siteModel.GetMachineDesigns();
      machineDesigns.Count.Should().Be(4);
      machineDesigns[0].Id.Should().Be(design1.Id);
      machineDesigns[0].Name.Should().Be(design1.Name);
      machineDesigns[0].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[0].AssetUid.Should().Be(machine1.ID);
      machineDesigns[0].StartDate.Should().Be(eventDate1);
      machineDesigns[0].EndDate.Should().Be(DateTime.MaxValue);

      machineDesigns[1].Id.Should().Be(design2.Id);
      machineDesigns[1].Name.Should().Be(design2.Name);
      machineDesigns[1].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[1].AssetUid.Should().Be(machine1.ID);
      machineDesigns[1].StartDate.Should().Be(eventDate2);
      machineDesigns[1].EndDate.Should().Be(DateTime.MaxValue);

      machineDesigns[2].Id.Should().Be(design2.Id);
      machineDesigns[2].Name.Should().Be(design2.Name);
      machineDesigns[2].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[2].AssetUid.Should().Be(machine2.ID);
      machineDesigns[2].StartDate.Should().Be(eventDate5);
      machineDesigns[2].EndDate.Should().Be(DateTime.MaxValue);

      machineDesigns[3].Id.Should().Be(design1.Id);
      machineDesigns[3].Name.Should().Be(design1.Name);
      machineDesigns[3].MachineId.Should().Be(Consts.LEGACY_ASSETID);
      machineDesigns[3].AssetUid.Should().Be(machine2.ID);
      machineDesigns[3].StartDate.Should().Be(eventDate7);
      machineDesigns[3].EndDate.Should().Be(DateTime.MaxValue);
    }
  }
}
