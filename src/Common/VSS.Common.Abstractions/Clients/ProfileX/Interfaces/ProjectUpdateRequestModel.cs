using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.ProfileX.Models;
using VSS.Common.Abstractions.Converters;

namespace VSS.Common.Abstractions.Clients.ProfileX.Interfaces
{
  public class ProjectUpdateRequestModel
  {
    public ProjectUpdateRequestModel()
    {
      Locations = new List<ProjectLocation>();
    }

    /// <summary>
    /// Name of the project
    /// maxLength: 25
    /// </summary>
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; }

    /// <summary>
    /// Description about the project
    /// </summary>
    [JsonProperty("description", Required = Required.Always)]
    public string Description { get; set;}

    /// <summary>
    /// Project Start Date
    /// </summary>
    [JsonProperty("dateTime", Required = Required.Always)]
    [JsonConverter(typeof(CustomDateFormatConverter), "yyyy-MM-dd")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Project End Date
    /// </summary>
    [JsonProperty("name", Required = Required.Always)]
    [JsonConverter(typeof(CustomDateFormatConverter), "yyyy-MM-dd")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Project Locations
    /// </summary>
    [JsonProperty("name", Required = Required.Always)]
    public List<ProjectLocation> Locations { get; set; }
  }
}