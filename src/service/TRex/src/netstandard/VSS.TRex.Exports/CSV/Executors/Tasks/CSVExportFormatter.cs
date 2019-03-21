using System;
using System.Globalization;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Exports.CSV.GridFabric;
using VSS.TRex.Types;
using GPSAccuracy = VSS.TRex.Types.GPSAccuracy;

namespace VSS.TRex.Exports.CSV.Executors.Tasks
{
  public class CSVExportFormatter
  {
    // these are public for unit tests
    public readonly CSVExportUserPreferences userPreference;
    public readonly OutputTypes outputType;
    public readonly bool isRawDataAsDBaseRequired;
    public readonly string nullString;

    private const int defaultDecimalPlaces = 3;
    public const double USFeetToMeters = 0.304800609601;
    public const double ImperialFeetToMeters = 0.3048;

    public double distanceConversionFactor { get; private set; }
    public string speedUnitString = "mph";
    public double speedConversionFactor;
    public string distanceUnitString = "FT";
    public string exportDateTimeFormatString;

    private readonly NumberFormatInfo nfiDefault;
    private readonly NumberFormatInfo nfiUser;


    public CSVExportFormatter(CSVExportUserPreferences userPreference, OutputTypes outputType, bool isRawDataAsDBaseRequired = false)
    {
      this.userPreference = userPreference;
      this.outputType = outputType;
      this.isRawDataAsDBaseRequired = isRawDataAsDBaseRequired;
      nullString = isRawDataAsDBaseRequired ? string.Empty : "?";

      SetupUserPreferences(userPreference);

      nfiUser = NumberFormatInfo.GetInstance(CultureInfo.InvariantCulture).Clone() as NumberFormatInfo;
      nfiUser.NumberDecimalSeparator = userPreference.DecimalSeparator;
      nfiUser.NumberGroupSeparator = userPreference.ThousandsSeparator;
      nfiUser.NumberDecimalDigits = defaultDecimalPlaces;
      nfiDefault = nfiUser.Clone() as NumberFormatInfo; // the dp will be altered on this
    }

    /// <summary>
    /// Use values direct from UserPreferenceData where possible.
    /// This method provides differences required for export format
    /// </summary>
    /// <param name="newUserPreference"></param>
    private void SetupUserPreferences(CSVExportUserPreferences newUserPreference)
    {
      switch (newUserPreference.Units)
      {
        case UnitsTypeEnum.US: // USSurveyFeet
        {
          distanceConversionFactor = USFeetToMeters;
          speedUnitString = "mph";
          speedConversionFactor = USFeetToMeters * 5280;
          distanceUnitString = "FT";
          break;
        }
        case UnitsTypeEnum.Metric:
        {
          distanceConversionFactor = 1.0;
          speedUnitString = "km/h";
          speedConversionFactor = 1000;
          distanceUnitString = "m";
          break;
        }
        case UnitsTypeEnum.Imperial:
        {
          distanceConversionFactor = ImperialFeetToMeters;
          speedUnitString = "mph";
          speedConversionFactor = ImperialFeetToMeters * 5280;
          distanceUnitString = "ft";
          break;
        }
      }

      if (outputType == OutputTypes.VedaAllPasses || outputType == OutputTypes.VedaFinalPass)
      {
        exportDateTimeFormatString = "yyyy-MMM-dd HH:mm:ss.fff";
      }
      else if (!string.IsNullOrEmpty(userPreference.DateSeparator) && !string.IsNullOrEmpty(userPreference.TimeSeparator) && !string.IsNullOrEmpty(userPreference.DecimalSeparator))
      {
        exportDateTimeFormatString = $"yyyy{userPreference.DateSeparator}MMM{userPreference.DateSeparator}dd HH{userPreference.TimeSeparator}mm{userPreference.TimeSeparator}ss{userPreference.DecimalSeparator}fff";
      }
      else
      {
        exportDateTimeFormatString = "yyyy-MMM-dd HH:mm:ss.fff";
      }
    }


