using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.Coord.ResultHandling
{
  /// <summary>
  /// The result representation of a coordinate system definition file validation request.
  /// </summary>
  public class CoordinateSystemValidationResult : ContractExecutionResult
  {
    /// <summary>
    /// Create an instance of the CoordinateSystemValidationResult.
    /// </summary>
    public static CoordinateSystemValidationResult CreateCoordinateSystemValidationResult(bool result)
    {
      return new CoordinateSystemValidationResult
      { 
        result = result
      };
    }
  }
}
