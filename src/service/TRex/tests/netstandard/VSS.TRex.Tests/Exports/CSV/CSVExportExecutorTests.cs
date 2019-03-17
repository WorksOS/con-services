using System;
using System.Collections.Generic;
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
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Requests;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportExecutorTests : IClassFixture<DITagFileFixture>
  {
    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "/", ":", TemperatureUnitEnum.Celsius, "/", ":" )]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "%", "^", TemperatureUnitEnum.Fahrenheit, "%", "^")]
    public void CSVExportExecutor_MapUserPreferences(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
      string dateSeparator, string timeSeparator, TemperatureUnitEnum temperatureUnits,
      string expectedDateSeparator, string expectedTimeSeparator)
    {
      var userPreference = new UserPreferences()
      { DateSeparator = dateSeparator, TimeSeparator = timeSeparator,
        DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeparator,
        Units = (int)units, TemperatureUnits = (int)temperatureUnits
      };

      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreference);
      csvUserPreference.DecimalSeparator.Should().Be(decimalSeparator);
      csvUserPreference.ThousandsSeparator.Should().Be(thousandsSeparator);
      csvUserPreference.Units.Should().Be(units);
      csvUserPreference.TemperatureUnits.Should().Be(temperatureUnits);
      csvUserPreference.DateSeparator.Should().Be(expectedDateSeparator);
      csvUserPreference.TimeSeparator.Should().Be(expectedTimeSeparator);
    }

    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "/", ":", "/", ":")]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "%", "^", "%", "^")]
    public void CSVExportExecutor_MapVetaRequestToCommonExportRequest(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
      string dateSeparator, string timeSeparator,
      string expectedDateSeparator, string expectedTimeSeparator)
    {
      var projectUid = Guid.NewGuid();
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      var theMachineName = "first machineName";
      string[] machineNames = new string[] { theMachineName };
      var userPreference = new UserPreferences()
        { DateSeparator = dateSeparator, TimeSeparator = timeSeparator,
          DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeparator, Units = (int)units };
     
      var request = new CompactionVetaExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreference, machineNames);
      request.Validate();
      var compactionCsvExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      compactionCsvExportRequest.CoordType.Should().Be(coordType);
      compactionCsvExportRequest.OutputType.Should().Be(outputType);

      compactionCsvExportRequest.UserPreferences.DecimalSeparator.Should().Be(decimalSeparator);
      compactionCsvExportRequest.UserPreferences.DateSeparator.Should().Be(expectedDateSeparator);
      compactionCsvExportRequest.UserPreferences.TimeSeparator.Should().Be(expectedTimeSeparator);

      compactionCsvExportRequest.MachineNames.Should().Equal(machineNames);
      compactionCsvExportRequest.RestrictOutputSize.Should().Be(false);
      compactionCsvExportRequest.RawDataAsDBase.Should().Be(false);
    }


    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "/", ":", true, false, "/", ":")]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "%", "^", false, true, "%", "^")]
    public void CSVExportExecutor_MapPassCountRequestToCommonExportRequest(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
      string dateSeparator, string timeSeparator, bool restrictOutputSize, bool rawDataAsDBase,
      string expectedDateSeparator, string expectedTimeSeparator)
    {
      var projectUid = Guid.NewGuid();
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.PassCountLastPass;
      var userPreference = new UserPreferences()
      {
        DateSeparator = dateSeparator,
        TimeSeparator = timeSeparator,
        DecimalSeparator = decimalSeparator,
        ThousandsSeparator = thousandsSeparator,
        Units = (int)units
      };

      var request = new CompactionPassCountExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreference, restrictOutputSize, rawDataAsDBase);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      compactionCSVExportRequest.CoordType.Should().Be(coordType);
      compactionCSVExportRequest.OutputType.Should().Be(outputType);

      compactionCSVExportRequest.UserPreferences.DecimalSeparator.Should().Be(decimalSeparator);
      compactionCSVExportRequest.UserPreferences.DateSeparator.Should().Be(expectedDateSeparator);
      compactionCSVExportRequest.UserPreferences.TimeSeparator.Should().Be(expectedTimeSeparator);

      compactionCSVExportRequest.MachineNames.Length.Should().Be(0);
      compactionCSVExportRequest.RestrictOutputSize.Should().Be(restrictOutputSize);
      compactionCSVExportRequest.RawDataAsDBase.Should().Be(rawDataAsDBase);
    }


    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "/", ":", true, false, "/", ":")]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "%", "^", false, true, "%", "^")]
    public void CSVExportExecutor_MapCommonExportRequestToArgument(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
      string dateSeparator, string timeSeparator, bool restrictOutputSize, bool rawDataAsDBase,
      string expectedDateSeparator, string expectedTimeSeparator)
    {
      var projectUid = Guid.NewGuid();
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      var theMachineName = "first machineName";
      string[] machineNames = new string[] { theMachineName };
      var userPreference = new UserPreferences()
      {
        DateSeparator = dateSeparator,
        TimeSeparator = timeSeparator,
        DecimalSeparator = decimalSeparator,
        ThousandsSeparator = thousandsSeparator,
        Units = (int)units
      };

      var request = new CompactionCSVExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreference, machineNames, restrictOutputSize, rawDataAsDBase);
      request.Validate();
      var csvExportRequestArgument = AutoMapperUtility.Automapper.Map<CSVExportRequestArgument>(request);

      csvExportRequestArgument.CoordType.Should().Be(coordType);
      csvExportRequestArgument.OutputType.Should().Be(outputType);

      csvExportRequestArgument.UserPreferences.DecimalSeparator.Should().Be(decimalSeparator);
      csvExportRequestArgument.UserPreferences.DateSeparator.Should().Be(expectedDateSeparator);
      csvExportRequestArgument.UserPreferences.TimeSeparator.Should().Be(expectedTimeSeparator);

      // these are mapped separately using CSVExportHelper.MapRequestedMachines()
      csvExportRequestArgument.MappedMachines.Count.Should().Be(0);
      csvExportRequestArgument.RawDataAsDBase.Should().Be(rawDataAsDBase);
    }


    [Fact]
    public void CSVExportExecutor_SiteModelNotFound()
    {
      var projectUid = Guid.NewGuid(); 
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      string[] machineNames = new string[] {"first machineName"};
      var userPreferences = new UserPreferences();

      var request = new CompactionVetaExportRequest(
        projectUid, filter, fileName,
        coordType, outputType, userPreferences, machineNames);
      request.Validate();
      var compactionCSVExportRequest = AutoMapperUtility.Automapper.Map<CompactionCSVExportRequest>(request);

      var executor = RequestExecutorContainer
        .Build<CSVExportExecutor>(DIContext.Obtain<IConfigurationStore>(),
          DIContext.Obtain<ILoggerFactory>(),
          DIContext.Obtain<IServiceExceptionHandler>());
      var result = Assert.Throws<ServiceException>(() => executor.Process(compactionCSVExportRequest));
      result.Code.Should().Be(HttpStatusCode.BadRequest);
      result.GetResult.Code.Should().Be(ContractExecutionStatesEnum.InternalProcessingError);
      result.GetResult.Message.Should().Be($"Site model {projectUid} is unavailable");
    }

    [Fact]
    public void CSVExportExecutor_StartEndDate()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.startUtc.Should().Be(startTime);
      startEndDate.endUtc.Should().Be(endTime);
    }

    [Fact]
    public void CSVExportExecutor_EndDateOnly()
    {
      var filter = Productivity3D.Filter.Abstractions.Models.Filter.CreateFilter(
        new DateTime(2019, 1, 10),
        null,"","",
        new List<MachineDetails>(), null, null, null, null, null, null
      );
      var filterResult = new FilterResult(null, filter, null, null, null, null, null, null);

      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      var startTime = DateTime.UtcNow.AddHours(-5);
      var endTime = startTime.AddHours(2);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(startTime, ProductionEventType.StartEvent);
      siteModel.MachinesTargetValues[0].StartEndRecordedDataEvents.PutValueAtDate(endTime, ProductionEventType.EndEvent);

      var startEndDate = CSVExportHelper.GetDateRange(siteModel, filterResult);
      filter.StartUtc.Should().NotBeNull();
      startEndDate.startUtc.Should().Be(filter.StartUtc.Value);
      startEndDate.endUtc.Should().Be(endTime);
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
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.CarryAllScraper, DeviceTypeEnum.SNM941, false, Guid.NewGuid());
      
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
    public void CSVExportExecutor_MachineHasNoEvents()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      
      var startEndDate = CSVExportHelper.GetDateRange(siteModel, null);
      startEndDate.startUtc.Should().Be(DateTime.MaxValue);
      startEndDate.endUtc.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void CSVExportExecutor_MachineNames_Success()
    {
      string[] machineNames = new[] {"Test Machine 1"};
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

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
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

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
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

      var mappedMachines = CSVExportHelper.MapRequestedMachines(siteModel, machineNames);
      mappedMachines.Count.Should().Be(0);
    }

    [Fact]
    public void CSVExportExecutor_MachineNamesMulti_NoneRequested()
    {
      string[] machineNames = null;
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      IMachine machine1 = siteModel.Machines.CreateNew("Test Machine 1", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine2 = siteModel.Machines.CreateNew("Test Machine 2", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      IMachine machine3 = siteModel.Machines.CreateNew("Test Machine 3", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());

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


