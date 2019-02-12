namespace VSS.MasterData.Models.Models
{
  /// <summary>
  ///  Describes user preference data returned by the preference master data service.
  /// </summary>
  public class UserPreferenceData 
  {
    public string Timezone { get; set; }
    public string Language { get; set; }
    public string Units { get; set; }
    public string DateFormat { get; set; }
    public string TimeFormat { get; set; }
    public string ThousandsSeparator { get; set; }
    public string DecimalSeparator { get; set; }
    public string DecimalPrecision { get; set; }
    public string AssetLabelDisplay { get; set; }
    public string MeterLabelDisplay { get; set; }
    public string LocationDisplay { get; set; }
    public string CurrencySymbol { get; set; }
    public string TemperatureUnit { get; set; }
    public string PressureUnit { get; set; }
    public string MapProvider { get; set; }
    public string BrowserRefresh { get; set; }

    public string DateSeparator { get; set; }
    public string TimeSeparator { get; set; }

    private const string DefaultUnits = "US";
    private const string DefaultDateSeparator = "-";
    private const string DefaultTimeSeparator = ":";
    private const string DefaultThousandsSeparator = ",";
    private const string DefaultDecimalSeparator = ".";
    private const string DefaultAssetLabelType = "Both"; //None=0, AssetId=1, SerialNumber=2, Both=3
    private const string DefaultTemperatureUnit = "Celsius";

    public UserPreferenceData()
    {
      Units = DefaultUnits;
      DateFormat = string.Format( $"yyyy{DefaultDateSeparator}MM{DefaultDateSeparator}dd");
      TimeFormat = string.Format($"HH{DefaultTimeSeparator}mm{DefaultTimeSeparator}ss.zzz");
      ThousandsSeparator = DefaultThousandsSeparator;
      DecimalSeparator = DefaultDecimalSeparator;
      AssetLabelDisplay = DefaultAssetLabelType;
      TemperatureUnit = DefaultTemperatureUnit;
      DateSeparator = DefaultDateSeparator;
      TimeSeparator = DefaultTimeSeparator;
    }

    /// <summary>
    /// Constructor with arguments.
    /// </summary>
    /// <param name="timezone"></param>
    /// <param name="language"></param>
    /// <param name="units"></param>
    /// <param name="dateFormat"></param>
    /// <param name="timeFormat"></param>
    /// <param name="thousandsSeparator"></param>
    /// <param name="decimalSeparator"></param>
    /// <param name="decimalPrecision"></param>
    /// <param name="assetLabelDisplay"></param>
    /// <param name="meterLabelDisplay"></param>
    /// <param name="locationDisplay"></param>
    /// <param name="currencySymbol"></param>
    /// <param name="temperatureUnit"></param>
    /// <param name="pressureUnit"></param>
    /// <param name="mapProvider"></param>
    /// <param name="browserRefresh"></param>
    /// <param name="dateSeparator"></param>
    /// <param name="timeSeparator"></param>
    public UserPreferenceData(
      string timezone,
      string language,
      string units,
      string dateFormat,
      string timeFormat,
      string thousandsSeparator,
      string decimalSeparator,
      string decimalPrecision,
      string assetLabelDisplay,
      string meterLabelDisplay,
      string locationDisplay,
      string currencySymbol,
      string temperatureUnit,
      string pressureUnit,
      string mapProvider,
      string browserRefresh,
      string dateSeparator,
      string timeSeparator
    )
    {
      Timezone = timezone;
      Language = language;
      Units = units;
      DateFormat = dateFormat;
      TimeFormat = timeFormat;
      ThousandsSeparator = thousandsSeparator;
      DecimalSeparator = decimalSeparator;
      DecimalPrecision = decimalPrecision;;
      AssetLabelDisplay = assetLabelDisplay;
      MeterLabelDisplay = meterLabelDisplay;
      LocationDisplay = locationDisplay;
      CurrencySymbol = currencySymbol;
      TemperatureUnit = temperatureUnit;
      PressureUnit = pressureUnit;
      MapProvider = mapProvider;
      BrowserRefresh = browserRefresh;
      DateSeparator = dateSeparator;
      TimeSeparator = timeSeparator;
    }
  }
}
