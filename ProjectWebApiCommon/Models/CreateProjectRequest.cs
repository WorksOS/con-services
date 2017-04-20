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
  public class CreateProjectRequest 
  {
    /// <summary>
    /// The unique ID of the project. if null, then one will be generated.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectUid", Required = Required.AllowNull)]
    public Guid? ProjectUid { get; set; }

    /// <summary>
    /// The unique ID of the customer which the project is to be associated with. 
    /// if null, then the customer from the header will be used.
    /// </summary>
    [JsonProperty(PropertyName = "CustomerUid", Required = Required.AllowNull)]
    public Guid? CustomerUid { get; set; }

    /// <summary>
    /// The legacy ID of the project.
    /// This long/int is required by Raptor as it can't handle Guids. 
    /// if null, then we generate it from the next available in the ProjectMDM database.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectId", Required = Required.AllowNull)]
    public int? ProjectId { get; set; }

    /// <summary>
    /// The type of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectType", Required = Required.Always)]
    [Required(ErrorMessage = "ProjectType is required.")]
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// The name of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectName", Required = Required.Always)]
    [Required(ErrorMessage = "ProjectName is required.")]
    public string ProjectName { get; set; }

    /// <summary>
    /// The description of the project.
    /// Can be up to 2000 characters
    /// </summary>
    [JsonProperty(PropertyName = "Description", Required = Required.Always)]
    public string Description { get; set; }

    /// <summary>
    /// The start date of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectStartDate", Required = Required.Always)]
    [Required(ErrorMessage = "ProjectStartDate is required.")]
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// The end date of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectEndDate", Required = Required.Always)]
    [Required(ErrorMessage = "ProjectEndDate is required.")]
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// The time zone of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectTimezone", Required = Required.Always)]
    [Required(ErrorMessage = "ProjectTimezone is required.")]
    public string ProjectTimezone { get; set; }
    
    /// <summary>
    /// The boundary of the project. This is immutable.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectBoundary", Required = Required.Always)]
    [Required(ErrorMessage = "ProjectBoundary is required.")]
    public string ProjectBoundary { get; set; }

    /// <summary>
    /// The legacy customer number.
    /// This is no longer required by raptor so will be optional.
    /// </summary>
    [JsonProperty(PropertyName = "CustomerId", Required = Required.AllowNull)]
    [Required(ErrorMessage = "CustomerId is required.")]
    public long? CustomerId { get; set; }

    /// <summary>
    /// The CS of the project. 
    /// This is required for landfills but optional for other project types.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileName", Required = Required.AllowNull)]
    public string CoordinateSystemFileName { get; set; }

    /// <summary>
    /// The guts of the CoordinateSystem to be contained in the CoordinateSystemFileName. 
    /// Required if CoordinateSystemFileName is provided.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileName", Required = Required.AllowNull)]
    public byte[] CoordinateSystemFileContent { get; set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectRequest()
    { }

    /// <summary>
    /// Create instance of CreateProjectRequest
    /// </summary>
    public static CreateProjectRequest CreateACreateProjectRequest(Guid? projectUid, Guid? customerUid, 
      int? projectId, ProjectType projectType, string projectName, string description,
      DateTime projectStartDate, DateTime projectEndDate, string projectTimezone, string projectBoundary,
      long? customerId, string coordinateSystemFileName, byte[] coordinateSystemFileContent
      )
    {
      return new CreateProjectRequest
      {
        ProjectUid = projectUid,
        CustomerUid = customerUid,
        ProjectId = projectId,
        ProjectType = projectType,
        ProjectName = projectName,
        Description = description,
        ProjectStartDate = projectStartDate,
        ProjectEndDate = projectEndDate,
        ProjectTimezone = projectTimezone,
        ProjectBoundary = projectBoundary,
        CustomerId = customerId,
        CoordinateSystemFileName = coordinateSystemFileName,
        CoordinateSystemFileContent = coordinateSystemFileContent
      };
    }
  }
}
