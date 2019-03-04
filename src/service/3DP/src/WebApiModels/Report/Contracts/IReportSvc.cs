using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Contracts
{
  public interface IReportSvc
  {
#if RAPTOR
    ProjectStatisticsResult PostProjectStatistics([FromBody] ProjectStatisticsRequest request);
#endif
  }
}
