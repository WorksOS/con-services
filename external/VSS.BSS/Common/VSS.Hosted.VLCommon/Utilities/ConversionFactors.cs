using System;

namespace VSS.Hosted.VLCommon
{
  public class ConversionFactors
  {
    public static readonly double ImperialFeetToMetres = 0.3048;
    public static readonly double USFeetToMetres = 0.304800609601;
    public static readonly double USGallonsToLiters = 3.785411784;
    public static readonly double USGallonsToImperialGallons = 0.83267384;

    // Unicode character for Degree (°) symbol 
    public const string DEGREE_SYMBOL = "\u00B0";
    public const string MINUTES_SYMBOL = "'";
    public const string SECONDS_SYMBOL = "\"";


    public static uint ConvertLatDegreesToGeodesic(decimal lat)
    {
      /*	ALC: Since the Geo-desic format can only represent positive
			integers, Latitudes are represented starting from the North Pole
			towards the South Pole, with 0 through 90 representing Northern 
			Latitudes and 90 through 180 representing Southern Latitudes.  
			Subtracting the Latitude from 90 gives the corresponding angle from
			the North Pole towards the equator for Northern Latitudes and adds 
			the Northern hemisphere to Southern Latitudes.  Multiplying this by
			16777215 (FFFFFF) and dividing by 180 converts the decimal latitude
			to a Geo-desic representation. */
      decimal x = (90.0M - lat);
      decimal y = x * 16777215.0M;
      decimal z = y / 180.0M;

      return ((uint)Math.Round(z, MidpointRounding.AwayFromZero));
    }

    public static double ConvertLatGeodesicToDegrees(uint geoLat)
    {
      decimal y = geoLat * 180.0M;
      decimal x = y / 16777215.0M;
      decimal degLat = 90M - x;

      return (double)degLat;
    }

    public static uint ConvertLonDegreesToGeodesic(decimal lon)
    {
      /*	ALC: Since the Geo-desic format can only represent positive 
			integers, Western Longitudes are represented as 180 through 360 
			in the Eastern direction. To accomplish this, change Western 
			Longitudes to a negative number and add 360 to get the 
			corresponding Eastern Longitude representation. */

      decimal geoLongitude = lon < 0.0M ? (lon + 360.0M) : lon;
      decimal x = (geoLongitude * 16777215.0M);
      decimal y = x / 360.0M;

      return ((uint)Math.Round(y, MidpointRounding.AwayFromZero));
    }

    public static double ConvertLonGeodesicToDegrees(uint geoLon)
    {
      decimal y = geoLon * 360.0M;
      decimal x = y / 16777215.0M;
      decimal degLon = x > 180M ? x - 360M : x;

      return (double)degLon;
    }

    public static string LatitudeToDMS(double latitude)
    {
      if (double.IsNaN(latitude))
        return "";
      string direction = latitude > 0 ? "N" : latitude < 0 ? "S" : "";
      return direction + " " + DegreesToDMS(latitude);
    }

    public static string LongitudeToDMS(double longitude)
    {
      if (double.IsNaN(longitude))
        return "";
      string direction = longitude > 0 ? "E" : longitude < 0 ? "W" : "";
      return direction + " " + DegreesToDMS(longitude);
    }

    public static string DegreesToDMS(double degDecimal)
    {
      degDecimal = Math.Abs(degDecimal);
      int deg = (int)Math.Floor(degDecimal);
      double minDecimal = 60 * (degDecimal - deg);
      int min = (int)Math.Floor(minDecimal);
      double secDecimal = 60 * (minDecimal - min);
      int sec = (int)Math.Round(secDecimal, 0);

      return addLeadingZero(deg) + DEGREE_SYMBOL + addLeadingZero(min) + MINUTES_SYMBOL + addLeadingZero(sec) + SECONDS_SYMBOL;
    }

    private static string addLeadingZero(int number)
    {
      if (number.ToString().Length >= 2)
        return number.ToString();
      else
        return "0" + number.ToString();
    }
  }
}
