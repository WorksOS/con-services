using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// The two end points of a stright line used for a profile calculation, defined in the cartesian grid coordinate system of the project
  /// </summary>
  public class ProfileGridPoints : IValidatable
  {
    /// <summary>
    /// X ordinate of the first profile end point. Values are expressed in meters.
    /// </summary>
    [JsonProperty(PropertyName = "x1", Required = Required.Always)]
    [Required]
    public double x1 { get; private set; }

    /// <summary>
    /// Y ordinate of the first profile end point. Values are expressed in meters.
    /// </summary>
    [JsonProperty(PropertyName = "y1", Required = Required.Always)]
    [Required]
    public double y1 { get; private set; }

    /// <summary>
    /// X ordinate of the second profile end point. Values are expressed in meters.
    /// </summary>
    [JsonProperty(PropertyName = "x2", Required = Required.Always)]
    [Required]
    public double x2 { get; private set; }

    /// <summary>
    /// Y ordinate of the second profile end point. Values are expressed in meters.
    /// </summary>
    [JsonProperty(PropertyName = "y2", Required = Required.Always)]
    [Required]
    public double y2 { get; private set; }

    /// <summary>
    /// Creates an instance of the ProfileGridPoints class.
    /// </summary>
    /// <param name="x1">The first X value.</param>
    /// <param name="y1">The first Y value.</param>
    /// <param name="x2">The second X value.</param>
    /// <param name="y2">The second Y value.</param>
    /// <returns>The created instance.</returns>
    public static ProfileGridPoints CreateProfileGridPoints(double x1, double y1, double x2, double y2)
    {
      return new ProfileGridPoints { x1 = x1, y1 = y1, x2 = x2, y2 = y2 };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // Nothing to validate...
    }
  }
}