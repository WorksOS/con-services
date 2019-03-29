using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Exports.CSV.Executors.Tasks;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Types;
using GPSAccuracy = VSS.TRex.Types.GPSAccuracy;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportFormatterTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void FormatterInitializationDefault()
    {
      var userPreferences = DefaultUserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      OutputTypes outputType = OutputTypes.PassCountAllPasses;

      var formatter = new CSVExportFormatter(csvUserPreference, outputType, false);
      formatter.UserPreference.DateSeparator.Should().Be("/");
      formatter.UserPreference.TimeSeparator.Should().Be(":");
      formatter.UserPreference.DecimalSeparator.Should().Be(".");
      formatter.UserPreference.ThousandsSeparator.Should().Be(",");
      formatter.OutputType.Should().Be(outputType);
      formatter.IsRawDataAsDBaseRequired.Should().Be(false);
      formatter.NullString.Should().Be("?");

      formatter.UserPreference.Units.Should().Be(UnitsTypeEnum.Metric);
      formatter.DistanceConversionFactor.Should().Be(1.0);
      formatter.SpeedUnitString.Should().Be("km/h");
      formatter.SpeedConversionFactor.Should().Be(1000);
      formatter.DistanceUnitString.Should().Be("m");
      formatter.ExportDateTimeFormatString.Should().Be("yyyy/MMM/dd HH:mm:ss.fff");
    }

    [Theory]
    [InlineData("*", "$", "&", "@", UnitsTypeEnum.US, OutputTypes.VedaAllPasses, false,
        "?", CSVExportFormatter.USFeetToMeters, "mph", CSVExportFormatter.USFeetToMeters * 5280, "FT",
        "yyyy-MMM-dd HH:mm:ss.fff")]
    [InlineData("-", ":", ".", ",", UnitsTypeEnum.US, OutputTypes.PassCountLastPass, true,
      "", CSVExportFormatter.USFeetToMeters, "mph", CSVExportFormatter.USFeetToMeters * 5280, "FT",
      "yyyy-MMM-dd HH:mm:ss.fff")]
    [InlineData("*", "$", "&", "@", UnitsTypeEnum.Metric, OutputTypes.PassCountLastPass, false,
      "?", 1.0, "km/h", 1000, "m",
      "yyyy*MMM*dd HH$mm$ss&fff")]
    [InlineData("-", ":", ".", ",", UnitsTypeEnum.Imperial, OutputTypes.PassCountLastPass, false,
      "?", CSVExportFormatter.ImperialFeetToMeters, "mph", CSVExportFormatter.ImperialFeetToMeters * 5280, "ft",
      "yyyy-MMM-dd HH:mm:ss.fff")]
    public void FormatterInitialization
      (string dateSeparator, string timeSeparator, string decimalSeparator, string thousandsSeparator,
      UnitsTypeEnum units,
      OutputTypes outputType, bool isRawDataAsDBaseRequired,
      string expectedNullString, double expectedDistanceConversionFactor, 
      string expectedSpeedUnitString, double expectedSpeedConversionFactor,
      string expectedDistanceUnitString, string expectedExportDateTimeFormatString
      )
    {
      var userPreference = new UserPreferences()
        // veta should ignore separators
        { DateSeparator = dateSeparator, TimeSeparator = timeSeparator,
          DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeparator,
          Units = (int) units
        };
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreference);

      var formatter = new CSVExportFormatter(csvUserPreference, outputType, isRawDataAsDBaseRequired);
      formatter.UserPreference.DateSeparator.Should().Be(dateSeparator);
      formatter.UserPreference.TimeSeparator.Should().Be(timeSeparator);
      formatter.UserPreference.DecimalSeparator.Should().Be(decimalSeparator);
      formatter.UserPreference.ThousandsSeparator.Should().Be(thousandsSeparator);
      formatter.OutputType.Should().Be(outputType);
      formatter.IsRawDataAsDBaseRequired.Should().Be(isRawDataAsDBaseRequired);
      // this depends on isRawDataAsDBaseRequired
      formatter.NullString.Should().Be(expectedNullString);

      formatter.UserPreference.Units.Should().Be(units);
      formatter.DistanceConversionFactor.Should().Be(expectedDistanceConversionFactor);
      formatter.SpeedUnitString.Should().Be(expectedSpeedUnitString);
      formatter.SpeedConversionFactor.Should().Be(expectedSpeedConversionFactor);
      formatter.DistanceUnitString.Should().Be(expectedDistanceUnitString);
      formatter.ExportDateTimeFormatString.Should().Be(expectedExportDateTimeFormatString);
    }

    [Theory]
    [InlineData("-", ":", ".", OutputTypes.PassCountLastPass, "13 Nov 77 18:34:12.123456", 0, "1977-Nov-13 18:34:12.123")]
    [InlineData("/", ":", ".", OutputTypes.PassCountLastPass, "13 Nov 77 6:34:12.123456", -13, "1977/Nov/12 17:34:12.123")]
    [InlineData("&", "@", "^", OutputTypes.PassCountLastPass, "13 Nov 77 18:34:12.123456", 13.5, "1977&Nov&14 08@04@12^123")]
    public void CellPassDateToString
    (string dateSeparator, string timeSeparator, string decimalSeparator, OutputTypes outputType,
      string value, double timeZoneOffset, string expectedResult)
    {
      DateTime valueDateTime = DateTime.Parse(value);

      var userPreferences = new UserPreferences(
        "",
        dateSeparator, timeSeparator, ",", decimalSeparator, timeZoneOffset, 
        0, 1, 0, 1, 1, 1);

      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      var formatter = new CSVExportFormatter(csvUserPreference, outputType, false);
      var result = formatter.FormatCellPassTime(valueDateTime);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, false, 24666.7123112f, "24666&713m")]
    [InlineData("&", "@", UnitsTypeEnum.US, false, 24666.7123112f, "80927&374FT")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, false, 24666.7123112f, "80927&536ft")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, true, 24666.7123112f, "80927&536")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, true, Consts.NullHeight, "")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, false, Consts.NullHeight, "?")]
    public void FormatElevationString
    (string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
      bool isRawDataAsDBaseRequired,
      float value, string expectedResult
    )
    {
      var userPreferences = new UserPreferences()
        { DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeparator, Units = (int)units };
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      OutputTypes outputType = OutputTypes.PassCountLastPass;
      var formatter = new CSVExportFormatter(csvUserPreference, outputType, isRawDataAsDBaseRequired);
      var result = formatter.FormatElevation(value);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(TemperatureUnitEnum.Fahrenheit, 2466, "475.9°F")]
    [InlineData(TemperatureUnitEnum.Celsius, 2466, "246.6°C")]
    [InlineData(TemperatureUnitEnum.None, 2466, "246.6°C")]
    [InlineData(TemperatureUnitEnum.Fahrenheit, CellPassConsts.NullMaterialTemperatureValue, "?")]
    [InlineData(TemperatureUnitEnum.Celsius, CellPassConsts.NullMaterialTemperatureValue, "?")]
    [InlineData(TemperatureUnitEnum.Celsius, CellPassConsts.NullMaterialTemperatureValue - 1, "409.5°C")]
    public void FormatLastPassValidTemperature(TemperatureUnitEnum temperatureUnits, ushort value, string expectedResult)
    {
      var userPreferences = new UserPreferences(){TemperatureUnits = (int)temperatureUnits };
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass);

      var result = formatter.FormatLastPassValidTemperature(value);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, 2466, "246.6")]
    [InlineData(false, 2466, "246.6Hz")]
    [InlineData(true, CellPassConsts.NullFrequency, "")]
    [InlineData(false, CellPassConsts.NullFrequency, "?")]
    public void FormatFrequency(bool isRawDataAsDBaseRequired, ushort value, string expectedResult)
    {
      var userPreferences = new UserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass, isRawDataAsDBaseRequired);

      var result = formatter.FormatFrequency(value);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(false, UnitsTypeEnum.US, GPSAccuracy.Fine, 2466, "Fine (8.091FT)")]
    [InlineData(true, UnitsTypeEnum.Metric, GPSAccuracy.Medium, 2466, "Medium (2.466)")]
    [InlineData(true, UnitsTypeEnum.Metric, GPSAccuracy.Coarse, 2466, "Coarse (2.466)")]
    [InlineData(true, UnitsTypeEnum.Metric, GPSAccuracy.Unknown, 2466, "unknown: Unknown (2.466)")]
    [InlineData(false, UnitsTypeEnum.US, GPSAccuracy.Fine, CellPassConsts.NullGPSTolerance, "?")]
    [InlineData(true, UnitsTypeEnum.US, GPSAccuracy.Fine, CellPassConsts.NullGPSTolerance, "")]
    public void FormatGPSAccuracy(bool isRawDataAsDBaseRequired, UnitsTypeEnum units, GPSAccuracy gpsAccuracy, int gpsTolerance, string expectedResult)
    {
      var userPreferences = new UserPreferences(){ Units = (int)units };
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass, isRawDataAsDBaseRequired);

      var result = formatter.FormatGPSAccuracy(gpsAccuracy, gpsTolerance);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(false, UnitsTypeEnum.US, 24687, "552.2mph")]
    [InlineData(true, UnitsTypeEnum.Metric, 24687, "888.7")]
    [InlineData(false, UnitsTypeEnum.US, Consts.NullMachineSpeed, "?")]
    [InlineData(true, UnitsTypeEnum.US, Consts.NullMachineSpeed, "")]
    public void FormatSpeed(bool isRawDataAsDBaseRequired, UnitsTypeEnum units, int speed, string expectedResult)
    {
      var userPreferences = new UserPreferences() { Units = (int)units };
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass, isRawDataAsDBaseRequired);

      var result = formatter.FormatSpeed(speed);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(true, 2466, "24.66")]
    [InlineData(false, 2466, "24.66mm")]
    [InlineData(true, CellPassConsts.NullAmplitude, "")]
    [InlineData(false, CellPassConsts.NullAmplitude, "?")]
    public void FormatAmplitude(bool isRawDataAsDBaseRequired, ushort value, string expectedResult)
    {
      var userPreferences = new UserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass, isRawDataAsDBaseRequired);

      var result = formatter.FormatAmplitude(value);
      result.Should().Be(expectedResult);
    }
    
    [Theory]
    [InlineData(MachineGear.Neutral, "Neutral")]
    [InlineData(MachineGear.Forward, "Forward")]
    [InlineData(MachineGear.Reverse, "Reverse")]
    [InlineData(MachineGear.Forward2, "Forward_2")]
    [InlineData(MachineGear.Forward3, "Forward_3")]
    [InlineData(MachineGear.Forward4, "Forward_4")]
    [InlineData(MachineGear.Forward5, "Forward_5")]
    [InlineData(MachineGear.Reverse2, "Reverse_2")]
    [InlineData(MachineGear.Reverse3, "Reverse_3")]
    [InlineData(MachineGear.Reverse4, "Reverse_4")]
    [InlineData(MachineGear.Reverse5, "Reverse_5")]
    [InlineData(MachineGear.Park, "Park")]
    [InlineData(MachineGear.Unknown, "Sensor_Failed")]
    public void FormatMachineGear(MachineGear value, string expectedResult)
    {
      var userPreferences = new UserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass);

      var result = formatter.FormatMachineGearValue(value);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(GPSMode.Old, "Old Position")]
    [InlineData(GPSMode.AutonomousPosition, "Autonomous")]
    [InlineData(GPSMode.Float, "Float")]
    [InlineData(GPSMode.Fixed, "RTK Fixed")]
    [InlineData(GPSMode.DGPS, "Differential_GPS")]
    [InlineData(GPSMode.SBAS, "SBAS")]
    [InlineData(GPSMode.LocationRTK, "Location_RTK")]
    [InlineData(GPSMode.NoGPS, "Not_Applicable")]
    [InlineData(GPSMode.Unknown5, "unknown: Unknown5")]
    public void FormatGPSMode(GPSMode value, string expectedResult)
    {
      var userPreferences = new UserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass);

      var result = formatter.FormatGPSMode(value);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(VibrationState.Off, "Off")]
    [InlineData(VibrationState.On, "On")]
    [InlineData(VibrationState.Invalid, "Not_Applicable")]
    public void FormatEventVibrationState(VibrationState value, string expectedResult)
    {
      var userPreferences = new UserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass);

      var result = formatter.FormatEventVibrationState(value);
      result.Should().Be(expectedResult);
    }

    [Fact]
    public void FormatEventVibrationState_Unknown()
    {
      var userPreferences = new UserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      var formatter = new CSVExportFormatter(csvUserPreference, OutputTypes.PassCountLastPass);

      var result = formatter.FormatEventVibrationState((VibrationState)100);
      result.Should().Be("unknown: 100");
    }


    private UserPreferences DefaultUserPreferences()
    {
      return new UserPreferences(
        string.Empty,
        CSVExportUserPreferences.DefaultDateSeparator,
        CSVExportUserPreferences.DefaultTimeSeparator,
        CSVExportUserPreferences.DefaultThousandsSeparator,
        CSVExportUserPreferences.DefaultDecimalSeparator,
        0,
        0,
        (int)CSVExportUserPreferences.DefaultUnits,
        0,
        0,
        (int)CSVExportUserPreferences.DefaultTemperatureUnits,
        3); // is this used?
    }
  }
}

