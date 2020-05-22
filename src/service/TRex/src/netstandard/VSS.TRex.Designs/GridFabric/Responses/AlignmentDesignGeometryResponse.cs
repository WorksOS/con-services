using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class AlignmentGeometryResponseLabel
  {
    /// <summary>
    /// Measured (as in walked) distance along the alignment from the start of the alignment, expressed in meters
    /// </summary>
    public double Station { get; set; }

    /// <summary>
    /// Contains the WGS84 latitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    /// Contains the WGS84 longitude (expressed decimal degrees) of the test insertion position
    /// </summary>
    public double Lon { get; set; }

    /// <summary>
    /// Text rotation expressed as a survey angle (north is 0, increasing clockwise), in decimal degrees.
    /// </summary>
    public double Rotation { get; set; }

    public AlignmentGeometryResponseLabel(double station, double lat, double lon, double rotation)
    {
      Station = station;
      Lat = lat;
      Lon = lon;
      Rotation = rotation;
    }
  }

  public class AlignmentDesignGeometryResponse : BaseRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

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
    /// <param name="vertices"></param>
    /// <param name="labels"></param>
    public AlignmentDesignGeometryResponse(double[][] vertices, AlignmentGeometryResponseLabel[] labels)
    {
      Vertices = vertices;
      Labels = labels;
    }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      throw new NotImplementedException();
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      throw new NotImplementedException();
    }
  }
}
