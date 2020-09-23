using CoreXModels;

namespace CoreX.Wrapper.UnitTests.ExpectedTestResultObjects
{
  public static class ExpectedCSDResults
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
        DatumSystemId = -1,
        EllipseA = 6377397.155,
        EllipseInverseFlat = 299.15281254477,
        EllipseName = "",
        LatitudeShiftGridFileName = "",
        LongitudeShiftGridFileName = "",
        HeightShiftGridFileName = "",
        TranslationX = 565.03999999999883,
        TranslationY = 49.909999999999535,
        TranslationZ = 465.84000000000168,
        RotationX = 0,
        RotationY = 0,
        RotationZ = 0,
        Scale = 0
      },
      ZoneInfo = new ZoneInfo
      {
        ZoneType = "RDStereographicZone",
        ShiftGridFileName = "",
        SnakeGridFileName = "",
        IsSouthGrid = false,
        IsWestGrid = false,
        OriginLatitude = 0.91029672689324714,
        OriginLongitude = 0.094032037519600251,
        OriginNorth = 463000,
        OriginEast = 155000,
        OriginScale = 0.9999079,
        HorizontalAdjustment = new ZoneHorizontalAdjustment
        {
          IsNullAdjustment = true,
          OriginEast = -9.99E+27,
          OriginNorth = -9.99E+27,
          Rotation = -9.99E+27,
          Scale = -9.99E+27,
          TranslationEast = -9.99E+27,
          TranslationNorth = -9.99E+27
        },
        VerticalAdjustment = new ZoneVerticalAdjustment
        {
          IsNullAdjustment = true,
          ConstantAdjustment = -9.99E+27,
          OriginEast = -9.99E+27,
          OriginNorth = -9.99E+27,
          SlopeEast = -9.99E+27,
          SlopeNorth = -9.99E+27
        }
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
        DatumSystemId = -1,
        EllipseA = 6378137.0000001024,
        EllipseInverseFlat = 298.25722210101,
        EllipseName = "",
        LatitudeShiftGridFileName = "",
        LongitudeShiftGridFileName = "",
        HeightShiftGridFileName = "",
        TranslationX = 0,
        TranslationY = 0,
        TranslationZ = 0,
        RotationX = 0,
        RotationY = 0,
        RotationZ = 0,
        Scale = 0
      },
      ZoneInfo = new ZoneInfo
      {
        ZoneType = "TransverseMercatorZone",
        ShiftGridFileName = "",
        SnakeGridFileName = "",
        IsSouthGrid = false,
        IsWestGrid = false,
        OriginLatitude = 0.42469678465194771,
        OriginLongitude = -1.4137166941154069,
        OriginNorth = 0,
        OriginEast = 200000.00000000105,
        OriginScale = 0.9999411764706,
        HorizontalAdjustment = new ZoneHorizontalAdjustment
        {
          IsNullAdjustment = false,
          OriginEast = 125252.12285763475,
          OriginNorth = 630739.07671598683,
          Rotation = 1.1539570115265393E-06,
          Scale = 1.0000128567812,
          TranslationEast = 2.1590155465803202,
          TranslationNorth = 3.1726109115404628
        },
        VerticalAdjustment = new ZoneVerticalAdjustment
        {
          IsNullAdjustment = false,
          ConstantAdjustment = -38.138331784666505,
          OriginEast = 124629.21257141378,
          OriginNorth = 631127.61201424443,
          SlopeEast = 7.7664261E-06,
          SlopeNorth = 6.782385E-06
        }
      }
    };
  }
}
