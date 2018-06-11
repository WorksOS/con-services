namespace VSS.TRex.Types
{
  public enum MissingTargetDataResultType
  {
    /// <summary>
    /// No problems due to missing target data could still be no data however.
    /// </summary>
    NoProblems,

    /// <summary>
    /// No result due to missing target data.
    /// </summary>
    NoResult,

    /// <summary>
    /// Partial result due to missing target data.
    /// </summary>
    PartialResult
  }
}
