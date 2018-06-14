using System;
using System.Threading.Tasks;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Implements a set of capabilities for coordinate conversion between WGS and grid contexts, and
  /// conversion of coordinate system files into CSIB (Coordinate System Information Block) strings
  /// </summary>
  public class CoordinateConversion : ICoordinateConversion
  {
    private CoordinatesApiClient serviceClient = new CoordinatesApiClient();

    public CoordinateConversion()
    {
    }

    /// <summary>
    ///  Converts a single WGS84 location to its grid location in the site calibration
    /// </summary>
    /// <param name="csib"></param>
    /// <param name="WGSLL"></param>
    /// <returns></returns>
    public XYZ WGS84ToCalibration(string csib, WGS84Point WGSLL)
    {
      Task<NEE> nee = serviceClient.GetNEEAsync(csib,
        new LLH
        {
          Latitude = WGSLL.Lat,
          Longitude = WGSLL.Lon,
          Height = 0
        });

      return new XYZ(nee.Result.East, nee.Result.North);
    }

    public NEE WGS84ToCalibration(string csib, LLH WGSLL) => serviceClient.GetNEEAsync(csib, WGSLL).Result;

    public XYZ[] WGS84ToCalibration(string csib, WGS84Point[] WGSLLs)
    {
      throw new NotImplementedException();
    }

    public NEE[] WGS84ToCalibration(string csib, LLH[] WGSLLs) //=> serviceClient.GetNEEAsync(csib, WGSLLs).Result;
    {
      throw new NotImplementedException();
    }

    public string DCFileToCSIB(string DCFileName) => serviceClient.ImportFromDCAsync(DCFileName).Result;

    public string DCFileContentToCSIB(string DCFileName, byte[] fileContent) => serviceClient.ImportFromDCDataAsync(DCFileName, fileContent).Result;
  }
}
