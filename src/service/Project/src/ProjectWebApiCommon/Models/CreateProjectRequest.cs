using System;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
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
    [JsonProperty(PropertyName = "ProjectUID", Required = Required.Default)]
    public Guid? ProjectUID { get; set; } = null;

    
    /// <summary>
    /// The unique ID of the customer which the project is to be associated with. 
    /// if null, then the customer from the header will be used.
    /// </summary>
    [JsonProperty(PropertyName = "CustomerUID", Required = Required.Default)]
    public Guid? CustomerUID { get; set; } = null;

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
    /// The start date of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectStartDate", Required = Required.Always)]
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// The end date of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectEndDate", Required = Required.Always)]
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// The time zone of the project.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectTimezone", Required = Required.Always)]
    public string ProjectTimezone { get; set; }
    
    /// <summary>
    /// The boundary of the project. This is now mutable.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectBoundary", Required = Required.Default)]
    public string ProjectBoundary { get; set; }

    /// <summary>
    /// The legacy customer number.
    /// This is no longer required by raptor so will be optional.
    /// </summary>
    [JsonProperty(PropertyName = "CustomerID", Required = Required.Default)]
    public long? CustomerID { get; set; } = 0;

    /// <summary>
    /// The CS of the project. 
    /// This is required for landfills but optional for other project types.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileName", Required = Required.Default)]
    public string CoordinateSystemFileName { get; set; } = string.Empty;

    /// <summary>
    /// The guts of the CoordinateSystem to be contained in the CoordinateSystemFileContent. 
    /// Required if CoordinateSystemFilenAME is provided.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFileContent", Required = Required.Default)]
    public byte[] CoordinateSystemFileContent { get; set; } = null;


    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectRequest()
    { }

    /// <summary>
    /// Create instance of CreateProjectRequest
    /// </summary>
    public static CreateProjectRequest CreateACreateProjectRequest(Guid? projectUid, Guid? customerUid, 
      ProjectType projectType, string projectName, string description,
      DateTime projectStartDate, DateTime projectEndDate, string projectTimezone, string projectBoundary,
      long? customerId, string coordinateSystemFileName, byte[] coordinateSystemFileContent
      )
    {
      return new CreateProjectRequest
      {
        ProjectUID = projectUid,
        CustomerUID = customerUid,
        ProjectType = projectType,
        ProjectName = projectName,
        Description = description,
        ProjectStartDate = projectStartDate,
        ProjectEndDate = projectEndDate,
        ProjectTimezone = projectTimezone,
        ProjectBoundary = projectBoundary,
        CustomerID = customerId,
        CoordinateSystemFileName = coordinateSystemFileName,
        CoordinateSystemFileContent = coordinateSystemFileContent
      };
    }
  }
}
