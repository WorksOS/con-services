using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// The request representation used to Create a project. 
  /// If CustomerUI, ProjectUID and ProjectID are null, then they will be populated via other means.
  /// This handles create of project, association to the customer and notification to raptor.????
  /// </summary>
  public class CreateProjectV2Request
  {
    /// <summary>
    /// Project type: Standard = 0 (default), Landfill = 1, ProjectMonitoring = 2 
    /// </summary>
    [JsonProperty(PropertyName = "projectType", Required = Required.Always)]
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// Start date for the new project.
    /// </summary>
    /// 
    /// // todo
    //[Required(ErrorMessage = "Required Field")]
    //[DateGreaterThan("endDate")]
    //[DataType(DataType.Date, ErrorMessage = "Must be valid date")]
    [JsonProperty(PropertyName = "startDate", Required = Required.Always)]
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// End date for the new project. Must be after the start date.
    /// </summary>
    ///     /// // todo
    //[Required(ErrorMessage = "Required Field")]
    //[DataType(DataType.Date, ErrorMessage = "Must be valid date")]
    [JsonProperty(PropertyName = "endDate", Required = Required.Always)]
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// The project name, between 1 and 30 characters long. It must be unique.
    /// </summary>
    [Required(ErrorMessage = "Required Field")]
    [StringLength(30, ErrorMessage = "projectName must be less or equal than 30 charaters", MinimumLength = 1)]
    [JsonProperty(PropertyName = "projectName", Required = Required.Always)]
    public string ProjectName { get; set; }

    /// <summary>
    /// The name of time zone of the project. This must be a standard Windows time zone name.
    /// </summary>
    //[Required(ErrorMessage = "Required Field")]
    [JsonProperty(PropertyName = "timeZoneName", Required = Required.Always)]
    public string ProjectTimezone { get; set; }

    /// <summary>
    /// The list of latitude, longitude points that make up the project boundary.
    /// The list must contain at least three points and no more than 50 points.
    /// The boundary must not self-intersect nor overlap (temporarally or spatially) other projects.
    /// </summary>
    //[Required(ErrorMessage = "Required Field")]
    //[MoreThanTwoPointsAttribute("boundaryLL")]
    [JsonProperty(PropertyName = "boundaryLL", Required = Required.Always)]
    public List<Models.PointLL> BoundaryLL { get; set; }
    /// <summary>
    /// The details of the coordinate system file from Trimble Business Center.
    /// </summary>
    //[Required(ErrorMessage = "Required Field")]
    [JsonProperty(PropertyName = "coordinateSystem", Required = Required.Always)]
    public BusinessCenterFile CoordinateSystem;

    
    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectV2Request()
    { }

    /// <summary>
    /// Create instance of CreateProjectV2Request
    /// </summary>
    public static CreateProjectV2Request CreateACreateProjectV2Request(
      ProjectType projectType, DateTime projectStartDate, DateTime projectEndDate, string projectName, 
      string projectTimezone, List<PointLL> boundaryLL,
      BusinessCenterFile coordinateSystem
      )
    {
      return new CreateProjectV2Request
      {
        ProjectType = projectType,
        ProjectStartDate = projectStartDate,
        ProjectEndDate = projectEndDate,
        ProjectName = projectName,
        ProjectTimezone = projectTimezone,
        BoundaryLL = boundaryLL,
        CoordinateSystem = coordinateSystem
      };
    }
  }
}
