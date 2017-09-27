using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using VSS.MasterData.Models.Handlers;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterListRequestFull : FilterListRequest
  {
    public string CustomerUid { get; set; }

    public bool IsApplicationContext { get; set; }

    public string UserId { get; set; }

    public string ProjectUid { get; set; }


    public static FilterListRequestFull CreateFilterListRequestFull(string customerUid,
      bool isApplicationContext, string userId,
      string projectUid, IEnumerable<FilterRequest> filterRequests)
    {
      return new FilterListRequestFull
      {
        filterRequests = filterRequests,
        CustomerUid = customerUid,
        IsApplicationContext = isApplicationContext,
        UserId = userId,
        ProjectUid = projectUid
      };
    }

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(CustomerUid) || Guid.TryParse(CustomerUid, out Guid customerUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);

      if (string.IsNullOrEmpty(UserId) || (IsApplicationContext == false && Guid.TryParse(UserId, out Guid userUidGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);

      if (string.IsNullOrEmpty(ProjectUid) || Guid.TryParse(ProjectUid, out Guid projectUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);

      foreach (var request in filterRequests)
      {
        if (!string.IsNullOrEmpty(request.Name))
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 24);

        if (!string.IsNullOrEmpty(request.FilterUid))
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 16);

        request.Validate(serviceExceptionHandler);
      }
    }
  }
}