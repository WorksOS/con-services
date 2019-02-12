using System;
using System.Globalization;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;

namespace VSS.TRex.Exports.CSV.Executors.Tasks
{
  public class Formatter
  {
    public UserPreferenceData userPreference;
    public OutputTypes coordinateOutputType;
    public readonly bool isRawDataAsDBaseRequired = false;
    public readonly string nullString;

    private const int defaultDecimalPlaces = 3;
    public const double USFeetToMeters = 0.304800609601;
    public const double ImperialFeetToMeters = 0.3048;

    public double distanceConversionFactor { get; set; }
    public string speedUnitString = "mph";
    public double speedConversionFactor;
    public string distanceUnitString = "FT";
    public string exportDateTimeFormatString;

    private NumberFormatInfo nfiDefault;
    public NumberFormatInfo nfiUser;


    public Formatter(UserPreferenceData userPreference, OutputTypes coordinateOutputType, bool isRawDataAsDBaseRequired = false)
    {
      this.userPreference = userPreference;
      this.coordinateOutputType = coordinateOutputType;
      this.isRawDataAsDBaseRequired = isRawDataAsDBaseRequired;
      nullString = isRawDataAsDBaseRequired ? string.Empty : "?";

      SetupUserPreferences(userPreference);
      nfiDefault = NumberFormatInfo.GetInstance(CultureInfo.InvariantCulture).Clone() as NumberFormatInfo;
      nfiUser = NumberFormatInfo.GetInstance(CultureInfo.InvariantCulture).Clone() as NumberFormatInfo;
      nfiUser.NumberDecimalSeparator = userPreference.DecimalSeparator;
      nfiUser.NumberGroupSeparator = userPreference.ThousandsSeparator;
      nfiUser.NumberDecimalDigits = defaultDecimalPlaces;
    }

    /// <summary>
    /// Use values direct from UserPreferenceData where possible.
    /// This method provides differences required for export format
    /// </summary>
    /// <param name="newUserPreference"></param>
    private void SetupUserPreferences(UserPreferenceData newUserPreference)
    {
      switch (newUserPreference.Units)
      {
        case "US":
        case "US Standard": // USSurveyFeet
        {
          distanceConversionFactor = USFeetToMeters;
          speedUnitString = "mph";
          speedConversionFactor = USFeetToMeters * 5280;
          distanceUnitString = "FT";
          break;
        }
        case "Metric":
        {
          distanceConversionFactor = 1.0;
          speedUnitString = "km/h";
          speedConversionFactor = 1000;
          distanceUnitString = "m";
          break;
        }
        case "Imperial":
        {
          distanceConversionFactor = ImperialFeetToMeters;
          speedUnitString = "mph";
          speedConversionFactor = ImperialFeetToMeters * 5280;
          distanceUnitString = "ft";
          break;
        }
      }

      if (coordinateOutputType == OutputTypes.VedaAllPasses || coordinateOutputType == OutputTypes.VedaFinalPass)
      {
        exportDateTimeFormatString = string.Format("yyyy-mmm-dd hh:nn:ss.zzz");
      }
      else if (!string.IsNullOrEmpty(userPreference.DateSeparator) && !string.IsNullOrEmpty(userPreference.TimeSeparator))
      {
        exportDateTimeFormatString = string.Format($"yyyy{userPreference.DateSeparator}mmm{userPreference.DateSeparator}dd hh{userPreference.TimeSeparator}nn{userPreference.TimeSeparator}ss{userPreference.DecimalSeparator}zzz");
      }
      else
      {
        exportDateTimeFormatString = string.Format("yyyy-mmm-dd hh:nn:ss.zzz");
      }
    }

    public string FormatElevation(float value)
    {
      if (value.Equals(Consts.NullHeight))
        return nullString;

      if (isRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value);

      return FormatDisplayDistance(value);
    }
    
    private string FormatDisplayDistanceUnitless(float value, int dp = -1)
    {
      if (dp < 0)
        dp = defaultDecimalPlaces;

      return DistanceFloatToStrF(value, 20, dp);
    }

    private string FormatDisplayDistance(float value, int dp = -1)
    {
      string result = FormatDisplayDistanceUnitless(value, dp);

      if (!value.Equals(Consts.NullHeight))
        result += distanceUnitString;
      return result;
    }

    // Converts a float value(in meters) to a string in the current distance units 
    private string DistanceFloatToStrF(float value, int precision, int digits)
    {
      if (value.Equals(Consts.NullHeight))
        return nullString;

      return (FloatToStrF(value / (float) distanceConversionFactor, precision, digits, true));
    }

    private string FloatToStrF(float value, int precision, int digits)
    {
      nfiDefault.NumberDecimalDigits = digits;
      var result = value.ToString("N", nfiDefault);
      return result;
    }

    private string FloatToStrF(float value, int precision, int digits, bool isUserFormattingRequired)
    {
      nfiUser.NumberDecimalDigits = digits;
      var result = value.ToString("N", nfiUser);
      return result;
    }

  }
}
