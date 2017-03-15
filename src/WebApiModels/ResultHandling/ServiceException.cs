using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace VSS.TagFileAuth.Service.WebApiModels.ResultHandling
{
  /// <summary>
  ///   This is an expected exception and should be ignored by unit test failure methods.
  /// </summary>
  public class ServiceException : HttpResponseException
  {
    /// <summary>
    ///   ServiceException class constructor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="result"></param>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result)
      : base(new HttpResponseMessage(code))
    {
      GetContent = JsonConvert.SerializeObject(result);
      Response.Content = new StringContent(GetContent);
    }

    /// <summary>
    /// 
    /// </summary>
    public string GetContent { get; set; }
  }
}