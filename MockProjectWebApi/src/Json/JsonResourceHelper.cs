using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;

namespace MockProjectWebApi.Json
{
  public class JsonResourceHelper
  {
    // Remember to set the Build Action of new .json resource files to 'Embedded Resource'.

    public static string GetFilterJson(string resourceName)
    {
      return GetJsonFromEmbeddedResource($"Filters.{resourceName}");
    }

    public static string GetUserPreferencesJson(string resourceName)
    {
      return GetJsonFromEmbeddedResource($"UserPreferences.{resourceName}");
    }

    public static string GetDimensionsFilterJson(string resourceName)
    {
      return GetJsonFromEmbeddedResource($"Filters.Dimensions.{resourceName}");
    }

    private static string GetJsonFromEmbeddedResource(string resourceName)
    {
      resourceName = $"MockProjectWebApi.Json.{resourceName}.json";


      using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName))
      {
        if (stream == null)
        {
          throw new Exception($"Error attempting to load resource '{resourceName}', stream cannot be null. Is the file marked as 'Embedded Resource'?");
        }

        var json = DeserializeFromStream(stream);
        return json;
      }
    }

    private static string DeserializeFromStream(Stream stream)
    {
      var serializer = new JsonSerializer();

      using (var sr = new StreamReader(stream))
      using (var jsonTextReader = new JsonTextReader(sr))
      {
        var obj = serializer.Deserialize<JObject>(jsonTextReader);

        return obj.ToString(Formatting.None);
      }
    }
  }
}