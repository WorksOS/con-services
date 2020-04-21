using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{

  /// <summary>
  ///   Single project descriptor
  /// </summary>
  ///   /// <seealso cref="ContractExecutionResult" />
  public class ProjectV5DescriptorResult : ContractExecutionResult
  {
    /// <summary>
    /// The id for the project.
    /// </summary>
    /// <value>
    /// The legacy project ID.
    /// </value>
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public long ShortRaptorProjectId { get; set; }

    /// <summary>
    /// The name for the project.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [JsonProperty(PropertyName = "name", Required = Required.Default)]
    public string Name { get; set; }

    /// <summary>
    /// The project type: Standard = 0 (default), Landfill = 1, ProjectMonitoring = 2  
    /// </summary>
    /// <value>
    /// The type of the project.
    /// </value>
    [JsonProperty(PropertyName = "projectType", Required = Required.Default)]
    public ProjectType ProjectType { get; set; }



    public override bool Equals(object obj)
    {
      if (!(obj is ProjectV5DescriptorResult otherProject)) return false;
      return otherProject.ShortRaptorProjectId == ShortRaptorProjectId
             && otherProject.ProjectType == ProjectType
             && otherProject.Name == Name
          ;
    }

    public override int GetHashCode() { return 0; }
  }
}
