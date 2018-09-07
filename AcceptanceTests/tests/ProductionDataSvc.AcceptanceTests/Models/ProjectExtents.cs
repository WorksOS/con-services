using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class ProjectExtents: RequestResult, IEquatable<ProjectExtents>
  {
    #region Members
    /// <summary>
    /// Minimum latitude of the production data extents in degrees
    /// </summary>
    public double minLat;
    /// <summary>
    /// Minimum longitude of the production data extents in degrees
    /// </summary>
    public double minLng;
    /// <summary>
    /// Maximum latitude of the production data extents in degrees
    /// </summary>
    public double maxLat;
    /// <summary>
    /// Maximum longitude of the production data extents in degrees
    /// </summary>
    public double maxLng;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public ProjectExtents()
        : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(ProjectExtents other)
    {
      if (other == null)
        return false;

      return Math.Round(this.minLat, 2) == Math.Round(other.minLat, 2) &&
             Math.Round(this.maxLat, 2) == Math.Round(other.maxLat, 2) &&
             Math.Round(this.minLng, 2) == Math.Round(other.minLng, 2) &&
             Math.Round(this.maxLng, 2) == Math.Round(other.maxLng, 2) &&
          this.Code == other.Code &&
          this.Message == other.Message;
    }

    public static bool operator ==(ProjectExtents a, ProjectExtents b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(ProjectExtents a, ProjectExtents b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is ProjectExtents && this == (ProjectExtents)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion

    #region ToString override
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion
  }
}
