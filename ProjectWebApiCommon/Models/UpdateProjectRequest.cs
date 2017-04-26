using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApi.Models
{
  /// <summary>
  /// The request representation used to Create a project. 
  /// If CustomerUI, ProjectUID and ProjectID are null, then they will be populated via other means.
  /// This handles create of project, association to the customer and notification to raptor.
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
    [JsonProperty(PropertyName = "Description", Required = Required.AllowNull)]
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
      string coordinateSystemFileName, byte[] coordinateSystemFileContent
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
        CoordinateSystemFileContent = coordinateSystemFileContent
      };
    }
  }
}
