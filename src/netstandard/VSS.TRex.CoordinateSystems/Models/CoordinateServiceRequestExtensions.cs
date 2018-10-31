using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems.Models
{
  public static class CoordinateServiceRequestExtensions
  {
    /// <summary>
    /// Serializes an array of <see cref="LLH"/> into the structure expected by the coordinate service
    /// for processing of multiple LLH records into NEE.
    /// </summary>
    /// <remarks>
    /// Note the deliberate order change, with Longitude preceeding Latitude in the LLH sequence.
    /// </remarks>
    public static double[,] ToRequestArray(this LLH[] LLHs)
    {
      var requestArray = new double[LLHs.Length, 3];

      for (int i = 0; i < LLHs.Length; i++)
      {
        requestArray[i, 0] = LLHs[i].Longitude;
        requestArray[i, 1] = LLHs[i].Latitude;
        requestArray[i, 2] = LLHs[i].Height;
      }

      return requestArray;
    }

    /// <summary>
    /// Serializes an array of <see cref="WGS84Point"/> into the structure expected by the coordinate service
    /// for processing of multiple LLH records into NEE.
    /// </summary>
    /// <remarks>
    /// Note the deliberate order change, with Longitude preceeding Latitude in the LLH sequence.
    /// </remarks>
    public static double[,] ToRequestArray(this WGS84Point[] wgs84Points)
    {
      var requestArray = new double[wgs84Points.Length, 3];

      for (int i = 0; i < wgs84Points.Length; i++)
      {
        requestArray[i, 0] = wgs84Points[i].Lon;
        requestArray[i, 1] = wgs84Points[i].Lat;
        requestArray[i, 2] = wgs84Points[i].Height;
      }

      return requestArray;
    }

    /// <summary>
    /// Converts an array of <see cref="NEE"/> objects to a multi dimensional array of doubles,
    /// representing the input array serialized without property labels.
    /// </summary>
    public static double[,] ToRequestArray(this NEE[] NEEs)
    {
      var requestArray = new double[NEEs.Length, 3];

      for (int i = 0; i < NEEs.Length; i++)
      {
        requestArray[i, 0] = NEEs[i].North;
        requestArray[i, 1] = NEEs[i].East;
        requestArray[i, 2] = NEEs[i].Elevation;
      }

      return requestArray;
    }

    /// <summary>
    /// Converts the multi dimensional array of doubles from the Coordinate service to an array of <see cref="NEE"/> objects.
    /// </summary>
    public static NEE[] ToNEEArray(this double[,] arrayData)
    {
      var result = new NEE[arrayData.Length / 3];

      for (int i = 0; i < arrayData.Length / 3; i++)
      {
        result[i] = new NEE
        {
          North = arrayData[i, 0],
          East = arrayData[i, 1],
          Elevation = arrayData[i, 2]
        };
      }

      return result;
    }

    /// <summary>
    /// Converts the multi dimensional array of doubles from the Coordinate service to an array of <see cref="LLH"/> objects.
    /// </summary>
    /// <remarks>
    /// Note the deliberate order change, with Longitude preceeding Latitude in the sequence we read
    /// from the LLH data.
    /// </remarks>
    public static LLH[] ToLLHArray(this double[,] arrayData)
    {
      var result = new LLH[arrayData.Length / 3];

      for (int i = 0; i < arrayData.Length / 3; i++)
      {
        result[i] = new LLH
        {
          Longitude = arrayData[i, 0],
          Latitude = arrayData[i, 1],
          Height = arrayData[i, 2]
        };
      }

      return result;
    }

    /// <summary>
    /// Converts an <see cref="XYZ"/> object to a <see cref="LLH"/> coordinate object.
    /// </summary>
    public static LLH ToLLH(this XYZ data)
    {
      return new LLH
      {
        Longitude = data.X,
        Latitude = data.Y,
        Height = data.Z
      };
    }

    /// <summary>
    /// Converts an <see cref="XYZ"/> object to a <see cref="NEE"/> coordinate object.
    /// </summary>
    public static NEE ToNEE(this XYZ data)
    {
      return new NEE
      {
        East = data.X,
        North = data.Y,
        Elevation = data.Z
      };
    }

    /// <summary>
    /// Converts an array of <see cref="XYZ"/> to an array of <see cref="LLH"/> objects.
    /// </summary>
    public static double[,] ToLLHRequestArray(this XYZ[] arrayData)
    {
      var requestArray = new double[arrayData.Length, 3];

      for (int i = 0; i < arrayData.Length; i++)
      {
        requestArray[i, 0] = arrayData[i].X;
        requestArray[i, 1] = arrayData[i].Y;
        requestArray[i, 2] = arrayData[i].Z;
      }

      return requestArray;
    }

    /// <summary>
    /// Converts an array of <see cref="XYZ"/> objects holding NEE data to a multi dimensional array of doubles,
    /// representing the input array serialized without property labels.
    /// </summary>
    public static double[,] ToNEERequestArray(this XYZ[] arrayData)
    {
      var requestArray = new double[arrayData.Length, 3];

      for (int i = 0; i < arrayData.Length; i++)
      {
        requestArray[i, 0] = arrayData[i].Y;
        requestArray[i, 1] = arrayData[i].X;
        requestArray[i, 2] = arrayData[i].Z;
      }

      return requestArray;
    }
  }
}
