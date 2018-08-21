using VSS.TRex.Types;

namespace VSS.TRex.Common
{
  /// <summary>
  /// A base class representing the generic result of requesting subgrids
  /// </summary>
  public class SubGridsPipelinedReponseBase : ISubGridsPipelinedReponseBase
  {
    /// <summary>
    /// The error status result from the pipeline execution
    /// </summary>
    public RequestErrorStatus ResultStatus { get; set; } = RequestErrorStatus.Unknown;
  }
}
