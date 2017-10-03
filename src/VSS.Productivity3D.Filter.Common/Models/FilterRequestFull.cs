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
        filterUid = filterUid,
        name = name,
        filterJson = filterJson,
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

      if (filterUid == null || (filterUid != string.Empty && Guid.TryParse(filterUid, out Guid filterUidGuid) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);
      }

      if (name == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);
      }

      if (filterJson == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);
      }

      if (filterJson == "")
      {
        // Newtonsoft.JSON treats emtpy strings as invalid JSON but for our purposes it is valid.
        return;
      }

      // Validate filterJson...
      try
      {
        var filter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);
        filter.Validate(serviceExceptionHandler);
      }
      catch (JsonReaderException exception)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 42, exception.Message);
      }
    }
  }
}