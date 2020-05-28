using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Scheduler.Models
{
  public static class JobRequestExtensions
  {
    public static T GetConvertedObject<T>(this object o) where T : class
    {
      if (typeof(T) == typeof(HeaderDictionary) && o is JToken)
      {
        try
        {
          var entries = JObject.FromObject(o).ToObject<Dictionary<string, object>>();
          var result = new HeaderDictionary();

          foreach (var entry in entries)
          {
            string[] values = JsonConvert.DeserializeObject<string[]>(entry.Value.ToString());

            result.Add(new KeyValuePair<string, StringValues>(entry.Key, values));
          }

          return result as T;
        }
        catch
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              $"Missing or Wrong parameters passed to job with expected type {typeof(T)} whilst provided with {o.GetType()}"));
        }
      }

      if (typeof(T) != typeof(string) && o is JToken)
      {
        try
        {
          return (o as JToken).ToObject<T>();
        }
        catch
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
          return (o as JToken).ToObject<T>();
        }
        catch
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
