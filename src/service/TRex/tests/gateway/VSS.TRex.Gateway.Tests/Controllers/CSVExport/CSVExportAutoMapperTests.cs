using System;
using FluentAssertions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Requests;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Controllers.CSVExport
{
  public class CSVExportAutoMapperTests
  {

    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "/", ":", TemperatureUnitEnum.Celsius, "/", ":" )]
    [InlineData("&", "@", UnitsTypeEnum.Metric, "%", "^", TemperatureUnitEnum.Fahrenheit, "%", "^")]
    public void CSVExportHelper_MapUserPreferences(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
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
    public void CSVExportHelper_MapVetaRequestToCommonExportRequest(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
      string dateSeparator, string timeSeparator,
      string expectedDateSeparator, string expectedTimeSeparator)
    {
      var projectUid = Guid.NewGuid();
      FilterResult filter = null;
      var fileName = "gotAFilename";
      var coordType = CoordType.LatLon;
      var outputType = OutputTypes.VedaAllPasses;
      var theMachineName = "first machineName";
      var machineNames = new[] { theMachineName };
      var userPreference = new UserPreferences()
        { DateSeparator = dateSeparator, TimeSeparator = timeSeparator,
          DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeparator, Units = (int)units };
     
      var request = new CompactionVetaExportRequest(
        projectUid, filter, fileName, coordType, outputType, userPreference, machineNames, null, null);
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
    public void CSVExportHelper_MapPassCountRequestToCommonExportRequest(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
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
        coordType, outputType, userPreference, restrictOutputSize, rawDataAsDBase, null, null);
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
    public void CSVExportHelper_MapCommonExportRequestToArgument(string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
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
  }
}


