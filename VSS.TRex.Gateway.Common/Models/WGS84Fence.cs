using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;

namespace VSS.TRex.Gateway.Common.Models
{
  /// <summary>
  /// A fence (or boundary) polygon with vertices expressed as WGS84 points
  /// </summary>
  public class WGS84Fence
  {
    private WGS84Fence()
    { }

    /// <summary>
    /// Array of WGS84 points defining the polygon. The polygon is implicitly closed (first and last points are not
    /// required to be the same).
    /// </summary>
  // todo  [MoreThanTwoPoints]
    [JsonProperty(PropertyName = "points", Required = Required.Always)]
    [Required]
    public WGSPoint[] Points { get; private set; }

    /// <summary>
    /// Creates the WSG84 fence object.
    /// </summary>
    /// <param name="wgsPoints">The WGS points.</param>
    public static WGS84Fence CreateWGS84Fence(WGSPoint[] wgsPoints)
    {
      return new WGS84Fence { Points = wgsPoints };
    }
  }
}
