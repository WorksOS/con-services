using MasterDataProxies.ResultHandling;

namespace WebApiModels.FileAccess.ResultHandling
{
  /// <summary>
  /// The result representation of a raw file access request.
  /// </summary>
  public class RawFileAccessResult : ContractExecutionResult
  {
    public byte[] fileContents { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private RawFileAccessResult()
    { }

    /// <summary>
    /// Create instance of RawFileAccessResult
    /// </summary>
    public static RawFileAccessResult CreateRawFileAccessResult(byte[] fileContents)
    {
      return new RawFileAccessResult
      {
        fileContents = fileContents
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static RawFileAccessResult HelpSample
    {
      //TODO:
      get
      {
        return new RawFileAccessResult();
      }
    }
  }
}
