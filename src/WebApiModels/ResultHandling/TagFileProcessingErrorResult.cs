
namespace WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a tag file processing error request.
  /// </summary>
  public class TagFileProcessingErrorResult : ContractExecutionResult 
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; set; }

    // acceptance tests cannot serialize with a private const.
    //private TagFileProcessingErrorResult()
    //{ }

    /// <summary>
    /// Create instance of TagFileProcessingErrorResult
    /// </summary>
    public static TagFileProcessingErrorResult CreateTagFileProcessingErrorResult(bool result)
    {
      return new TagFileProcessingErrorResult
      {
        result = result
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static TagFileProcessingErrorResult HelpSample
    {
      get { return CreateTagFileProcessingErrorResult(true); }
    }
  }
}