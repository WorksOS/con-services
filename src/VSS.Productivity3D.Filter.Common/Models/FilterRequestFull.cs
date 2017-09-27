using Newtonsoft.Json;
using System;
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

    public static FilterRequestFull Create(string customerUid,
      bool isApplicationContext, string userId,
      string projectUid, string filterUid = "",
      string name = "", string filterJson = "", string boundaryUid = "")
    {
      return new FilterRequestFull
      {
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson,
        CustomerUid = customerUid,
        IsApplicationContext = isApplicationContext,
        UserId = userId,
        ProjectUid = projectUid
      };
    }

    public override void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(CustomerUid) || Guid.TryParse(CustomerUid, out Guid customerUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);
      }

      if (string.IsNullOrEmpty(UserId) || (IsApplicationContext == false && Guid.TryParse(UserId, out Guid userUidGuid) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);
      }

      if (string.IsNullOrEmpty(ProjectUid) || Guid.TryParse(ProjectUid, out Guid projectUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      if (FilterUid == null || (FilterUid != string.Empty && Guid.TryParse(FilterUid, out Guid filterUidGuid) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);
      }

      if (Name == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);
      }

      if (FilterJson == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);
      }

      if (FilterJson == "")
      {
        // Newtonsoft.JSON treats emtpy strings as invalid JSON but for our purposes it is valid.
        return;
      }

      // Validate FilterJson...
      try
      {
        var filter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(FilterJson);
        filter.Validate(serviceExceptionHandler);
      }
      catch (JsonReaderException exception)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 42, exception.Message);
      }
    }
  }
}