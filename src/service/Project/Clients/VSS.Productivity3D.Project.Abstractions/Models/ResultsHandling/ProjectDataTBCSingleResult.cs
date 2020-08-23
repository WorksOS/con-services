using System;
using System.Globalization;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  /// <summary>
  ///   Single project descriptor
  /// </summary>
  ///   /// <seealso cref="ContractExecutionResult" />
  public class ProjectDataTBCSingleResult : ContractExecutionResult
  {
    /// <summary>
    ///   Gets or sets a value indicating whether this instance is archived.
    ///       Yes gets archived and non-archived
    /// </summary>
    [JsonProperty(PropertyName = "isArchived", Required = Required.Default)]
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// The name for the project.
    /// </summary>
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }

    /// <summary>
    /// Timezone is now calculated by cws from the bounding box
    ///    Do we need to convert it back to the format:  "W. Europe Standard Time"?
    ///       TBC may not use this field
    /// </summary>
    [JsonProperty(PropertyName = "projectTimeZone", Required = Required.Default)]
    public string ProjectTimeZone { get; set; } 


    /// <summary>
    /// The project type: Standard = 0 (default), only type supported in WorksOS  
    /// </summary>
    [JsonProperty(PropertyName = "projectType", Required = Required.Default)]
    public int ProjectType { get; set; } = 0;

    /// <summary>
    /// The project type: Standard = 0 (default), only type supported in WorksOS  
    /// </summary>
    [JsonProperty(PropertyName = "projectTypeName", Required = Required.Default)]
    public string ProjectTypeName { get; set; } = "Standard";
    
    /// <summary>
    /// The start date for the project. Obsolete in WorksOS
    /// </summary>
    [JsonProperty(PropertyName = "startDate", Required = Required.Default)]
    public string StartDate { get; set; } = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The end date for the project. Obsolete in WorksOS
    /// </summary>
    [JsonProperty(PropertyName = "endDate", Required = Required.Default)]
    public string EndDate { get; set; } = DateTime.MaxValue.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets or sets the project uid.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public string ProjectUid { get; set; }

    /// <summary>
    /// Gets or sets the project geofence.
    ///    In format: "POLYGON((170 10, 190 10, 190 40, 170 40, 170 10))"
    /// </summary>
    [JsonProperty(PropertyName = "projectGeofenceWKT", Required = Required.Default)]
    public string ProjectGeofenceWKT { get; set; }

    /// <summary>
    /// The short id generated from the ProjectUid which TBC uses in its legacy code.
    /// </summary>
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public long LegacyProjectId { get; set; }
    
    /// <summary>
    /// Gets or sets the CustomerUID which the project is associated with
    /// </summary>
    [JsonProperty(PropertyName = "customerUid", Required = Required.Default)]
    public string CustomerUid { get; set; } 

    /// <summary>
    /// Gets or sets the customer Id from legacy VisionLink. Obsolete in WorksOS
    /// </summary>
    [JsonProperty(PropertyName = "legacyCustomerId", Required = Required.Default)]
    public string LegacyCustomerId { get; set; } = "0"; 

    /// <summary>
    /// Gets or sets the CoordinateSystem FileName which the project is associated with
    /// </summary>
    [JsonProperty(PropertyName = "coordinateSystemFileName", Required = Required.Default)]
    public string CoordinateSystemFileName { get; set; }

    public override bool Equals(object obj)
    {
      if (!(obj is ProjectDataTBCSingleResult otherProject)) return false;
      return otherProject.IsArchived == this.IsArchived
             && otherProject.Name == this.Name
             && otherProject.ProjectTimeZone == this.ProjectTimeZone
             && otherProject.ProjectType == this.ProjectType
             && otherProject.ProjectTypeName == this.ProjectTypeName
             && otherProject.StartDate == this.StartDate
             && otherProject.EndDate == this.EndDate
             && otherProject.ProjectUid == this.ProjectUid
             && otherProject.ProjectGeofenceWKT == this.ProjectGeofenceWKT
             && otherProject.LegacyProjectId == this.LegacyProjectId
             && otherProject.CustomerUid == this.CustomerUid
             && otherProject.LegacyCustomerId == this.LegacyCustomerId
             && otherProject.CoordinateSystemFileName == this.CoordinateSystemFileName
        ;
    }
  }
}
