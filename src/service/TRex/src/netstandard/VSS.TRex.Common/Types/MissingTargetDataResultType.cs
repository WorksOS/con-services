namespace VSS.TRex.Types
{
  public enum MissingTargetDataResultType
  {
    /// <summary>
    /// 0 - No problems due to missing target data could still be no data however.
    /// </summary>
    NoProblems,

    /// <summary>
    /// 1 - No result due to missing target data.
    /// </summary>
    NoResult,

    /// <summary>
    /// 2 - Partial result due to missing target data.
    /// </summary>
    PartialResult,

    /// <summary>
    /// 3 - Partial result with some values Missing Machine Target.
    /// </summary>
    PartialResultMissingTarget
  }
}
