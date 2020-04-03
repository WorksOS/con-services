using System;
using VSS.TRex.Common.Models;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids.Interfaces
{
  public interface ISubGridsRequestArgument
  {
    /// <summary>
    /// The request ID for the sub grid request
    /// </summary>
    Guid RequestID { get; set; } 

    /// <summary>
    /// The grid data type to extract from the processed sub grids
    /// </summary>
    GridDataType GridDataType { get; set; } 

    /// <summary>
    /// The serialized contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all sub grids that need to be requested for production data
    /// </summary>
    byte[] ProdDataMaskBytes { get; set; }

    /// <summary>
    /// The serialized contents of the SubGridTreeSubGridExistenceBitMask that notes the address of all sub grids that need to be requested for surveyed surface data ONLY
    /// </summary>
    byte[] SurveyedSurfaceOnlyMaskBytes { get; set; }

    /// <summary>
    /// Denotes whether results of these requests should include any surveyed surfaces in the site model
    /// </summary>
    bool IncludeSurveyedSurfaceInformation { get; set; }

    AreaControlSet AreaControlSet { get; set; } 
  }
}
