using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Analytics.CCAStatistics;
using VSS.TRex.Analytics.CCAStatistics.GridFabric;
using VSS.TRex.Analytics.CMVChangeStatistics;
using VSS.TRex.Analytics.CMVChangeStatistics.GridFabric;
using VSS.TRex.Analytics.CMVStatistics;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Analytics.CutFillStatistics;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/productiondata")]
  public class ProductionDataController : ControllerBase
  {
    private const int TWO_DIGITS_NUMBER = 10;
    private const int THREE_DIGITS_NUMBER = 100;

    /// <summary>
    /// Gets production data CMV Details.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("cmvdetails/{siteModelID}")]
    public JsonResult GetCMVDetails(string siteModelID)
    {
      const int CMV_DENOMINATOR = 10;
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var cmvBands = new[] {50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 1000, 1200};

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
                Filters = new FilterSet() {Filters = new[] {new CombinedFilter()}},
                CMVDetailValues = cmvBands
              }
            );

            if (result != null)
            {
              string resultString = $"<b>CMV Details Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";

              var anyTwoDigitsNumber = cmvBands.ToList().Find(s => (s / CMV_DENOMINATOR) >= TWO_DIGITS_NUMBER);
              var anyThreeDigitsNumber = cmvBands.ToList().Find(s => (s / CMV_DENOMINATOR) >= THREE_DIGITS_NUMBER);

              for (int i = 0; i < cmvBands.Length; i++)
              {
                string space = anyThreeDigitsNumber > 0 && cmvBands[i] / CMV_DENOMINATOR < THREE_DIGITS_NUMBER ? "&nbsp;&nbsp;" : string.Empty;

                if (anyTwoDigitsNumber > 0 && cmvBands[i] / CMV_DENOMINATOR < TWO_DIGITS_NUMBER)
                  space += "&nbsp;&nbsp;";

                resultString += $"<b>{space}{cmvBands[i] / CMV_DENOMINATOR}</b> - {result.Percents[i]:##0.#0}%<br/>";
              }

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
    /// Gets production data CMV Summary.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("cmvsummary/{siteModelID}")]
    public JsonResult GetCMVSummary(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

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
                Filters = new FilterSet(new CombinedFilter()),
                CMVPercentageRange = new CMVRangePercentageRecord(80, 120),
                OverrideMachineCMV = false,
                OverridingMachineCMV = 50
              }
            );

            if (result != null)
            {
              string resultString = $"<b>CMV Summary Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";
              resultString += $"<b>Above CMV Percentage:</b> {result.AboveTargetPercent}<br/>";
              resultString += $"<b>Within CMV Percentage Range:</b> {result.WithinTargetPercent}<br/>";
              resultString += $"<b>Below CMV Percentage:</b> {result.BelowTargetPercent}<br/>";
              resultString += $"<b>Total Area Covered in Sq Meters:</b> {result.TotalAreaCoveredSqMeters}<br/>";

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
    /// Gets production data CMV Change.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("cmvchange/{siteModelID}")]
    public JsonResult GetCMVChange(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var cmvPercentBands = new double[] { -50.0, -20.0, -10.0, -5.0, 0.0, 5.0, 10.0, 20.0, 50.0 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            CMVChangeStatisticsOperation operation = new CMVChangeStatisticsOperation();
            CMVChangeStatisticsResult result = operation.Execute(
              new CMVChangeStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                CMVChangeDetailsDatalValues = cmvPercentBands
              }
            );

            if (result != null)
            {
              string resultString = $"<b>CMV Percent Change Details Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";

              var anyNegativeNumber = cmvPercentBands.ToList().Find(s => s < 0);
              var anyTwoDigitsNumber = cmvPercentBands.ToList().Find(s => Math.Abs(s) >= TWO_DIGITS_NUMBER);

              for (int i = 0; i < cmvPercentBands.Length; i++)
              {
                string space = anyNegativeNumber < 0 && cmvPercentBands[i] >= 0 ? "&nbsp;&nbsp;" : string.Empty;

                if (Math.Abs(anyTwoDigitsNumber) > 0 && Math.Abs(cmvPercentBands[i]) >= 0 && Math.Abs(cmvPercentBands[i]) < TWO_DIGITS_NUMBER)
                  space += "&nbsp;&nbsp;";

                resultString += $"<b>{space}{cmvPercentBands[i]:##0.#0}%</b> - {result.Percents[i]:##0.#0}%<br/>";
              }

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
    /// Gets production data MDP Summary.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("mdpsummary/{siteModelID}")]
    public JsonResult GetMDPSummary(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            MDPStatisticsOperation operation = new MDPStatisticsOperation();
            MDPStatisticsResult result = operation.Execute(
              new MDPStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                MDPPercentageRange = new MDPRangePercentageRecord(80, 120),
                OverrideMachineMDP = false,
                OverridingMachineMDP = 1000
              }
            );

            if (result != null)
            {
              string resultString = $"<b>MDP Summary Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";
              resultString += $"<b>Above MDP Percentage:</b> {result.AboveTargetPercent}<br/>";
              resultString += $"<b>Within MDP Percentage Range:</b> {result.WithinTargetPercent}<br/>";
              resultString += $"<b>Below MDP Percentage:</b> {result.BelowTargetPercent}<br/>";
              resultString += $"<b>Total Area Covered in Sq Meters:</b> {result.TotalAreaCoveredSqMeters}<br/>";

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
    /// Gets production data Pass Count Details.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("passcountdetails/{siteModelID}")]
    public JsonResult GetPassCountDetails(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var passCountBands = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            PassCountStatisticsOperation operation = new PassCountStatisticsOperation();
            PassCountStatisticsResult result = operation.Execute(
              new PassCountStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                PassCountDetailValues = passCountBands
              }
            );

            if (result != null)
            {
              string resultString = $"<b>Pass Count Details Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";

              var anyTwoDigitsNumber = passCountBands.ToList().Find(s => s >= TWO_DIGITS_NUMBER);
              for (int i = 0; i < passCountBands.Length; i++)
              {
                string space = anyTwoDigitsNumber > 0 && passCountBands[i] < TWO_DIGITS_NUMBER ? "&nbsp;&nbsp;" : string.Empty;
                resultString += $"<b>{space}{passCountBands[i]}</b> - {result.Percents[i]:##0.#0}%<br/>";
              }

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
    /// Gets production data Pass Count Summary.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("passcountsummary/{siteModelID}")]
    public JsonResult GetPassCountSummary(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            PassCountStatisticsOperation operation = new PassCountStatisticsOperation();
            PassCountStatisticsResult result = operation.Execute(
              new PassCountStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                OverridingTargetPassCountRange = new PassCountRangeRecord(3, 10),
                OverrideTargetPassCount = false
              }
            );

            if (result != null)
            {
              string resultString = $"<b>Pass Count Summary Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";
              resultString += $"<b>Above Pass Count Percentage:</b> {result.AboveTargetPercent}<br/>";
              resultString += $"<b>Within Pass Count Percentage Range:</b> {result.WithinTargetPercent}<br/>";
              resultString += $"<b>Below Pass Count Percentage:</b> {result.BelowTargetPercent}<br/>";
              resultString += $"<b>Total Area Covered in Sq Meters:</b> {result.TotalAreaCoveredSqMeters}<br/>";

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
    /// Gets production data CCA Summary.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("ccasummary/{siteModelID}")]
    public JsonResult GetCCASummary(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            CCAStatisticsOperation operation = new CCAStatisticsOperation();
            CCAStatisticsResult result = operation.Execute(
              new CCAStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } }
              }
            );

            if (result != null)
            {
              string resultString = $"<b>CCA Summary Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";
              resultString += $"<b>Above CCA Percentage:</b> {result.AboveTargetPercent}<br/>";
              resultString += $"<b>Within CCA Percentage Range:</b> {result.WithinTargetPercent}<br/>";
              resultString += $"<b>Below CCA Percentage:</b> {result.BelowTargetPercent}<br/>";
              resultString += $"<b>Total Area Covered in Sq Meters:</b> {result.TotalAreaCoveredSqMeters}<br/>";

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
    /// Gets production data Temperature Details.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("temeraturedetails/{siteModelID}")]
    public JsonResult GetTemperatureDetails(string siteModelID)
    {
      const int TEMP_DENOMINATOR = 10;
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var temperatureBands = new[] { 0, 120, 140, 160, 4000 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            TemperatureStatisticsOperation operation = new TemperatureStatisticsOperation();
            TemperatureStatisticsResult result = operation.Execute(
              new TemperatureStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                TemperatureDetailValues = temperatureBands
              }
            );

            if (result != null)
            {
              string resultString = $"<b>Temperature Details Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";

              var anyTwoDigitsNumber = temperatureBands.ToList().Find(s => (s / TEMP_DENOMINATOR) >= TWO_DIGITS_NUMBER && (s / TEMP_DENOMINATOR) < THREE_DIGITS_NUMBER);
              var anyThreeDigitsNumber = temperatureBands.ToList().Find(s => (s / TEMP_DENOMINATOR) >= THREE_DIGITS_NUMBER);

              for (int i = 0; i < temperatureBands.Length; i++)
              {
                string space = anyThreeDigitsNumber > 0 && temperatureBands[i] / TEMP_DENOMINATOR < THREE_DIGITS_NUMBER ? "&nbsp;&nbsp;" : string.Empty;

                if (anyTwoDigitsNumber > 0 && temperatureBands[i] / TEMP_DENOMINATOR < TWO_DIGITS_NUMBER)
                  space += "&nbsp;&nbsp;";

                resultString += $"<b>{space}{(temperatureBands[i] / TEMP_DENOMINATOR):##0.0}</b> - {result.Percents[i]:##0.#0}%<br/>";
              }

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
    /// Gets production data Temperature Summary.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("temeraturesummary/{siteModelID}")]
    public JsonResult GetTemperartureSummary(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            TemperatureStatisticsOperation operation = new TemperatureStatisticsOperation();
            TemperatureStatisticsResult result = operation.Execute(
              new TemperatureStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                OverrideTemperatureWarningLevels = true,
                OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(10, 150)
              }
            );

            if (result != null)
            {
              string resultString = $"<b>Temperature Summary Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";
              resultString += $"<b>Above Temperature Percentage:</b> {result.AboveTargetPercent}<br/>";
              resultString += $"<b>Within Temperature Percentage Range:</b> {result.WithinTargetPercent}<br/>";
              resultString += $"<b>Below Temperature Percentage:</b> {result.BelowTargetPercent}<br/>";
              resultString += $"<b>Total Area Covered in Sq Meters:</b> {result.TotalAreaCoveredSqMeters}<br/>";

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
    /// Gets production data Machine Speed Summary.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("machinespeedsummary/{siteModelID}")]
    public JsonResult GetMachineSpeedSummary(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            SpeedStatisticsOperation operation = new SpeedStatisticsOperation();
            SpeedStatisticsResult result = operation.Execute(
              new SpeedStatisticsArgument()
              {
                ProjectID = siteModel.ID,
                Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
                TargetMachineSpeed = new MachineSpeedExtendedRecord(5, 50)
              }
            );

            if (result != null)
            {
              string resultString = $"<b>Machine Speed Summary Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";
              resultString += $"<b>Above Machine Speed Percentage:</b> {result.AboveTargetPercent}<br/>";
              resultString += $"<b>Within Machine Speed Percentage Range:</b> {result.WithinTargetPercent}<br/>";
              resultString += $"<b>Below Machine Speed Percentage:</b> {result.BelowTargetPercent}<br/>";
              resultString += $"<b>Total Area Covered in Sq Meters:</b> {result.TotalAreaCoveredSqMeters}<br/>";

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
    /// Gets production data Cut/Fill statistics.
    /// </summary>
    /// <param name="siteModelID">Grid to return the data from.</param>
    /// <returns></returns>
    [HttpGet("cutfillstatistics/{siteModelID}")]
    public JsonResult GetCutFillStatistics(string siteModelID)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var offsets = new[] { 0.5, 0.2, 0.1, 0, -0.1, -0.2, -0.5 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          Stopwatch sw = new Stopwatch();
          sw.Start();
          try
          {
            CutFillStatisticsOperation operation = new CutFillStatisticsOperation();
            CutFillStatisticsResult result = operation.Execute(new CutFillStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet { Filters = new[] { new CombinedFilter() } },
              DesignID = Guid.Empty, // TODO (cmbDesigns.Items.Count == 0) ? Guid.Empty : (cmbDesigns.SelectedValue as Design).ID,
              Offsets = offsets
            });

            if (result != null)
            {
              string resultString = $"<b>Cut/Fill statistics Results (in {sw.Elapsed}) :</b><br/>";
              resultString += "<b>================================================</b><br/>";

              var anyNegativeNumber = offsets.ToList().Find(s => s < 0);
              var anyTwoDigitsNumber = offsets.ToList().Find(s => Math.Abs(s) >= TWO_DIGITS_NUMBER);

              for (int i = 0; i < offsets.Length; i++)
              {
                string space = anyNegativeNumber < 0 && offsets[i] >= 0 ? "&nbsp;&nbsp;" : string.Empty;

                if (Math.Abs(anyTwoDigitsNumber) > 0 && Math.Abs(offsets[i]) >= 0 && Math.Abs(offsets[i]) < TWO_DIGITS_NUMBER)
                  space += "&nbsp;&nbsp;";

                resultString += $"<b>{space}{offsets[i]:##0.0}</b> - {result.Percents[i]:##0.#0}%<br/>";
                }

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
