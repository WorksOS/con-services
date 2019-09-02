using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Productivity3D.Models
{
  /// <summary>
  /// This is the ContractExecutionResult from Productivity3D in a simplified form for proxies and mocks.
  /// </summary>
  public class BaseMasterDataResult : IMasterDataModel
  {
    /// <summary>
    ///   Defines machine-readable code.
    /// </summary>
    /// <value>
    ///   Result code.
    /// </value>
    public int Code { get; set; }

    /// <summary>
    ///   Defines user-friendly message.
    /// </summary>
    /// <value>
    ///   The message string.
    /// </value>
    public string Message { get; set; }

    public List<string> GetIdentifiers() => new List<string>();
  }
}
