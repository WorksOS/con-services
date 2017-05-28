
using ASNodeDecls;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.Common.ResultHandling
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