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
    public string CSIB;

    private GEOCsibBlobContainer _geoCsibBlobContainer;
    private CSMCsibBlobContainer _csmCsibBlobContainer;
    private PointerPointer_IGeodeticXTransformer _transformer;
    private PointerPointer_IGeodeticXTransformer Transformer => GetGeodeticXTransformer();

    static CoreX()
    {
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
      var xmlFilePath = Path.Combine(ROOT_DATA_FOLDER, DATABASE_PATH, "CoordSystemDatabase.xml");

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
    /// Extract the CSIB from a DC file string.
    /// </summary>
    public string GetCSIBFromDCFileContent(string fileContent)
    {
      var csmCsibBlobContainer = new CSMCsibBlobContainer();

      // Slow, 2.5 seconds, need to speed up.
      var resultCode = (csmErrorCode)CsdManagementPINVOKE.csmGetCSIBFromDCFileData(fileContent, false, CppFileListCallback.getCPtr(Utils.FileListCallBack), CppEmbeddedDataCallback.getCPtr(Utils.EmbeddedDataCallback), CSMCsibBlobContainer.getCPtr(csmCsibBlobContainer));

      if (resultCode != (int)csmErrorCode.cecSuccess)
      {
        throw new Exception($"{nameof(GetCSIBFromDCFileContent)}: Get CSIB from file content failed, error {resultCode}");
      }

      var bytes = Utils.IntPtrToSByte(csmCsibBlobContainer.pCSIBData, (int)csmCsibBlobContainer.CSIBDataLength);

      return Convert.ToBase64String(Array.ConvertAll<sbyte, byte>(bytes, sb => unchecked((byte)sb)));
    }

    /// <summary>
    /// Gets the CoreX CSIB for a given DC file.
    /// </summary>
    /// <param name="filePath">Fully qualified path to the source .DC file.</param>
    public string GetCSIBFromDCFile(string filePath)
    {
      string dcStr;
      using (var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
      {
        dcStr = streamReader.ReadToEnd();
      }

      return GetCSIBFromDCFileContent(dcStr);
    }

    /// <summary>
    /// Sets the CoreX CSIB for a given DC file.
    /// </summary>
    /// <param name="filePath">Fully qualified path to the source .DC file.</param>
    public CoreX SetCSIBFromDCFile(string filePath)
    {
      string dcStr;
      using (var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8))
      {
        dcStr = streamReader.ReadToEnd();
      }

      _csmCsibBlobContainer = new CSMCsibBlobContainer();

      // Slow, 2.5 seconds, need to speed up.
      var resultCode = (csmErrorCode)CsdManagementPINVOKE.csmGetCSIBFromDCFileData(dcStr, false, CppFileListCallback.getCPtr(Utils.FileListCallBack), CppEmbeddedDataCallback.getCPtr(Utils.EmbeddedDataCallback), CSMCsibBlobContainer.getCPtr(_csmCsibBlobContainer));

      if (resultCode != (int)csmErrorCode.cecSuccess)
      {
        throw new Exception($"TransformSelectCsdSystem: '{filePath}' failed, error {resultCode}");
      }

      var bytes = Utils.IntPtrToSByte(_csmCsibBlobContainer.pCSIBData, (int)_csmCsibBlobContainer.CSIBDataLength);

      CSIB = Convert.ToBase64String(Array.ConvertAll<sbyte, byte>(bytes, sb => unchecked((byte)sb)));

      _geoCsibBlobContainer = new GEOCsibBlobContainer(bytes);

      if (_geoCsibBlobContainer.Length < 1)
      {
        throw new Exception($"Failed to set CSIB from coordinate system file '{filePath}'");
      }

      return this;
    }

    public CoreX SetCsibFromBase64String(string csibStr)
    {
      var bytes = Array.ConvertAll(Convert.FromBase64String(csibStr), b => unchecked((sbyte)b));
      _geoCsibBlobContainer = new GEOCsibBlobContainer(bytes);

      if (_geoCsibBlobContainer.Length < 1)
      {
        throw new Exception($"Failed to set CSIB from base64 string, '{csibStr}'");
      }

      return this;
    }

    /// <summary>
    /// Transform an NEE to LLH with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>Returns LLH object in radians.</returns>
    public LLH TransformNEEToLLH(NEE nee, CoordinateTypes fromType, CoordinateTypes toType)
    {
      Transformer.get().Transform(
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
    public LLH[] TransformNEEToLLH(NEE[] coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var llhCoordinates = new LLH[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        var nee = coordinates[i];

        llhCoordinates[i] = TransformNEEToLLH(nee, fromType, toType);
      }

      return llhCoordinates;
    }

    /// <summary>
    /// Transform an LLH to NEE with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>A NEE point of the LLH provided coordinates in radians.</returns>
    public NEE TransformLLHToNEE(LLH coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      Transformer.get().Transform(
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
    public NEE[] TransformLLHToNEE(LLH[] coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      var neeCoordinates = new NEE[coordinates.Length];

      for (var i = 0; i < coordinates.Length; i++)
      {
        var llh = coordinates[i];
        neeCoordinates[i] = TransformLLHToNEE(llh, fromType, toType);
      }

      return neeCoordinates;
    }

    /// <summary>
    /// Create the GeodeticXTransformer for use with this object instance.
    /// </summary>
    private PointerPointer_IGeodeticXTransformer GetGeodeticXTransformer()
    {
      if (_transformer == null)
      {
        _transformer = new PointerPointer_IGeodeticXTransformer();

        var errorCode = GeodeticX.geoCreateTransformer(_geoCsibBlobContainer, _transformer);

        if (errorCode != geoErrorCode.gecSuccess)
        {
          throw new Exception("Failed to create GeodeticX transformer");
        }
      }

      return _transformer;
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
