using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.Report.Contracts
{
  public interface IReportSvc
  {
#if RAPTOR
    Task<ProjectStatisticsResult> PostProjectStatistics([FromBody] ProjectStatisticsRequest request);
#endif
  }
}
