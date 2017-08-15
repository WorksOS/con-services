using Newtonsoft.Json;
using System;
using System.Net;
using VSS.MasterData.Models.Handlers;

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
    [JsonProperty(PropertyName = "filterJson", Required = Required.Always)]
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

    public string userId { get; set; }
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
        userId = userUid
      };
    }

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(customerUid) || Guid.TryParse(customerUid, out Guid customerUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 27);

      if (string.IsNullOrEmpty(userId) || (isApplicationContext == false && Guid.TryParse(userId, out Guid userUidGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 28);

      if (string.IsNullOrEmpty(projectUid) || Guid.TryParse(projectUid, out Guid projectUidGuid) == false)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);

      if (filterUid == null
        || ( filterUid != string.Empty && Guid.TryParse(filterUid, out Guid filterUidGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);

      if (name == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);

      if (filterJson == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);

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
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4, null, exception.GetBaseException().Message);
      }
    }
  }
}