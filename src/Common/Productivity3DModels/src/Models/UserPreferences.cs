namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  ///  Describes user preference data.
  /// </summary>
  public struct UserPreferences
  {
    public string TimeZone { get; set; }
    public string DateSeparator { get; set; }
    public string TimeSeparator { get; set; }
    public string ThousandsSeparator { get; set; }
    public string DecimalSeparator { get; set; }
    public double TimeZoneOffset { get; set; }
    public int Language { get; set; }
    public int Units { get; set; }
    public int DateTimeFormat { get; set; }
    public int NumberFormat { get; set; }
    public int TemperatureUnits { get; set; }
    public int AssetLabelTypeID { get; set; }

    /// <summary>
    /// Constractor with arguments.
    /// </summary>
    /// <param name="timeZone"></param>
    /// <param name="dateSeparator"></param>
    /// <param name="timeSeparator"></param>
    /// <param name="thousandsSeparator"></param>
    /// <param name="decimalSeparator"></param>
    /// <param name="timeZoneOffset"></param>
    /// <param name="language"></param>
    /// <param name="units"></param>
    /// <param name="dateTimeFormat"></param>
    /// <param name="numberFormat"></param>
    /// <param name="temperatureUnits"></param>
    /// <param name="assetLabelTypeID"></param>
    public UserPreferences(
      string timeZone,
      string dateSeparator,
      string timeSeparator,
      string thousandsSeparator,
      string decimalSeparator,
      double timeZoneOffset,
      int language,
      int units,
      int dateTimeFormat,
      int numberFormat,
      int temperatureUnits,
      int assetLabelTypeID
    )
    {
      TimeZone = timeZone;
      DateSeparator = dateSeparator;
      TimeSeparator = timeSeparator;
      ThousandsSeparator = thousandsSeparator;
      DecimalSeparator = decimalSeparator;
      TimeZoneOffset = timeZoneOffset;
      Language = language;
      Units = units;
      DateTimeFormat = dateTimeFormat;
      NumberFormat = numberFormat;
      TemperatureUnits = temperatureUnits;
      AssetLabelTypeID = assetLabelTypeID;
    }
  }
}
