namespace VSS.Hydrology.WebApi.TTM
{
  public static class Consts
  {
    public const int MaxSmallIntValue = 0x7FFF;
    public const byte TTMMajorVersion = 1;
    public const byte TTMMinorVersion = 0;
    public const string TTMFileIdentifier = "TNL TIN DTM FILE\0\0\0\0";

    public const double NullReal = 1E308;
    public const double NullDouble = NullReal;
    public const int NoNeighbour = -1;
    public const int MaxStartPoints = 50;

    public const double DefaultCoordinateResolution = 0.000000001; // 0.000001 mm
    public const double DefaultElevationResolution = 0.000000001; // 0.000001 mm
  }
}
