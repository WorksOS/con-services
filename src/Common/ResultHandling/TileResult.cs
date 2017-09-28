using ASNodeDecls;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class TileResult : ContractExecutionResult
  {
    /// <summary>
    /// Private constructor
    /// </summary>
    private TileResult()
    {}


    public byte[] TileData { get; private set; }
    public bool TileOutsideProjectExtents { get; private set; }

    /// <summary>
    /// Create instance of TileResult
    /// </summary>
    public static TileResult CreateTileResult(byte[] data, TASNodeErrorStatus raptorResult)
    {
      return new TileResult
      {
        TileData = data,
        TileOutsideProjectExtents = raptorResult!=TASNodeErrorStatus.asneOK
      };
    }
  }
}