using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Report.Contracts
{
  public interface IReportSvc
  {
    ProjectStatisticsResult PostProjectStatistics([FromBody] ProjectStatisticsRequest request);
  }
}
