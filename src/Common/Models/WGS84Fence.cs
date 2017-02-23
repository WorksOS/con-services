
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VSS.Raptor.Service.Common.Models
{
    /// <summary>
    ///     A fence (or boundary) polygon with vertices expressed as WGS84 points
    /// </summary>
    public class WGS84Fence
    {
        private WGS84Fence()
        {
        }

        /// <summary>
        ///     Array of WGS84 points defining the polygon. The polygon is implicitly closed (first and last points are not
        ///     required to be the same).
        /// </summary>
        [MoreThanTwoPointsAttribute]
        [JsonProperty(PropertyName = "points", Required = Required.Always)]
        [Required]
        public WGSPoint[] points { get; private set; }

        /// <summary>
        ///     Creates the WSG84 fence object.
        /// </summary>
        /// <param name="wgsPoints">The WGS points.</param>
        /// <returns></returns>
        public static WGS84Fence CreateWGS84Fence(WGSPoint[] wgsPoints)
        {
            return new WGS84Fence {points = wgsPoints};
        }
    }
}