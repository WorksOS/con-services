using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  /// The request representation used to Create a project via TBC.
  /// A project (type=AcceptsTagFiles) will be created via cws/project
  ///   TBC also send CustomerUID and CustomerName, we ignore and get them via TIDAuthentication
  ///                 ProjectType; ProjectStartDate; ProjectEndDate; which we ignore
  ///                 ProjectTimezone which we ignore, it is created in cws based on the boundary
  /// </summary>
  public class CreateProjectV5Request
  {
    /// <summary>
    /// The project name, between 1 and 30 characters long. It must be unique.
    /// </summary>
    [JsonProperty(PropertyName = "projectName", Required = Required.Always)]
    public string ProjectName { get; set; }

    /// <summary>
    /// The list of latitude, longitude points to be validated as per AcceptsTagFiles projects
    /// </summary>
    [JsonProperty(PropertyName = "boundaryLL", Required = Required.Always)]
    public List<TBCPoint> BoundaryLL { get; set; }

    /// <summary>
    /// The details of the coordinate system file from Trimble Business Center.
    /// </summary>
    [JsonProperty(PropertyName = "coordinateSystem", Required = Required.Always)]
    public BusinessCenterFile CoordinateSystem { get; set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private CreateProjectV5Request()
    { }

    /// <summary>
    /// Create instance of CreateProjectV2Request
    /// </summary>
    public static CreateProjectV5Request CreateACreateProjectV5Request(
      string projectName, List<TBCPoint> boundaryLL, BusinessCenterFile coordinateSystem
      )
    {
      return new CreateProjectV5Request
      {
        ProjectName = projectName,
        BoundaryLL = boundaryLL,
        CoordinateSystem = coordinateSystem
      };
    }
  }
}
