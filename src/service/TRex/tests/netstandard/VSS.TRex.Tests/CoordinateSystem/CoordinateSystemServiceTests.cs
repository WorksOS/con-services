using System;
using System.IO;
using System.Linq;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Tests.Properties;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinateSystemServiceTests : IClassFixture<CoordinatesAPIClientTestDIFixture>
  {
    private const string DIMENSIONS_2012_DC_CSIB = "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

    [Fact]
    public void CoordinateService_SimpleLLHToNEE()
    {
      var NEECoords = ConvertCoordinates.LLHToNEE(DIMENSIONS_2012_DC_CSIB,
        new LLH
        {
          Latitude = 36.2073144965672,
          Longitude = -115.024944388223,
          Height = 550.96678869192408
        });

      Assert.True(Math.Abs(NEECoords.North - 1204) < 0.001, $"Expected {1204}, but found {NEECoords.North}");
      Assert.True(Math.Abs(NEECoords.East - 2313) < 0.001, $"Expected {2313}, but found {NEECoords.East}");
      Assert.True(Math.Abs(NEECoords.Elevation - 609) < 0.001, $"Expected {609}, but found {NEECoords.Elevation}");
    }

    [Fact]
    public void CoordinateService_ManyLLHToNEE()
    {
      var NEECoords = ConvertCoordinates.LLHToNEE(DIMENSIONS_2012_DC_CSIB,
        new[]
        {
          new LLH { Latitude = 36.21, Longitude = -115.01, Height = 10 },
          new LLH { Latitude = 36.22, Longitude = -115.02, Height = 11 },
          new LLH { Latitude = 36.23, Longitude = -115.03, Height = 12 }
        }).NEECoordinates.ToList();

      Assert.True(Math.Abs(NEECoords[0].North - 3656.9996220201547) < 0.001, $"Expected {3656.9996220201547}, but found {NEECoords[0].North}");
      Assert.True(Math.Abs(NEECoords[0].East - 1502.0980247307239) < 0.001, $"Expected {1502.0980247307239}, but found {NEECoords[0].East}");
      Assert.True(Math.Abs(NEECoords[0].Elevation - 68.058950967814724) < 0.001, $"Expected {68.058950967814724}, but found {NEECoords[0].Elevation}");

      Assert.True(Math.Abs(NEECoords[1].North - 2757.6347846893877) < 0.001, $"Expected {2757.6347846893877}, but found {NEECoords[1].North}");
      Assert.True(Math.Abs(NEECoords[1].East - 2611.7640792344355) < 0.001, $"Expected {2611.7640792344355}, but found {NEECoords[1].East}");
      Assert.True(Math.Abs(NEECoords[1].Elevation - 69.1538811614891) < 0.001, $"Expected {69.1538811614891}, but found {NEECoords[1].Elevation}");

      Assert.True(Math.Abs(NEECoords[2].North - 1858.4988322410918) < 0.001, $"Expected {1858.4988322410918}, but found {NEECoords[2].North}");
      Assert.True(Math.Abs(NEECoords[2].East - 3721.5247073087949) < 0.001, $"Expected {3721.5247073087949}, but found {NEECoords[2].East}");
      Assert.True(Math.Abs(NEECoords[2].Elevation - 70.248819491614839) < 0.001, $"Expected {70.248819491614839}, but found {NEECoords[2].Elevation}");
    }

    [Fact]
    public void CoordinateService_SimpleNEEToLLH()
    {
      var LLHCoords = ConvertCoordinates.NEEToLLH(DIMENSIONS_2012_DC_CSIB, new NEE { East = 2313, North = 1204, Elevation = 609 });

      Assert.True(Math.Abs(LLHCoords.Longitude - -115.024944388223) < 0.0001, $"Expected {-115.024944388223}, but found {LLHCoords.Longitude}");
      Assert.True(Math.Abs(LLHCoords.Latitude - 36.2073144965672) < 0.0001, $"Expected {36.2073144965672}, but found {LLHCoords.Latitude}");
      Assert.True(Math.Abs(LLHCoords.Height - 550.96678869192408) < 0.0001, $"Expected {550.96678869192408}, but found {LLHCoords.Height}");
    }

    [Fact]
    public void CoordinateService_ManyNEEToLLH()
    {
      var requestArray = new[] {
        new NEE { East = 2313, North = 1204, Elevation = 609 },
        new NEE { East = 2313, North = 1204, Elevation = 609 }
      };

      var LLHCoords = ConvertCoordinates.NEEToLLH(DIMENSIONS_2012_DC_CSIB, requestArray).LLHCoordinates;

      Assert.True(Math.Abs(LLHCoords[0].Longitude - -115.03727717865179) < 0.0001, $"Expected {-115.03727717865179}, but found {LLHCoords[0].Longitude}");
      Assert.True(Math.Abs(LLHCoords[0].Latitude - 36.21730699569774) < 0.0001, $"Expected {36.21730699569774}, but found {LLHCoords[0].Latitude}");
      Assert.True(Math.Abs(LLHCoords[0].Height - 550.87194700441933) < 0.0001, $"Expected {550.87194700441933}, but found {LLHCoords[0].Height}");

      Assert.True(Math.Abs(LLHCoords[1].Longitude - -115.03727717865179) < 0.0001, $"Expected {-115.03727717865179}, but found {LLHCoords[1].Longitude}");
      Assert.True(Math.Abs(LLHCoords[1].Latitude - 36.21730699569774) < 0.0001, $"Expected {36.21730699569774}, but found {LLHCoords[1].Latitude}");
      Assert.True(Math.Abs(LLHCoords[1].Height - 550.87194700441933) < 0.0001, $"Expected {550.87194700441933}, but found {LLHCoords[1].Height}");
    }

    [Fact]
    public void CoordinateService_SimpleXYZLLHToNEE()
    {
      // XYZ coordinate holding LLH data.
      var NEECoords = ConvertCoordinates.LLHToNEE(DIMENSIONS_2012_DC_CSIB, new XYZ(-115.01, 36.21, 10));

      Assert.True(Math.Abs(NEECoords.X - 3656.9996220201547) < 0.001, $"Expected {3656.9996220201547}, but found {NEECoords.X}");
      Assert.True(Math.Abs(NEECoords.Y - 1502.0980247307239) < 0.001, $"Expected {1502.0980247307239}, but found {NEECoords.Y}");
      Assert.True(Math.Abs(NEECoords.Z - 68.058950967814724) < 0.001, $"Expected {68.058950967814724}, but found {NEECoords.Z}");
    }

    [Fact]
    public void CoordinateService_ManyXYZLLHToNEE()
    {
      // XYZ coordinates holding LLH data values.
      var coords = new[]
      {
        new XYZ(-115.01, 36.21, 10),
        new XYZ(-115.02, 36.22, 11),
        new XYZ(-115.03, 36.23, 12)
      };

      var NEECoords = ConvertCoordinates.LLHToNEE(DIMENSIONS_2012_DC_CSIB, coords).NEECoordinates;

      Assert.True(Math.Abs(NEECoords[0].Y - 3656.9996220201547) < 0.001, $"Expected {3656.9996220201547}, but found {NEECoords[0].Y}");
      Assert.True(Math.Abs(NEECoords[0].X - 1502.0980247307239) < 0.001, $"Expected {1502.0980247307239}, but found {NEECoords[0].X}");
      Assert.True(Math.Abs(NEECoords[0].Z - 68.058950967814724) < 0.001, $"Expected {68.058950967814724}, but found {NEECoords[0].Z}");

      Assert.True(Math.Abs(NEECoords[1].Y - 2757.6347846893877) < 0.001, $"Expected {2757.6347846893877}, but found {NEECoords[1].Y}");
      Assert.True(Math.Abs(NEECoords[1].X - 2611.7640792344355) < 0.001, $"Expected {2611.7640792344355}, but found {NEECoords[1].X}");
      Assert.True(Math.Abs(NEECoords[1].Z - 69.1538811614891) < 0.001, $"Expected {69.1538811614891}, but found {NEECoords[1].Z}");

      Assert.True(Math.Abs(NEECoords[2].Y - 1858.4988322410918) < 0.001, $"Expected {1858.4988322410918}, but found {NEECoords[2].Y}");
      Assert.True(Math.Abs(NEECoords[2].X - 3721.5247073087949) < 0.001, $"Expected {3721.5247073087949}, but found {NEECoords[2].X}");
      Assert.True(Math.Abs(NEECoords[2].Z - 70.248819491614839) < 0.001, $"Expected {70.248819491614839}, but found {NEECoords[2].Z}");
    }

    [Fact]
    public void CoordinateService_SimpleXYZNEEToLLH()
    {
      // XYZ coordinate holding NEE data.
      var LLHCoords = ConvertCoordinates.NEEToLLH(DIMENSIONS_2012_DC_CSIB, new XYZ(2313, 1204, 609));

      Assert.True(Math.Abs(LLHCoords.X - -115.024944388223) < 0.001, $"Expected {-115.024944388223}, but found {LLHCoords.X}");
      Assert.True(Math.Abs(LLHCoords.Y - 36.2073144965672) < 0.001, $"Expected {36.2073144965672}, but found {LLHCoords.Y}");
      Assert.True(Math.Abs(LLHCoords.Z - 550.96678869192408) < 0.001, $"Expected {550.96678869192408}, but found {LLHCoords.Z}");
    }

    [Fact]
    public void CoordinateService_ManyXYZNEEToLLH()
    {
      // XYZ coordinates holding NEE data values.
      var requestArray = new[] {
        new XYZ(2313, 1204, 609),
        new XYZ(2314, 1205, 610)
      };

      var LLHCoords = ConvertCoordinates.NEEToLLH(DIMENSIONS_2012_DC_CSIB, requestArray).LLHCoordinates;

      Assert.True(Math.Abs(LLHCoords[0].X - -115.03727717865179) < 0.0001, $"Expected {-115.03727717865179}, but found {LLHCoords[0].X}");
      Assert.True(Math.Abs(LLHCoords[0].Y - 36.21730699569774) < 0.0001, $"Expected {36.21730699569774}, but found {LLHCoords[0].Y}");
      Assert.True(Math.Abs(LLHCoords[0].Z - 550.87194700441933) < 0.0001, $"Expected {550.87194700441933}, but found {LLHCoords[0].Z}");

      Assert.True(Math.Abs(LLHCoords[1].X - -115.03727717865179) < 0.0001, $"Expected {-115.03727717865179}, but found {LLHCoords[1].X}");
      Assert.True(Math.Abs(LLHCoords[1].Y - 36.21730699569774) < 0.0001, $"Expected {36.21730699569774}, but found {LLHCoords[1].Y}");
      Assert.True(Math.Abs(LLHCoords[1].Z - 551.871861184413) < 0.0001, $"Expected {551.871861184413}, but found {LLHCoords[1].Z}");
    }

    [Fact]
    public void CoordinateService_SimpleWGS84PointToXYZNEE()
    {
      var NEECoords = ConvertCoordinates.WGS84ToCalibration(DIMENSIONS_2012_DC_CSIB, new WGS84Point(-115.01, 36.21, 10));

      Assert.True(Math.Abs(NEECoords.Y - 3656.9996220201547) < 0.001, $"Expected {3656.9996220201547}, but found {NEECoords.Y}");
      Assert.True(Math.Abs(NEECoords.X - 1502.0980247307239) < 0.001, $"Expected {1502.0980247307239}, but found {NEECoords.X}");
      Assert.True(Math.Abs(NEECoords.Z - 68.058950967814724) < 0.001, $"Expected {68.058950967814724}, but found {NEECoords.Z}");
    }
    [Fact]
    public void CoordinateService_ManyWGS84PointToXYZNEE()
    {
      var points = new[]
      {
        new WGS84Point(-115.01, 36.21, 10),
        new WGS84Point(-115.02, 36.22, 11)
      };

      var NEECoords = ConvertCoordinates.WGS84ToCalibration(DIMENSIONS_2012_DC_CSIB, points);

      Assert.True(Math.Abs(NEECoords[0].Y - 3656.9996220201547) < 0.001, $"Expected {3656.9996220201547}, but found {NEECoords[0].Y}");
      Assert.True(Math.Abs(NEECoords[0].X - 1502.0980247307239) < 0.001, $"Expected {1502.0980247307239}, but found {NEECoords[0].X}");
      Assert.True(Math.Abs(NEECoords[0].Z - 68.058950967814724) < 0.001, $"Expected {68.058950967814724}, but found {NEECoords[0].Z}");

      Assert.True(Math.Abs(NEECoords[1].Y - 2757.6347846893877) < 0.001, $"Expected {2757.6347846893877}, but found {NEECoords[1].Y}");
      Assert.True(Math.Abs(NEECoords[1].X - 2611.7640792344355) < 0.001, $"Expected {2611.7640792344355}, but found {NEECoords[1].X}");
      Assert.True(Math.Abs(NEECoords[1].Z - 69.1538811614891) < 0.001, $"Expected {69.1538811614891}, but found {NEECoords[1].Z}");
    }

    [Fact]
    public void CoordinateService_ImportCSIBFromDCAsync()
    {
      var tmpFolder = Path.Combine(Directory.GetCurrentDirectory(), "tmp");

      Directory.CreateDirectory(tmpFolder);

      var filepath = Path.Combine(tmpFolder, "BootCamp_2012.dc");

      File.WriteAllBytes(filepath, Resources.BootCamp_2012);

      string result = ConvertCoordinates.DCFileToCSIB(filepath);
      Assert.True(DIMENSIONS_2012_DC_CSIB == result);
    }

    [Fact]
    public void CoordinateService_ImportFromDCContentAsync()
    {
      var csib = ConvertCoordinates.DCFileContentToCSIB("BootCamp_2012.dc", Resources.BootCamp_2012);

      Assert.True(DIMENSIONS_2012_DC_CSIB == csib);
    }
  }
}
