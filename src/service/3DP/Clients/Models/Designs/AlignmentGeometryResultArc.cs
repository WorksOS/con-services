using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Productivity3D.Models.Designs
{
  public class AlignmentGeometryResultArc
  {
    /// <summary>
    /// Latitude of the first WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    [JsonProperty(PropertyName = "lat1", Required = Required.Always)] 
    public double Lat1 { get; set; }

    /// <summary>
    /// Longitude of the first WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    [JsonProperty(PropertyName = "lon1", Required = Required.Always)] 
    public double Lon1 { get; set; }

    /// <summary>
    /// Elevation of the arc start point, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "elev1", Required = Required.Always)] 
    public double Elev1 { get; set; }

    /// <summary>
    /// Latitude of the second WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    [JsonProperty(PropertyName = "lat2", Required = Required.Always)] 
    public double Lat2 { get; set; }

    /// <summary>
    /// Longitude of the second WGS84 point on the arc, expressed in decimal degrees
    /// </summary>
    [JsonProperty(PropertyName = "lon2", Required = Required.Always)] 
    public double Lon2 { get; set; }

    /// <summary>
    /// Elevation of the arc end arc, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "elev2", Required = Required.Always)] 
    public double Elev2 { get; set; }

    /// <summary>
    /// Latitude of the center WGS84 point of the arc, expressed in decimal degrees
    /// </summary>
    [JsonProperty(PropertyName = "latC", Required = Required.Always)] 
    public double LatC { get; set; }

    /// <summary>
    /// Longitude of the center WGS84 point of the arc, expressed in decimal degrees
    /// </summary>
    [JsonProperty(PropertyName = "lonC", Required = Required.Always)] 
    public double LonC { get; set; }

    /// <summary>
    /// Elevation of the arc center point, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "elevC", Required = Required.Always)] 
    public double ElevC { get; set; }

    /// <summary>
    /// Details if the arc moves clockwise from the first point to the second point.
    /// </summary>
    public bool CW { get; set; }

    // ReSharper disable once UnusedMember.Local
    private AlignmentGeometryResultArc() { }

    public AlignmentGeometryResultArc(double lat1, double lon1, double elev1, double lat2, double lon2, double elev2, double latC, double lonC, double elevC, bool cw)
    {
      Lat1 = lat1;
      Lon1 = lon1;
      Elev1 = elev1;
      Lat2 = lat2;
      Lon2 = lon2;
      Elev1 = elev2;
      LatC = latC;
      LonC = lonC;
      ElevC = elevC;
      CW = cw;
    }
  }
}
