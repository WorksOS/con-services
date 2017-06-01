using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace src.Models
{

  public class UserPreferenceResult
  {
    public string PreferenceKeyName { get; set; }
    public string PreferenceJson { get; set; }
    public string PreferenceKeyUID { get; set; }
    public string SchemaVersion { get; set; }
  }

  //This is what the PreferenceJson is
  /*
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
  }
  */
}
