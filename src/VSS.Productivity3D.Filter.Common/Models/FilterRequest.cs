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
    [JsonProperty(Required = Required.Default)]
    public string FilterUid { get; set; } = string.Empty;

    /// <summary>
    /// The name to be upserted, if empty then filter is transient
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The filter containing the Json string. May be empty if all defaults
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string FilterJson { get; set; } = string.Empty;

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
      if (this.FilterUid == null
          || (this.FilterUid != string.Empty && Guid.TryParse(this.FilterUid, out Guid _) == false))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 2);
      }

      //null name is an error but empty string indicates a transient filter
      if (Name == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 3);
      }

      if (FilterJson == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 4);
      }

      if (FilterJson == string.Empty)
      {
        // Newtonsoft.JSON treats empty strings as invalid JSON but for our purposes it is valid.
        return;
      }

      // Validate filterJson... 
      FilterModel(serviceExceptionHandler)?.Validate(serviceExceptionHandler);
    }

    public MasterData.Models.Models.Filter FilterModel(IServiceExceptionHandler serviceExceptionHandler)
    {    
      try
      {
        return string.IsNullOrEmpty(FilterJson) ? null : JsonConvert.DeserializeObject<MasterData.Models.Models.Filter>(FilterJson);
      }
      catch (JsonReaderException exception)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 42, exception.Message);
      }
      return null;      
    }
  }
}