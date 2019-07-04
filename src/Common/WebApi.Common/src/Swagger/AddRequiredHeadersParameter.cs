using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VSS.WebApi.Common.Swagger
{
  public class AddRequiredHeadersParameter : IOperationFilter
  {
    public void Apply(Operation operation, OperationFilterContext context)
    {
      if (operation.Parameters == null)
        operation.Parameters = new List<IParameter>();

      operation.Parameters.Add(new NonBodyParameter
      {
        Name = "X-VisionLink-CustomerUid",
        In = "header",
        Type = "string",
        Description = "Used to identify the Vision Link Customer for the request (if applicable)",
        Required = false
      });

      operation.Parameters.Add(new NonBodyParameter
      {
        Name = "Authorization",
        In = "header",
        Type = "string",
        Description = "Trimble Authentication token (Required if authenticating via Trimble Authentication)",
        Required = false
      });

      operation.Parameters.Add(new NonBodyParameter
      {
        Name = "X-Jwt-Assertion",
        In = "header",
        Type = "string",
        Description = "JWT Assertion token (normally provided by Trimble Authentication after the user/service is successfully authenticated)",
        Required = false
      });
    }
  }
}
