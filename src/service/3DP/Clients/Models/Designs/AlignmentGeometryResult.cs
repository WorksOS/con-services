using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Designs
{
  public class AlignmentGeometryResult : ContractExecutionResult
  {
    /// <summary>
    /// The array of vertices describing a poly line representation of the alignment center line
    /// Vertices is an array of arrays containing three doubles, containing the WGS84 latitude (index 0, decimal degrees),
    /// longitude (index 1, decimal degrees) and station (ISO units; meters) of each point along the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "vertices", Required = Required.Always)]
    public double[][] Vertices { get; }

    /// <summary>
    /// The array of labels to be rendered along the alignment. These are generated according to the interval specified
    /// in the request and relevant features within the alignment
    /// </summary>
    [JsonProperty(PropertyName = "labels", Required = Required.Always)]
    public AlignmentGeometryResultLabel[] Labels { get; }

    /// <summary>
    /// Constructs an alignment master geometry result from supplied vertices and labels
    /// </summary>
    /// <param name="code"></param>
    /// <param name="vertices"></param>
    /// <param name="labels"></param>
    /// <param name="message"></param>
    public AlignmentGeometryResult(int code, double[][] vertices, AlignmentGeometryResultLabel[] labels, string message = DefaultMessage) : base(code, message)
    {
      Vertices = vertices;
      Labels = labels;
    }
  }
}