    public string FormatCellPassTime(DateTime value)
    {
      value = value.AddHours(userPreference.ProjectTimeZoneOffset);
      return value.ToString(exportDateTimeFormatString);
    }

    public string FormatCellPos(double value)
    {
      string result;

      if (isRawDataAsDBaseRequired)
        result =  FormatDisplayDistanceUnitless(value, false);
      else
        result = FormatDisplayDistance(value, false);
      return result;
    }

    public string RadiansToLatLongString(double latLong, int dp)
    {
      var latLonDegrees = MathUtilities.RadiansToDegrees(latLong);
      return DoubleToStrF(latLonDegrees, 18, true, dp);
    }

    public string FormatElevation(double value)
    {
      if (value.Equals(Consts.NullHeight))
        return nullString;

      if (isRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value, false);

      return FormatDisplayDistance(value, false);
    }

    public string FormatRadioLatency(int value)
    {
      return value == CellPassConsts.NullRadioLatency ? nullString : value.ToString();
    }

    public string FormatSpeed(int value)
    {
      return value == Consts.NullMachineSpeed ? nullString : FormatDisplayVelocity(value/100.00, -1);
    }

    public string FormatGPSMode(GPSMode value)
    {
      var result = string.Empty;
      switch (value)
      {
        case GPSMode.Old: result = "Old Position"; break;
        case GPSMode.AutonomousPosition: result = "Autonomous"; break;
        case GPSMode.Float: result = "Float"; break;
        case GPSMode.Fixed: result = "RTK Fixed"; break;
        case GPSMode.DGPS: result = "Differential_GPS"; break;
        case GPSMode.SBAS: result = "SBAS"; break;
        case GPSMode.LocationRTK: result = "Location_RTK"; break;
        case GPSMode.NoGPS: result = "Not_Applicable"; break;
        default: result = $"unknown: {value}"; break;
      }

      return result;
    }

    public string FormatGPSAccuracy(GPSAccuracy gpsAccuracy, int gpsTolerance)
    {
      if (gpsTolerance == CellPassConsts.NullGPSTolerance)
        return nullString;

      string toleranceString;
      if (isRawDataAsDBaseRequired)
        toleranceString = FormatDisplayDistanceUnitless(gpsTolerance / 1000.000, false);
      else
        toleranceString = FormatDisplayDistance(gpsTolerance / 1000.000, false);

      return $"{FormatGPSAccuracyValue(gpsAccuracy)} ({toleranceString})";
    }

    public string FormatPassCount(int value) => value == CellPassConsts.NullPassCountValue ? nullString : $"{value.ToString()}";

    // As CCV/MDP/RMV are reported in 10th, no units...
    public string FormatCompactionCCVTypes(short value)
    {
      return value == CellPassConsts.NullCCV ? nullString : $"{DoubleToStrF(value / CellPassConsts.CCVvalueRatio, 18, true, 1)}"; 
    }

    public string FormatFrequency(ushort value)
    {
      if (value == CellPassConsts.NullFrequency)
        return nullString;
      if (isRawDataAsDBaseRequired)
        return DoubleToStrF(value / CellPassConsts.CCVvalueRatio, 18, true, 1);

      return DoubleToStrF(value / CellPassConsts.CCVvalueRatio, 18, true, 1) + "Hz";
    }

    public string FormatAmplitude(ushort value)
    {
      if (value == CellPassConsts.NullAmplitude)
        return nullString;
      if (isRawDataAsDBaseRequired)
        return DoubleToStrF(value / CellPassConsts.AmplitudeRatio, 18, true, 2);

      return DoubleToStrF(value / CellPassConsts.AmplitudeRatio, 18, true, 2) + "mm";
    }

    public string FormatTargetThickness(double value)
    {
      if (value.Equals(CellPassConsts.NullOverridingTargetLiftThicknessValue))
        return nullString;

      if (isRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value, false);

      return FormatDisplayDistance(value, false);
    }

