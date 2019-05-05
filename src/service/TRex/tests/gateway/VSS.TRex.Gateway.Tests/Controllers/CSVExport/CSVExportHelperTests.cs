using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.CSVExport
{
  public class CSVExportHelperTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void CSVExportHelper_StartEndDate()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.startUtc.Should().Be(startTime);
      startEndDate.endUtc.Should().Be(endTime);
    }

    [Fact]
    public void CSVExportHelper_EndDateOnly()
    {
      var filter = new Productivity3D.Filter.Abstractions.Models.Filter(
        DateTime.SpecifyKind(new DateTime(2018, 1, 10), DateTimeKind.Utc),
        DateTime.SpecifyKind(new DateTime(2019, 2, 11), DateTimeKind.Utc), "","",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, filterResult);
      filter.StartUtc.Should().NotBeNull();
      startEndDate.startUtc.Should().Be(filter.StartUtc.Value);
      startEndDate.endUtc.Should().Be(filter.EndUtc.Value);
    }

    [Fact]
    public void CSVExportHelper_DateRangeFromFilter()
    {
      var filter = new Productivity3D.Filter.Abstractions.Models.Filter(
        DateTime.SpecifyKind(new DateTime(2019, 1, 10), DateTimeKind.Utc),
        null, "", "",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, filterResult);
      filter.StartUtc.Should().NotBeNull();
      startEndDate.startUtc.Should().Be(filter.StartUtc.Value);
      startEndDate.endUtc.Should().Be(endTime);
    }

    [Fact]
    public void CSVExportHelper_MultiMachines()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // Due to site\machineList and events\machineList being different entities
      //     the first created on machine create, the 2nd the first time events are accessed
      //   For this test we need to create both machines before accessing any events.
      //   This is ok, because in reality, when a machine is added, it generates a machineChangedEvent,
      //     which will re-create the event\machineList with the new machine.
      var machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.CarryAllScraper, DeviceTypeEnum.SNM941, false, Guid.NewGuid());
      
      var startTime1 = DateTime.UtcNow.AddHours(-5);
      var endTime1 = startTime1.AddHours(4.5);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime1, ProductionEventType.EndEvent);
      var startTime2 = DateTime.UtcNow.AddHours(-7);
      var endTime2 = startTime2.AddHours(2);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime2, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime2, ProductionEventType.EndEvent);


      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.startUtc.Should().Be(startTime2);
      startEndDate.endUtc.Should().Be(endTime1);
    }

    [Fact]
    public void CSVExportHelper_MachineHasNoEvents()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.startUtc.Should().Be(Consts.MAX_DATETIME_AS_UTC);
      startEndDate.endUtc.Should().Be(Consts.MIN_DATETIME_AS_UTC);
    }

    [Fact]
    public void CSVExportHelper_MachineNames_Success()
    {
      var machineNames = new[] {"Test Machine 1"};
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(1);
      mappedMachines[machine1.InternalSiteModelMachineIndex].Uid.Should().Be(machine1.ID);
      mappedMachines[machine1.InternalSiteModelMachineIndex].InternalSiteModelMachineIndex.Should().Be(machine1.InternalSiteModelMachineIndex);
      mappedMachines[machine1.InternalSiteModelMachineIndex].Name.Should().Be(machine1.Name);
    }

    [Fact]
    public void CSVExportHelper_MachineNamesMulti_Success()
    {
      string[] machineNames = new[] { "Test Machine 1", "Test Machine 3" };
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      var machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(2);
      mappedMachines[0].Uid.Should().Be(machine1.ID);
      mappedMachines[0].InternalSiteModelMachineIndex.Should().Be(machine1.InternalSiteModelMachineIndex);
      mappedMachines[0].Name.Should().Be(machine1.Name);

      mappedMachines[1].Uid.Should().Be(machine3.ID);
      mappedMachines[1].InternalSiteModelMachineIndex.Should().Be(machine3.InternalSiteModelMachineIndex);
      mappedMachines[1].Name.Should().Be(machine3.Name);
    }

    [Fact]
    public void CSVExportHelper_MachineNamesMulti_NotFound()
    {
      var machineNames = new[] { "Test Machine 4", "Test Machine 0" };
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(0);
    }

    [Fact]
    public void CSVExportHelper_MachineNamesMulti_NoneRequested()
    {
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, null);
      mappedMachines.Count.Should().Be(0);
    }

    [Fact]
    public void CSVExportHelper_MachineNamesMulti_NoneAvailable()
    {
      var machineNames = new[] { "Test Machine 4", "Test Machine 0" };
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
     
      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(0);
    }
  }
}


