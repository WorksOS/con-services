using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MockProjectWebApi.Json
{
  public class JsonResourceHelper
  {
    // Remember to set the Build Action of new .json resource files to 'Embedded Resource'.

    public static string GetFilterJson(string resourceName)
    {
      return GetJsonFromEmbeddedResource($"MockProjectWebApi.Json.Filters.{resourceName}.json");
    }

    public static string GetGoldenDataFilterJson(string resourceName)
    {
      return GetJsonFromEmbeddedResource($"MockProjectWebApi.Json.Filters.GoldenData.{resourceName}.json");
    }

    public static string GetDimensionsFilterJson(string resourceName)
    {
      return GetJsonFromEmbeddedResource($"MockProjectWebApi.Json.Filters.Dimensions.{resourceName}.json");
    }

    private static string GetJsonFromEmbeddedResource(string resourceName)
    {
      using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
      {
        if (stream == null)
        {
          throw new Exception($"Error attempting to load resource '{resourceName}', stream cannot be null. Is the file marked as 'Embedded Resource'?");
        }

        var json = new StreamReader(stream).ReadToEnd();

        return Regex.Replace(json, @"\s+", string.Empty);
      }
    }
  }
}