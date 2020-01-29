using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models;

namespace CCSS.TagFileSplitter.WebAPI.Utilities
{
  /// <summary>
  /// Utility methods for JSON based objects.
  /// </summary>
  public class JsonUtilities
  {
    /// <summary>
    /// Serialize the request ignoring the Data property so not to overwhelm the logs.
    /// </summary>
    public static string SerializeObjectIgnoringProperties(ProjectID request, params string[] properties)
    {
      return JsonConvert.SerializeObject(
        request,
        Formatting.None,
        new JsonSerializerSettings { ContractResolver = new JsonContractPropertyResolver(properties) });
    }
  }
}
