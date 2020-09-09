using CoreXModels;

namespace CoreX.Wrapper.UnitTests.ExpectedTestResultObjects
{
  public class ExpectedCSDResults
  {
    public static CoordinateSystem Netherlaneds_With_Geoid = new CoordinateSystem
    {
      SystemName = "Netherlands/RD",
      GeoidInfo = new GeoidInfo
      {
        GeoidFileName = "demin.ggf",
        GeoidName = "Netherlands (De Min)",
        GeoidSystemId = 0
      },
      DatumInfo = new DatumInfo
      {
        DatumName = "DatumSevenParameters",
        DatumType = "SevenParameter",
        DatumSystemId = -1
      },
      ZoneInfo = new ZoneInfo
      {
        ShiftGridFileName = "",
        SnakeGridFileName = ""
      }
    };

    public static CoordinateSystem Florida_East_0901_NAD_1983_No_Geoid = new CoordinateSystem
    {
      SystemName = "United States/State Plane 1983",
      GeoidInfo = new GeoidInfo
      {
        GeoidFileName = "",
        GeoidName = "",
        GeoidSystemId = 0
      },
      DatumInfo = new DatumInfo
      {
        DatumName = "DatumThreeParameters",
        DatumType = "Molodensky",
        DatumSystemId = -1
      },
      ZoneInfo = new ZoneInfo
      {
        ShiftGridFileName = "",
        SnakeGridFileName = ""
      }
    };
  }
}
