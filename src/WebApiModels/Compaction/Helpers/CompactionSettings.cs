using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.WebApiModels.Report.Models;

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

    public static Filter CompactionTileFilter(DateTime? startUtc, DateTime? endUtc, long? onMachineDesignId, bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, MachineDetails machine)
    {
      var layerMethod = layerNumber.HasValue ? (FilterLayerMethod?)null : FilterLayerMethod.TagfileLayerNumber;

     return Filter.CreateFilter(null, null, null, startUtc, endUtc, onMachineDesignId, null, vibeStateOn, null, elevationType,
         null, null, null, null, null, null, null, null, null, layerMethod, null, null, layerNumber, null, new List<MachineDetails> {machine}, 
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

    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;

  }
}
