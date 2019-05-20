using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class CalculateDesignElevationPatchResponse : BaseRequestResponse 
  {
    public DesignProfilerRequestResult CalcResult { get; set; } = DesignProfilerRequestResult.UnknownError;

    /// <summary>
    /// The patch of elevations (a ClientHeightLeafSubGrid instance)
    /// </summary>
    public IClientHeightLeafSubGrid Heights { get; set; } = new ClientHeightLeafSubGrid();

    public override void FromBinary(IBinaryRawReader reader)
    {
      // Transient message so no versioning...
      CalcResult = (DesignProfilerRequestResult) reader.ReadByte();

      if (reader.ReadBoolean())
      {
        Heights = new ClientHeightLeafSubGrid();
        Heights.FromBytes(reader.ReadByteArray());
      }
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      // Transient message so no versioning...
      writer.WriteByte((byte)CalcResult);
      writer.WriteBoolean(Heights != null);
      if (Heights != null)
        writer.WriteByteArray(Heights.ToBytes());
    }
  }
}
