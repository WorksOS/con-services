using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  #region Request
  /// <summary>
  /// The request representation for a linear based profile request for design and surveyed surface types.
  /// </summary>
  public class ProfileV2Request
  {
    /// <summary>
    /// The project to perform the request against
    /// </summary>
    public long? projectUid { get; set; }

    /// <summary>
    /// The start latitude degrees
    /// </summary>
    public double startLatDegrees { get; set; }

    /// <summary>
    /// The end latitude degrees
    /// </summary>
    public double endLatDegrees { get; set; }

    /// <summary>
    /// The start longitude degrees
    /// </summary>
    public double startLonDegrees { get; set; }

    /// <summary>
    /// The end longitude degrees
    /// </summary>
    public double endLonDegrees { get; set; }

    /// <summary>
    /// The imported file's Uid to run this request against
    /// </summary>
    public Guid importedFileUid { get; set; }

    /// <summary>
    /// The Id of the imported file to run this request against
    /// </summary>
    public int importedFileId { get; set; }

    /// <summary>
    /// The Filter Uid to include in this request
    /// Value may be null.
    /// </summary>
    public Guid filterUid { get; set; }
  }
  #endregion

  #region Result
  /// <summary>
  /// Base class containing common information relevant to linear and alignment based profile calculations
  /// </summary>
  public class BaseProfileV2 : RequestResult, IEquatable<BaseProfileV2>
  {
    #region Members
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    /// 
    public Guid callId;

    /// <summary>
    /// Was the profile calculation successful?
    /// </summary>
    /// 
    public bool success;

    /// <summary>
    /// The minimum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
    /// the first position of the profile line or alignment.
    /// </summary>
    /// 
    public double minStation;

    /// <summary>
    /// The maximum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
    /// the first position of the profile line or alignment.
    /// </summary>
    /// 
    public double maxStation;

    /// <summary>
    /// The minimum elevation across all cells processed in the profile result
    /// </summary>
    /// 
    public double minHeight;

    /// <summary>
    /// The maximum elevation across all cells processed in the profile result
    /// </summary>
    /// 
    public double maxHeight;

    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    /// 
    public double gridDistanceBetweenProfilePoints;
    #endregion

    #region Constructor
    protected BaseProfileV2() :
        base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(BaseProfileV2 other)
    {
      if (other == null)
        return false;

      return callId == other.callId &&
          success == other.success &&
          Math.Round(minStation, 3) == Math.Round(other.minStation, 3) &&
          Math.Round(maxStation, 3) == Math.Round(other.maxStation, 3) &&
          Math.Round(minHeight, 3) == Math.Round(other.minHeight, 3) &&
          Math.Round(maxHeight, 3) == Math.Round(other.maxHeight, 3) &&
          Math.Round(gridDistanceBetweenProfilePoints, 3) == Math.Round(other.gridDistanceBetweenProfilePoints, 3) &&
          Code == other.Code &&
          Message == other.Message;
    }

    public static bool operator ==(BaseProfileV2 a, BaseProfileV2 b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(BaseProfileV2 a, BaseProfileV2 b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is BaseProfileV2 && this == (BaseProfileV2)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }
    #endregion
  }

  /// <summary>
  /// The representation of a profile computed as a straight line between two points in the cartesian grid coordinate system of the project or
  /// by following a section of an alignment centerline.
  /// </summary>
  /// 
  public class ProfileV2Result : BaseProfileV2
  {
    #region Members
    /// <summary>
    /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
    /// </summary>
    /// 
    public List<ProfileCell> cells;

    /// <summary>
    /// A geometrical representation of the profile which defines the actual portion of the line or alignment used for the profile.
    /// </summary>
    /// 
    public List<StationLLPoint> alignmentPoints;
    #endregion

    #region Equality test
    public bool Equals(ProfileV2Result other)
    {
      if (other == null)
        return false;

      if (cells.Count != other.cells.Count)
        return false;
      if (alignmentPoints.Count != other.alignmentPoints.Count)
        return false;

      for (int i = 0; i < cells.Count; ++i)
      {
        if (cells[i] != other.cells[i])
          return false;
      }

      for (int i = 0; i < alignmentPoints.Count; ++i)
      {
        if (alignmentPoints[i] != other.alignmentPoints[i])
          return false;
      }

      return this == other;
    }

    public static bool operator ==(ProfileV2Result a, ProfileV2Result b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(ProfileV2Result a, ProfileV2Result b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is ProfileV2Result && this == (ProfileV2Result)obj;
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
  #endregion
}
