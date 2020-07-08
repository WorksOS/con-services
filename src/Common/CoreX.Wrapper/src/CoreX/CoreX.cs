using System;
using System.IO;
using System.Text;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.Types;
using Trimble.CsdManagementWrapper;
using Trimble.GeodeticXWrapper;

namespace CoreX.Wrapper
{
  public class CoreX : IDisposable
  {
    private PointerPointer_IGeodeticXTransformer _transformer;

    static CoreX()
    {
      // CoreX library appears to not be thread safe. If you attempt this from the default constructor you'll hit C++ 
      // memory errors in the CsdManagementPINVOKE() call.
      SetupTGL();
    }

    /// <summary>
    /// Setup the underlying CoreXDotNet singleton management classes.
    /// </summary>
    private static void SetupTGL()
    {
      const string ROOT_DATA_FOLDER = "Data";
      const string DATABASE_PATH = "TGL_CsdDatabase";

      var geodataPath = Path.Combine(ROOT_DATA_FOLDER, "GeoData");
      var xmlFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), ROOT_DATA_FOLDER, DATABASE_PATH, "CoordSystemDatabase.xml");

      if (!File.Exists(xmlFilePath))
      {
        throw new Exception($"Failed to find TGL CSD database file '{xmlFilePath}'.");
      }

      using var reader = new StreamReader(xmlFilePath);
      var xmlData = reader.ReadToEnd();
      var resultCode = (csmErrorCode)CsdManagementPINVOKE.csmLoadCoordinateSystemDatabase(xmlData);

      if (resultCode != (int)csmErrorCode.cecSuccess)
      {
        throw new Exception($"Error '{resultCode}' attempting to load coordinate system database '{xmlFilePath}'");
      }

