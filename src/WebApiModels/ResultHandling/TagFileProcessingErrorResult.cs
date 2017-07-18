
namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a tag file processing error request.
  /// </summary>
  public class TagFileProcessingErrorResult : ContractExecutionResult
  {
    /// <summary>
    /// Create instance of TagFileProcessingErrorResult
    /// </summary>
    public static TagFileProcessingErrorResult CreateTagFileProcessingErrorResult(bool result,
      ContractExecutionStatesEnum code = ContractExecutionStatesEnum.ExecutedSuccessfully,
      string message = "success")
    {
      return new TagFileProcessingErrorResult
      {
        Result = result,
        Code = code,
        Message = message
      };
    }
  }
}