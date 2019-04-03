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
  /// <summary>
  /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
  /// perform subgrid processing.
  /// </summary>
  public class CellDatumRequestComputeFunc_ClusterCompute : BaseComputeFunc, IComputeFuncArgument<CellDatumRequestArgument_ClusterCompute>, IComputeFunc<CellDatumResponse_ClusterCompute>
  {
    private const byte VERSION_NUMBER = 1;

    private static readonly ILogger Log = Logging.Logger.CreateLogger<CellDatumRequestComputeFunc_ClusterCompute>();

    public CellDatumRequestArgument_ClusterCompute Argument { get; set; }

    public CellDatumResponse_ClusterCompute Invoke()
    {
      Log.LogInformation("In CellDatumRequestComputeFunc_ClusterCompute.Invoke()");

      try
      {
        CellDatumComputeFuncExecutor_ClusterCompute request = new CellDatumComputeFuncExecutor_ClusterCompute();

        Log.LogInformation("Executing CellDatumRequestComputeFunc_ClusterCompute.Execute()");

        if (Argument == null)
        {
          throw new ArgumentException("Argument for ComputeFunc must be provided");
        }
        return request.Execute(Argument, new SubGridSpatialAffinityKey(Argument.ProjectID, Argument.OTGCellX, Argument.OTGCellY));
      }
      finally
      {
        Log.LogInformation("Exiting CellDatumRequestComputeFunc_ClusterCompute.Invoke()");
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
        Argument = new CellDatumRequestArgument_ClusterCompute();
        Argument.FromBinary(reader);
      }
    }
  }
}
