using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Converters;

namespace VSS.Common.Abstractions.Clients.ProfileX.Models
{
  public class ProjectCreateRequestModel
  {
    public ProjectCreateRequestModel()
    {
      Locations = new List<ProjectLocation>();
    }

    /// <summary>
    /// Name of the project
    /// </summary>
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; }

    /// <summary>
    /// A brief description about the project
    /// </summary>
    [JsonProperty("description", Required = Required.Always)]
    public string Description { get; set; }

    /// <summary>
    /// Start date of the project in yyyy-MM-dd format.
    /// </summary>
    [JsonProperty("startDate", Required = Required.Always)]
    [JsonConverter(typeof(CustomDateFormatConverter), "yyyy-MM-dd")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the project in yyyy-MM-dd format.
    /// </summary>
    [JsonProperty("endDate", Required = Required.Always)]
    [JsonConverter(typeof(CustomDateFormatConverter), "yyyy-MM-dd")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Location of the project
    /// </summary>
    [JsonProperty("locations")]
    public List<ProjectLocation> Locations { get; set; }

  }
}