using System.Collections.Generic;
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
    public List<XYZS> ProfileLine { get; private set; }

    public bool HasData() => (ProfileLine?.Count ?? 0) > 0;

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static DesignProfileResult Create(List<XYZS> profileLine)
    {
      return new DesignProfileResult
      {
        ProfileLine = profileLine
      };
    }
  }
}
