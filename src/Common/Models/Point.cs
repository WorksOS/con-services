using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// A spatial coordinate within the grid coordinate system used by a project.
  /// </summary>
  public class Point : IValidatable
  {
    /// <summary>
    /// The X-ordinate of the position, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "x", Required = Required.Always)]
    [Required]
    public double x { get; private set; }

    /// <summary>
    /// The Y-ordinate of the position, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "y", Required = Required.Always)]
    [Required]
    public double y { get; private set; }

    
    /// <summary>
    /// Private constructor
    /// </summary>
    private Point()
    {}

    /// <summary>
    /// Create instance of Point
    /// </summary>
    public static Point CreatePoint(
        double x,
        double y
        )
    {
      return new Point
             {
                 x = x,
                 y = y
             };
    }

    /// <summary>
    /// Create example instance of Point to display in Help documentation.
    /// </summary>
    public static Point HelpSample => new Point
    {
      x = 82.3,
      y = 130.12
    };

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //Nothing else to validate
    }
  }
}