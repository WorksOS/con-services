using Newtonsoft.Json;

namespace VSP.MasterData.Project.WebAPI.Helpers
{
  public class JsonHelper
  {
    public string SerializeObjectToJson<T>(T msg)
    {
      return JsonConvert.SerializeObject(msg);
    }

    public T DeserializeJsonToObject<T>(string msg)
    {
      return JsonConvert.DeserializeObject<T>(msg);
    }
  }
}