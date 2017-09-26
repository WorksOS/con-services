using System;
using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;

namespace VSS.Productivity3D.Filter.Common.Models
{
  public class FilterRequest
  {
    /// <summary>
    /// The FilterUid whose filter is to be udpated, empty for create
    /// </summary>
    [JsonProperty(PropertyName = "FilterUid", Required = Required.Default)]
    public string FilterUid { get; set; } = string.Empty;

    /// <summary>
    /// The Name to be upserted, if empty then filter is transient
    /// </summary>
    [JsonProperty(PropertyName = "Name", Required = Required.Default)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The filter containing the Json string. May be empty if all defaults
    /// </summary>
    [JsonProperty(PropertyName = "FilterJson", Required = Required.Always)]
    public string FilterJson { get; set; } = string.Empty;


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
        FilterUid = filterUid,
        Name = name,
        FilterJson = filterJson
      };
    }

    public virtual void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (FilterUid == null
          || (FilterUid != string.Empty && Guid.TryParse(FilterUid, out Guid filterUidGuid) == false))
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);

      if (Name == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);

      if (FilterJson == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);

      if (FilterJson == "")
      {
        // Newtonsoft.JSON treats emtpy strings as invalid JSON but for our purposes it is valid.
        return;
      }

      // Validate filterJson...
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