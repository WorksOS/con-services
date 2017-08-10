using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;

namespace VSS.MasterData.Models.Models
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
            return new WGSPoint
            {
              Lat = 35.109149 * ConversionConstants.DEGREES_TO_RADIANS,
              Lon = -106.604076 * ConversionConstants.DEGREES_TO_RADIANS
            };
          }
        }

        /// <summary>
        /// Validates all properties
        /// </summary>
        public void Validate([FromServices] IServiceExceptionHandler serviceExceptionHandler)
        {
          //nothign else to validate
        }
    }

  public class ConversionConstants
  {
    /// <summary>
    /// Value to convert from decimal degrees to radians.
    /// </summary>
    public const double DEGREES_TO_RADIANS = Math.PI / 180;

    /// <summary>
    /// Null date value returned by Raptor.
    /// </summary>
    public static readonly DateTime PDS_MIN_DATE = new DateTime(1899, 12, 30, 0, 0, 0);

    /// <summary>
    /// Value to convert from km/h to cm/s
    /// </summary>
    public static readonly double KM_HR_TO_CM_SEC = 27.77777778; //1.0 / 3600 * 100000;

  }

}