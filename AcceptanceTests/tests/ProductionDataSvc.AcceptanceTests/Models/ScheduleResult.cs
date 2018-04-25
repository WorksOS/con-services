using System;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class ScheduleResult : RequestResult, IEquatable<ScheduleResult>
  {
    #region Members
    public string JobId { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor: success result by default
    /// </summary>
    public ScheduleResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(ScheduleResult other)
    {
      if (other == null)
        return false;

      return this.JobId == other.JobId &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(ScheduleResult a, ScheduleResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(ScheduleResult a, ScheduleResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is ScheduleResult && this == (ScheduleResult)obj;
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
