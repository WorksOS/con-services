using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;
using FluentAssertions;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportExecutorTests : IClassFixture<DITagFileFixture>
  {
    // todoJeannie

    [Fact]
    public void CSVExportExecutor_SiteModelNotFound()
    {
      var projectUid = Guid.NewGuid(); 
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] {"first machineName"};
      var userPreferences = new UserPreferenceData();

      var request = CompactionVetaExportRequest.CreateRequest(
        projectUid, filter, fileName,
        coordType, outputType, machineNames, userPreferences);
      request.Validate();

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(request));
      result.Code.Should().Be(HttpStatusCode.BadRequest);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be($"Site model {projectUid} is unavailable");
    }

    [Fact]
    public void CSVExportExecutor_StartEndDate()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.Item1.Should().Be(startTime);
      startEndDate.Item2.Should().Be(endTime);
    }

    [Fact]
    public void CSVExportExecutor_EndDateOnly()
    {
      var filter = Filter.CreateFilter(
        new DateTime(2019, 1, 10),
        null,"","",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);

      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, filterResult);
      filter.StartUtc.Should().NotBeNull();
      startEndDate.Item1.Should().Be(filter.StartUtc.Value);
      startEndDate.Item2.Should().Be(endTime);
    }

    [Fact]
    public void CSVExportExecutor_MultiMachines()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // Due to site\machineList and events\machineList being different entities
      //     the first created on machine create, the 2nd the first time events are accessed
      //   For this test we need to create both machines before accessing any events.
      //   This is ok, because in reality, when a machine is added, it generates a machineChangedEvent,
      //     which will re-create the event\machineList with the new machine.
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.CarryAllScraper, 15, false, Guid.NewGuid());
      
      var startTime1 = DateTime.UtcNow.AddHours(-5);
      var endTime1 = startTime1.AddHours(4.5);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime1, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine1.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime1, ProductionEventType.EndEvent);
      var startTime2 = DateTime.UtcNow.AddHours(-7);
      var endTime2 = startTime2.AddHours(2);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(startTime2, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[machine2.InternalSiteModelMachineIndex].StartEndRecordedDataEvents.PutValueAtDate(endTime2, ProductionEventType.EndEvent);


      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.Item1.Should().Be(startTime2);
      startEndDate.Item2.Should().Be(endTime1);
    }

    [Fact]
    public void CSVExportExecutor_MachineHasNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.Item1.Should().Be(DateTime.MaxValue);
      startEndDate.Item2.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void CSVExportExecutor_MachineNames_Success()
    {
      string[] machineNames = new[] {"Test Machine 1"};
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(1);
      mappedMachines[machine1.InternalSiteModelMachineIndex].Uid.Should().Be(machine1.ID);
      mappedMachines[machine1.InternalSiteModelMachineIndex].InternalSiteModelMachineIndex.Should().Be(machine1.InternalSiteModelMachineIndex);
      mappedMachines[machine1.InternalSiteModelMachineIndex].Name.Should().Be(machine1.Name);
    }

    [Fact]
    public void CSVExportExecutor_MachineNamesMulti_Success()
    {
      string[] machineNames = new[] { "Test Machine 1", "Test Machine 3" };
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, 1, false, Guid.NewGuid());

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
    public void CSVExportExecutor_MachineNamesMulti_NotFound()
    {
      string[] machineNames = new[] { "Test Machine 4", "Test Machine 0" };
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, 1, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(0);
    }

    [Fact]
    public void CSVExportExecutor_MachineNamesMulti_NoneRequested()
    {
      string[] machineNames = null;
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, 1, false, Guid.NewGuid());
      IMachine machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, 1, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(0);
    }

    [Fact]
    public void CSVExportExecutor_MachineNamesMulti_NoneAvailable()
    {
      string[] machineNames = new[] { "Test Machine 4", "Test Machine 0" };
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
     
      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(0);
    }
  }
}


