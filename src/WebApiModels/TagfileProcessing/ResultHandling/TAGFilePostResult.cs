using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.TagfileProcessing.ResultHandling
{
  /// <summary>
  /// REpresents response from the service after TAG file POST request
  /// </summary>
  public class TagFilePostResult : ContractExecutionResult
  {
    /// <summary>
    /// Private constructor
    /// </summary>
    private TagFilePostResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TagFilePostResult Create()
    {
      return new TagFilePostResult();
    }
  }
}
