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
  public class Formatter
  {
    // these are public for unit tests
    public CSVExportUserPreferences userPreference;
    public OutputTypes outputType;
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
    private NumberFormatInfo nfiUser;


    public Formatter(CSVExportUserPreferences userPreference, OutputTypes outputType, bool isRawDataAsDBaseRequired = false)
    {
      this.userPreference = userPreference;
      this.outputType = outputType;
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
        exportDateTimeFormatString = string.Format("yyyy-MMM-dd HH:mm:ss.fff");
      }
      else if (!string.IsNullOrEmpty(userPreference.DateSeparator) && !string.IsNullOrEmpty(userPreference.TimeSeparator) && !string.IsNullOrEmpty(userPreference.DecimalSeparator))
      {
        exportDateTimeFormatString = string.Format($"yyyy{userPreference.DateSeparator}MMM{userPreference.DateSeparator}dd HH{userPreference.TimeSeparator}mm{userPreference.TimeSeparator}ss{userPreference.DecimalSeparator}fff");
      }
      else
      {
        exportDateTimeFormatString = string.Format("yyyy-MMM-dd HH:mm:ss.fff");
      }
    }


    public string FormatCellPassTime(DateTime value)
    {
      value = value.AddHours(userPreference.ProjectTimeZoneOffset);
      return value.ToString(exportDateTimeFormatString);
    }

    public string FormatCellPos(double value)
    {
      if (isRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value);
      return FormatDisplayDistance(value);
    }

    public string RadiansToLatLongString(double latLong, int decimalPlaces)
    {
      var latLonDegrees = MathUtilities.RadiansToDegrees(latLong);
      return DoubleToStrF(latLonDegrees, 18, decimalPlaces, true);
    }

    public string FormatElevation(double value)
    {
      if (value.Equals(Consts.NullHeight))
        return nullString;

      if (isRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value);

      return FormatDisplayDistance(value);
    }

    public string FormatRadioLatency(int value)
    {
      if (value == CellPassConsts.NullRadioLatency)
        return nullString;
      return value.ToString();
    }

    public string FormatSpeed(int value)
    {
      if (value == Consts.NullMachineSpeed)
        return nullString;
      return FormatDisplayVelocity(value / 100, -1);
    }

    public string FormatGPSMode(GPSMode value)
    {
      switch (value)
      {
        case GPSMode.Old: return "S_27782_Old_Position";
        case GPSMode.AutonomousPosition: return "S_27786_Autonomous";
        case GPSMode.Float: return "S_27787_RTK_Float";
        case GPSMode.Fixed: return "S_27788_RTK_Fixed";
        case GPSMode.DGPS: return "S_27789_Differential_GPS";
        case GPSMode.SBAS: return "S_28028_SBAS";
        case GPSMode.LocationRTK: return "S_28026_Location_RTK";
        case GPSMode.NoGPS: return "S_28169_Not_Applicable";
        default: return $"unknown: {value}";
      }
    }

    public string FormatGPSAccuracy(GPSAccuracy gpsAccuracy, int gpsTolerance)
    {
      if (gpsTolerance == CellPassConsts.NullGPSTolerance)
        return nullString;
      return string.Format($"{FormatGPSAccuracyValue(gpsAccuracy)} ({FormatDisplayDistance(gpsTolerance / 1000)})");
    }

    public string FormatPassCount(int value)
    {
      if (value == CellPassConsts.NullPassCountValue)
        return nullString;
      return string.Format($"{value.ToString()}");
    }

    // As CCV/MDP/RMV are reported in 10th, no units...
    public string FormatCompactionCCVTypes(short value)
    {
      if (value == CellPassConsts.NullCCV)
        return nullString;
      return string.Format($"{DoubleToStrF(value / CellPassConsts.CCVvalueRatio, 18,1, true)}"); 
    }

    public string FormatFrequency(ushort value)
    {
      if (value == CellPassConsts.NullFrequency)
        return nullString;
      if (isRawDataAsDBaseRequired)
        return DoubleToStrF(value / CellPassConsts.CCVvalueRatio, 18, 1, true);

      return DoubleToStrF(value / CellPassConsts.CCVvalueRatio, 18, 1, true) + "Hz";
    }

    public string FormatAmplitude(ushort value)
    {
      if (value == CellPassConsts.NullAmplitude)
        return nullString;
      if (isRawDataAsDBaseRequired)
        return DoubleToStrF(value / CellPassConsts.AmplitudeRatio, 18, 2, true);

      return DoubleToStrF(value / CellPassConsts.AmplitudeRatio, 18, 2, true) + "mm";
    }

    public string FormatTargetThickness(double value)
    {
      if (value.Equals(CellPassConsts.NullOverridingTargetLiftThicknessValue))
        return nullString;

      if (isRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value);

      return FormatDisplayDistance(value);
    }

    public string FormatMachineGearValue(MachineGear value)
    {
      switch (value)
      {
        case MachineGear.Neutral: return "S_27790_Neutral";
        case MachineGear.Forward: return "S_27791_Forward";
        case MachineGear.Reverse: return "S_27792_Reverse";
        case MachineGear.Forward2: return "S_27794_Forward_2";
        case MachineGear.Forward3: return "S_27795_Forward_3";
        case MachineGear.Forward4: return "S_27796_Forward_4";
        case MachineGear.Forward5: return "S_27797_Forward_5";
        case MachineGear.Reverse2: return "S_27798_Reverse_2";
        case MachineGear.Reverse3: return "S_27799_Reverse_3";
        case MachineGear.Reverse4: return "S_27800_Reverse_4";
        case MachineGear.Reverse5: return "S_27801_Reverse_5";
        case MachineGear.Park: return "S_28168_Park";
        default: return "S_27793_Sensor_Failed";
      }
    }

    public string FormatEventVibrationState(VibrationState value)
    {
      switch (value)
      {
        case VibrationState.Off: return "S_27802_Off";
        case VibrationState.On: return "S_27803_On";
        case VibrationState.Invalid: return "S_28169_Not_Applicable";
        default: return $"unknown: {value}";
      }
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
      return DoubleToStrF(tempValue, 18, 1) + "°" + tempSymbol;
    }

    private string FormatGPSAccuracyValue(GPSAccuracy value)
    {
      switch (value)
      {
        case GPSAccuracy.Fine: return "S_28445_Fine";
        case GPSAccuracy.Medium: return "S_28447_Medium";
        case GPSAccuracy.Coarse: return "S_28446_Coarse";
        default: return $"unknown: {value}";
      }
    }

    private string FormatDisplayVelocity(int value, int dp)
    {
      if (value == Consts.NullMachineSpeed)
        return nullString;

      if (dp < 0)
        dp = 1;

      var result = DoubleToStrF((value * 3600) / speedConversionFactor, 20, dp, true);
      if (!isRawDataAsDBaseRequired)
        result += speedUnitString;
      return result;
    }

    private string FormatDisplayDistanceUnitless(double value, int dp = -1)
    {
      if (dp < 0)
        dp = defaultDecimalPlaces;

      return DistanceDoubleToStrF(value, 20, dp);
    }

    private string FormatDisplayDistance(double value, int dp = -1)
    {
      string result = FormatDisplayDistanceUnitless(value, dp);

      if (!value.Equals(Consts.NullHeight))
        result += distanceUnitString;
      return result;
    }

    // Converts a value(in meters) to a string in the current distance units 
    private string DistanceDoubleToStrF(double value, int precision, int digits)
    {
      if (value.Equals(Consts.NullHeight))
        return nullString;

      return (DoubleToStrF(value / distanceConversionFactor, precision, digits, true));
    }

    private string DoubleToStrF(double value, int precision, int digits)
    {
      nfiDefault.NumberDecimalDigits = digits;
      var result = value.ToString("N", nfiDefault);
      return result;
    }

    private string DoubleToStrF(double value, int precision, int digits, bool isUserFormattingRequired)
    {
      nfiUser.NumberDecimalDigits = digits;
      var result = value.ToString("N", nfiUser);
      return result;
    }

  }
}
