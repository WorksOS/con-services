using System;
using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Common.Exceptions
{
  /// <summary>
  ///   This is an expected exception and should be ignored by unit test failure methods.
  /// </summary>
  public class ServiceException : Exception
  {
    /// <summary>
    ///   ServiceException class constructor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="result"></param>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result)
    {
      GetResult = result;
      Code = code;
    }

    /// <summary>
    ///   ServiceException class constructor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="result"></param>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result, Exception innerException)
    {
      GetResult = result;
      Code = code;
      InnerException = innerException;
    }

    private string formatException()
    {
      if (InnerException == null)
        return JsonConvert.SerializeObject(GetResult);
      return
        $"{JsonConvert.SerializeObject(GetResult)} with inner exception {InnerException.Message} stack {InnerException.StackTrace} source {InnerException.Source}";
    }

    public Exception InnerException { get; private set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public string GetContent => formatException();

    public HttpStatusCode Code { get; private set; }

    public void OverrideBadRequest(HttpStatusCode newStatusCode)
    {
      if (Code == HttpStatusCode.BadRequest &&
          GetResult.Code == ContractExecutionStatesEnum.InternalProcessingErrorConst)
      {
        Code = newStatusCode;
      }
    }


    /// <summary>
    /// The result causing the exception
    /// </summary>
    public ContractExecutionResult GetResult { get; }
  }
}