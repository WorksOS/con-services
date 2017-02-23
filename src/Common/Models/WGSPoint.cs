using System;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Utilities;
using System.ComponentModel.DataAnnotations;

namespace VSS.Raptor.Service.Common.Models
{
    /// <summary>
    ///     A point specified in WGS 84 latitude/longtitude coordinates
    /// </summary>
  public class WGSPoint : IValidatable
    {
        private WGSPoint()
        {
        }

        /// <summary>
        ///     WGS84 latitude, expressed in radians
        /// </summary>
        [DecimalIsWithinRange(-Math.PI/2, Math.PI/2)]
        [JsonProperty(PropertyName = "Lat", Required = Required.Always)]
        [Required]
        public double Lat { get; private set; }

        /// <summary>
        ///     WSG84 longitude, expressed in radians
        /// </summary>
        [DecimalIsWithinRange(-Math.PI, Math.PI)]
        [JsonProperty(PropertyName = "Lon", Required = Required.Always)]
        [Required]
        public double Lon { get; private set; }

        /// <summary>
        ///     Creates the point.
        /// </summary>
        /// <param name="lat">The latitude.</param>
        /// <param name="lon">The longtitude.</param>
        /// <returns></returns>
        public static WGSPoint CreatePoint(double lat, double lon)
        {
            return new WGSPoint {Lat = lat, Lon = lon};
        }

        /// <summary>
        /// Create example instance of WGSPoint to display in Help documentation.
        /// </summary>
        public static WGSPoint HelpSample
        {
          get
          {
            return new WGSPoint()
            {
              Lat = 35.109149 * ConversionConstants.DEGREES_TO_RADIANS,
              Lon = -106.604076 * ConversionConstants.DEGREES_TO_RADIANS
            };
          }
        }

        /// <summary>
        /// Validates all properties
        /// </summary>
        public void Validate()
        {
          //nothign else to validate
        }
    }
}