using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CellDatumResult : ResponseBase
  {
    /// <summary>
    /// THe display mode used in the original request
    /// </summary>
    public DisplayMode displayMode;

    /// <summary>
    /// The internal result code resulting from the request.
    /// </summary>
    public short returnCode;

    /// <summary>
    /// The value from the request, scaled in accordance with the underlying attribute domain.
    /// </summary>
    public double value;

    /// <summary>
    /// The date and time of the value.
    /// </summary>
    public DateTime timestamp { get; set; }

    public CellDatumResult()
        : base("success")
    { }
  }
}
