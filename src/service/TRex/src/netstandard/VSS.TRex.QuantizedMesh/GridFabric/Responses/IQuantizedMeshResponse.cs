using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.QuantizedMesh.GridFabric.Responses
{
  public interface IQuantizedMeshResponse
  {
    void SetQMTile(byte[] qmTile);
  }
}
