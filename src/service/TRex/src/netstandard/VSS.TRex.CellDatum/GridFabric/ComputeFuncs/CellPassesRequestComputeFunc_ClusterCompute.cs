using System;
using System.Reflection;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.CellDatum.Executors;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Responses;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.CellDatum.GridFabric.ComputeFuncs
{
  public class CellPassesRequestComputeFunc_ClusterCompute : BaseComputeFunc, IComputeFuncArgument<CellPassesRequestArgument_ClusterCompute>, IComputeFunc<CellPassesResponse>
  {
    private const byte VERSION_NUMBER = 1;

    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellPassesRequestComputeFunc_ClusterCompute>();

    public CellPassesRequestArgument_ClusterCompute Argument { get; set; }

    public CellPassesResponse Invoke()
    {
      Log.LogInformation($"In {nameof(CellPassesRequestComputeFunc_ClusterCompute)}.Invoke()");

      try
      {
        var request = new CellPassesComputeFuncExecutor_ClusterCompute(); 

        Log.LogInformation($"Executing {nameof(CellPassesRequestComputeFunc_ClusterCompute)}.Execute()");

        if (Argument == null)
        {
          throw new ArgumentException("Argument for ComputeFunc must be provided");
        }
        return request.Execute(Argument, new SubGridSpatialAffinityKey(SubGridSpatialAffinityKey.DEFAULT_SPATIAL_AFFINITY_VERSION_NUMBER_TICKS, Argument.ProjectID, Argument.OTGCellX, Argument.OTGCellY));
      }
      finally
      {
        Log.LogInformation($"Exiting {nameof(CellPassesRequestComputeFunc_ClusterCompute)}.Invoke()");
      }
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
      writer.WriteBoolean(Argument != null);
      Argument?.ToBinary(writer);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
      if (reader.ReadBoolean())
      {
        Argument = new CellPassesRequestArgument_ClusterCompute();
        Argument.FromBinary(reader);
      }
    }
  }
}
