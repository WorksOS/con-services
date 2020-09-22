using System.Net;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using CCSS.WorksOS.Reports.Common.DataGrabbers;
using CCSS.WorksOS.Reports.Common.Models;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Reports.Core.DataGrabbers;

namespace CCSS.WorksOS.Reports.Common.Helpers
{
  public static class DataGrabberHelper
  {
    public static IDataGrabber CreateDataGrabber(ILogger logger, IServiceExceptionHandler serviceExceptionHandler, IWebRequest gracefulClient, 
      GenericComposerRequest composerRequest)
    {
      switch (composerRequest.ReportRequest.ReportTypeEnum)
      {
        case ReportType.Summary:
          return new SummaryDataAPIDataGrabber(logger, serviceExceptionHandler, gracefulClient, composerRequest);
        case ReportType.Grid:
          return new GridDataGrabber(logger, serviceExceptionHandler, gracefulClient, composerRequest);
        case ReportType.StationOffset:
          return new StationOffsetDataGrabber(logger, serviceExceptionHandler, gracefulClient, composerRequest);
        default:
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Invalid report type."));
      }
    }
  }
}
