
namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a tag file processing error request.
  /// </summary>
  public class TagFileProcessingErrorResult : ContractExecutionResult 
  {    
    /// <summary>
    /// Create instance of TagFileProcessingErrorResult
    /// </summary>
    public static TagFileProcessingErrorResult CreateTagFileProcessingErrorResult(bool result)
    {
      return new TagFileProcessingErrorResult
      {
        Result = result
      };
    }    
  }
}