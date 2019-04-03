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
    public readonly CSVExportUserPreferences UserPreference;
    public readonly OutputTypes OutputType;
    public readonly bool IsRawDataAsDBaseRequired;
    public readonly string NullString;

    private const int DefaultDecimalPlaces = 3;
    public const double USFeetToMeters = 0.304800609601;
    public const double ImperialFeetToMeters = 0.3048;

    public double DistanceConversionFactor { get; private set; }
    public string SpeedUnitString = "mph";
    public double SpeedConversionFactor;
    public string DistanceUnitString = "FT";
    public string ExportDateTimeFormatString;

    private readonly NumberFormatInfo _nfiDefault;
    private readonly NumberFormatInfo _nfiUser;


    public CSVExportFormatter(CSVExportUserPreferences userPreference, OutputTypes outputType, bool isRawDataAsDBaseRequired = false)
    {
      UserPreference = userPreference;
      OutputType = outputType;
      IsRawDataAsDBaseRequired = isRawDataAsDBaseRequired;
      NullString = isRawDataAsDBaseRequired ? string.Empty : "?";

      SetupUserPreferences(userPreference);

      _nfiUser = NumberFormatInfo.GetInstance(CultureInfo.InvariantCulture).Clone() as NumberFormatInfo;
      _nfiUser.NumberDecimalSeparator = userPreference.DecimalSeparator ?? _nfiUser.NumberDecimalSeparator;
      _nfiUser.NumberGroupSeparator = userPreference.ThousandsSeparator ?? _nfiUser.NumberGroupSeparator;
      _nfiUser.NumberDecimalDigits = DefaultDecimalPlaces;
      _nfiDefault = _nfiUser.Clone() as NumberFormatInfo; // the dp will be altered on this
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
        case UnitsTypeEnum.Metric:
        {
          DistanceConversionFactor = 1.0;
          SpeedUnitString = "km/h";
          SpeedConversionFactor = 1000;
          DistanceUnitString = "m";
          break;
        }
        case UnitsTypeEnum.Imperial:
        {
          DistanceConversionFactor = ImperialFeetToMeters;
          SpeedUnitString = "mph";
          SpeedConversionFactor = ImperialFeetToMeters * 5280;
          DistanceUnitString = "ft";
          break;
        }
        default:   // case UnitsTypeEnum.US: // USSurveyFeet
        {
          DistanceConversionFactor = USFeetToMeters;
          SpeedUnitString = "mph";
          SpeedConversionFactor = USFeetToMeters * 5280;
          DistanceUnitString = "FT";
          break;
        }
      }

      if (OutputType == OutputTypes.VedaAllPasses || OutputType == OutputTypes.VedaFinalPass ||
          string.IsNullOrEmpty(UserPreference.DateSeparator) || string.IsNullOrEmpty(UserPreference.TimeSeparator) || string.IsNullOrEmpty(UserPreference.DecimalSeparator))
        ExportDateTimeFormatString = "yyyy-MMM-dd HH:mm:ss.fff";
      else
        ExportDateTimeFormatString = $"yyyy{UserPreference.DateSeparator}MMM{UserPreference.DateSeparator}dd HH{UserPreference.TimeSeparator}mm{UserPreference.TimeSeparator}ss{UserPreference.DecimalSeparator}fff";
    }


    public string FormatCellPassTime(DateTime value)
    {
      value = value.AddHours(UserPreference.ProjectTimeZoneOffset);
      return value.ToString(ExportDateTimeFormatString);
    }

    public string FormatCellPos(double value) 
      => IsRawDataAsDBaseRequired ? FormatDisplayDistanceUnitless(value, false) 
                                  : FormatDisplayDistance(value, false);

    public string RadiansToLatLongString(double latLong, int dp)
    {
      var latLonDegrees = MathUtilities.RadiansToDegrees(latLong);
      return DoubleToStrF(latLonDegrees, 18, true, dp);
    }

    public string FormatElevation(double value)
    {
      if (value.Equals(Consts.NullHeight))
        return NullString;

      if (IsRawDataAsDBaseRequired)
        return FormatDisplayDistanceUnitless(value, false);

      return FormatDisplayDistance(value, false);
    }

    public string FormatRadioLatency(int value) 
      => value == CellPassConsts.NullRadioLatency ? NullString : value.ToString();

    public string FormatSpeed(int value) 
      => value == Consts.NullMachineSpeed 
        ? NullString 
        : FormatDisplayVelocity(value/100.00, -1);

    public string FormatGPSMode(GPSMode value)
    {
      string result;
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
        return NullString;

      var toleranceString = IsRawDataAsDBaseRequired 
        ? FormatDisplayDistanceUnitless(gpsTolerance / 1000.000, false) 
        : FormatDisplayDistance(gpsTolerance / 1000.000, false);

      return $"{FormatGPSAccuracyValue(gpsAccuracy)} ({toleranceString})";
    }

    public string FormatPassCount(int value) => value == CellPassConsts.NullPassCountValue ? NullString : $"{value.ToString()}";

    // As CCV/MDP/RMV are reported in 10th, no units...
    public string FormatCompactionCCVTypes(short value) 
      => value == CellPassConsts.NullCCV ? NullString
        : $"{DoubleToStrF(value * 1.0 / CellPassConsts.CCVvalueRatio, 18, true, 1)}";

    public string FormatFrequency(ushort value)
    {
      if (value == CellPassConsts.NullFrequency)
        return NullString;

      return IsRawDataAsDBaseRequired ? DoubleToStrF(value * 1.0 / CellPassConsts.CCVvalueRatio, 18, true, 1) 
        : DoubleToStrF(value * 1.0 / CellPassConsts.CCVvalueRatio, 18, true, 1) + "Hz";
    }

    public string FormatAmplitude(ushort value) 
      => value == CellPassConsts.NullAmplitude ? NullString 
        : IsRawDataAsDBaseRequired ? DoubleToStrF(value * 1.00 / CellPassConsts.AmplitudeRatio, 18, true, 2) 
                                   : DoubleToStrF(value * 1.00 / CellPassConsts.AmplitudeRatio, 18, true, 2) + "mm";

    public string FormatTargetThickness(double value)
      => value.Equals(CellPassConsts.NullOverridingTargetLiftThicknessValue) ? NullString 
        : IsRawDataAsDBaseRequired ? FormatDisplayDistanceUnitless(value, false) 
                                   : FormatDisplayDistance(value, false);

    public string FormatMachineGearValue(MachineGear value)
    {
      string result;
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
        case MachineGear.SensorFailed: result = "Sensor_Failed"; break;
        case MachineGear.Null: result = NullString; break;
        default: result = $"unknown: {value}"; break;
      }

      return result;
    }

    public string FormatEventVibrationState(VibrationState value)
    {
      string result;
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
        return NullString;

      var tempSymbol = "C";
      var tempValue = value * 1.00 / CellPassConsts.CCVvalueRatio;
      if (UserPreference.TemperatureUnits == TemperatureUnitEnum.Fahrenheit)
      {
        tempSymbol = "F";
        tempValue = tempValue * 9 / 5 + 32;
      }
      return DoubleToStrF(tempValue, 18, false, 1) + "°" + tempSymbol;
    }

    private string FormatGPSAccuracyValue(GPSAccuracy value)
    {
      string result;
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
      if (dp < 0)
        dp = 1;

      var result = DoubleToStrF((value * 3600) / SpeedConversionFactor, 20, true, dp);
      if (!IsRawDataAsDBaseRequired)
        result += SpeedUnitString;
      return result;
    }

    private string FormatDisplayDistanceUnitless(double value, bool isFormattingRequired, int dp = -1)
    {
      if (dp < 0)
        dp = DefaultDecimalPlaces;

      return DistanceDoubleToStrF(value, 20, isFormattingRequired, dp);
    }

    private string FormatDisplayDistance(double value, bool isFormattingRequired, int dp = -1)
    {
      var result = FormatDisplayDistanceUnitless(value, isFormattingRequired, dp);

      if (!value.Equals(Consts.NullHeight))
        result += DistanceUnitString;
      return result;
    }

    // Converts a value(in meters) to a string in the current distance units 
    private string DistanceDoubleToStrF(double value, int precision, bool isFormattingRequired, int dp ) 
      => value.Equals(Consts.NullHeight) ? NullString 
                                             : DoubleToStrF(value / DistanceConversionFactor, precision, isFormattingRequired, dp);

    private string DoubleToStrF(double value, int precision, bool isFormattingRequired, int dp)
    {
      if (isFormattingRequired)
      {
        _nfiUser.NumberDecimalDigits = dp;
        return value.ToString("N", _nfiUser);
      }

      _nfiDefault.NumberDecimalDigits = dp;
      return value.ToString("F", _nfiDefault);
    }

  }
}
