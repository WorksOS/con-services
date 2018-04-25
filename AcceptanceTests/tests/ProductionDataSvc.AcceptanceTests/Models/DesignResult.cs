using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class DesignResult : RequestResult, IEquatable<DesignResult>
  {
    #region Members
    /// <summary>
    /// Array of design boundaries in GeoJson format.
    /// </summary>
    /// 
    public JObject[] designBoundaries { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    public DesignResult() :
      base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(DesignResult other)
    {
      if (other == null)
        return false;

      if (this.designBoundaries.Length != other.designBoundaries.Length)
        return false;

      for (var i = 0; i < this.designBoundaries.Length; i++)
      {
        if (other.designBoundaries[i].ToString() != this.designBoundaries[i].ToString())
          return false;
      }

      return this.Code == other.Code && this.Message == other.Message;
    }

    public static bool operator ==(DesignResult a, DesignResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(DesignResult a, DesignResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is DesignResult && this == (DesignResult)obj;
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
