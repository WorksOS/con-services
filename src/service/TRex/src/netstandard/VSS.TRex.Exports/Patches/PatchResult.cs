using System.IO;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Contains the prepared elevation and time data patch result for the client to consume.
  /// </summary>
  public class PatchResult : BasePatchResult<SubgridDataPatchRecord_ElevationAndTime>
  {
    protected override void WriteDataPatch(SubgridDataPatchRecord_ElevationAndTime patch, BinaryWriter bw)
    {
      bw.Write(patch.ElevationOffsetSize);
      bw.Write(patch.TimeOrigin);
      bw.Write(patch.TimeOffsetSize);

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        switch (patch.ElevationOffsetSize)
        {
          case 1:
            bw.Write((byte)(patch.Data[x, y].ElevationOffset & 0xFF));
            break;
          case 2:
            bw.Write((ushort)(patch.Data[x, y].ElevationOffset & 0xFFFF));
            break;
          case 4:
            bw.Write((uint)(patch.Data[x, y].ElevationOffset & 0xFFFFFFFF));
            break;
          default: throw new System.ArgumentException("Unknown bytes size for elevation offset");
        }

        switch (patch.TimeOffsetSize)
        {
          case 1:
            bw.Write((byte)(patch.Data[x, y].TimeOffset & 0xFF));
            break;
          case 2:
            bw.Write((ushort)(patch.Data[x, y].TimeOffset & 0xFFFF));
            break;
          case 4:
            bw.Write((uint)(patch.Data[x, y].TimeOffset & 0xFFFFFFFF));
            break;
          default: throw new System.ArgumentException("Unknown bytes size for time offset");
        }
      });
    }

    protected override void WriteAdditionalInformation(BinaryWriter bw)
    {
      // Nothing to implement here...
    }
  }
}
