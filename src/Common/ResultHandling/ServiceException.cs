using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using System.Net.Http;
using System.Web.Http;

namespace VSS.Raptor.Service.Common.ResultHandling
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
    public string GetContent { get; private set; }
  }
}
