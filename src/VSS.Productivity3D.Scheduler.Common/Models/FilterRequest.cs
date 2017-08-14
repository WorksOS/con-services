using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using  VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterRequest
  {
    /// <summary>
    /// The filterUid whose filter is to be udpated, empty for create
    /// </summary>
    [JsonProperty(PropertyName = "filterUid", Required = Required.Default)]
    public string filterUid { get; set; } = string.Empty;

    /// <summary>
    /// The name to be upserted, if empty then filter is transient
    /// </summary>
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// The filter containing the Json string. May be empty if all defaults
    /// </summary>
    [JsonProperty(PropertyName = "FilterJson", Required = Required.Always)]
    public string filterJson { get; set; } = string.Empty;

    /// <summary>
    /// Private constructor
    /// </summary>
    protected FilterRequest()
    {
    }

    /// <summary>
    /// Create instance of FilterRequest
    /// </summary>
    public static FilterRequest CreateFilterRequest(string name, string filterJson)
    {
      return new FilterRequest
      {
        name = name,
        filterJson = filterJson
      };
    }
  }

  public class FilterRequestFull : FilterRequest
  {
    public string customerUid { get; set; }

    public string userUid { get; set; }
    public string projectUid { get; set; }

    public bool isApplicationContext { get; set; }

    public static FilterRequestFull CreateFilterFullRequest(string customerUid, 
      bool isApplicationContext, string userUid, 
      string projectUid, string filterUid = null,
      string name = "", string filterJson = "")
    {
      return new FilterRequestFull
      {
        projectUid = projectUid,
        filterUid = filterUid,
        name = name,
        filterJson = filterJson,
        customerUid = customerUid,
        isApplicationContext = isApplicationContext,
        userUid = userUid
      };
    }

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(customerUid) || Guid.TryParse(customerUid, out Guid customerUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);

      if (string.IsNullOrEmpty(userUid) || (isApplicationContext == false && Guid.TryParse(userUid, out Guid userUidGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);

      if (string.IsNullOrEmpty(projectUid) || Guid.TryParse(projectUid, out Guid projectUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);

      if (filterUid == string.Empty
          || (filterUid != null && Guid.TryParse(filterUid, out Guid filterUidGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);

      if (name == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);

      if (filterJson == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);

      // Validate filterJson...
      var filter = JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(filterJson);
      filter.Validate(serviceExceptionHandler);

    }
  }
}
