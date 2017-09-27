using System;
using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// This is an expected exception and should be ignored by unit test failure methods.
  /// </summary>
  public class ServiceException : Exception
  {
    /// <summary>
    /// ServiceException class constructor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="result"></param>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result)
    {
      ExecutionResult = result;
      Content = JsonConvert.SerializeObject(result);
      Code = code;
    }

    /// <summary>
    /// Gets the exception content as a serialized object.
    /// </summary>
    public string Content { get; }

    public HttpStatusCode Code { get; }

    /// <summary>
    /// Gets the <see cref="ContractExecutionResult"/> behind the exception.
    /// </summary>
    public ContractExecutionResult ExecutionResult { get; }
  }
}