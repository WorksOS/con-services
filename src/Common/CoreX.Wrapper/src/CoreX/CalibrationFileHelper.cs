using System;
using System.IO;
using System.Text;
using CoreX.Wrapper.Types;

namespace CoreX.Wrapper
{
  public static class CalibrationFileHelper
  {
    public static (string id, string name) GetProjectionTypeCode(byte[] dcFileArray)
    {
      const string PROJECTION_KEY = "64";
      var fs = new MemoryStream(dcFileArray);

      using (var sr = new StreamReader(fs, Encoding.UTF8))
      {
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith(PROJECTION_KEY)) { continue; }

          var projectionTypeCode = line[4].ToString();

          return (projectionTypeCode, Projection.GetProjectionName(projectionTypeCode));
        }
      }

      throw new Exception("Calibration file doesn't contain Projection data");
    }

    public static string GetGeoidModelName(byte[] dcFileArray)
    {
      const string VERTICAL_ADJUST = "81";
      var fs = new MemoryStream(dcFileArray);

      using (var sr = new StreamReader(fs, Encoding.UTF8))
      {
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith(VERTICAL_ADJUST)) { continue; }

          return line.Substring(85, 32).Trim();
        }
      }

      throw new Exception("Calibration file doesn't contain Projection data");
    }
  }
}
