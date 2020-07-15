﻿using System;
using System.IO;
using System.Text;
using CoreX.Models;
using CoreX.Types;
using CoreX.Wrapper.Extensions;
using CoreX.Wrapper.Types;
using Trimble.CsdManagementWrapper;
using Trimble.GeodeticXWrapper;

namespace CoreX.Wrapper
{
  public class CoreX : IDisposable
  {
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
      // We may receive coordinate system file content that's been uploaded (encoded) from a web api, must decode first.
      fileContent = fileContent.DecodeFromBase64();

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
      using var transformer = GeodeticXTransformer(CreateCsibBlobContainer(csib)).get();

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
      using var transformer = GeodeticXTransformer(CreateCsibBlobContainer(csib)).get();

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
      var transformer = new PointerPointer_IGeodeticXTransformer();
      var result = GeodeticX.geoCreateTransformer(geoCsibBlobContainer, transformer);

      if (result != geoErrorCode.gecSuccess)
      {
        throw new Exception($"Failed to create GeodeticX transformer, error '{result}'");
      }

      return transformer;
    }

    private static bool ValidateCsib(string csib)
    {
      var sb = new StringBuilder();
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
      { }

      _disposed = true;
    }
  }
}
