using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for ConversionFactorsTest and is intended
    ///to contain all ConversionFactorsTest Unit Tests
    ///</summary>
  [TestClass()]
  public class ConversionFactorsTest
  {
    /// <summary>
    ///A test for ConvertLonDegreesToGeodesic
    ///</summary>
    [TestMethod()]
    public void ConvertLonDegreesToGeodesicTest()
    {
      Decimal lon = 15; 
      uint expected = 699051;
      uint actual = 0;
      actual = ConversionFactors.ConvertLonDegreesToGeodesic(lon);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = 15");
      lon = -95.123456789M;
      expected = 12344141;
      actual = ConversionFactors.ConvertLonDegreesToGeodesic(lon);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = -95.123456789M");
      lon = 27.66657829M;
      expected = 1289356;
      actual = ConversionFactors.ConvertLonDegreesToGeodesic(lon);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = 27.66657829M");
      lon = -127.66657829M;
      expected = 10827522;
      actual = ConversionFactors.ConvertLonDegreesToGeodesic(lon);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = -127.66657829M");
    }

    /// <summary>
    ///A test for ConvertLatDegreesToGeodesic
    ///</summary>
    [TestMethod()]
    public void ConvertLatDegreesToGeodesicTest()
    {
      Decimal lat = 15;
      uint expected = 6990506;
      uint actual;
      actual = ConversionFactors.ConvertLatDegreesToGeodesic(lat);
      Assert.AreEqual(expected, actual);
      Assert.AreEqual(expected, actual, "GeoDesic Lat not equal lon = 15");
      lat = -95.123456789M;
      expected = 17254756;
      actual = ConversionFactors.ConvertLatDegreesToGeodesic(lat);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = -95.123456789M");
      lat = 27.66657829M;
      expected = 5809896;
      actual = ConversionFactors.ConvertLatDegreesToGeodesic(lat);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = 27.66657829M");
      lat = -127.66657829M;
      expected = 20287994;
      actual = ConversionFactors.ConvertLatDegreesToGeodesic(lat);
      Assert.AreEqual(expected, actual, "GeoDesic Lon not equal lon = -127.66657829M");
    }

    /// <summary>
    ///A test for ConvertLatGeodesicToDegrees
    ///</summary>
    [TestMethod()]
    public void ConvertLatGeodesicToDegreesTest()
    {
      uint geoLat = 20287994; 
      double expected = -127.66657; 
      double actual;
      actual = Math.Round(ConversionFactors.ConvertLatGeodesicToDegrees(geoLat), 5);
      Assert.AreEqual(expected, actual);
      geoLat = 6990506;
      expected = 15;
      actual = Math.Round(ConversionFactors.ConvertLatGeodesicToDegrees(geoLat), 5);
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    ///A test for ConvertLonGeodesicToDegrees
    ///</summary>
    [TestMethod()]
    public void ConvertLonGeodesicToDegreesTest()
    {
      uint geoLon = 10827522;
      double expected = -127.6666; 
      double actual;
      actual = Math.Round(ConversionFactors.ConvertLonGeodesicToDegrees(geoLon), 4);
      Assert.AreEqual(expected, actual);
      geoLon = 699051;
      expected = 15.00000;
      actual = Math.Round(ConversionFactors.ConvertLonGeodesicToDegrees(geoLon), 4);
      Assert.AreEqual(expected, actual);
    }
  }
}
