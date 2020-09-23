using System.Collections.Generic;
using System.Linq;

namespace CCSS.WorksOS.Reports.Common.Helpers
{
  public static class DataGrabberHelper
  {
    // todoJeannie could this go in GenericDataGrabber?
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
