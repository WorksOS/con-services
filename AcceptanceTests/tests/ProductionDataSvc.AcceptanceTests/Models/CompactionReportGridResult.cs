using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RaptorSvcAcceptTestsCommon.Models;
using RaptorSvcAcceptTestsCommon.Utils;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  #region Result
  public class CompactionReportGridResult : RequestResult, IEquatable<CompactionReportGridResult>
  {
    #region Members
    public GridReport ReportData { get; set; }

    public int Code { get; set; }

    public string Message { get; set; }
    #endregion

    #region Constructor
    public CompactionReportGridResult() : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(CompactionReportGridResult other)
    {
      if (other == null)
        return false;

      return this.ReportData == other.ReportData &&
             this.Code == other.Code &&
             this.Message == other.Message;
    }

    public static bool operator ==(CompactionReportGridResult a, CompactionReportGridResult b)
    {
      if ((object)a == null || (object)b == null)
        return Object.Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(CompactionReportGridResult a, CompactionReportGridResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is CompactionReportGridResult && this == (CompactionReportGridResult)obj;
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
