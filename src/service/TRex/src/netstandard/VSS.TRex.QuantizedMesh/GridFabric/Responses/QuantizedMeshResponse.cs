using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;
//using VSS.TRex.QuantizedMesh.Abstractions.GridFabric.Responses;

namespace VSS.TRex.QuantizedMesh.GridFabric.Responses
{
  /// <summary>
  /// Contains the response quantized mesh for a tile request. 
  /// </summary>
  public class QuantizedMeshResponse : SubGridsPipelinedResponseBase, IQuantizedMeshResponse, IAggregateWith<IQuantizedMeshResponse>
  {

    public virtual IQuantizedMeshResponse AggregateWith(IQuantizedMeshResponse other)
    {
      // Composite the quantized mesh tile  held in this response with the QM held in 'other'
      // ....

      return null;
    }

    public virtual void SetQMTile(byte[] qmTile)
    {
      // No implementation in base class
    }
  }
}
