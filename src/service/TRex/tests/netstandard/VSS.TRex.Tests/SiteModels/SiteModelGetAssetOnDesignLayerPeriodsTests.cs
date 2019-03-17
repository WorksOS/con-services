using System;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelGetAssetOnDesignLayerPeriodsTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void GetAssetOnDesignLayerPeriods_NoMachineNoRecordingPeriodNoLayersNoDesign()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
    
      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(0);
    }

    [Fact]
    public void GetAssetOnDesignLayerPeriods_OneMachineNoRecordingPeriodNoLayersNoDesign()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(0);
    }

    [Fact]
    public void GetAssetOnDesignLayerPeriods_LateLayerNoDesign()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      DateTime referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddMinutes(-30);

      ushort layerId2 = 222;
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(endReportPeriod1, layerId2);

      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(0);
    }

    [Fact]
    public void GetAssetOnDesignLayerPeriods_TwoLayersNoDesign()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      DateTime referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddMinutes(-30);

      ushort layerId1 = 111;
      ushort layerId2 = 222;
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod1, layerId1);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(endReportPeriod1, layerId2);

      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(1);
      assetOnDesignLayerPeriods[0].AssetId.Should().Be(Consts.LEGACY_ASSETID);
      assetOnDesignLayerPeriods[0].DesignId.Should().Be(Consts.kNoDesignNameID);
      assetOnDesignLayerPeriods[0].LayerId.Should().Be(layerId1);
      assetOnDesignLayerPeriods[0].StartDate.Should().Be(startReportPeriod1);
      assetOnDesignLayerPeriods[0].EndDate.Should().Be(endReportPeriod1);
      assetOnDesignLayerPeriods[0].AssetUid.Should().Be(machine.ID);
    }

    [Fact]
    public void GetAssetOnDesignLayerPeriods_TwoLayersOneDesign()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");

      DateTime referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddMinutes(-30);

      ushort layerId1 = 111;
      ushort layerId2 = 222;
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(startReportPeriod1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod1, layerId1);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(endReportPeriod1, layerId2);

      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(1);
      assetOnDesignLayerPeriods[0].AssetId.Should().Be(Consts.LEGACY_ASSETID);
      assetOnDesignLayerPeriods[0].DesignId.Should().Be(design1.Id);
      assetOnDesignLayerPeriods[0].LayerId.Should().Be(layerId1);
      assetOnDesignLayerPeriods[0].StartDate.Should().Be(startReportPeriod1);
      assetOnDesignLayerPeriods[0].EndDate.Should().Be(endReportPeriod1);
      assetOnDesignLayerPeriods[0].AssetUid.Should().Be(machine.ID);
    }

    [Fact]
    public void GetAssetOnDesignLayerPeriods_MultiLayersDesignsReportPeriods()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");
      var design2 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName2");

      DateTime referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddDays(-1).AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddDays(-1).AddMinutes(-30);

      var startReportPeriod2 = referenceDate.AddMinutes(-60);
      var endReportPeriod2 = referenceDate.AddMinutes(-30);

      ushort layerId1 = 111;
      ushort layerId2 = 222;
      ushort layerId3 = 222;
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(startReportPeriod1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod1, layerId1);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod1.AddMinutes(5), layerId2);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);

      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod2, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod2, layerId2);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(startReportPeriod2.AddMinutes(6), design2.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod2.AddMinutes(6), layerId3);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod2, ProductionEventType.EndEvent);


      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(4);
      assetOnDesignLayerPeriods[0].DesignId.Should().Be(design1.Id);
      assetOnDesignLayerPeriods[0].LayerId.Should().Be(layerId1);
      assetOnDesignLayerPeriods[0].StartDate.Should().Be(startReportPeriod1);
      assetOnDesignLayerPeriods[0].EndDate.Should().Be(startReportPeriod1.AddMinutes(5));
      assetOnDesignLayerPeriods[0].AssetUid.Should().Be(machine.ID);

      assetOnDesignLayerPeriods[1].DesignId.Should().Be(design1.Id);
      assetOnDesignLayerPeriods[1].LayerId.Should().Be(layerId2);
      assetOnDesignLayerPeriods[1].StartDate.Should().Be(startReportPeriod1.AddMinutes(5));
      assetOnDesignLayerPeriods[1].EndDate.Should().Be(endReportPeriod1);
      assetOnDesignLayerPeriods[1].AssetUid.Should().Be(machine.ID);

      assetOnDesignLayerPeriods[2].DesignId.Should().Be(design1.Id);
      assetOnDesignLayerPeriods[2].LayerId.Should().Be(layerId2);
      assetOnDesignLayerPeriods[2].StartDate.Should().Be(startReportPeriod2);
      assetOnDesignLayerPeriods[2].EndDate.Should().Be(startReportPeriod2.AddMinutes(6));
      assetOnDesignLayerPeriods[2].AssetUid.Should().Be(machine.ID);

      assetOnDesignLayerPeriods[3].DesignId.Should().Be(design2.Id);
      assetOnDesignLayerPeriods[3].LayerId.Should().Be(layerId3);
      assetOnDesignLayerPeriods[3].StartDate.Should().Be(startReportPeriod2.AddMinutes(6));
      assetOnDesignLayerPeriods[3].EndDate.Should().Be(endReportPeriod2);
      assetOnDesignLayerPeriods[3].AssetUid.Should().Be(machine.ID);
    }

    [Fact]
    public void GetAssetOnDesignLayerPeriods_FirstLayerWithinReportPeriod()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine = siteModel.Machines.CreateNew("Test Machine Source", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var design1 = siteModel.SiteModelMachineDesigns.CreateNew("DesignName1");

      DateTime referenceDate = DateTime.UtcNow;
      var startReportPeriod1 = referenceDate.AddDays(-1).AddMinutes(-60);
      var endReportPeriod1 = referenceDate.AddDays(-1).AddMinutes(-30);

      var startReportPeriod2 = referenceDate.AddMinutes(-60);
      var endReportPeriod2 = referenceDate.AddMinutes(-30);

      ushort layerId1 = 111;

      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].MachineDesignNameIDStateEvents.PutValueAtDate(startReportPeriod1, design1.Id);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod1, ProductionEventType.EndEvent);

      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startReportPeriod2, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].LayerIDStateEvents.PutValueAtDate(startReportPeriod2.AddMinutes(6), layerId1);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endReportPeriod2, ProductionEventType.EndEvent);


      var assetOnDesignLayerPeriods = siteModel.GetAssetOnDesignLayerPeriods();
      assetOnDesignLayerPeriods.Count.Should().Be(1);

      assetOnDesignLayerPeriods[0].DesignId.Should().Be(design1.Id);
      assetOnDesignLayerPeriods[0].LayerId.Should().Be(layerId1);
      assetOnDesignLayerPeriods[0].StartDate.Should().Be(startReportPeriod2.AddMinutes(6));
      assetOnDesignLayerPeriods[0].EndDate.Should().Be(endReportPeriod2);
      assetOnDesignLayerPeriods[0].AssetUid.Should().Be(machine.ID);
    }
  }
}
