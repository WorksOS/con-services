
namespace VSS.TagFileAuth.Service.WebApiModels.ResultHandling
{
  /// <summary>
  /// The result representation of a tag file processing error request.
  /// </summary>
  public class TagFileProcessingErrorResult : ContractExecutionResult // , IHelpSample
  {
    /// <summary>
    /// The result of the request. True for success and false for failure.
    /// </summary>
    public bool result { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
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