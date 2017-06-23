
using System.Buffers;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Common.Filters.Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace VSS.Raptor.Service.Common.JsonConverters
{
    public class ProjectIDJsonInputFormatter : JsonInputFormatter
    {
      public ProjectIDJsonInputFormatter(ILogger logger, JsonSerializerSettings serializerSettings, ArrayPool<char> charPool,
        ObjectPoolProvider objectPoolProvider) : base(logger, serializerSettings, charPool, objectPoolProvider)
      {
      }

      public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
      {
        var task = base.ReadRequestBodyAsync(context, encoding);
        var result = task.ContinueWith((t) =>
          GetProjectId(context, t.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
        return result;
      }

      private InputFormatterResult GetProjectId(InputFormatterContext context, InputFormatterResult result)
      {
        if (!result.HasError && result.Model is ProjectID)
        {
          var projectID = result.Model as ProjectID;
          if (!projectID.projectId.HasValue)
          {
            projectID.projectId = (context.HttpContext.User as RaptorPrincipal).GetProjectId(projectID.projectUid);
          }
        }
        return result;
      }

  }

  public static class MvcOptionsExtensions
  {
    public static void UseProjectIDJsonInputFormatter(this MvcOptions opts, ILogger<MvcOptions> logger, ObjectPoolProvider objectPoolProvider)
    {
      opts.InputFormatters.RemoveType<JsonInputFormatter>();
      var serializerSettings = new JsonSerializerSettings();    
      var jsonInputFormatter = new ProjectIDJsonInputFormatter(logger, serializerSettings, ArrayPool<char>.Shared, objectPoolProvider);
      opts.InputFormatters.Add(jsonInputFormatter);
    }
  }

}
