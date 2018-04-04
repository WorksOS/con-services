using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.Common.JsonConverters
{
  public class ProjectIDJsonInputFormatter : JsonInputFormatter
  {
    public ProjectIDJsonInputFormatter(ILogger logger, JsonSerializerSettings serializerSettings, ArrayPool<char> charPool,
      ObjectPoolProvider objectPoolProvider) : base(logger, serializerSettings, charPool, objectPoolProvider)
    { }

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
          projectID.projectId = (context.HttpContext.User as RaptorPrincipal).GetLegacyProjectId(projectID.projectUid);
        }
      }

      return result;
    }
  }
}