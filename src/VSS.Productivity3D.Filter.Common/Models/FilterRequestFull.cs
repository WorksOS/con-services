using System;
using System.Collections.Generic;
using System.Net;
using VSS.MasterData.Models.Handlers;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterRequestFull : FilterRequest
  {
    public string CustomerUid { get; set; }

    public bool IsApplicationContext { get; set; }

    public string UserId { get; set; }

    public string ProjectUid { get; set; }

    public IDictionary<string, string> CustomHeaders { get; set; }

    public static FilterRequestFull Create(IDictionary<string, string> customHeaders, string customerUid, bool isApplicationContext, string userId, string projectUid, FilterRequest request = null)
    {
      return new FilterRequestFull
      {
        FilterUid = request?.FilterUid ?? string.Empty,
        Name = request?.Name ?? string.Empty,
        FilterJson = request?.FilterJson ?? string.Empty,
        CustomerUid = customerUid,
        IsApplicationContext = isApplicationContext,
        UserId = userId,
        ProjectUid = projectUid,
        CustomHeaders = customHeaders
      };
    }

    public override void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(CustomerUid) || Guid.TryParse(CustomerUid, out Guid _) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);
      }

      if (string.IsNullOrEmpty(UserId) || (IsApplicationContext == false && Guid.TryParse(UserId, out Guid _) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);
      }

      if (string.IsNullOrEmpty(ProjectUid) || Guid.TryParse(ProjectUid, out Guid _) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      base.Validate(serviceExceptionHandler);
    }
  }
}