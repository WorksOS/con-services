﻿using System.ComponentModel;
using CoreX.Interfaces;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.UnitTests.Types;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class DcFileTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly IConvertCoordinates _convertCoordinates;

    private const double LL_CM_TOLERANCE = 0.00000001;
    private const double GRID_CM_TOLERANCE = 0.01;

    public DcFileTests(UnitTestBaseFixture testFixture)
    {
      _convertCoordinates = testFixture.ConvertCoordinates;
    }

    public string GetCSIBFromDC(string dcFilename) =>
      _convertCoordinates.DCFileToCSIB(DCFile.GetFilePath(dcFilename));

    [Theory]
    [Description("Sanity tests validating only height varies when VERT_ADJUST is present.")]
    [InlineData(36.21730699569774, -115.0372771786517, 608.9999852774359, ReturnAs.Degrees, DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST)]
    [InlineData(0.63211125328050133, -2.007779249296807, 608.99998527743593, ReturnAs.Radians, DCFile.DIMENSIONS_2012_DC_FILE_WITHOUT_VERT_ADJUST)]
    [InlineData(36.21730699569774, -115.0372771786517, 550.8719470044193, ReturnAs.Degrees, DCFile.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST)]
    [InlineData(0.63211125328050133, -2.007779249296807, 550.87194700441933, ReturnAs.Radians, DCFile.DIMENSIONS_2012_DC_FILE_WITH_VERT_ADJUST)]
    public void CoordinateService_SimpleXYZNEEToLLH(double lat, double lon, double height, ReturnAs returnAs, string dcFilename)
    {
      var csib = GetCSIBFromDC(dcFilename);

      var xyz = _convertCoordinates.NEEToLLH(csib, new XYZ(2313, 1204, 609), returnAs);

      xyz.Should().NotBeNull();
      xyz.X.Should().BeApproximately(lon, LL_CM_TOLERANCE);
      xyz.Y.Should().BeApproximately(lat, LL_CM_TOLERANCE);
      xyz.Z.Should().BeApproximately(height, LL_CM_TOLERANCE);
    }

    //[Theory]
    //[InlineData(DCFile.BOOTCAMP_2012_WITH_VERT_ADJUST)]
    //[InlineData(DCFile.BOOTCAMP_2012_WITHOUT_VERT_ADJUST)]
    //public void CoordinateService_ImportCSIBFromDC(string dcFilename)
    //{
    //  var csib = GetCSIBFromDC(DCFile.GetFilePath(dcFilename));

    //  csib.Should().Be(_csib);
    //}
  }
}
