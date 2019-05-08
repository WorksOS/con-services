using System;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelGetAssetOnDesignPeriodsTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void GetAssetOnDesignPeriods_NoMachineNoDesignsNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      var assetOnDesignPeriods = siteModel.GetAssetOnDesignPeriods();
      assetOnDesignPeriods.Count.Should().Be(0);
    }

    [Fact]
    public void GetAssetOnDesignPeriods_OneMachineNoDesignsNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      var assetOnDesignPeriods = siteModel.GetAssetOnDesignPeriods();
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      assetOnDesignPeriods.Count.Should().Be(0);
    }

    [Fact]
    public void GetAssetOnDesignPeriods_OneMachineOneDesignsNoEvents()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      
      var assetOnDesignPeriods = siteModel.GetAssetOnDesignPeriods();
      assetOnDesignPeriods.Count.Should().Be(0);
    }

    [Fact]
    public void GetAssetOnDesignPeriods_OneMachineTwoDesignsSimpleEvents()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      var referenceDate = DateTime.UtcNow;
      var eventDate1 = referenceDate.AddMinutes(-60);
      var eventDate2 = referenceDate.AddMinutes(-30);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate2, design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.Count().Should().Be(2);
      
      var assetOnDesignPeriods = siteModel.GetAssetOnDesignPeriods();
      assetOnDesignPeriods.Count.Should().Be(2);
      assetOnDesignPeriods[0].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[0].OnMachineDesignName.Should().Be(design1.Name);
      assetOnDesignPeriods[0].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID); 
      assetOnDesignPeriods[0].AssetUid.Should().Be(machine.ID);
      assetOnDesignPeriods[0].StartDate.Should().Be(eventDate1);
      assetOnDesignPeriods[0].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);

      assetOnDesignPeriods[1].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[1].OnMachineDesignName.Should().Be(design2.Name);
      assetOnDesignPeriods[1].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[1].AssetUid.Should().Be(machine.ID);
      assetOnDesignPeriods[1].StartDate.Should().Be(eventDate2);
      assetOnDesignPeriods[1].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);
    }

    [Fact]
    public void GetAssetOnDesignPeriods_OneMachineTwoDesignsDuplicateEvents()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      var referenceDate = DateTime.UtcNow;
      var eventDate1 = referenceDate.AddMinutes(-60);
      var eventDate2 = referenceDate.AddMinutes(-30);
      var eventDate3 = referenceDate.AddMinutes(-15);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate2, design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(eventDate3, design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.Count().Should().Be(3);

      var assetOnDesignPeriods = siteModel.GetAssetOnDesignPeriods();
      assetOnDesignPeriods.Count.Should().Be(2);
      assetOnDesignPeriods[0].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[0].OnMachineDesignName.Should().Be(design1.Name);
      assetOnDesignPeriods[0].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[0].AssetUid.Should().Be(machine.ID);
      assetOnDesignPeriods[0].StartDate.Should().Be(eventDate1);
      assetOnDesignPeriods[0].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);

      assetOnDesignPeriods[1].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[1].OnMachineDesignName.Should().Be(design2.Name);
      assetOnDesignPeriods[1].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[1].AssetUid.Should().Be(machine.ID);
      assetOnDesignPeriods[1].StartDate.Should().Be(eventDate2);
      assetOnDesignPeriods[1].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);
    }

    [Fact]
    public void GetAssetOnDesignPeriods_TwoMachinesTwoDesignsDuplicateEvents()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // a little gotcha is that machines must be added before any events
      //    to do with adding event should 'normally' generate a NewMachine event which trigger a whole series of events to the siteModel.
      var machine1 = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var machine2 = siteModel.Machines.CreateNew("Test Machine Source 2", "", MachineType.WheelLoader, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      var referenceDate = DateTime.UtcNow;
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


      var assetOnDesignPeriods = siteModel.GetAssetOnDesignPeriods();
      assetOnDesignPeriods.Count.Should().Be(4);
      assetOnDesignPeriods[0].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[0].OnMachineDesignName.Should().Be(design1.Name);
      assetOnDesignPeriods[0].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[0].AssetUid.Should().Be(machine1.ID);
      assetOnDesignPeriods[0].StartDate.Should().Be(eventDate1);
      assetOnDesignPeriods[0].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);

      assetOnDesignPeriods[1].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[1].OnMachineDesignName.Should().Be(design2.Name);
      assetOnDesignPeriods[1].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[1].AssetUid.Should().Be(machine1.ID);
      assetOnDesignPeriods[1].StartDate.Should().Be(eventDate2);
      assetOnDesignPeriods[1].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);

      assetOnDesignPeriods[2].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[2].OnMachineDesignName.Should().Be(design2.Name);
      assetOnDesignPeriods[2].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[2].AssetUid.Should().Be(machine2.ID);
      assetOnDesignPeriods[2].StartDate.Should().Be(eventDate5);
      assetOnDesignPeriods[2].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);

      assetOnDesignPeriods[3].OnMachineDesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignPeriods[3].OnMachineDesignName.Should().Be(design1.Name);
      assetOnDesignPeriods[3].MachineId.Should().Be(Consts.NULL_LEGACY_ASSETID);
      assetOnDesignPeriods[3].AssetUid.Should().Be(machine2.ID);
      assetOnDesignPeriods[3].StartDate.Should().Be(eventDate7);
      assetOnDesignPeriods[3].EndDate.Should().Be(Consts.MAX_DATETIME_AS_UTC);
    }
  }
}
