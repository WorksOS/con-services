using ASNode.UserPreferences;

namespace VSS.Productivity3D.Common.Models
{
  public static class Preferences
  {
    public const string DefaultDateSeparator = "/";
    public const string DefaultTimeSeparator = ":";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";
    public const int DefaultAssetLabelTypeId = 3;//None=0, AssetId=1, SerialNumber=2, Both=3
    public const int DefaultTemperatureUnit = (int)TemperatureUnitEnum.Celsius;
    public const int DefaultDateTimeFormat = 0;//not used by Raptor
    public const int DefaultNumberFormat = 0;//not used by Raptor

    public static TASNodeUserPreferences EmptyUserPreferences()
    {
      return __Global.Construct_TASNodeUserPreferences(null, null, null, null, null, 0.0, 0, 0, 0, 0, 0, 0);
    }
  }
}