using System;
using System.IO;
using System.Threading.Tasks;
using VSS.TRex.CoordinateSystems;
using Xunit;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinateSystemServiceTests
  {
    private static string J_Dimensions_2012 =
      "QM0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS05KA6TXFY6ZE4H6NQX8J3VAX79TTF82VPSV1KVR8W9V7BM1N3MEY5QHACSFNCK7VWPNY52RXGC1G9BPBS1QWA7ZVM6T2E0WMDY7P6CXJ68RB4CHJCDSVR6000047S29YVT08000";

    [Fact(Skip ="No Service")]
    public void Test_CoordinateConversionService_SimpleLLHToNEE_Dimensions_2012()
    {
      CoordinatesApiClient client = new CoordinatesApiClient();

      Task<NEE> nee = client.GetNEEAsync(J_Dimensions_2012,
        new LLH
        {
          // Results from NEE = 1204, 2313, 609, in test below
          Latitude = 36.2073144965672,
          Longitude = -115.024944388223,
          Height = 550.96678869192408
        });

      Assert.True(Math.Abs(nee.Result.North - 1204) < 0.001 &&
                  Math.Abs(nee.Result.East - 2313) < 0.001 &&
                  Math.Abs(nee.Result.Elevation - 609) < 0.001,
        $"Coordinates not as expected NEE(1204, 2313, 609), versus NEE({nee.Result.North}, {nee.Result.East}, {nee.Result.Elevation})");
    }

    [Fact(Skip = "No Service")]
    public void Test_CoordinateConversionService_ManyLLHToNEE_Dimensions_2012()
    {
      CoordinatesApiClient client = new CoordinatesApiClient();

      Task<NEE[]> nee = client.GetNEEsAsync(J_Dimensions_2012,
        new LLH[]
        { new LLH {
          // Results from NEE = 1204, 2313, 609, in test below
          Latitude = 36.2073144965672,
          Longitude = -115.024944388223,
          Height = 550.96678869192408}
        });

      Assert.True(Math.Abs(nee.Result[0].North - 1204) < 0.001 &&
                  Math.Abs(nee.Result[0].East - 2313) < 0.001 &&
                  Math.Abs(nee.Result[0].Elevation - 609) < 0.001,
        $"Coordinates not as expected NEE(1204, 2313, 609), versus NEE({nee.Result[0].North}, {nee.Result[0].East}, {nee.Result[0].Elevation})");
    }

    [Fact(Skip = "No Service")]
    public void Test_CoordinateConversionService_SimpleNEEToLLH_Dimensions_2012()
    {
      CoordinatesApiClient client = new CoordinatesApiClient();

      Task<LLH> llh = client.GetLLHAsync(J_Dimensions_2012,
        new NEE
        {
          East = 2313,
          North = 1204,
          Elevation = 609
        });

      Assert.True(Math.Abs(llh.Result.Longitude - -115.024944388223) < 0.0001 &&
                  Math.Abs(llh.Result.Latitude - 36.2073144965672) < 0.0001 &&
                  Math.Abs(llh.Result.Height - 550.96678869192408) < 0.0001,
        $"Coordinates not as expected LLH(36.217402584467969, -115.00054662586074, -58.129469261248616), versus LLH({llh.Result.Latitude}, {llh.Result.Longitude}, {llh.Result.Height}");
    }

    [Fact(Skip = "No Service")]
    public void Test_CoordinateConversionService_ImportFromDCAsync_Dimensions()
    {
      CoordinatesApiClient client = new CoordinatesApiClient();

      Task<string> csib = client.ImportFromDCAsync(@"J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\BC Data\Sites\BootCamp 2012\BootCamp 2012.dc");

      Assert.True(csib.Result != "");
    }

    [Fact(Skip = "No Service")]
    public void Test_CoordinateConversionService_ImportFromDCContentAsync_Dimensions()
    {
      CoordinatesApiClient client = new CoordinatesApiClient();

      Task<string> csib = client.ImportFromDCContentAsync(@"J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\BC Data\Sites\BootCamp 2012\BootCamp 2012.dc",
        File.ReadAllBytes(@"J:\PP\Construction\Office software\SiteVision Office\Test Files\VisionLink Data\Dimensions 2012\BC Data\Sites\BootCamp 2012\BootCamp 2012.dc"));

      Assert.True(csib.Result != "");
    }

  }
}
