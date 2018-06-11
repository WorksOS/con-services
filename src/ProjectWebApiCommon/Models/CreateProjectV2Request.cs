using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// The request representation used to Create a project via TBC. 
  ///   TBC also send CustomerUid and CustomerName on the end, but Legacy and NGen 
  ///      dont' use this but gets them via TIDAuthenticaition
  /// </summary>
  public class CreateProjectV2Request
  {
    /// <summary>
    /// Project type: Standard = 0 (default), Landfill = 1, ProjectMonitoring = 2 
    /// </summary>
    [JsonProperty(PropertyName = "ProjectType", Required = Required.Always)]
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// Start date for the new project.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "StartDate", Required = Required.Always)]
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// End date for the new project. Must be after the start date.
    /// </summary>
    [JsonProperty(PropertyName = "EndDate", Required = Required.Always)]
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// The project name, between 1 and 30 characters long. It must be unique.
    /// </summary>
    [JsonProperty(PropertyName = "ProjectName", Required = Required.Always)]
    public string ProjectName { get; set; }

    /// <summary>
    /// The name of time zone of the project. This must be a standard Windows time zone name.
    /// </summary>
    [JsonProperty(PropertyName = "TimeZoneName", Required = Required.Always)]
    public string ProjectTimezone { get; set; }

    /// <summary>
    /// The list of latitude, longitude points that make up the project boundary.
    /// The list must contain at least three points and no more than 50 points.
    /// The boundary must not self-intersect nor overlap (temporarally or spatially) other projects.
    /// </summary>
    [JsonProperty(PropertyName = "BoundaryLL", Required = Required.Always)]
    public List<Point> BoundaryLL { get; set; }

    /// <summary>
    /// The details of the coordinate system file from Trimble Business Center.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystem", Required = Required.Always)]
    public BusinessCenterFile CoordinateSystem { get; set; }


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
      string projectTimezone, List<Point> boundaryLL,
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
