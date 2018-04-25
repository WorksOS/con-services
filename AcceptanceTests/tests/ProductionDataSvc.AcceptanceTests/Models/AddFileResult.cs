using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class AddFileResult : RequestResult, IEquatable<AddFileResult>
  {
    #region Members
    /// <summary>
    /// The minimum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int minZoomLevel;
    /// <summary>
    /// The maximum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int maxZoomLevel;
    #endregion

    #region Constructor
    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    public AddFileResult() :
      base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(AddFileResult other)
    {
      if (other == null)
        return false;

      return this.minZoomLevel == other.minZoomLevel &&
             this.maxZoomLevel == other.maxZoomLevel &&
             this.Code == other.Code && this.Message == other.Message;
    }

    public static bool operator ==(AddFileResult a, AddFileResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(AddFileResult a, AddFileResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is AddFileResult && this == (AddFileResult)obj;
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
