using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// The two end points of a stright line used for a profile calculation, defined in WGS84 latitude longitude coordinates.
  /// </summary>
  /// 
  public class ProfileLLPoints : IValidatable
  {
    /// <summary>
    /// Latitude ordinate of the first profile end point. Values are expressed in radians.
    /// </summary>
    /// 
    [DecimalIsWithinRange(-Math.PI/2, Math.PI/2)]
    [JsonProperty(PropertyName = "lat1", Required = Required.Always)]
    [Required]
    public double lat1 { get; private set; }

    /// <summary>
    /// Longitude ordinate of the first profile end point. Values are expressed in radians.
    /// </summary>
    /// 
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    [JsonProperty(PropertyName = "lon1", Required = Required.Always)]
    [Required]
    public double lon1 { get; private set; }

    /// <summary>
    /// Latitude ordinate of the second profile end point. Values are expressed in radians.
    /// </summary>
    /// 
    [DecimalIsWithinRange(-Math.PI/2, Math.PI/2)]
    [JsonProperty(PropertyName = "lat2", Required = Required.Always)]
    [Required]
    public double lat2 { get; private set; }

    /// <summary>
    /// Longitude ordinate of the second profile end point. Values are expressed in radians.
    /// </summary>
    /// 
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    [JsonProperty(PropertyName = "lon2", Required = Required.Always)]
    [Required]
    public double lon2 { get; private set; }

 /// <summary>
    /// Creates an instance of the ProfileLLPoints class.
    /// </summary>
    /// <param name="lat1">The first latitude value.</param>
    /// <param name="lon1">The first longitude value.</param>
    /// <param name="lat2">The second latitude value.</param>
    /// <param name="lon2">The second longitude value.</param>
    /// <returns>The created instance.</returns>
    /// 
    public static ProfileLLPoints CreateProfileLLPoints(double lat1, double lon1, double lat2, double lon2)
    {
      return new ProfileLLPoints { lat1 = lat1, lon1 = lon1, lat2 = lat2, lon2 = lon2 };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // Nothing else to validate...
    }
  }
}