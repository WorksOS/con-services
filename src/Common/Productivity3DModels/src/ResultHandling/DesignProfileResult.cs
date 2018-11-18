using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by Design Profile request
  /// </summary>
  public class DesignProfileResult : ContractExecutionResult
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private DesignProfileResult()
    { }

    /// <summary>
    /// Resulting geometry from a design profile line computation
    /// </summary>
    public XYZS[] ProfileLine { get; private set; }

    public bool HasData() => true;

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static DesignProfileResult Create(XYZS[] profileLine)
    {
      return new DesignProfileResult
      {
        ProfileLine = profileLine
      };
    }
  }
}
