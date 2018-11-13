using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.TRex.Gateway.Common.Converters
{
  public static class ConvertObjectForLogging
  {
    /// <summary>
    /// Serialize the request ignoring specified properties so not to overwhelm the logs.
    /// </summary>
    public static string SerializeObjectIgnoringProperties<T>(T request, params string[] properties)
    {
      return JsonConvert.SerializeObject(
          request,
          Formatting.None,
          new JsonSerializerSettings { ContractResolver = new JsonContractPropertyResolver(properties) });
    }
  }
}
