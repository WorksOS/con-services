using System;
using System.Buffers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Common.JsonConverters
{
  public class ProjectIdJsonInputFormatter : JsonInputFormatter
  {
    public ProjectIdJsonInputFormatter(ILogger logger, JsonSerializerSettings serializerSettings, ArrayPool<char> charPool,
      ObjectPoolProvider objectPoolProvider) : base(logger, serializerSettings, charPool, objectPoolProvider)
    { }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
      var result = await GetProjectId(context, await base.ReadRequestBodyAsync(context, encoding));
      return result;
    }

    private async Task<InputFormatterResult> GetProjectId(InputFormatterContext context, InputFormatterResult result)
    {
      if (!result.HasError && result.Model is ProjectID projectId)
      {
        if (!projectId.ProjectId.HasValue)
        {
          if (!projectId.ProjectUid.HasValue)
          {
            throw new ArgumentException("Project identifier cannot be null");
          }

          projectId.ProjectId = await ((RaptorPrincipal) context.HttpContext.User).GetLegacyProjectId(projectId.ProjectUid.Value);
        }
      }

      return result;
    }
  }
}
