using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class CellDatumResponse : ContractExecutionResult
  {
    /// <summary>
    /// THe display mode used in the original request
    /// </summary>
    public DisplayMode displayMode { get; protected set; }

    /// <summary>
    /// The internal result code resulting from the request.
    /// Values are: 0 = Value found, 1 = No value found, 2 = Unexpected error
    /// </summary>
    public short returnCode { get; protected set; }

    /// <summary>
    /// The value from the request, scaled in accordance with the underlying attribute domain.
    /// </summary>
    public double? value { get; protected set; }

    /// <summary>
    /// The date and time of the value.
    /// </summary>
    public DateTime timestamp { get; protected set; }

    public bool ShouldSerializevalue() => (returnCode == 0) && value.HasValue;

    /// <summary>
    /// Create instance of CellDatumResponse
    /// </summary>
    public CellDatumResponse(
      DisplayMode displayMode,
      short returnCode,
      double? value,
      DateTime timestamp)
    {
      this.displayMode = displayMode;
      this.returnCode = returnCode;
      this.value = displayMode == DisplayMode.CCV || displayMode == DisplayMode.MDP ? value / 10 : value;
      this.timestamp = timestamp;
    }
  }
}
