using ASNodeDecls;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;

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

    /// <summary>
    /// Create example instance of TileResult to display in Help documentation.
    /// </summary>
    public static TileResult HelpSample
    {
      get
      {
        return new TileResult()
        {
        };
      }
    }
  }
}