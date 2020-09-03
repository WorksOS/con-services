using CoreX.Types;
using CoreXModels;

namespace CoreX.Interfaces
{
  public interface ICoreXWrapper
  {
    /// <summary>
    /// Returns the filepath to the geodetic database files; defined by the TGL_GEODATA_PATH environment variable.
    /// </summary>
    string GeodeticDatabasePath { get; }

    /// <summary>
    /// Provides a null conversion between the 2D coordinates in a WGS84 LL point and a XYZ NEE point.
    /// Only the 2D coordinates are used, and directly copied from the LL point to the XY point, maintaining the
    /// X == Longitude and Y == Latitude sense of the relative coordinates
    /// </summary>
    XYZ NullWGSLLToXY(WGS84Point wgsPoint);

    /// <summary>
    /// Takes an LLH and converts it into NEE data.
    /// </summary>
    NEE LLHToNEE(string csib, LLH coordinates, InputAs inputAs);

    /// <summary>
    /// Takes an array of LLH and converts it into an array of NEE.
    /// </summary>
    NEE[] LLHToNEE(string csib, LLH[] coordinates, InputAs inputAs);

    /// <summary>
    /// Converts XYZ coordinates holding LLH data into an XYZ containing NEE data.
    /// </summary>
    XYZ LLHToNEE(string csib, XYZ coordinates, InputAs inputAs);

    /// <summary>
    /// Takes an array of XYZ and uses the Coordinate Service to convert it into NEE data.
    /// </summary>
    XYZ[] LLHToNEE(string csib, XYZ[] coordinates, InputAs inputAs);

    /// <summary>
    /// Converts XYZ coordinates holding NEE grid data into LLH data.
    /// </summary>
    XYZ NEEToLLH(string csib, XYZ coordinates, ReturnAs returnAs = ReturnAs.Radians);

    /// <summary>
    /// Takes an array of XYZ and uses the Coordinate Service to convert it into LLH data.
    /// </summary>
    XYZ[] NEEToLLH(string csib, XYZ[] coordinates, ReturnAs returnAs = ReturnAs.Radians);

    /// <summary>
    /// Takes a NEE and uses the Coordinate Service to convert it into LLH data.
    /// </summary>
    LLH NEEToLLH(string csib, NEE coordinates, ReturnAs returnAs = ReturnAs.Radians);

    /// <summary>
    /// Takes an array of NEE and uses the Coordinate Service to convert it into LLH data.
    /// </summary>
    LLH[] NEEToLLH(string csib, NEE[] coordinates, ReturnAs returnAs = ReturnAs.Radians);

    /// <summary>
    /// Uses the Coordinate Service to convert WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    XYZ WGS84ToCalibration(string csib, WGS84Point wgs84Point, InputAs inputAs);

    /// <summary>
    /// Uses the Coordinate Service to convert an array of WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    XYZ[] WGS84ToCalibration(string csib, WGS84Point[] wgs84Points, InputAs inputAs);

    /// <summary>
    /// Takes the full path and name of a DC file, reads it and uses the Trimble Coordinate service to convert it into a
    /// csib string
    /// </summary>
    string DCFileToCSIB(string filePath);

    /// <summary>
    /// Takes the content of a DC file and returns a CSIB string.
    /// </summary>
    string GetCSIBFromDCFileContent(string fileContent);

    /// <summary>
    /// Gets the list of known datums.
    /// </summary>
    Datum[] GetDatums();

    /// <summary>
    /// Get the ICoordinateSystem record for a given CSD selection.
    /// </summary>
    string GetCSIBFromCSDSelection(string zoneGroupNameString, string zoneNameString);

    /// <summary>
    /// Return the coordinate system for a given DC file string.
    /// </summary>
    CoordinateSystem GetCSDFromDCFileContent(string fileContent);
  }
}
