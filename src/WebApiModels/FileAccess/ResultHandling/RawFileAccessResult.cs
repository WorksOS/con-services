using VSS.MasterDataProxies.ResultHandling;

namespace VSS.Productivity3D.FileAccess.Service.WebAPI.Models.FileAccess.ResultHandling
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
    public static RawFileAccessResult HelpSample => new RawFileAccessResult();
  }
}