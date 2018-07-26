using System.Buffers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.JsonConverters;

namespace VSS.Productivity3D.Common.Extensions
{
  public static class MvcOptionsExtensions
  {
    public static void UseProjectIdJsonInputFormatter(this MvcOptions opts, ILogger<MvcOptions> logger, ObjectPoolProvider objectPoolProvider)
    {
      opts.InputFormatters.RemoveType<JsonInputFormatter>();
      var serializerSettings = new JsonSerializerSettings();    
      var jsonInputFormatter = new ProjectIdJsonInputFormatter(logger, serializerSettings, ArrayPool<char>.Shared, objectPoolProvider);
      opts.InputFormatters.Add(jsonInputFormatter);
    }
  }
}
