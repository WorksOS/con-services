using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using Formatter = VSS.TRex.Exports.CSV.Executors.Tasks.Formatter;

namespace VSS.TRex.Tests.Exports.CSV
{
  public class CSVExportFormatterTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void CSVExportFormatter_Default()
    {
      var userPreference = new UserPreferenceData();
      OutputTypes coordinateOutputType = OutputTypes.PassCountAllPasses;
      bool isRawDataAsDBaseRequired = false;

      var formatter = new Formatter(userPreference, coordinateOutputType, isRawDataAsDBaseRequired);
      formatter.userPreference.DateSeparator.Should().Be("-");
      formatter.userPreference.TimeSeparator.Should().Be(":");
      formatter.userPreference.DecimalSeparator.Should().Be(".");
      formatter.userPreference.ThousandsSeparator.Should().Be(",");
      formatter.coordinateOutputType.Should().Be(coordinateOutputType);
      formatter.isRawDataAsDBaseRequired.Should().Be(isRawDataAsDBaseRequired);
      formatter.nullString.Should().Be("?");

      formatter.userPreference.Units.Should().Be("US");
      formatter.distanceConversionFactor.Should().Be(Formatter.USFeetToMeters);
      formatter.speedUnitString.Should().Be("mph");
      formatter.speedConversionFactor.Should().Be(Formatter.USFeetToMeters * 5280);
      formatter.distanceUnitString.Should().Be("FT");
      formatter.exportDateTimeFormatString.Should().Be("yyyy-mmm-dd hh:nn:ss.zzz");
    }

    [Theory]
    [InlineData("*", "$", "&", "@", "US", OutputTypes.VedaAllPasses, false,
        "?", Formatter.USFeetToMeters, "mph", Formatter.USFeetToMeters * 5280, "FT",
        "yyyy-mmm-dd hh:nn:ss.zzz")]
    [InlineData("-", ":", ".", ",", "US", OutputTypes.PassCountLastPass, true,
      "", Formatter.USFeetToMeters, "mph", Formatter.USFeetToMeters * 5280, "FT",
      "yyyy-mmm-dd hh:nn:ss.zzz")]
    [InlineData("*", "$", "&", "@", "Metric", OutputTypes.PassCountLastPass, false,
      "?", 1.0, "km/h", 1000, "m",
      "yyyy*mmm*dd hh$nn$ss&zzz")]
    [InlineData("-", ":", ".", ",", "Imperial", OutputTypes.PassCountLastPass, false,
      "?", Formatter.ImperialFeetToMeters, "mph", Formatter.ImperialFeetToMeters * 5280, "ft",
      "yyyy-mmm-dd hh:nn:ss.zzz")]
    public void CSVExportFormatter_Variable
      (string dateSeparator, string timeSeparator, string decimalSeparator, string thousandsSeperator,
      string units,
      OutputTypes coordinateOutputType, bool isRawDataAsDBaseRequired,
      string expectedNullString, double expectedDistanceConversionFactor, 
      string expectedSpeedUnitString, double expectedSpeedConversionFactor,
      string expectedDistanceUnitString, string expectedExportDateTimeFormatString
      )
    {
      var userPreference = new UserPreferenceData()
        // veta should ignore these
        { DateSeparator = dateSeparator, TimeSeparator = timeSeparator,
          DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeperator,
          Units = units
        };

      var formatter = new Formatter(userPreference, coordinateOutputType, isRawDataAsDBaseRequired);
      formatter.userPreference.DateSeparator.Should().Be(dateSeparator);
      formatter.userPreference.TimeSeparator.Should().Be(timeSeparator);
      formatter.userPreference.DecimalSeparator.Should().Be(decimalSeparator);
      formatter.userPreference.ThousandsSeparator.Should().Be(thousandsSeperator);
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
    [InlineData("&", "@", "Metric", false, 24666.7123112f, "24@666&710m")]
    [InlineData("&", "@", "US", false, 24666.7123112f, "80@927&380FT")]
    [InlineData("&", "@", "Imperial", false, 24666.7123112f, "80@927&530ft")]
    [InlineData("&", "@", "Imperial", true, 24666.7123112f, "80@927&530")]
    [InlineData("&", "@", "Imperial", true, -3.4E38f, "")]
    [InlineData("&", "@", "Imperial", false, -3.4E38f, "?")]
    public void CSVExportFormatter_FloatToString
      (string decimalSeparator, string thousandsSeperator, string units,
        bool isRawDataAsDBaseRequired,
        float value, string expectedResult
      )
    {
      var userPreference = new UserPreferenceData()
       { DecimalSeparator = decimalSeparator, ThousandsSeparator = thousandsSeperator, Units = units };

      OutputTypes coordinateOutputType = OutputTypes.PassCountLastPass;
      var formatter = new Formatter(userPreference, coordinateOutputType, isRawDataAsDBaseRequired);
      var result = formatter.FormatElevation(value);
      result.Should().Be(expectedResult);
    }
  }
}

