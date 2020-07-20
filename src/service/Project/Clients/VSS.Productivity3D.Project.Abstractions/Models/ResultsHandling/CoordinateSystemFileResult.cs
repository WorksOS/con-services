using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class CoordinateSystemFileResult : ContractExecutionResult
  {
    public CoordinateSystemFileResult()
    {
    }

    public CoordinateSystemFileResult(int code, string message) : base(code, message)
    {
    }

    public string FileName { get; set; }
    /// <summary>
    /// Base64 encoded contents
    /// </summary>
    public string Contents { get; set; }
  }
}
