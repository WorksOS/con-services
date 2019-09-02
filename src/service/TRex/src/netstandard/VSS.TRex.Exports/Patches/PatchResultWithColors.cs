using System.Drawing;
using System.IO;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Contains the prepared elevation and color data result for the client to consume.
  /// </summary>
  public class PatchResultWithColors : BasePatchResult<SubgridDataPatchRecord_ElevationAndColor>
  {
    public bool RenderValuesToColours;

    protected override void WriteAdditionalInformation(BinaryWriter bw)
    {
      bw.Write(RenderValuesToColours);
    }

    protected override void WriteDataPatch(SubgridDataPatchRecord_ElevationAndColor patch, BinaryWriter bw)
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        bw.Write(patch.Data[x, y].ElevationOffset);
        if (RenderValuesToColours)
          bw.Write(patch.Data[x, y].Colour.ToArgb());
      });
    }
  }
}
