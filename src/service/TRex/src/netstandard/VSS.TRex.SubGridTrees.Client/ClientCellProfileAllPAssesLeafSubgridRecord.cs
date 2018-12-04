using System;
using System.IO;
using System.Linq;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  public struct ClientCellProfileAllPassesLeafSubgridRecord : IBinaryReaderWriter, IEquatable<ClientCellProfileAllPassesLeafSubgridRecord>
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
      TotalPasses = reader.ReadInt32();
      CellPasses = new ClientCellProfileLeafSubgridRecord[TotalPasses];

      for (int i = 0; i < TotalPasses; i++)
        CellPasses[i].Read(reader);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(TotalPasses);
      for (int i = 0; i < TotalPasses; i++)
        CellPasses[i].Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    public bool Equals(ClientCellProfileAllPassesLeafSubgridRecord other)
    {
      return TotalPasses == other.TotalPasses && 
             !CellPasses.Where((x, i) => !x.Equals(other.CellPasses[i])).Any();
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is ClientCellProfileAllPassesLeafSubgridRecord other && Equals(other);
    }

    public override int GetHashCode() => base.GetHashCode();
  }
}
