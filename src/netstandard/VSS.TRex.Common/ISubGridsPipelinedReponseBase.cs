using VSS.TRex.Types;

namespace VSS.TRex.Common
{
  public interface ISubGridsPipelinedReponseBase
  {
    /// <summary>
    /// The error status result from the pipeline execution
    /// </summary>
    RequestErrorStatus ResultStatus { get; set; }
  }
}
