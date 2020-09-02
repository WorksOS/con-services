using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.Designs.Executors;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.GridFabric.ComputeFuncs
{
  public class AlignmentDesignStationRangeComputeFunc : BaseComputeFunc, IComputeFunc<DesignSubGridRequestArgumentBase, AlignmentDesignStationRangeResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AlignmentDesignStationRangeComputeFunc>();

    public AlignmentDesignStationRangeResponse Invoke(DesignSubGridRequestArgumentBase args)
    {
      try
      {
        var executor = new CalculateAlignmentDesignStationRange();

        var stationRange = executor.Execute(DIContext.ObtainRequired<ISiteModels>().GetSiteModel(args.ProjectID), args.ReferenceDesign.DesignID);

        if (stationRange.StartStation == double.MaxValue || stationRange.EndStation == double.MinValue)
          return new AlignmentDesignStationRangeResponse()
          {
            RequestResult = DesignProfilerRequestResult.FailedToCalculateAlignmentStationRange
          };

        return new AlignmentDesignStationRangeResponse
        {
          RequestResult = DesignProfilerRequestResult.OK,
          StartStation = stationRange.Item1,
          EndStation = stationRange.Item2
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, $"Failed to compute alignment design station range. Site Model ID: {args.ProjectID} design ID: {args.ReferenceDesign.DesignID}");
        return new AlignmentDesignStationRangeResponse { RequestResult = DesignProfilerRequestResult.UnknownError };
      }
    }
  }
}
