using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling
{
  public class AlignmentOffsetResult : ContractExecutionResult
  {

    /// <summary>
    /// The start offset for the alignment file
    /// </summary>
    [JsonProperty(PropertyName = "StartOffset")]
    public double StartOffset { get; private set; }

    /// <summary>
    /// The ent offset for the alignment file
    /// </summary>
    [JsonProperty(PropertyName = "EndOffset")]
    public double EndOffset { get; private set; }

    public static AlignmentOffsetResult CreateAlignmentOffsetResult(double startOffset, double endOffset)
    {
      return new AlignmentOffsetResult() {StartOffset = startOffset, EndOffset = endOffset};
    }

  }
}
