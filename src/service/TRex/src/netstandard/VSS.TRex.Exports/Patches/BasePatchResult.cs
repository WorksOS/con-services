using System.IO;
using System.Text;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Contains the prepared result for the client to consume
  /// </summary>
  public abstract class BasePatchResult<T> where T : SubgridDataPatchRecordBase
  {
    public int TotalNumberOfPagesToCoverFilteredData;
    public int MaxPatchSize;
    public int PatchNumber;
    public double CellSize;

    public T[] Patch;

    protected abstract void WriteAdditionalInformation(BinaryWriter bw);

    protected abstract void WriteDataPatch(T patch, BinaryWriter bw);

    protected virtual void WritePatchInformation(BinaryWriter bw)
    {
      bw.Write(TotalNumberOfPagesToCoverFilteredData);

      WriteAdditionalInformation(bw);

      bw.Write(Patch?.Length ?? 0);
      bw.Write(CellSize);
    }

    public byte[] ConstructResultData()
    {
      using (var ms = RecyclableMemoryStreamManagerHelper.Manager.GetStream())
      {
        using (var bw = new BinaryWriter(ms, Encoding.UTF8, true))
        {
          WritePatchInformation(bw);

          if (Patch != null)
          {
            foreach (var patch in Patch)
            {
              bw.Write(patch.CellOriginX);
              bw.Write(patch.CellOriginY);
              bw.Write(patch.IsNull);

              if (!patch.IsNull)
              {
                bw.Write(patch.ElevationOrigin);

                WriteDataPatch(patch, bw);
              }
            }
          }

          return ms.ToArray();
        }
      }
    }

  }
}
