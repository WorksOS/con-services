using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.FileAccess.ResultHandling
{
  /// <summary>
  /// The result representation of a file access request.
  /// </summary>
  public class FileAccessResult : ContractExecutionResult
  {
    /// <summary>
    /// Private constructor
    /// </summary>
    private FileAccessResult()
    { }

    /// <summary>
    /// Create instance of FileAccessResult
    /// </summary>
    public static FileAccessResult CreateFileAccessResult()
    {
      return new FileAccessResult();
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static FileAccessResult HelpSample
    {
      get { return new FileAccessResult(); }
    }
  }
}
