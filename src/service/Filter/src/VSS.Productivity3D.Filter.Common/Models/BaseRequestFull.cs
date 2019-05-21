using System;
using System.Collections.Generic;
using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Models
{
  /// <summary>
  /// All the common parameters reequired for boundary requests.
  /// </summary>
  public class BaseRequestFull
  {
    public string CustomerUid { get; set; }
    public bool IsApplicationContext { get; set; }
    public string UserUid { get; set; }
    public string ProjectUid { get; set; }
    public IDictionary<string, string> CustomHeaders { get; set; }

    /// <summary>
    /// Determines whether CRUD operations should result in a Kafka message being sent.
    /// </summary>
    public bool SendKafkaMessages = true;

    /// <summary>
    /// Returns a new instance of <see cref="BaseRequestFull"/> using the provided inputs.
    /// </summary>
    public static BaseRequestFull Create(
      string customerUid,
      bool isApplicationContext,
      ProjectData projectData,
      string userUid,
      IDictionary<string, string> customHeaders)
    {
      return new BaseRequestFull
      {
        IsApplicationContext = isApplicationContext,
        ProjectUid = projectData?.ProjectUid,
        CustomerUid = customerUid,
        UserUid = userUid,
        CustomHeaders = customHeaders
      };
    }

    public virtual void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(CustomerUid) || Guid.TryParse(CustomerUid, out Guid customerUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);
      }

      if (string.IsNullOrEmpty(UserUid) || (IsApplicationContext == false && Guid.TryParse(UserUid, out Guid userUidGuid) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);
      }

      if (string.IsNullOrEmpty(ProjectUid) || Guid.TryParse(ProjectUid, out Guid projectUidGuid) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }
    }
  }
}
