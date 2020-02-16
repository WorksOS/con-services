using Newtonsoft.Json;

namespace VSS.Hosted.VLCommon.Services.MDM.Common
{
  public class JsonHelper
  {
    public static string SerializeObjectToJson<T>(T msg)
    {
      return JsonConvert.SerializeObject(msg);
    }

    public static T DeserializeJsonToObject<T>(string msg)
    {
      return JsonConvert.DeserializeObject<T>(msg);
    }
  }
}