using System;
using System.Net;
using Newtonsoft.Json;

namespace CCSS.CWS.Client
{
  public static class ExceptionExtensions
  {
    public static bool IsNotFoundException(this Exception e)
    {
      try
      {
        //Message of the form $"{result.StatusCode} {contents}" from GracefulWebRequest.
        //We want to deserialize the contents to check for 404.
        var index = e.Message.IndexOf(' ');
        var contents = e.Message.Substring(index);
        var error = JsonConvert.DeserializeObject<CwsError>(contents);
        return error.status == 404;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}
