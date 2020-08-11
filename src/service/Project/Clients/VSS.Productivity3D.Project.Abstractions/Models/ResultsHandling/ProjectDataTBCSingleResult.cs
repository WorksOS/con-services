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
    /// The id for the project.
    /// </summary>
    /// <value>
    /// The legacy project ID.
    /// </value>
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public long LegacyProjectId { get; set; }

    /// <summary>
    /// The name for the project.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }

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
    /// The project type: Standard = 0 (default), only type supported in WorksOS  
    /// </summary>
    [JsonProperty(PropertyName = "projectType", Required = Required.Default)]
    public int ProjectType { get; set; } = 0;



    public override bool Equals(object obj)
    {
      if (!(obj is ProjectDataTBCSingleResult otherProject)) return false;
      return otherProject.LegacyProjectId == this.LegacyProjectId
             && otherProject.ProjectType == this.ProjectType
             && otherProject.Name == this.Name
             && otherProject.StartDate == this.StartDate
             && otherProject.EndDate == this.EndDate
        ;
    }
  }
}
