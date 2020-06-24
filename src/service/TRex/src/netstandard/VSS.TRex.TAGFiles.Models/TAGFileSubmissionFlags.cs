using System;

namespace VSS.TRex.TAGFiles.Models
{
  [Flags]
  public enum TAGFileSubmissionFlags
  {
    None = 0,

    /// <summary>
    /// Place tha TAG file into the TAG file archive once submitted
    /// </summary>
    AddToArchive = 1,

    /// <summary>
    /// Notify the project rebuilder when this TAG file has completed processing
    /// </summary>
    NotifyRebuilderOnProceesing = 2
  }
}
