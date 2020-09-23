using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class SummaryDataBase : ContractExecutionResult
  {
    public byte[] MapImage { get; set; }
    public NameValueCollection ReportUrlQueryCollection { get; set; }
    public OptionalSummaryReportRoute ReportEnum { get; set; }
  }
}