    public string FormatMachineGearValue(MachineGear value)
    {
      var result = string.Empty;
      switch (value)
      {
        case MachineGear.Neutral: result = "Neutral"; break;
        case MachineGear.Forward: result = "Forward"; break;
        case MachineGear.Reverse: result = "Reverse"; break;
        case MachineGear.Forward2: result = "Forward_2"; break;
        case MachineGear.Forward3: result = "Forward_3"; break;
        case MachineGear.Forward4: result = "Forward_4"; break;
        case MachineGear.Forward5: result = "Forward_5"; break;
        case MachineGear.Reverse2: result = "Reverse_2"; break;
        case MachineGear.Reverse3: result = "Reverse_3"; break;
        case MachineGear.Reverse4: result = "Reverse_4"; break;
        case MachineGear.Reverse5: result = "Reverse_5"; break;
        case MachineGear.Park: result = "Park"; break;
        default: result = "Sensor_Failed"; break;
      }

      return result;
    }

    public string FormatEventVibrationState(VibrationState value)
    {
      var result = string.Empty;
      switch (value)
      {
        case VibrationState.Off: result = "Off"; break;
        case VibrationState.On: result = "On"; break;
        case VibrationState.Invalid: result = "Not_Applicable"; break;
        default: result = $"unknown: {value}"; break;
      }

      return result;
    }

    // As Material Temperature is reported in 10th of a degree Celsius...
    // Note: We do not translate the Celcius units in the formatted string.
    public string FormatLastPassValidTemperature(ushort value)
    {
      if (value.Equals(CellPassConsts.NullMaterialTemperatureValue))
        return nullString;

      string tempSymbol = "C";
      var tempValue = value / CellPassConsts.CCVvalueRatio;
      if (userPreference.TemperatureUnits == TemperatureUnitEnum.Fahrenheit)
      {
        tempSymbol = "F";
        tempValue = tempValue * 9 / 5 + 32;
      }
      return DoubleToStrF(tempValue, 18, false, 1) + "°" + tempSymbol;
    }

    private string FormatGPSAccuracyValue(GPSAccuracy value)
    {
      var result = string.Empty;
      switch (value)
      {
        case GPSAccuracy.Fine: result = "Fine"; break;
        case GPSAccuracy.Medium: result = "Medium"; break;
        case GPSAccuracy.Coarse: result = "Coarse"; break;
        default: result = $"unknown: {value}"; break;
      }

      return result;
    }

    private string FormatDisplayVelocity(double value, int dp)
    {
      if (value == Consts.NullMachineSpeed)
        return nullString;

      if (dp < 0)
        dp = 1;

      var result = DoubleToStrF((value * 3600) / speedConversionFactor, 20, true, dp);
      if (!isRawDataAsDBaseRequired)
        result += speedUnitString;
      return result;
    }

    private string FormatDisplayDistanceUnitless(double value, bool isFormattingRequired, int dp = -1)
    {
      if (dp < 0)
        dp = defaultDecimalPlaces;

      return DistanceDoubleToStrF(value, 20, isFormattingRequired, dp);
    }

    private string FormatDisplayDistance(double value, bool isFormattingRequired, int dp = -1)
    {
      string result = FormatDisplayDistanceUnitless(value, isFormattingRequired, dp);

      if (!value.Equals(Consts.NullHeight))
        result += distanceUnitString;
      return result;
    }

    // Converts a value(in meters) to a string in the current distance units 
    private string DistanceDoubleToStrF(double value, int precision, bool isFormattingRequired, int dp )
    {
      return value.Equals(Consts.NullHeight) ? nullString : DoubleToStrF(value / distanceConversionFactor, precision, isFormattingRequired, dp);
    }

    private string DoubleToStrF(double value, int precision, bool isFormattingRequired, int dp)
    {
      if (isFormattingRequired)
      {
        nfiUser.NumberDecimalDigits = dp;
        return value.ToString("N", nfiUser);
      }
      else
      {
        nfiDefault.NumberDecimalDigits = dp;
        return value.ToString("F", nfiDefault);
      }
    }

  }
}
