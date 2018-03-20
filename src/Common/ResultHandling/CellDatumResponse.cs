using System;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class CellDatumResponse : ContractExecutionResult
    {
        /// <summary>
        /// THe display mode used in the original request
        /// </summary>
    public DisplayMode displayMode { get; private set; }

        /// <summary>
        /// The internal result code resulting from the request.
        /// Values are: 0 = Value found, 1 = No value found, 2 = Unexpected error
        /// </summary>
    public short returnCode { get; private set; }

        /// <summary>
        /// The value from the request, scaled in accordance with the underlying attribute domain.
        /// </summary>
    public double value { get; private set; }

    /// <summary>
    /// The date and time of the value.
    /// </summary>
    public DateTime timestamp { get; private set; }
    

       /// <summary>
        /// Private constructor
        /// </summary>
        private CellDatumResponse()
        {}


    /// <summary>
    /// Create instance of CellDatumResponse
    /// </summary>
    public static CellDatumResponse CreateCellDatumResponse(
      DisplayMode displayMode,
      short returnCode,
      double value,
      DateTime timestamp
      )
    {
      return new CellDatumResponse
             {
                 displayMode = displayMode,
                 returnCode = returnCode,
                 value = value,
                 timestamp = timestamp
             };
    }
    }
}