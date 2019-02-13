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
    public void CSVExportFormatter_Default()
    {
      var userPreferences = DefaultUserPreferences();
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);
      OutputTypes coordinateOutputType = OutputTypes.PassCountAllPasses;
      bool isRawDataAsDBaseRequired = false;

      var formatter = new Formatter(csvUserPreference, coordinateOutputType, isRawDataAsDBaseRequired);
      formatter.userPreference.DateSeparator.Should().Be("-");
      formatter.userPreference.TimeSeparator.Should().Be(":");
      formatter.userPreference.DecimalSeparator.Should().Be(".");
      formatter.userPreference.ThousandsSeparator.Should().Be(",");
      formatter.coordinateOutputType.Should().Be(coordinateOutputType);
      formatter.isRawDataAsDBaseRequired.Should().Be(isRawDataAsDBaseRequired);
      formatter.nullString.Should().Be("?");

      formatter.userPreference.Units.Should().Be(UnitsTypeEnum.US);
      formatter.distanceConversionFactor.Should().Be(Formatter.USFeetToMeters);
      formatter.speedUnitString.Should().Be("mph");
      formatter.speedConversionFactor.Should().Be(Formatter.USFeetToMeters * 5280);
      formatter.distanceUnitString.Should().Be("FT");
      formatter.exportDateTimeFormatString.Should().Be("yyyy-mmm-dd hh:nn:ss.zzz");
    }

    [Theory]
    [InlineData("*", "$", "&", "@", UnitsTypeEnum.US, OutputTypes.VedaAllPasses, false,
        "?", Formatter.USFeetToMeters, "mph", Formatter.USFeetToMeters * 5280, "FT",
        "yyyy-mmm-dd hh:nn:ss.zzz")]
    [InlineData("-", ":", ".", ",", UnitsTypeEnum.US, OutputTypes.PassCountLastPass, true,
      "", Formatter.USFeetToMeters, "mph", Formatter.USFeetToMeters * 5280, "FT",
      "yyyy-mmm-dd hh:nn:ss.zzz")]
    [InlineData("*", "$", "&", "@", UnitsTypeEnum.Metric, OutputTypes.PassCountLastPass, false,
      "?", 1.0, "km/h", 1000, "m",
      "yyyy*mmm*dd hh$nn$ss&zzz")]
    [InlineData("-", ":", ".", ",", UnitsTypeEnum.Imperial, OutputTypes.PassCountLastPass, false,
      "?", Formatter.ImperialFeetToMeters, "mph", Formatter.ImperialFeetToMeters * 5280, "ft",
      "yyyy-mmm-dd hh:nn:ss.zzz")]
    public void CSVExportFormatter_Variable
      (string dateSeparator, string timeSeparator, string decimalSeparator, string thousandsSeparator,
      UnitsTypeEnum units,
      OutputTypes coordinateOutputType, bool isRawDataAsDBaseRequired,
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

      var formatter = new Formatter(csvUserPreference, coordinateOutputType, isRawDataAsDBaseRequired);
      formatter.userPreference.DateSeparator.Should().Be(dateSeparator);
      formatter.userPreference.TimeSeparator.Should().Be(timeSeparator);
      formatter.userPreference.DecimalSeparator.Should().Be(decimalSeparator);
      formatter.userPreference.ThousandsSeparator.Should().Be(thousandsSeparator);
      formatter.coordinateOutputType.Should().Be(coordinateOutputType);
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
    [InlineData("&", "@", UnitsTypeEnum.Metric, false, 24666.7123112f, "24@666&710m")]
    [InlineData("&", "@", UnitsTypeEnum.US, false, 24666.7123112f, "80@927&380FT")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, false, 24666.7123112f, "80@927&530ft")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, true, 24666.7123112f, "80@927&530")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, true, -3.4E38f, "")]
    [InlineData("&", "@", UnitsTypeEnum.Imperial, false, -3.4E38f, "?")]
    public void CSVExportFormatter_FloatToString
      (string decimalSeparator, string thousandsSeparator, UnitsTypeEnum units,
        bool isRawDataAsDBaseRequired,
        float value, string expectedResult
      )
    {
      var userPreferences = new UserPreferences()
       { DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeparator, Units = (int) units };
      var csvUserPreference = AutoMapperUtility.Automapper.Map<CSVExportUserPreferences>(userPreferences);

      OutputTypes coordinateOutputType = OutputTypes.PassCountLastPass;
      var formatter = new Formatter(csvUserPreference, coordinateOutputType, isRawDataAsDBaseRequired);
      var result = formatter.FormatElevation(value);
      result.Should().Be(expectedResult);
    }
    private UserPreferences DefaultUserPreferences()
    {
      return new UserPreferences(
        string.Empty,
        CSVExportUserPreferences.DefaultDateSeparator,
        CSVExportUserPreferences.DefaultTimeSeparator,
        //Hardwire number format as "xxx,xxx.xx" or it causes problems with the CSV file as comma is the column separator.
        //To respect user preferences requires Raptor to enclose formatted numbers in quotes.
        //This bug is present in CG since it uses user preferences separators.
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

