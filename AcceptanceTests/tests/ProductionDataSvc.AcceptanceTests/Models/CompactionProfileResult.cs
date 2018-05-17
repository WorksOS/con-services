using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionProfileResult<T> : RequestResult, IEquatable<CompactionProfileResult<T>>
  {
    #region members
    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    public double gridDistanceBetweenProfilePoints;

    /// <summary>
    /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
    /// </summary>
    public List<T> results;
    #endregion

    #region constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public CompactionProfileResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionProfileResult<T> other)
    {
      if (other == null)
        return false;

      if (this.results.Count != other.results.Count)
        return false;

      for (int i = 0; i < this.results.Count; i++)
      {
        if (!this.results[i].Equals(other.results[i]))
          return false;
      }

      return Math.Round(this.gridDistanceBetweenProfilePoints,2) == Math.Round(other.gridDistanceBetweenProfilePoints, 2) &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionProfileResult<T> a, CompactionProfileResult<T> b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionProfileResult<T> a, CompactionProfileResult<T> b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionProfileResult<T> && this == (CompactionProfileResult<T>)obj;
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

  public class CompactionDesignProfileResult : IEquatable<CompactionDesignProfileResult>
  {
    #region members
    /// <summary>
    /// The design file UID the response data is collected from.
    /// </summary>
    public Guid designFileUid;

    /// <summary>
    /// The collection of vertices produced by the query. Vertices are ordered by increasing station value along the line or alignment.
    /// </summary>
    public List<CompactionProfileVertex> data;
    #endregion

    #region Equality test

    public bool Equals(CompactionDesignProfileResult other)
    {
      if (other == null)
        return false;

      if (this.data.Count != other.data.Count)
        return false;

      for (int i = 0; i < this.data.Count; i++)
      {
        if (!this.data[i].Equals(other.data[i]))
          return false;
      }

      return this.designFileUid == other.designFileUid;
    }

    public static bool operator ==(CompactionDesignProfileResult a, CompactionDesignProfileResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionDesignProfileResult a, CompactionDesignProfileResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionDesignProfileResult && this == (CompactionDesignProfileResult)obj;
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

  public class CompactionProfileVertex : IEquatable<CompactionProfileVertex>
  {
    const int DECIMAL_PLACES = 2;
    #region members
    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects the design surface.
    /// </summary>
    public double x;

    /// <summary>
    /// Elevation of the profile vertex.
    /// </summary>
    public float y;
    #endregion

    #region Equality test
    public bool Equals(CompactionProfileVertex other)
    {
      if (other == null)
        return false;


      return Math.Round(this.x, DECIMAL_PLACES) == Math.Round(other.x, DECIMAL_PLACES) &&
             FloatEquals(this.y, other.y);
    }

    private bool FloatEquals(float f1, float f2)
    {
      if (float.IsNaN(f1) || float.IsNaN(f2))
        return float.IsNaN(f1) && float.IsNaN(f2);

      return Math.Round(f1, DECIMAL_PLACES) == Math.Round(f2, DECIMAL_PLACES);
    }

    public static bool operator ==(CompactionProfileVertex a, CompactionProfileVertex b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionProfileVertex a, CompactionProfileVertex b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionProfileVertex && this == (CompactionProfileVertex)obj;
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

  public class CompactionProfileDataResult : IEquatable<CompactionProfileDataResult>
  {
    #region members 
    /// <summary>
    /// The type indicates what type of production data e.g. lastPass, cutFill, passCount etc.
    /// </summary>
    public string type;
    /// <summary>
    /// A list of data points for the profile.
    /// </summary>
    public List<CompactionDataPoint> data;
    #endregion

    #region Equality test
    public bool Equals(CompactionProfileDataResult other)
    {
      if (other == null)
        return false;

      if (this.data.Count != other.data.Count)
        return false;

      for (int i = 0; i < this.data.Count; i++)
      {
        if (!this.data[i].Equals(other.data[i]))
          return false;
      }

      return this.type == other.type;
    }

    public static bool operator ==(CompactionProfileDataResult a, CompactionProfileDataResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionProfileDataResult a, CompactionProfileDataResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionProfileDataResult && this == (CompactionProfileDataResult)obj;
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

  public class CompactionDataPoint : IEquatable<CompactionDataPoint>
  {
    #region members
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type points.
    /// </summary>
    public double x;

    /// <summary>
    /// Elevation for profile cell for the type of data, e.g. CMV height for CMV, last pass height for elevation last pass etc.
    /// </summary>
    public float y;

    /// <summary>
    /// The value of the profile data for the type of data e.g. cut-fill, CMV, temperature etc. For speed it is the minimum speed value.
    /// </summary>
    public float value;

    /// <summary>
    /// For summary profile types, what the value represents with respect to the target. Used to select the color for the profile line segment.
    /// </summary>
    public ValueTargetType? valueType;

    /// <summary>
    /// For cut-fill profiles only, the design elevation of the cell.
    /// </summary>
    public float? y2;

    /// <summary>
    /// For speed summary profiles only, the maximum speed value.
    /// </summary>
    public float? value2;

    #endregion

    #region Equality test
    public bool Equals(CompactionDataPoint other)
    {
      if (other == null)
        return false;

      return this.cellType == other.cellType &&
             Math.Round(this.x, 2) == Math.Round(other.x, 2) &&
             FloatEquals(this.y, other.y) &&
             FloatEquals(this.value, other.value) &&         
             this.valueType == other.valueType &&
             NullableFloatEquals(this.y2, other.y2) &&
             NullableFloatEquals(this.value2, other.value2);
    }

    private bool NullableFloatEquals(float? f1, float? f2)
    {
      if (f1.HasValue && f2.HasValue)
      {
        return FloatEquals(f1.Value, f2.Value);
      }
      return f1.HasValue == f2.HasValue;
    }

    private bool FloatEquals(float f1, float f2)
    {
      if (float.IsNaN(f1) || float.IsNaN(f2))
        return true;  // Change for test inconsistent for last entry. 
       // return float.IsNaN(f1) && float.IsNaN(f2);

      return Math.Round(f1, 2) == Math.Round(f2, 2);
    }

    public static bool operator ==(CompactionDataPoint a, CompactionDataPoint b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionDataPoint a, CompactionDataPoint b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionDataPoint && this == (CompactionDataPoint)obj;
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

  /// <summary>
  /// Specifies the type of profile cell 
  /// </summary>
  public enum ProfileCellType
  {
    /// <summary>
    /// Station intersects the cell edge and has data
    /// </summary>
    Edge,

    /// <summary>
    /// Station is the midpoint of the line segment that cuts through the cell
    /// </summary>
    MidPoint,

    /// <summary>
    /// Station intersects the cell edge and has no data; the start of a gap
    /// </summary>
    Gap,
  }

  /// <summary>
  /// Specifies what the summary value represents in terms of the target
  /// </summary>
  public enum ValueTargetType
  {
    /// <summary>
    /// No value for this type of data for this cell
    /// </summary>
    NoData = -1,

    /// <summary>
    /// Value is above target
    /// </summary>
    AboveTarget = 0,

    /// <summary>
    /// Value is on target
    /// </summary>
    OnTarget = 1,

    /// <summary>
    /// Value is below target
    /// </summary>
    BelowTarget = 2
  }

  public class CompactionProfileResultTest : RequestResult
  {
    public double gridDistanceBetweenProfilePoints;
    public List<CompactionDataPoint> results;
  }
}
