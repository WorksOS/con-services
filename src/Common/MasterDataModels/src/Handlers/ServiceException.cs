using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Common.Exceptions
{
  /// <summary>
  /// This is an expected exception and should be ignored by unit test failure methods.
  /// </summary>
  public class ServiceException : Exception
  {
    public new Exception InnerException { get; private set; } = null;
    public ContractExecutionResult GetResult { get; }

    public string GetContent => FormatException(false);
    public string GetFullContent => FormatException(true);

    public HttpStatusCode Code { get; private set; }

    private static readonly JsonSerializerSettings _serializerSettings;

    static ServiceException()
    {
      _serializerSettings = new JsonSerializerSettings
      {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      };
    }

    /// <summary>
    /// ServiceException class constructor.
    /// </summary>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result) : base(result.Message)
    {
      GetResult = result;
      Code = code;
    }

    /// <summary>
    /// ServiceException class constructor.
    /// </summary>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result, Exception innerException) : base(result.Message)
    {
      GetResult = result;
      Code = code;
      InnerException = innerException;
    }

    private string FormatException(bool includeInnerException) =>
      InnerException == null || !includeInnerException
        ? JsonConvert.SerializeObject(GetResult, _serializerSettings)
        : $"{JsonConvert.SerializeObject(GetResult, _serializerSettings)} with inner exception {InnerException.Message} stack {InnerException.StackTrace} source {InnerException.Source}";

    public void OverrideBadRequest(HttpStatusCode newStatusCode)
    {
      if (Code == HttpStatusCode.BadRequest &&
          GetResult.Code == ContractExecutionStatesEnum.InternalProcessingErrorConst)
      {
        Code = newStatusCode;
      }
    }
  }
}
