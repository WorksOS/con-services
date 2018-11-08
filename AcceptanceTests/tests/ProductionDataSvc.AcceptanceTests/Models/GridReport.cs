using System;
using System.Collections.Generic;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GridReport
  {
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<GridReportRows> Rows { get; set; }
  }
}