      CsdManagementPINVOKE.csmSetGeodataPath(geodataPath);
      GeodeticX.geoSetGeodataPath(geodataPath);
    }

    /// <summary>
    /// Returns the CSIB from a DC file string.
    /// </summary>
    public static string GetCSIBFromDCFileContent(string fileContent)
    {
      var csmCsibBlobContainer = new CSMCsibBlobContainer();

      // Slow, takes 2.5 seconds, need to speed up somehow?
      var result = (csmErrorCode)CsdManagementPINVOKE.csmGetCSIBFromDCFileData(fileContent, false, CppFileListCallback.getCPtr(Utils.FileListCallBack), CppEmbeddedDataCallback.getCPtr(Utils.EmbeddedDataCallback), CSMCsibBlobContainer.getCPtr(csmCsibBlobContainer));

      if (result != (int)csmErrorCode.cecSuccess)
      {
        throw new Exception($"{nameof(GetCSIBFromDCFileContent)}: Get CSIB from file content failed, error {result}");
      }

      var bytes = Utils.IntPtrToSByte(csmCsibBlobContainer.pCSIBData, (int)csmCsibBlobContainer.CSIBDataLength);

      return Convert.ToBase64String(Array.ConvertAll(bytes, sb => unchecked((byte)sb)));
    }

    /// <summary>
    /// Returns the CSIB from a DC file given it's filepath.
    /// </summary>
    public static string GetCSIBFromDCFile(string filePath)
    {
      using var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
      var dcStr = streamReader.ReadToEnd();

      return GetCSIBFromDCFileContent(dcStr);
    }

    public static bool ValidateCsibString(string csib) => ValidateCsib(csib);

    /// <summary>
    /// Transform an NEE to LLH with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns LLH object in radians.</returns>
    public LLH TransformNEEToLLH(string csib, NEE nee, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var transformer = GeodeticXTransformer(CreateCsibBlobContainer(csib)).get();

      transformer.Transform(
        (geoCoordinateTypes)fromType,
        nee.East,
        nee.North,
        nee.Elevation,
        (geoCoordinateTypes)toType,
        out var toY, out var toX, out var toZ);

      // The toX and toY parameters mirror the order of the input parameters fromX and fromY; they are not grid coordinate positions.
      return new LLH
      {
        Latitude = toY,
        Longitude = toX,
        Height = toZ
      };
    }

    /// <summary>
    /// Transform an array of NEE points to an array of LLH coordinates with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns an array of LLH coordinates in radians.</returns>
    public LLH[] TransformNEEToLLH(string csib, NEE[] coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var llhCoordinates = new LLH[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        var nee = coordinates[i];

        llhCoordinates[i] = TransformNEEToLLH(csib, nee, fromType, toType);
      }

      return llhCoordinates;
    }

    /// <summary>
    /// Transform an LLH to NEE with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>A NEE point of the LLH provided coordinates in radians.</returns>
    public NEE TransformLLHToNEE(string csib, LLH coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var transformer = GeodeticXTransformer(CreateCsibBlobContainer(csib)).get();

      transformer.Transform(
        (geoCoordinateTypes)fromType,
        coordinates.Latitude,
        coordinates.Longitude,
        coordinates.Height,
        (geoCoordinateTypes)toType,
        out var toX, out var toY, out var toZ);

      return new NEE
      {
        North = toY,
        East = toX,
        Elevation = toZ
      };
    }

    /// <summary>
    /// Transform an array of LLH coordinates to an array of NEE points with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns an array of NEE points in radians.</returns>
    public NEE[] TransformLLHToNEE(string csib, LLH[] coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var neeCoordinates = new NEE[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        var llh = coordinates[i];
        neeCoordinates[i] = TransformLLHToNEE(csib, llh, fromType, toType);
      }

      return neeCoordinates;
    }

    private GEOCsibBlobContainer CreateCsibBlobContainer(string csibStr)
    {
      if (string.IsNullOrEmpty(csibStr))
      {
        throw new ArgumentNullException(csibStr, $"{nameof(CreateCsibBlobContainer)}: csibStr cannot be null");
      }

      var bytes = Array.ConvertAll(Convert.FromBase64String(csibStr), b => unchecked((sbyte)b));
      var geoCsibBlobContainer = new GEOCsibBlobContainer(bytes);

      if (geoCsibBlobContainer.Length < 1)
      {
        throw new Exception($"Failed to set CSIB from base64 string, '{csibStr}'");
      }

      return geoCsibBlobContainer;
    }

    private PointerPointer_IGeodeticXTransformer GeodeticXTransformer(GEOCsibBlobContainer geoCsibBlobContainer)
    {
      _transformer = new PointerPointer_IGeodeticXTransformer();
      var result = GeodeticX.geoCreateTransformer(geoCsibBlobContainer, _transformer);

      if (result != geoErrorCode.gecSuccess)
      {
        throw new Exception($"Failed to create GeodeticX transformer, error '{result}'");
      }

      return _transformer;
    }

    private static bool ValidateCsib(string csib)
    {
      csib = "84 78 76 32 67 83 73 66 0 0 0 0 0 0 38 64 66 101 108 103 105 117 109 47 76 97 109 98 101 114 116 0 48 48 54 50 49 49 65 76 97 109 98 101 114 116 32 55 50 32 40 99 111 114 114 101 99 116 105 101 103 114 105 100 41 48 70 66 101 108 103 105 117 109 47 76 97 109 98 101 114 116 0 76 97 109 98 101 114 116 32 55 50 32 40 99 111 114 114 101 99 116 105 101 103 114 105 100 41 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 3 0 0 66 101 108 103 105 117 109 32 50 48 48 53 0 0 73 110 116 101 114 110 97 116 105 111 110 97 108 32 49 57 50 52 0 0 0 0 0 0 229 84 88 65 60 92 141 252 235 63 88 65 121 55 199 65 153 183 90 192 151 234 207 214 1 38 74 64 4 15 218 147 84 238 89 192 169 167 61 127 44 96 187 190 86 211 31 62 134 149 194 62 108 252 20 33 224 186 226 190 17 202 166 83 253 255 239 63 4 0 0 24 45 68 84 251 33 249 63 189 222 38 56 157 131 179 63 39 49 8 28 134 153 84 65 119 190 159 26 128 79 2 65 0 0 0 0 0 0 240 63 0 0 0 0 0 0 240 63 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 128 186 48 246 172 147 236 63 158 158 194 4 10 213 235 63 1 0 0 72 66 71 48 51 32 40 66 101 108 103 105 117 109 41 0 0 72 66 71 48 51 46 71 71 70 0 0 0 0 1 67 58 92 80 114 111 103 114 97 109 68 97 116 97 92 84 114 105 109 98 108 101 92 71 101 111 68 97 116 97 92 92 66 101 108 103 105 117 109 46 115 103 102 0 0 67 58 92 80 114 111 103 114 97 109 68 97 116 97 92 84 114 105 109 98 108 101 92 71 101 111 68 97 116 97 92 92 66 101 108 103 105 117 109 46 115 103 102 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1 3 66 0 101 0 108 0 103 0 105 0 117 0 109 0 47 0 76 0 97 0 109 0 98 0 101 0 114 0 116 0 0 0 76 0 97 0 109 0 98 0 101 0 114 0 116 0 32 0 55 0 50 0 32 0 40 0 99 0 111 0 114 0 114 0 101 0 99 0 116 0 105 0 101 0 103 0 114 0 105 0 100 0 41 0 0 0 0 0 0 0 66 0 101 0 108 0 103 0 105 0 117 0 109 0 32 0 50 0 48 0 48 0 53 0 0 0 73 0 110 0 116 0 101 0 114 0 110 0 97 0 116 0 105 0 111 0 110 0 97 0 108 0 32 0 49 0 57 0 50 0 52 0 0 0 0 0 0 0 72 0 66 0 71 0 48 0 51 0 32 0 40 0 66 0 101 0 108 0 103 0 105 0 117 0 109 0 41 0 0 0 0 0 0 0 0 0 0 0 0 0 123 78 83 69 61 54 49 57 48 59 80 83 69 61 51 49 51 55 48 59 125 0 0 0 0 123 86 83 69 61 53 55 49 48 59 86 68 69 61 53 49 49 48 59 125 0";
      StringBuilder sb = new StringBuilder();
      byte[] bytes = Encoding.ASCII.GetBytes(csib);

      for (var i = 0; i < bytes.Length; i++)
      {
        sb.Append(bytes[i] + " ");
      }

      string[] blocks = sb.ToString().TrimEnd().Split(' ');
      sbyte[] data = new sbyte[blocks.Length];

      int index = 0;
      foreach (string b in blocks)
      {
        data[index++] = (sbyte)Convert.ToByte(b);
      }

      var csmCsibData = new CSMCsibBlobContainer(data);
      var csFromCSIB = new CSMCoordinateSystemContainer();
      var csmErrorCode = CsdManagement.csmImportCoordSysFromCsib(csmCsibData, csFromCSIB);

      return csmErrorCode == csmErrorCode.cecSuccess;
    }

    private bool _disposed = false;

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
      {
        return;
      }

      if (disposing)
      {
        GeodeticX.geoDestroyTransformer(_transformer);
      }

      _disposed = true;
    }
  }
}
