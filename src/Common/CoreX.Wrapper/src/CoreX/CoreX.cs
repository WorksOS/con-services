using System;
using System.IO;
using System.Text;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.Extensions;
using CoreX.Wrapper.Types;
using Microsoft.Extensions.Logging;
using Trimble.CsdManagementWrapper;
using Trimble.GeodeticXWrapper;
using VSS.Common.Abstractions.Configuration;

namespace CoreX.Wrapper
{
  public class CoreX : IDisposable
  {
    public string GeodeticDatabasePath;

    private static readonly object _lock = new object();
    private readonly ILogger _log;

    public CoreX(ILoggerFactory loggerFactory, IConfigurationStore configStore)
    {
      _log = loggerFactory.CreateLogger<CoreX>();

      lock (_lock)
      {
        GeodeticDatabasePath = configStore.GetValueString("TGL_GEODATA_PATH", "GeoData");
        _log.LogInformation($"CoreX {nameof(SetupTGL)}: TGL_GEODATA_PATH='{GeodeticDatabasePath}'");

        SetupTGL();
      }

      CoreXGeodataLogger.DumpGeodataFiles(_log, GeodeticDatabasePath);
    }

    /// <summary>
    /// Setup the underlying CoreXDotNet singleton management classes.
    /// </summary>
    private void SetupTGL()
    {
      var xmlFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "CoordSystemDatabase.xml");

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

      _log.LogInformation($"CoreX {nameof(SetupTGL)}: GeodeticDatabasePath='{GeodeticDatabasePath}'");

      if (string.IsNullOrEmpty(GeodeticDatabasePath))
      {
        throw new Exception("Environment variable TGL_GEODATA_PATH must be set to the Geodetic data folder.");
      }
      if (!Directory.Exists(GeodeticDatabasePath))
      {
        throw new Exception($"Failed to find directory '{GeodeticDatabasePath}' defined by environment variable TGL_GEODATA_PATH.");
      }

      // CoreX static classes aren't thread safe singletons.
      CsdManagementPINVOKE.csmSetGeodataPath(GeodeticDatabasePath);
      GeodeticX.geoSetGeodataPath(GeodeticDatabasePath);
    }

    /// <summary>
    /// Returns the CSIB from a DC file string.
    /// </summary>
    public string GetCSIBFromDCFileContent(string fileContent)
    {
      // We may receive coordinate system file content that's been uploaded (encoded) from a web api, must decode first.
      fileContent = fileContent.DecodeFromBase64();

      using var csmCsibBlobContainer = new CSMCsibBlobContainer();

      // CsdManagementPINVOKE isn't a thread safe singleton.
      lock (_lock)
      {
        // Slow, takes 2.5 seconds, need to speed up somehow?
        var result = (csmErrorCode)CsdManagementPINVOKE.csmGetCSIBFromDCFileData(
          fileContent,
          false,
          CppFileListCallback.getCPtr(Utils.FileListCallBack),
          CppEmbeddedDataCallback.getCPtr(Utils.EmbeddedDataCallback),
          CSMCsibBlobContainer.getCPtr(csmCsibBlobContainer));

        if (result != (int)csmErrorCode.cecSuccess)
        {
          throw new InvalidOperationException($"{nameof(GetCSIBFromDCFileContent)}: Get CSIB from file content failed, error {result}");
        }

        var bytes = Utils.IntPtrToSByte(csmCsibBlobContainer.pCSIBData, (int)csmCsibBlobContainer.CSIBDataLength);

        return Convert.ToBase64String(Array.ConvertAll(bytes, sb => unchecked((byte)sb)));
      }
    }

    /// <summary>
    /// Returns the CSIB from a DC file given it's filepath.
    /// </summary>
    public string GetCSIBFromDCFile(string filePath)
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
      using var transformer = GeodeticXTransformer(csib);

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

      using var transformer = GeodeticXTransformer(csib);

      for (var i = 0; i < coordinates.Length; i++)
      {
        var nee = coordinates[i];

        transformer.Transform(
          (geoCoordinateTypes)fromType,
          nee.East,
          nee.North,
          nee.Elevation,
          (geoCoordinateTypes)toType,
          out var toY, out var toX, out var toZ);

        // The toX and toY parameters mirror the order of the input parameters fromX and fromY; they are not grid coordinate positions.
        llhCoordinates[i] = new LLH
        {
          Latitude = toY,
          Longitude = toX,
          Height = toZ
        };
      }

      return llhCoordinates;
    }

    /// <summary>
    /// Transform an LLH to NEE with variable from and to coordinate type inputs.
    /// </summary>
    /// <returns>A NEE point of the LLH provided coordinates in radians.</returns>
    public NEE TransformLLHToNEE(string csib, LLH coordinates, CoordinateTypes fromType, CoordinateTypes toType)
    {
      using var transformer = GeodeticXTransformer(csib);

      transformer.Transform(
        (geoCoordinateTypes)fromType,
        coordinates.Latitude,
        coordinates.Longitude,
        coordinates.Height,
        (geoCoordinateTypes)toType,
        out var toY, out var toX, out var toZ);

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

      using var transformer = GeodeticXTransformer(csib);

      for (var i = 0; i < coordinates.Length; i++)
      {
        var llh = coordinates[i];

        transformer.Transform(
          (geoCoordinateTypes)fromType,
          llh.Latitude,
          llh.Longitude,
          llh.Height,
          (geoCoordinateTypes)toType,
          out var toY, out var toX, out var toZ);

        neeCoordinates[i] = new NEE
        {
          North = toY,
          East = toX,
          Elevation = toZ
        };
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

    private IGeodeticXTransformer GeodeticXTransformer(string csib)
    {
      using var geoCsibBlobContainer = CreateCsibBlobContainer(csib);
      using var transformer = new PointerPointer_IGeodeticXTransformer();

      var result = GeodeticX.geoCreateTransformer(geoCsibBlobContainer, transformer);

      if (result != geoErrorCode.gecSuccess)
      {
        throw new Exception($"Failed to create GeodeticX transformer, error '{result}'");
      }

      return transformer.get();
    }

    private static bool ValidateCsib(string csib)
    {
      var sb = new StringBuilder();
      var bytes = Encoding.ASCII.GetBytes(csib);

      for (var i = 0; i < bytes.Length; i++)
      {
        sb.Append(bytes[i] + " ");
      }

      var blocks = sb.ToString().TrimEnd().Split(' ');
      var data = new sbyte[blocks.Length];

      var index = 0;
      foreach (var b in blocks)
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
      { }

      _disposed = true;
    }
  }
}
