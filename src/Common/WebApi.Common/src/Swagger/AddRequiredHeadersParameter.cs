using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VSS.Common.Abstractions.Http;

namespace VSS.WebApi.Common.Swagger
{
  public class AddRequiredHeadersParameter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      if (operation.Parameters == null)
        operation.Parameters = new List<OpenApiParameter>();

      operation.Parameters.Add(new OpenApiParameter()
      {
        Name = HeaderConstants.X_VISION_LINK_CUSTOMER_UID,
        In = ParameterLocation.Header,
        Description = "Used to identify the Vision Link Customer for the request (if applicable)",
        Required = false
      });

      operation.Parameters.Add(new OpenApiParameter()
      {
        Name = HeaderConstants.AUTHORIZATION,
        In = ParameterLocation.Header,
        Description = "Trimble Authentication token (Required if authenticating via Trimble Authentication)",
        Required = false
      });

      operation.Parameters.Add(new OpenApiParameter()
      {
        Name = HeaderConstants.X_JWT_ASSERTION,
        In = ParameterLocation.Header,
        Description = "JWT Assertion token (normally provided by Trimble Authentication after the user/service is successfully authenticated)",
        Required = false
      });
    }
  }
}
