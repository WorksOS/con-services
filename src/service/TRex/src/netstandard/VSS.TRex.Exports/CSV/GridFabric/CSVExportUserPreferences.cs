using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  ///  Describes user preference data specific to TRex exports
  /// </summary>
  public class CSVExportUserPreferences : IFromToBinary
  {
    public string DateSeparator { get; set; }
    public string TimeSeparator { get; set; }
    public string ThousandsSeparator { get; set; }
    public string DecimalSeparator { get; set; }
    public UnitsTypeEnum Units { get; set; }
    public TemperatureUnitEnum TemperatureUnits { get; set; }

    public const string DefaultDateSeparator = "-";
    public const string DefaultTimeSeparator = ":";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";
    public const UnitsTypeEnum DefaultUnits = UnitsTypeEnum.US;
    public const TemperatureUnitEnum DefaultTemperatureUnits = TemperatureUnitEnum.Celsius;

    public CSVExportUserPreferences()
    {
      DateSeparator = DefaultDateSeparator;
      TimeSeparator = DefaultTimeSeparator;
      ThousandsSeparator = DefaultThousandsSeparator;
      DecimalSeparator = DefaultDecimalSeparator;
      Units = DefaultUnits;
      TemperatureUnits = DefaultTemperatureUnits;
    }


    public CSVExportUserPreferences CreatePreferences(
      string dateSeparator,
      string timeSeparator,
      string thousandsSeparator,
      string decimalSeparator,
      UnitsTypeEnum units,
      TemperatureUnitEnum temperatureUnits
    )
    {
      return new CSVExportUserPreferences()
      {
        DateSeparator = dateSeparator,
        TimeSeparator = timeSeparator,
        ThousandsSeparator = thousandsSeparator,
        DecimalSeparator = decimalSeparator,
        Units = units,
        TemperatureUnits = temperatureUnits
      };
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteString(DateSeparator);
      writer.WriteString(TimeSeparator);
      writer.WriteString(ThousandsSeparator);
      writer.WriteString(DecimalSeparator);
      writer.WriteInt((int)Units);
      writer.WriteInt((int)TemperatureUnits);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      DateSeparator = reader.ReadString();
      TimeSeparator = reader.ReadString();
      ThousandsSeparator = reader.ReadString();
      DecimalSeparator = reader.ReadString();
      Units = (UnitsTypeEnum)reader.ReadInt();
      TemperatureUnits = (TemperatureUnitEnum)reader.ReadInt();
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ DateSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ TimeSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ ThousandsSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ DecimalSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ Units.GetHashCode();
        hashCode = (hashCode * 397) ^ TemperatureUnits.GetHashCode();
        return hashCode;
      }
    }
  }
}
