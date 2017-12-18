using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  #region Result
  public class CompactionReportResult : RequestResult, IEquatable<CompactionReportResult>
  {
    #region Members
    public GridReport ReportData { get; set; }

    public new int Code { get; set; }

    public new string Message { get; set; }
    #endregion

    #region Constructor
    public CompactionReportResult() : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionReportResult other)
    {
      if (other == null)
        return false;

      return this.ReportData == other.ReportData &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionReportResult a, CompactionReportResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionReportResult a, CompactionReportResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionReportResult && this == (CompactionReportResult)obj;
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