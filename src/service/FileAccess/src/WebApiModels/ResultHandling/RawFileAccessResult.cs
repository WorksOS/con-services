using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.FileAccess.WebAPI.Models.ResultHandling
{
  /// <summary>
  /// The result representation of a raw file access request.
  /// </summary>
  public class RawFileAccessResult : ContractExecutionResult
  {
    public byte[] fileContents { get; private set; }
    public bool Success => fileContents?.Length > 0;

    public static RawFileAccessResult Create(byte[] fileContents = null)
    {
      return new RawFileAccessResult
      {
        fileContents = fileContents
      };
    }
  }
}
