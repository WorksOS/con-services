using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Gateway.Common.Converters;
using Formatter = VSS.TRex.Exports.CSV.Executors.Tasks.Formatter;

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

      var formatter = new Formatter(csvUserPreference, outputType, false);
      formatter.userPreference.DateSeparator.Should().Be("-");
      formatter.userPreference.TimeSeparator.Should().Be(":");
      formatter.userPreference.DecimalSeparator.Should().Be(".");
      formatter.userPreference.ThousandsSeparator.Should().Be(",");
      formatter.outputType.Should().Be(outputType);
      formatter.isRawDataAsDBaseRequired.Should().Be(false);
      formatter.nullString.Should().Be("?");

      formatter.userPreference.Units.Should().Be(UnitsTypeEnum.US);
      formatter.distanceConversionFactor.Should().Be(Formatter.USFeetToMeters);
      formatter.speedUnitString.Should().Be("mph");
      formatter.speedConversionFactor.Should().Be(Formatter.USFeetToMeters * 5280);
      formatter.distanceUnitString.Should().Be("FT");
      formatter.exportDateTimeFormatString.Should().Be("yyyy-MMM-dd HH:mm:ss.fff");
    }

    [Theory]
    [InlineData("*", "$", "&", "@", UnitsTypeEnum.US, OutputTypes.VedaAllPasses, false,
        "?", Formatter.USFeetToMeters, "mph", Formatter.USFeetToMeters * 5280, "FT",
        "yyyy-MMM-dd HH:mm:ss.fff")]
    [InlineData("-", ":", ".", ",", UnitsTypeEnum.US, OutputTypes.PassCountLastPass, true,
      "", Formatter.USFeetToMeters, "mph", Formatter.USFeetToMeters * 5280, "FT",
      "yyyy-MMM-dd HH:mm:ss.fff")]
    [InlineData("*", "$", "&", "@", UnitsTypeEnum.Metric, OutputTypes.PassCountLastPass, false,
      "?", 1.0, "km/h", 1000, "m",
      "yyyy*MMM*dd HH$mm$ss&fff")]
    [InlineData("-", ":", ".", ",", UnitsTypeEnum.Imperial, OutputTypes.PassCountLastPass, false,
      "?", Formatter.ImperialFeetToMeters, "mph", Formatter.ImperialFeetToMeters * 5280, "ft",
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

      var formatter = new Formatter(csvUserPreference, outputType, isRawDataAsDBaseRequired);
      formatter.userPreference.DateSeparator.Should().Be(dateSeparator);
      formatter.userPreference.TimeSeparator.Should().Be(timeSeparator);
      formatter.userPreference.DecimalSeparator.Should().Be(decimalSeparator);
      formatter.userPreference.ThousandsSeparator.Should().Be(thousandsSeparator);
      formatter.outputType.Should().Be(outputType);
      formatter.isRawDataAsDBaseRequired.Should().Be(isRawDataAsDBaseRequired);
      // this depends on isRawDataAsDBaseRequired
      formatter.nullString.Should().Be(expectedNullString);

      formatter.userPreference.Units.Should().Be(units);
      formatter.distanceConversionFactor.Should().Be(expectedDistanceConversionFactor);
      formatter.speedUnitString.Should().Be(expectedSpeedUnitString);
      formatter.speedConversionFactor.Should().Be(expectedSpeedConversionFactor);
      formatter.distanceUnitString.Should().Be(expectedDistanceUnitString);
      formatter.exportDateTimeFormatString.Should().Be(expectedExportDateTimeFormatString);
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

      var formatter = new Formatter(csvUserPreference, outputType, false);
      var result = formatter.FormatCellPassTime(valueDateTime);
      result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("&", "@", UnitsTypeEnum.Metric, false, 24666.7123112f, "24@666&713m")]
    [InlineData("&", "@", UnitsTypeEnum.US, false, 24666.7123112f, "80@927&374FT")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, false, 24666.7123112f, "80@927&536ft")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, true, 24666.7123112f, "80@927&536")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, true, -3.4E38f, "")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, false, -3.4E38f, "?")]
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
      var formatter = new Formatter(csvUserPreference, outputType, isRawDataAsDBaseRequired);
      var result = formatter.FormatElevation(value);
      result.Should().Be(expectedResult);
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

