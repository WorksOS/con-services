using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/productiondata")]
  public class ProductionDataController : ControllerBase
  {
    /// <summary>
    /// Gets production data CMV Details.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("cmvdetails/{siteModelID}")]
    public JsonResult GetCMVDetails(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var cmvBands = new[] { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            CMVStatisticsOperation operation = new CMVStatisticsOperation();
            CMVStatisticsResult result = operation.Execute(
              new CMVStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                CMVDetailValues = cmvBands
              }
            );

            if (result != null)
            {
              string resultString = $"<b>CMV Details Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";

              for (int i = 0; i < cmvBands.Length; i++)
                resultString += $"{cmvBands[i] / 10} - {result.Percents[i]:##0.#0}%<br/>";

              resultToReturn = resultString;
            }
            else
              resultToReturn = "<b>No result</b>";
          }
          finally
          {
            sw.Stop();
          }
        }
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Retrieves the list of available production data request types.
    /// </summary>
    /// <returns></returns>
    [HttpGet("requesttypes")]
    public JsonResult GetRequestTypes()
    {
      return new JsonResult(new List<(DisplayMode id, string name)>
        {
          (DisplayMode.CCV, "CMV Details"),
          (DisplayMode.CCVSummary, "CMV Summary"),
          (DisplayMode.CMVChange, "CMV Change"),
          (DisplayMode.MDPSummary, "MDP Summary"),
          (DisplayMode.PassCount, "Pass Count Details"),
          (DisplayMode.PassCountSummary, "Pass Count Summary"),
          (DisplayMode.CCASummary, "CCA Summary"),
          (DisplayMode.TemperatureDetail, "Temperature Details"),
          (DisplayMode.TemperatureSummary, "Temperature Summary"),
          (DisplayMode.TargetSpeedSummary, "Machine Speed Summary"),
          (DisplayMode.CutFill, "Cut/Fill Statistics"),
        });
    }
  }
}
