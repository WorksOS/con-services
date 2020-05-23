using Apache.Ignite.Core.Binary;
using VSS.Productivity3D.Productivity3D.Models.Designs;
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
    /// The array of array of vertices describing a poly line representation of the alignment center line
    /// Vertices is an array of arrays containing three doubles, containing the WGS84 latitude (index 0, decimal degrees),
    /// longitude (index 1, decimal degrees) and station (ISO units; meters) of each point along the alignment.
    /// </summary>
    public double[][][] Vertices { get; set; }

    /// <summary>
    /// The array of arcs describing all arc elements present along the alignment.
    /// </summary>
    public AlignmentGeometryResultArc[] Arcs { get; set; }

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
    public AlignmentDesignGeometryResponse(DesignProfilerRequestResult requestResult, double[][][] vertices, AlignmentGeometryResponseLabel[] labels)
    {
      RequestResult = requestResult;
      Vertices = vertices;
      Labels = labels;
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt((int)RequestResult);

      var verticesCount = Vertices?.Length ?? 0;
      writer.WriteInt(verticesCount);

      if (verticesCount > 0)
      {
        // Write the vertices
        foreach (var vertices in Vertices)
        {
          var vertexCount = vertices?.Length ?? 0;
          writer.WriteInt(vertexCount);
          if (vertexCount > 0)
          {
            // ReSharper disable once PossibleNullReferenceException
            foreach (var vertex in vertices)
            {
              writer.WriteDouble(vertex[0]);
              writer.WriteDouble(vertex[1]);
              writer.WriteDouble(vertex[2]);
            }
          }
        }
      }

      // Write the arcs
      var arcCount = Arcs?.Length ?? 0;
      writer.WriteInt(arcCount);
      if (arcCount > 0)
      {
        foreach (var arc in Arcs)
        {
          writer.WriteDouble(arc.Lat1);
          writer.WriteDouble(arc.Lon1);
          writer.WriteDouble(arc.Elev1);
          writer.WriteDouble(arc.Lat2);
          writer.WriteDouble(arc.Lon2);
          writer.WriteDouble(arc.Elev2);
          writer.WriteDouble(arc.LatC);
          writer.WriteDouble(arc.LonC);
          writer.WriteDouble(arc.ElevC);
          writer.WriteBoolean(arc.CW);
        }
      }

      // Write the labels
      var count = Labels?.Length ?? 0;
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

      var vertexArrayCount = reader.ReadInt();
      if (vertexArrayCount > 0)
      {
        Vertices = new double[vertexArrayCount][][];
        for (var index = 0; index < vertexArrayCount; index++)
        {
          // Read the vertices
          var vertexCount = reader.ReadInt();
          if (vertexCount > 0)
          {
            Vertices[index] = new double[vertexCount][];

            for (var i = 0; i < vertexCount; i++)
            {
              Vertices[index][i] = new[] {reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble()};
            }
          }
        }
      }

      // Read the arcs
      var arcCount = reader.ReadInt();
      if (arcCount > 0)
      {
        Arcs = new AlignmentGeometryResultArc[arcCount];
        for (var i = 0; i < arcCount; i++)
        {
          Arcs[i] = new AlignmentGeometryResultArc
            (reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
            reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
            reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(),
            reader.ReadBoolean());
        }
      }

      // Read the labels
      var count = reader.ReadInt();
      if (count > 0)
      {
        Labels = new AlignmentGeometryResponseLabel[count];
        for (var i = 0; i < count; i++)
        {
          Labels[i] = new AlignmentGeometryResponseLabel
            (reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
        }
      }
    }
  }
}
