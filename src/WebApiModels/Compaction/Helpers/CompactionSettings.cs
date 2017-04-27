using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes;
using VSS.Raptor.Service.WebApiModels.Report.Models;
using ColorValue = VSS.Raptor.Service.Common.Models.ColorValue;

namespace VSS.Raptor.Service.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Default settings for compaction end points. For consistency all compaction end points should use these settings.
  /// They should be passed to Raptor for tiles and for retrieving data and also returned to the client UI (albeit in a simplfied form).
  /// </summary>
  public static class CompactionSettings
  {
    public static LiftBuildSettings CompactionLiftBuildSettings
    {
      get
      {
        try
        {
          return JsonConvert.DeserializeObject<LiftBuildSettings>(
            "{'liftDetectionType': '4', 'machineSpeedTarget': { 'MinTargetMachineSpeed': '333', 'MaxTargetMachineSpeed': '417'}}");
          //liftDetectionType 4 = None, speeds are cm/sec (12 - 15 km/hr)        
        }
        catch (Exception ex)
        {
          throw new ServiceException(HttpStatusCode.InternalServerError,
              new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              ex.Message));
        }
      }
    }

    public static Filter CompactionDateFilter(DateTime? startUtc, DateTime? endUtc)
    { 
      Filter filter;
      try
      {
        filter = !startUtc.HasValue && !endUtc.HasValue
          ? null
          : JsonConvert.DeserializeObject<Filter>(string.Format("{{'startUTC': '{0}', 'endUTC': '{1}'}}", startUtc, endUtc));
      }
      catch (Exception ex)
      {
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            ex.Message));
      }
      return filter;
    }

    public static Filter CompactionTileFilter(DateTime? startUtc, DateTime? endUtc, long? onMachineDesignId, bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, List<MachineDetails> machines)
    {
      var layerMethod = layerNumber.HasValue ? FilterLayerMethod.TagfileLayerNumber : FilterLayerMethod.None;

      return Filter.CreateFilter(null, null, null, startUtc, endUtc, onMachineDesignId, null, vibeStateOn, null, elevationType,
         null, null, null, null, null, null, null, null, null, layerMethod, null, null, layerNumber, null, machines, 
         null, null, null, null, null, null, null);
    }

    public static CMVSettings CompactionCmvSettings
    {
      get
      {
        return CMVSettings.CreateCMVSettings(70, 100, 120, 20, 80, false);
      }
    }

    public static double[] CompactionCmvPercentChangeSettings
    {
      get
      {
        return new double[] { 5, 20, 50, NO_CCV };
      }
    }

    public static PassCountSettings CompactionPassCountSettings
    {
      get
      {
        return PassCountSettings.CreatePassCountSettings(new int[] {1,2,3,4,5,6,7,8,9});
      }
    }

    public static List<ColorPalette> CompactionPalette(DisplayMode mode)
    {
      /*
      List<ColorValue> palette = new List<ColorValue>();
      switch (mode)
      {
        case DisplayMode.Height:
          colorValues = new List<ColorValue>();
          for (int i = 1; i < raptorPalette.Length - 1; i++)
          {
            colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
          }
          elevationPalette = DetailPalette.CreateDetailPalette(colorValues, raptorPalette[raptorPalette.Length - 1].Colour, raptorPalette[0].Colour);
          break;
        case DisplayMode.CCV:
          colorValues = new List<ColorValue>();
          for (int i = 0; i < raptorPalette.Length; i++)
          {
            colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
          }
          //above hardcoded in Raptor to RGB 128,128,128. No below required as minimum is 0.
          cmvDetailPalette = DetailPalette.CreateDetailPalette(colorValues, Colors.Gray, null);
          break;
        case DisplayMode.PassCount:
          colorValues = new List<ColorValue>();
          for (int i = 0; i < raptorPalette.Length - 1; i++)
          {
            colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
          }
          passCountDetailPalette = DetailPalette.CreateDetailPalette(colorValues, raptorPalette[raptorPalette.Length - 1].Colour, null);
          break;
        case DisplayMode.PassCountSummary:
          passCountSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[2].Colour, raptorPalette[1].Colour, raptorPalette[0].Colour);
          break;
        case DisplayMode.CutFill:
          colorValues = new List<ColorValue>();
          for (int i = raptorPalette.Length - 1; i >= 0; i--)
          {
            colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
          }
          cutFillPalette = DetailPalette.CreateDetailPalette(colorValues, null, null);
          break;
        case DisplayMode.TemperatureSummary:
          temperatureSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[2].Colour, raptorPalette[1].Colour, raptorPalette[0].Colour);
          break;
        case DisplayMode.CCVPercentSummary:
          cmvSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[3].Colour, raptorPalette[0].Colour, raptorPalette[2].Colour);
          break;
        case DisplayMode.MDPPercentSummary:
          mdpSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[3].Colour, raptorPalette[0].Colour, raptorPalette[2].Colour);
          break;
        case DisplayMode.TargetSpeedSummary:
          speedSummaryPalette = SummaryPalette.CreateSummaryPalette(raptorPalette[2].Colour, raptorPalette[1].Colour, raptorPalette[0].Colour);
          break;
        case DisplayMode.CMVChange:
          colorValues = new List<ColorValue>();
          for (int i = 1; i < raptorPalette.Length - 1; i++)
          {
            colorValues.Add(ColorValue.CreateColorValue(raptorPalette[i].Colour, raptorPalette[i].Value));
          }
          cmvPercentChangePalette = DetailPalette.CreateDetailPalette(colorValues, raptorPalette[raptorPalette.Length - 1].Colour, raptorPalette[0].Colour);
          break;
      }
      return palette;
      */
      return null;
    }

    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;

  }
}
