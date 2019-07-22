using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
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
using VSS.TRex.Analytics.ElevationStatistics;
using VSS.TRex.Analytics.ElevationStatistics.GridFabric;
using VSS.TRex.Analytics.MDPStatistics;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Analytics.PassCountStatistics;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Analytics.SpeedStatistics;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Analytics.TemperatureStatistics;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;

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
    [HttpPost("cmvdetails/{siteModelID}")]
    public async Task<JsonResult> GetCMVDetails([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
    {
      const int CMV_DENOMINATOR = 10;
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var cmvBands = new[] { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 1000, 1200 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
          var sw = new Stopwatch();
          sw.Start();

          var operation = new CMVStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new CMVStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              CMVDetailValues = cmvBands,
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data CMV Summary.
    /// </summary>
    [HttpPost("cmvsummary/{siteModelID}")]
    public async Task<JsonResult> GetCMVSummary([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new CMVStatisticsOperation();

          var result = await operation.ExecuteAsync(
            new CMVStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet(new CombinedFilter()),
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data CMV Change.
    /// </summary>
    [HttpPost("cmvchange/{siteModelID}")]
    public async Task<JsonResult> GetCMVChange([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new CMVChangeStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new CMVChangeStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              CMVChangeDetailsDataValues = cmvPercentBands,
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data MDP Summary.
    /// </summary>
    [HttpPost("mdpsummary/{siteModelID}")]
    public async Task<JsonResult> GetMDPSummary([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new MDPStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new MDPStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data Pass Count Details.
    /// </summary>
    [HttpPost("passcountdetails/{siteModelID}")]
    public async Task<JsonResult> GetPassCountDetails([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new PassCountStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new PassCountStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              PassCountDetailValues = passCountBands,
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data Pass Count Summary.
    /// </summary>
    [HttpPost("passcountsummary/{siteModelID}")]
    public async Task<JsonResult> GetPassCountSummary([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new PassCountStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new PassCountStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data CCA Summary.
    /// </summary>
    [HttpPost("ccasummary/{siteModelID}")]
    public async Task<JsonResult> GetCCASummary([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new CCAStatisticsOperation();
          var result = await operation.ExecuteAsync(
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data Temperature Details.
    /// </summary>
    [HttpPost("temeraturedetails/{siteModelID}")]
    public async Task<JsonResult> GetTemperatureDetails([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new TemperatureStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new TemperatureStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              TemperatureDetailValues = temperatureBands,
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data Temperature Summary.
    /// </summary>
    [HttpPost("temeraturesummary/{siteModelID}")]
    public async Task<JsonResult> GetTemperatureSummary([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new TemperatureStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new TemperatureStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data Machine Speed Summary.
    /// </summary> 
    [HttpPost("machinespeedsummary/{siteModelID}")]
    public async Task<JsonResult> GetMachineSpeedSummary([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new SpeedStatisticsOperation();
          var result = await operation.ExecuteAsync(
            new SpeedStatisticsArgument()
            {
              ProjectID = siteModel.ID,
              Filters = new FilterSet() { Filters = new[] { new CombinedFilter() } },
              Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data Cut/Fill statistics.
    /// </summary>
    [HttpPost("cutfillstatistics/{siteModelID}")]
    public async Task<JsonResult> GetCutFillStatistics([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides,
      [FromQuery] Guid cutFillDesignUid,
      [FromQuery] double? cutFillOffset)
    {
      string resultToReturn;

      if (!Guid.TryParse(siteModelID, out Guid UID))
        resultToReturn = $"<b>Invalid Site Model UID: {siteModelID}</b>";
      else if (cutFillDesignUid == Guid.Empty)
        resultToReturn = "<b>Missing CutFill Design UID</b>";
      else
      {
        var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(UID, false);
        var offsets = new[] { 0.5, 0.2, 0.1, 0, -0.1, -0.2, -0.5 };

        if (siteModel == null)
          resultToReturn = $"<b>Site model {UID} is unavailable</b>";
        else
        {
                    var sw = new Stopwatch();
          sw.Start();

          var operation = new CutFillStatisticsOperation();
          var result = await operation.ExecuteAsync(new CutFillStatisticsArgument()
          {
            ProjectID = siteModel.ID,
            Filters = new FilterSet { Filters = new[] { new CombinedFilter() } },
            ReferenceDesign = new DesignOffset(cutFillDesignUid, cutFillOffset ?? 0), 
            Offsets = offsets,
            Overrides = overrides
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
      }

      return new JsonResult(resultToReturn);
    }

    /// <summary>
    /// Gets production data elevation statistics.
    /// </summary>
    [HttpPost("elevationrange/{siteModelID}")]
    public async Task<JsonResult> GetElevationRange([FromRoute]string siteModelID, [FromBody] OverrideParameters overrides)
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
          var sw = new Stopwatch();
          sw.Start();

          var operation = new ElevationStatisticsOperation();
          var result = await operation.ExecuteAsync(new ElevationStatisticsArgument()
          {
            ProjectID = siteModel.ID,
            Filters = new FilterSet { Filters = new[] { new CombinedFilter() } },
            Overrides = overrides
          });

          if (result != null)
          {
            var resultString = $"<b>Elevation Statistics Result (in {sw.Elapsed}) :</b><br/>";
            resultString += "<b>================================================</b><br/>";
            resultString += $"<b>Minimum Elevation:</b> {result.MinElevation}<br/>";
            resultString += $"<b>Maximum Elevation:</b> {result.MaxElevation}<br/>";
            resultString += $"<b>Coverage Area:</b> {result.CoverageArea}<br/>";
            resultString += "<b>Bounding Extents:</b><br/>";
            resultString += $"<b>Minimum X:</b> {result.BoundingExtents.MinX}<br/>";
            resultString += $"<b>Minimum Y:</b> {result.BoundingExtents.MinY}<br/>";
            resultString += $"<b>Minimum Z:</b> {result.BoundingExtents.MinZ}<br/>";
            resultString += $"<b>Maximum X:</b> {result.BoundingExtents.MaxX}<br/>";
            resultString += $"<b>Maximum Y:</b> {result.BoundingExtents.MaxY}<br/>";
            resultString += $"<b>Maximum Z:</b> {result.BoundingExtents.MaxZ}<br/>";

            resultToReturn = resultString;
          }
          else
            resultToReturn = "<b>No result</b>";
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
          (DisplayMode.CCVPercentSummary, "CMV Summary"),
          (DisplayMode.CMVChange, "CMV Change"),
          (DisplayMode.MDPPercentSummary, "MDP Summary"),
          (DisplayMode.PassCount, "Pass Count Details"),
          (DisplayMode.PassCountSummary, "Pass Count Summary"),
          (DisplayMode.CCASummary, "CCA Summary"),
          (DisplayMode.TemperatureDetail, "Temperature Details"),
          (DisplayMode.TemperatureSummary, "Temperature Summary"),
          (DisplayMode.TargetSpeedSummary, "Machine Speed Summary"),
          (DisplayMode.CutFill, "Cut/Fill Statistics"),
          ((DisplayMode)Enum.GetNames(typeof(DisplayMode)).Length, "Elevation Range")
        });
    }
  }
}
