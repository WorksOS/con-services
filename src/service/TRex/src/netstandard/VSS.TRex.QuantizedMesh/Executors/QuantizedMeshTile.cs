using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.QuantizedMesh.Abstractions;

namespace VSS.TRex.QuantizedMesh.Executors
{
  public class QuantizedMeshTile : IQuantizedMeshTile
  {
    private readonly byte[] container;

    public QuantizedMeshTile()
    {
      // dummy tile for now
      container = new byte[] { 0x41, 0x42, 0x41, 0x42, 0x41, 0x42, 0x41 };
    }

    public byte[] GetQMTile()
    {
      // todo implement make tile
      return container;
    }
  }


}
