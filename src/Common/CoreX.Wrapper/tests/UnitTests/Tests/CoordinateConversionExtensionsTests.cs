using System.Linq;
using CoreX.Extensions;
using CoreXModels;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class CoordinateConversionExtensionsTests
  {
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

      for (var i = 0; i < result.Count / 3; i++)
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

      for (var i = 0; i < result.Count / 3; i++)
      {
        Assert.Equal(neeData[i, i], result[i].East);
        Assert.Equal(neeData[i, i + 1], result[i].North);
        Assert.Equal(neeData[i, i + 2], result[i].Elevation);
      }
    }

    [Fact]
    public void XYZ_Should_convert_to_NEE_object()
    {
      var xyzCoords = new XYZ { X = 2300, Y = 1200, Z = 10 };
      var neeCoords = xyzCoords.ToNEE();

      Assert.Equal(xyzCoords.Y, neeCoords.North);
      Assert.Equal(xyzCoords.X, neeCoords.East);
      Assert.Equal(xyzCoords.Z, neeCoords.Elevation);
    }

    [Fact]
    public void XYZ_Should_convert_to_LLH_object()
    {
      var xyzCoords = new XYZ { X = 2300, Y = 1200, Z = 10 };
      var llhCoords = xyzCoords.ToLLH();

      Assert.Equal(xyzCoords.Y, llhCoords.Latitude);
      Assert.Equal(xyzCoords.X, llhCoords.Longitude);
      Assert.Equal(xyzCoords.Z, llhCoords.Height);
    }
  }
}
