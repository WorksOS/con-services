using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems.Interfaces
{
  public interface ICoordinateConversion
  {
    XYZ WGS84ToCalibration(string csib, WGS84Point WGSLL);
    NEE WGS84ToCalibration(string csib, LLH WGSLL);

    XYZ[] WGS84ToCalibration(string csib, WGS84Point[] WGSLLs);
    NEE[] WGS84ToCalibration(string csib, LLH[] WGSLLs);

    string DCFileToCSIB(string DCFileName);
    string DCFileContentToCSIB(string DCFileName, byte[] fileContent);

  }
}
