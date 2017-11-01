using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MockProjectWebApi.Json
{
  public class JsonResourceHelper
  {
    public static string GetFilterJson(string resourceName)
    {
      // Remember to set the Build Action of new .json resource files to 'Embedded Resource'.

      return GetJsonFromEmbeddedResource($"MockProjectWebApi.Json.Filters.{resourceName}.json");
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