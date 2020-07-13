using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Designs
{
  public class AlignmentGeometryResult : ContractExecutionResult, IMasterDataModel
  {
    [JsonProperty(PropertyName = "designUid", Required = Required.Always)]
    public Guid DesignUid { get; set; }

    [JsonProperty(PropertyName = "fileName", Required = Required.Default)]
    public string FileName { get; set; }

    /// <summary>
    /// The collection of arrays of vertices describing a poly line representation of the alignment center line
    /// Vertices is a collection of arrays of arrays containing three doubles, containing the WGS84 latitude (index 0, decimal degrees),
    /// longitude (index 1, decimal degrees) and station (ISO units; meters) of each point along the alignment.
    /// Each element in the collection is a series of vertices describing a contiguous section of the alignment. There may many many
    /// such collections separated by arcs, or possibly even gaps.
    /// </summary>
    [JsonProperty(PropertyName = "vertices", Required = Required.Always)]
    public double[][][] Vertices { get; }

    /// <summary>
    /// The array of labels to be rendered along the alignment. These are generated according to the interval specified
    /// in the request and relevant features within the alignment
    /// </summary>
    [JsonProperty(PropertyName = "labels", Required = Required.Always)]
    public AlignmentGeometryResultLabel[] Labels { get; }

    /// <summary>
    /// The array of arcs describing all arc elements present along the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "arcs", Required = Required.Always)]
    public AlignmentGeometryResultArc[] Arcs { get; }

    /// <summary>
    /// Constructs an alignment master geometry result from supplied vertices and labels
    /// </summary>
    /// <param name="code"></param>
    /// <param name="designUid"></param>
    /// <param name="vertices"></param>
    /// <param name="arcs"></param>
    /// <param name="labels"></param>
    /// <param name="message"></param>
    public AlignmentGeometryResult(int code, Guid designUid, string fileName, double[][][] vertices, AlignmentGeometryResultArc[] arcs, AlignmentGeometryResultLabel[] labels, string message = DefaultMessage) : base(code, message)
    {
      DesignUid = designUid;
      FileName = fileName;
      Vertices = vertices;
      Arcs = arcs;
      Labels = labels;
    }

    /// <summary>
    /// No caching currently required
    /// </summary>
    /// <returns></returns>
    public List<string> GetIdentifiers() => new List<string>() {DesignUid.ToString()};
  }
}
