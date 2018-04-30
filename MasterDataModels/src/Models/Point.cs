using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;

namespace VSS.MasterData.Models.Models
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
    public double x { get; set; }

    /// <summary>
    /// The Y-ordinate of the position, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "y", Required = Required.Always)]
    [Required]
    public double y { get; set; }

    public double Latitude => y;

    public double Longitude => x;

    public Point(double lat, double lon)
    {
      x = lon;
      y = lat;
    }

    public Point()
    {
    }

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
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      //Nothing else to validate
    }

    #region Equality test

    public bool Equals(Point other)
    {
      if (other == null)
        return false;

      const double EPSILON = 0.000001;

      return Math.Abs(this.Latitude - other.Latitude) < EPSILON &&
             Math.Abs(this.Longitude - other.Longitude) < EPSILON;

    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      //Nothing else to validate
    }

    public static bool operator ==(Point a, Point b)
    {
      if ((object) a == null || (object) b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(Point a, Point b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is Point && this == (Point) obj;
    }

    #endregion
  }
}
