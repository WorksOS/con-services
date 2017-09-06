
namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a tag file processing error request.
  /// </summary>
  public class TagFileProcessingErrorResult : ContractExecutionResultWithResult
  {
    /// <summary>
    /// Create instance of TagFileProcessingErrorResult
    /// </summary>
    public static TagFileProcessingErrorResult CreateTagFileProcessingErrorResult(bool result,
      int code = 0,
      int customCode = 0)
    {
      return new TagFileProcessingErrorResult
      {
        Result = result,
        Code = code,
        Message = _contractExecutionStatesEnum.FirstNameWithOffset(customCode)
      };
    }
  }
}