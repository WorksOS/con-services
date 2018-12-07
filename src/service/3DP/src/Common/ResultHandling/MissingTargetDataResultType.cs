namespace VSS.Productivity3D.Common.ResultHandling
{
  public enum MissingTargetDataResultType
  {
    /// <summary>
    /// No problems due to missing target data could still be no data however.
    /// </summary>
    NoProblems = 0,

    /// <summary>
    /// No result due to missing target data.
    /// </summary>
    NoResult = 1,

    /// <summary>
    /// Partial result due to missing target data.
    /// </summary>
    PartialResult = 2,

    /// <summary>
    /// Partial result with some values Missing Machine Target.
    /// </summary>
    PartialResultMissingTarget = 3
  }
}
