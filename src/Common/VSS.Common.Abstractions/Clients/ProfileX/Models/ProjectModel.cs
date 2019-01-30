using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VSS.Common.Abstractions.Clients.ProfileX.Enums;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  public class ProjectModel
  {
    /// <summary>
    /// TRN ID of the project
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// ID or name of the creating user
    /// </summary>
    [JsonProperty("createdBy")]
    public string CreatedBy { get; set; }

    /// <summary>
    /// ID or name of the user who last updated the project profile
    /// </summary>
    [JsonProperty("updatedBy")]
    public string UpdatedBy { get; set; }

    /// <summary>
    /// Timestamp that shows the time and date when the project was created. ISO 8601 format (e.g. 2018-02-04T00:00:00.00Z)
    /// </summary>
    [JsonProperty("createdTimeStamp")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? CreatedTimeStamp { get; set; }

    /// <summary>
    /// Timestamp that shows the time and date when the project information was last updated. ISO 8601 format (e.g. 2018-06-04T00:00:00.00Z)
    /// </summary>
    [JsonProperty("updatedTimeStamp")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? UpdatedTimeStamp { get; set; }

    /// <summary>
    /// Allowed values: ACTIVE, INACTIVE, PENDING, DELETED
    /// </summary>
    [JsonProperty("lifeState")]
    [JsonConverter(typeof(StringEnumConverter))]
    public LifeState LifeState { get; set; }

    /// <summary>
    /// ID or name of the of the user who deleted the profile from the system
    /// </summary>
    [JsonProperty("deletedBy")]
    public string DeletedBy { get; set; }

    /// <summary>
    /// Timestamp that shows the time and date when the project information was deleted. ISO 8601 format (e.g. 2018-06-04T00:00:00.00Z)
    /// </summary>
    [JsonProperty("deletedTimeStamp")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? DeletedTimeStamp { get; set; }

    /// <summary>
    /// Name of the project
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// A brief description of the project
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; }

    /// <summary>
    /// Start date of the project.
    /// </summary>
    [JsonProperty("startDate")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End or tentative finish date of the project
    /// </summary>
    [JsonProperty("endDate")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? EndDate { get; set; }
  }
}