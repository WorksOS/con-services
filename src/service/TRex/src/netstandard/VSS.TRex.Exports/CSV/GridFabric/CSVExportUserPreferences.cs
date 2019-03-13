using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  ///  Describes user preference data specific to TRex exports
  /// </summary>
  public class CSVExportUserPreferences : IFromToBinary
  {
    public string DateSeparator { get; private set; }
    public string TimeSeparator { get; private set; }
    public string ThousandsSeparator { get; private set; }
    public string DecimalSeparator { get; private set; }
    public UnitsTypeEnum Units { get; private set; }
    public TemperatureUnitEnum TemperatureUnits { get; private set; }
    public double ProjectTimeZoneOffset { get; private set; }

    public const string DefaultDateSeparator = "/";
    public const string DefaultTimeSeparator = ":";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";
    public const UnitsTypeEnum DefaultUnits = UnitsTypeEnum.Metric;
    public const TemperatureUnitEnum DefaultTemperatureUnits = TemperatureUnitEnum.Celsius;


    public CSVExportUserPreferences()
    {
      Clear();
    }

    private void Clear()
    {
      DateSeparator = DefaultDateSeparator;
      TimeSeparator = DefaultTimeSeparator;
      ThousandsSeparator = DefaultThousandsSeparator;
      DecimalSeparator = DefaultDecimalSeparator;
      Units = DefaultUnits;
      TemperatureUnits = DefaultTemperatureUnits;
      ProjectTimeZoneOffset = 0;
    }

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteString(DateSeparator);
      writer.WriteString(TimeSeparator);
      writer.WriteString(ThousandsSeparator);
      writer.WriteString(DecimalSeparator);
      writer.WriteInt((int)Units);
      writer.WriteInt((int)TemperatureUnits);
      writer.WriteDouble(ProjectTimeZoneOffset);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      DateSeparator = reader.ReadString();
      TimeSeparator = reader.ReadString();
      ThousandsSeparator = reader.ReadString();
      DecimalSeparator = reader.ReadString();
      Units = (UnitsTypeEnum)reader.ReadInt();
      TemperatureUnits = (TemperatureUnitEnum)reader.ReadInt();
      ProjectTimeZoneOffset = reader.ReadDouble();
    }
  }
}
