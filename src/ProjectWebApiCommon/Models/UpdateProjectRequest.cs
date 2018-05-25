using System;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// The request representation used to Upsert a project. 
  /// If CustomerUI, ProjectUID are null, then they will be populated via other means.
  /// </summary>
  public class UpdateProjectRequest 
  {
    /// <summary>
    /// The unique ID of the project. if null, then one will be generated.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Always)]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The type of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectType", Required = Required.Always)]
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// The name of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectName", Required = Required.Always)]
    public string ProjectName { get; set; }

    /// <summary>
    /// The description of the project.
    /// Can be up to 2000 characters
    /// </summary>
    [JsonProperty(PropertyName = "Description", Required = Required.Default)]
    public string Description { get; set; }

    /// <summary>
    /// The end date of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectEndDate", Required = Required.Always)]
    public DateTime ProjectEndDate { get; set; }
  
    /// <summary>
    /// The CS of the project. 
    /// This is required for landfills but optional for other project types.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileName", Required = Required.Default)]
    public string CoordinateSystemFileName { get; set; } = string.Empty;

    /// <summary>
    /// The guts of the CoordinateSystem to be contained in the CoordinateSystemFileName. 
    /// Required if CoordinateSystemFileName is provided.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileContent", Required = Required.Default)]
    public byte[] CoordinateSystemFileContent { get; set; } = null;

    /// <summary>
    /// The boundary of the project. This is now mutable.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectBoundary", Required = Required.Always)]
    public string ProjectBoundary { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private UpdateProjectRequest()
    { }

    /// <summary>
    /// Create instance of CreateProjectRequest
    /// </summary>
    public static UpdateProjectRequest CreateUpdateProjectRequest(Guid projectUid, 
      ProjectType projectType, string projectName, string description,
      DateTime projectEndDate, 
      string coordinateSystemFileName, byte[] coordinateSystemFileContent, string projectBoundary
      )
    {
      return new UpdateProjectRequest
      {
        ProjectUid = projectUid,
        ProjectType = projectType,
        ProjectName = projectName,
        Description = description,
        ProjectEndDate = projectEndDate,
        CoordinateSystemFileName = coordinateSystemFileName,
        CoordinateSystemFileContent = coordinateSystemFileContent,
        ProjectBoundary = projectBoundary
      };
    }
  }
}
