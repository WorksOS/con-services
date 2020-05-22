using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class AlignmentDesignGeometryResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public AlignmentDesignGeometryResponse() { }

    public DesignProfilerRequestResult RequestResult { get; set; }

    /// <summary>
    /// The array of vertices describing a poly line representation of the alignment center line
    /// Vertices is an array of arrays containing three doubles, containing the WGS84 latitude (index 0, decimal degrees),
    /// longitude (index 1, decimal degrees) and station (ISO units; meters) of each point along the alignment.
    /// </summary>
    public double[][] Vertices { get; set; }

    /// <summary>
    /// The array of labels to be rendered along the alignment. These are generated according to the interval specified
    /// in the request and relevant features within the alignment
    /// </summary>
    public AlignmentGeometryResponseLabel[] Labels { get; set; }

    /// <summary>
    /// Constructs an alignment master geometry result from supplied vertices and labels
    /// </summary>
    /// <param name="requestResult"></param>
    /// <param name="vertices"></param>
    /// <param name="labels"></param>
    public AlignmentDesignGeometryResponse(DesignProfilerRequestResult requestResult, double[][] vertices, AlignmentGeometryResponseLabel[] labels)
    {
      RequestResult = requestResult;
      Vertices = vertices;
      Labels = labels;
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)RequestResult);

      // Write the vertices
      var count = Vertices?.Length ?? 0;
      writer.WriteInt(count);
      if (count > 0)
      {
        // ReSharper disable once PossibleNullReferenceException
        foreach (var vertex in Vertices)
        {
          writer.WriteDouble(vertex[0]);
          writer.WriteDouble(vertex[1]);
          writer.WriteDouble(vertex[2]);
        }
      }

      // Write the labels
      count = Labels?.Length ?? 0;
      writer.WriteInt(count);
      if (count > 0)
      {
        // ReSharper disable once PossibleNullReferenceException
        foreach (var label in Labels)
        {
          writer.WriteDouble(label.Station);
          writer.WriteDouble(label.Lat);
          writer.WriteDouble(label.Lon);
          writer.WriteDouble(label.Rotation);
        }
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      RequestResult = (DesignProfilerRequestResult) reader.ReadInt();

      // Read the vertices
      var count = reader.ReadInt();
      if (count > 0)
      {
        Vertices = new double[count][];

        for (var i = 0; i < count; i++)
        {
          Vertices[i] = new[] {reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble()};
        }
      }

      // Read the labels
      count = reader.ReadInt();
      if (count > 0)
      {
        Labels = new AlignmentGeometryResponseLabel[count];
        for (var i = 0; i < count; i++)
        {
          Labels[i] = new AlignmentGeometryResponseLabel(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        }
      }
    }
  }
}
