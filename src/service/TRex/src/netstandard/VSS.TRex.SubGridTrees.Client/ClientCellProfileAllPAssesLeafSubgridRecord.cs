using System;
using System.IO;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  public struct ClientCellProfileAllPAssesLeafSubgridRecord : IBinaryReaderWriter
  {
    public int TotalPasses;

    public ClientCellProfileLeafSubgridRecord[] CellPasses;

    public void Clear()
    {
      TotalPasses = 0;
      CellPasses = new ClientCellProfileLeafSubgridRecord[0];
    }

    public void Read(BinaryReader reader)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);
  }
}
