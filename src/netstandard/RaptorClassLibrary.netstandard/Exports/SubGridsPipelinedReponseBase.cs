using VSS.TRex.Types;

namespace VSS.TRex.Exports
{
  /// <summary>
  /// A base class representing the generic result of requesting subgrids
  /// </summary>
    public class SubGridsPipelinedReponseBase
    {
      /// <summary>
      /// The error status result from the pipeline execution
      /// </summary>
      public RequestErrorStatus ResultStatus { get; set; }

  }
}
