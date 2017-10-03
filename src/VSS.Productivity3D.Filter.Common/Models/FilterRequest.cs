using System;
using System.Net;
using Newtonsoft.Json;
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
    { }

    /// <summary>
    /// Returns a new instance of <see cref="FilterRequest"/> using the provided inputs.
    /// </summary>
    public static FilterRequest Create(string filterUid, string name, string filterJson)
    {
      return new FilterRequest
      {
        filterUid = filterUid,
        name = name,
        filterJson = filterJson
      };
    }

    public virtual void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (filterUid == null
          || (filterUid != string.Empty && Guid.TryParse(filterUid, out Guid filterUidGuid) == false))
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
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 42, exception.Message);
      }
    }
  }
}