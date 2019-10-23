using System;
using System.Net;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.Models
{
  public static class JobRequestExtensions
  {
    public static T GetConvertedObject<T>(this object o) where T : class
    {
      T result;

      if (typeof(T) != typeof(string) && o is JToken)
      {
        try
        {
          result = (o as JToken).ToObject<T>();
          return result;
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              $"Missing or Wrong parameters passed to job with expected type {typeof(T)} whilst provided with {o.GetType()}"));
        }
      }

      if (typeof(T) != typeof(string) && o is JObject)
      {
        try
        {
          result = (o as JObject).ToObject<T>();
          return result;
        }
        catch (Exception e)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              $"Missing or Wrong parameters passed to job with expected type {typeof(T)} whilst provided with {o.GetType()}"));
        }
      }

      if (o is T)
      {
        return o as T;
      }

      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"Missing or Wrong parameters passed to job with expected type {typeof(T)} whilst provided with {o?.GetType()}"));
    }
  }
}
