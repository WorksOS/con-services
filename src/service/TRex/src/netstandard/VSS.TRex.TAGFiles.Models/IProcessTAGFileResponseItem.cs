using System;
using VSS.TRex.Common.Interfaces;

namespace VSS.TRex.TAGFiles.Models
{
  public interface IProcessTAGFileResponseItem : IFromToBinary
  {
    string FileName { get; set; }

    Guid AssetUid { get; set; }

    bool Success { get; set; }

    string Exception { get; set; }

    TAGReadResult ReadResult { get; set; }

    TAGFileSubmissionFlags SubmissionFlags { get; set; }
  }
}
