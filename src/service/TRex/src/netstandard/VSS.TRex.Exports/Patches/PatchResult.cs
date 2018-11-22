using System.IO;
using System.Text;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public class PatchResult
  {
    public int TotalNumberOfPagesToCoverFilteredData;
    public int MaxPatchSize;
    public int PatchNumber;
    public double CellSize;

    public SubgridDataPatchRecord_ElevationAndTime[] Patch;

    public byte[] ConstructResultData()
    {
      var ms = new MemoryStream();

      var bw = new BinaryWriter(ms, Encoding.UTF8, true);

      bw.Write(TotalNumberOfPagesToCoverFilteredData);
      bw.Write(Patch?.Length ?? 0);
      bw.Write(CellSize);

      if (Patch == null)
        return ms.ToArray();

      foreach (var patch in Patch)
      {
        bw.Write(patch.CellOriginX);
        bw.Write(patch.CellOriginY);
        bw.Write(patch.IsNull);

        if (patch.IsNull)
          continue;

        bw.Write(patch.ElevationOrigin);
        bw.Write(patch.ElevationOffsetSize);
        bw.Write(patch.TimeOrigin);
        bw.Write(patch.TimeOffsetSize);

        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          switch (patch.ElevationOffsetSize)
          {
            case 1: bw.Write((byte)(patch.Data[x, y].ElevationOffset & 0xFF)); break;
            case 2: bw.Write((ushort)(patch.Data[x, y].ElevationOffset & 0xFFFF)); break;
            case 4: bw.Write((uint)(patch.Data[x, y].ElevationOffset & 0xFFFFFFFF)); break;
            default: throw new System.ArgumentException("Unknown bytes size for elevation offset");
          }

          switch (patch.TimeOffsetSize)
          {
            case 1: bw.Write((byte)(patch.Data[x, y].TimeOffset & 0xFF)); break;
            case 2: bw.Write((ushort)(patch.Data[x, y].TimeOffset & 0xFFFF)); break;
            case 4: bw.Write((uint)(patch.Data[x, y].TimeOffset & 0xFFFFFFFF)); break;
            default: throw new System.ArgumentException("Unknown bytes size for time offset");
          }
        });
      }

      return ms.ToArray();
    }
  }
}
