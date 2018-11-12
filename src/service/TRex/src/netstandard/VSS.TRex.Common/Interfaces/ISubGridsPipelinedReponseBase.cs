using VSS.TRex.Types;

namespace VSS.TRex.Common.Interfaces
{
  public interface ISubGridsPipelinedReponseBase
  {
    /// <summary>
    /// The error status result from the pipeline execution
    /// </summary>
    RequestErrorStatus ResultStatus { get; set; }
  }
}
