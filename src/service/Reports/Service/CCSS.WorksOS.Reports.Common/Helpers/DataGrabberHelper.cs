using System.Collections.Generic;
using System.Linq;
using System.Net;
using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using CCSS.WorksOS.Reports.Common.DataGrabbers;
using CCSS.WorksOS.Reports.Common.Models;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

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
          return new SummaryDataGrabber(logger, serviceExceptionHandler, gracefulClient, composerRequest);
        case ReportType.StationOffset:
          return new StationOffsetDataGrabber(logger, serviceExceptionHandler, gracefulClient, composerRequest);
        case ReportType.Grid:
          return new GridDataGrabber(logger, serviceExceptionHandler, gracefulClient, composerRequest);
        default:
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(9, "Invalid report type."));
      }
    }

    public static string AppendOrUpdateQueryParam(string url, string queryParamName, string queryParamValue)
    {
      var requestURL = url;
      bool isQueryParamAdded = false;
      if (url.Contains("?"))
      {
        var urlParts = url.Split('?');
        var urlAbsolutePath = urlParts[0];
        var queryParamString = urlParts[1];
        if (!string.IsNullOrEmpty(queryParamString))
        {
          var queryParamCollection = queryParamString.Split('&').ToList();
          List<string> replacedParamCollection = new List<string>();

          foreach (string queryParam in queryParamCollection)
          {
            var replacedQueryParam = queryParam;

            var paramName = queryParam.Substring(0, queryParam.IndexOf('='));

            if (paramName.Equals(queryParamName))
            {
              replacedQueryParam = queryParamName + "=" + queryParamValue;
              isQueryParamAdded = true;
            }
            replacedParamCollection.Add(replacedQueryParam);
          }
          if (!isQueryParamAdded)
          {
            replacedParamCollection.Add(queryParamName + "=" + queryParamValue);
            isQueryParamAdded = true;
          }
          queryParamString = string.Join("&", replacedParamCollection);
          requestURL = urlAbsolutePath + "?" + queryParamString;
        }
        else
        {
          isQueryParamAdded = true;
          requestURL = requestURL + queryParamName + "=" + queryParamValue;
        }
      }
      if (!isQueryParamAdded)
      {
        requestURL = requestURL + "?" + queryParamName + "=" + queryParamValue;
      }
      return requestURL;
    }
  }
}
