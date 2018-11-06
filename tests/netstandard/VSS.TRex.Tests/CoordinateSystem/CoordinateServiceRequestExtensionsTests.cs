using System.Linq;
using VSS.TRex.CoordinateSystems.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinateServiceRequestExtensionsTests
  {
    [Fact]
    public void Array_of_LLH_objects_Should_serialize_to_multidim_array_of_doubles()
    {
      var llhData = new[]
      {
        new LLH { Latitude = 36.21, Longitude = -115.01, Height = 10 },
        new LLH { Latitude = 36.22, Longitude = -115.02, Height = 11 },
        new LLH { Latitude = 36.23, Longitude = -115.03, Height = 12 }
      };

      var result = llhData.ToRequestArray();

      for (int i = 0; i < llhData.Length; i++)
      {
        Assert.Equal(llhData[i].Longitude, result[i, 0]);
        Assert.Equal(llhData[i].Latitude, result[i, 1]);
        Assert.Equal(llhData[i].Height, result[i, 2]);
      }
    }

    [Fact]
    public void Array_of_WGS84Points_Should_serialize_to_multidim_array_of_doubles()
    {
      var wgs84Points = new[]
      {
        new WGS84Point(36.21, -115.01, 10),
        new WGS84Point(36.22, -115.02, 11),
        new WGS84Point(36.23, -115.03, 12)
      };

      var result = wgs84Points.ToRequestArray();

      for (int i = 0; i < wgs84Points.Length; i++)
      {
        Assert.Equal(wgs84Points[i].Lon, result[i, 0]);
        Assert.Equal(wgs84Points[i].Lat, result[i, 1]);
        Assert.Equal(wgs84Points[i].Height, result[i, 2]);
      }
    }

    [Fact]
    public void Array_of_NEE_objects_Should_serialize_to_multidim_array_of_doubles()
    {
      var neeData = new[]
      {
        new NEE { East = 2313, North = 1204, Elevation = 609 },
        new NEE { East = 2300, North = 1200, Elevation = 10 }
      };

      var result = neeData.ToRequestArray();

      for (int i = 0; i < neeData.Length; i++)
      {
        Assert.Equal(neeData[i].North, result[i, 0]);
        Assert.Equal(neeData[i].East, result[i, 1]);
        Assert.Equal(neeData[i].Elevation, result[i, 2]);
      }
    }

    [Fact]
    public void Array_of_LLH_doubles_Should_serialize_to_collection_of_LLH_objects()
    {
      var llhData = new[,]
      {
        { -115.01, 36.21, 10 },
        { -115.02, 36.22, 11 },
        { -115.03, 36.23, 12 }
      };

      var result = llhData.ToLLHArray().ToList();

      for (int i = 0; i < result.Count / 3; i++)
      {
        Assert.Equal(llhData[i, i], result[i].Longitude);
        Assert.Equal(llhData[i, i + 1], result[i].Latitude);
        Assert.Equal(llhData[i, i + 2], result[i].Height);
      }
    }

    [Fact]
    public void Array_of_NEE_doubles_Should_deserialize_to_collection_of_NEE_objects()
    {
      var neeData = new[,]
      {
        { 3656.9996220201547, 1502.0980247307239, 68.058950967814724 },
        { 2757.6347846893877, 2611.7640792344355, 69.1538811614891 },
        { 1858.4988322410918, 3721.5247073087949, 70.248819491614839 }
      };

      var result = neeData.ToNEEArray().ToList();

      for (int i = 0; i < result.Count / 3; i++)
      {
        Assert.Equal(neeData[i, i], result[i].North);
        Assert.Equal(neeData[i, i + 1], result[i].East);
        Assert.Equal(neeData[i, i + 2], result[i].Elevation);
      }
    }

    [Fact]
    public void XYZ_Should_convert_to_NEE_object()
    {
      var XYZCoords = new XYZ { X = 2300, Y = 1200, Z = 10 };
      var NEECoords = XYZCoords.ToNEE();

      Assert.Equal(XYZCoords.Y, NEECoords.North);
      Assert.Equal(XYZCoords.X, NEECoords.East);
      Assert.Equal(XYZCoords.Z, NEECoords.Elevation);
    }

    [Fact]
    public void XYZ_array_Should_convert_to_NEE_objects()
    {
      var XYZCoords = new[]
      {
        new XYZ { X  = 2313, Y = 1204, Z = 609 },
        new XYZ { X = 2300, Y = 1200, Z = 10 }
      };

      var NEECoords = XYZCoords.ToNEERequestArray();

      for (int i = 0; i < NEECoords.Length / 3; i++)
      {
        Assert.Equal(XYZCoords[i].Y, NEECoords[i, 0]);
        Assert.Equal(XYZCoords[i].X, NEECoords[i, 1]);
        Assert.Equal(XYZCoords[i].Z, NEECoords[i, 2]);
      }
    }

    [Fact]
    public void XYZ_Should_convert_to_LLH_object()
    {
      var XYZCoords = new XYZ { X = 2300, Y = 1200, Z = 10 };
      var LLHCoords = XYZCoords.ToLLH();

      Assert.Equal(XYZCoords.Y, LLHCoords.Latitude);
      Assert.Equal(XYZCoords.X, LLHCoords.Longitude);
      Assert.Equal(XYZCoords.Z, LLHCoords.Height);
    }

    [Fact]
    public void XYZ_array_Should_convert_to_LLH_objects()
    {
      var XYZCoords = new[]
      {
        new XYZ { X  = 2313, Y = 1204, Z = 609 },
        new XYZ { X = 2300, Y = 1200, Z = 10 }
      };

      var LLHCoords = XYZCoords.ToLLHRequestArray();

      for (int i = 0; i < LLHCoords.Length / 3; i++)
      {
        Assert.Equal(XYZCoords[i].X, LLHCoords[i, 0]);
        Assert.Equal(XYZCoords[i].Y, LLHCoords[i, 1]);
        Assert.Equal(XYZCoords[i].Z, LLHCoords[i, 2]);
      }
    }
  }
}
