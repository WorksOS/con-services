using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASNode.UserPreferences;

namespace VSS.Productivity3D.Common.Models
{
  public static class Preferences
  {
    public const string DefaultDateSeparator = "/";
    public const string DefaultTimeSeparator = ":";
    public const string DefaultThousandsSeparator = ",";
    public const string DefaultDecimalSeparator = ".";
    public const int DefaultAssetLabelTypeId = 3;
    public const int DefaultTemperatureUnit = 1;
    public const int DefaultDateTimeFormat = 0;
    public const int DefaultNumberFormat = 0;

    public static TASNodeUserPreferences EmptyUserPreferences()
    {
      return __Global.Construct_TASNodeUserPreferences(null, null, null, null, null, 0.0, 0, 0, 0, 0, 0, 0);
    }

  }
}
