using System;
using System.IO;
using System.Linq;
using System.Text;
using CoreX.Extensions;
using CoreX.Types;
using CoreX.Wrapper.Extensions;
using CoreX.Wrapper.Types;
using CoreXModels;
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

      // CoreX static classes aren't thread safe singletons.
      lock (_lock)
      {
        GeodeticDatabasePath = configStore.GetValueString("TGL_GEODATA_PATH", "Geodata");
        _log.LogInformation($"CoreX {nameof(SetupTGL)}: TGL_GEODATA_PATH='{GeodeticDatabasePath}'");

        SetupTGL();
      }
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
      CsdManagement.csmLoadCoordinateSystemDatabase(xmlData)
        .Validate($"attempting to load coordinate system database '{xmlFilePath}'");

      _log.LogInformation($"CoreX {nameof(SetupTGL)}: GeodeticDatabasePath='{GeodeticDatabasePath}'");

      if (string.IsNullOrEmpty(GeodeticDatabasePath))
      {
        throw new Exception("Environment variable TGL_GEODATA_PATH must be set to the Geodetic data folder.");
      }
      if (!Directory.Exists(GeodeticDatabasePath))
      {
        _log.LogInformation($"Failed to find directory '{GeodeticDatabasePath}' defined by environment variable TGL_GEODATA_PATH.");
      }
      else
      {
        CoreXGeodataLogger.DumpGeodataFiles(_log, GeodeticDatabasePath);
      }

      CsdManagement.csmSetGeodataPath(GeodeticDatabasePath);
      GeodeticX.geoSetGeodataPath(GeodeticDatabasePath);
    }

    /// <summary>
    /// Returns the CSIB from a DC file string.
    /// </summary>
    public string GetCSIBStringFromDCFileContent(string fileContent) => GetCSIB(GetCSIBFromDCFileContent(fileContent));

    private CSMCsibBlobContainer GetCSIBFromDCFileContent(string fileContent)
    {
      // We may receive coordinate system file content that's been uploaded (encoded) from a web api, must decode first.
      fileContent = fileContent.DecodeFromBase64();

      lock (_lock)
      {
        var csmCsibBlobContainer = new CSMCsibBlobContainer();

        // Slow, takes 2.5 seconds, need to speed up somehow?
        var result = CsdManagement.csmGetCSIBFromDCFileData(
          fileContent,
          false,
          Utils.FileListCallBack,
          Utils.EmbeddedDataCallback,
          csmCsibBlobContainer);

        if (result != (int)csmErrorCode.cecSuccess)
        {
          switch ($"{result}")
          {
            case "cecGRID_FILE_OPEN_ERROR":
              {
                var geoidModelName = CalibrationFileHelper.GetGeoidModelName(Encoding.UTF8.GetBytes(fileContent));
                throw new InvalidOperationException($"{nameof(GetCSIBFromDCFileContent)}: Geodata file not found for geoid model '{geoidModelName}'");
              }
            default:
              {
                throw new InvalidOperationException($"{nameof(GetCSIBFromDCFileContent)}: Get CSIB from file content failed, error {result}");
              }
          }
        }

        return csmCsibBlobContainer;
      }
    }

    /// <summary>
    /// Returns the CSIB from a DC file given it's filepath.
    /// </summary>
    public string GetCSIBFromDCFile(string filePath)
    {
      using var streamReader = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
      var dcStr = streamReader.ReadToEnd();

      return GetCSIBStringFromDCFileContent(dcStr);
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
        nee.North,
        nee.East,
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
          nee.North,
          nee.East,
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

    private GEOCsibBlobContainer CreateGeoCsibBlobContainer(string csibStr)
    {
      if (string.IsNullOrEmpty(csibStr))
      {
        throw new ArgumentNullException(csibStr, $"{nameof(CreateGeoCsibBlobContainer)}: csibStr cannot be null");
      }

      var bytes = Array.ConvertAll(Convert.FromBase64String(csibStr), b => unchecked((sbyte)b));
      var csibBlobContainer = new GEOCsibBlobContainer(bytes);

      if (csibBlobContainer.Length < 1)
      {
        throw new Exception($"Failed to set CSIB from base64 string, '{csibStr}'");
      }

      return csibBlobContainer;
    }

    private CSMCsibBlobContainer CreateCSMCsibBlobContainer(string csibStr)
    {
      if (string.IsNullOrEmpty(csibStr))
      {
        throw new ArgumentNullException(csibStr, $"{nameof(CreateGeoCsibBlobContainer)}: csibStr cannot be null");
      }

      var bytes = Array.ConvertAll(Convert.FromBase64String(csibStr), b => unchecked((sbyte)b));
      return new CSMCsibBlobContainer(bytes);
    }


    private IGeodeticXTransformer GeodeticXTransformer(string csib)
    {
      using var geoCsibBlobContainer = CreateGeoCsibBlobContainer(csib);
      using var transformer = new PointerPointer_IGeodeticXTransformer();

      GeodeticX.geoCreateTransformer(geoCsibBlobContainer, transformer)
        .Validate($"attempting to create GeodeticX transformer");

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

      using var csmCsibData = new CSMCsibBlobContainer(data);
      using var csFromCSIB = new CSMCoordinateSystemContainer();

      lock (_lock)
      {
        CsdManagement.csmImportCoordSysFromCsib(csmCsibData, csFromCSIB)
          .Validate("attempting to import coordinate system from CSMCsibBlobContainer");
      }

      return true;
    }

    public Datum[] GetDatums()
    {
      using var returnListStruct = new CSMStringListContainer();

      lock (_lock)
      {
        var resultCode = CsdManagement.csmGetListOfDatums(returnListStruct);

        if (resultCode == csmErrorCode.cecSuccess)
        {
          var datums = returnListStruct.stringList.Split(new[] { CsdManagement.STRING_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
              .Select(s => new Datum(
                datumSystemId: int.Parse(s.Split(CsdManagement.ITEM_SEPERATOR)[0]),
                datumType: int.Parse(s.Split(CsdManagement.ITEM_SEPERATOR)[1]),
                datumName: s.Split(CsdManagement.ITEM_SEPERATOR)[3]));

          if (datums == null)
          {
            throw new Exception($"Error attempting to retrieve list of datums, null result");
          }

          if (!datums.Any())
          {
            throw new Exception($"No datums found");
          }

          return datums.ToArray();
        }
      }

      return null;
    }

    /// <inheritdoc/> 
    private ICoordinateSystem GetDatumBySystemId(int datumSystemId)
    {
      using var datumContainer = new CSMCoordinateSystemContainer();

      lock (_lock)
      {
        CsdManagement.csmGetDatumFromCSDSelectionById((uint)datumSystemId, false, null, null, datumContainer)
          .Validate($"attempting to retrieve datum {datumSystemId} by id.");
      }

      return datumContainer.GetSelectedRecord();
    }

    /// <inheritdoc/> 
    public string GetCSIBFromCSDSelection(string zoneGroupNameString, string zoneNameQueryString)
    {
      lock (_lock)
      {
        using var retStructZoneGroups = new CSMStringListContainer();

        CsdManagement.csmGetListOfZoneGroups(retStructZoneGroups)
          .Validate("attempting to retrieve list of zone groups");

        var zoneGroups = retStructZoneGroups
          .stringList
          .Split(new[] { CsdManagement.STRING_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries)
          .Select(s => s.Split(CsdManagement.ITEM_SEPERATOR)[1]);

        if (!zoneGroups.Any())
        {
          throw new Exception("The count of zone groups should be greater than 0");
        }

        var zoneGroupName = zoneGroupNameString.Substring(zoneGroupNameString.IndexOf(",") + 1);
        using var retStructListOfZones = new CSMStringListContainer();

        CsdManagement.csmGetListOfZones(zoneGroupName, retStructListOfZones)
          .Validate($"attempting to retrieve list of zones for group '{zoneGroupName}'");

        var zones = retStructListOfZones
          .stringList
          .Split(new[] { CsdManagement.STRING_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

        if (!zones.Any())
        {
          throw new Exception($"The count of zones in {zoneGroupName} should be greater than 0");
        }

        var zoneElement = Array.Find(zones, element => element.EndsWith(zoneNameQueryString, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(zoneElement))
        {
          throw new Exception($"Could not find '{zoneNameQueryString}' in the list of zones for group '{zoneGroupName}'");
        }

        var zoneName = zoneElement.Substring(zoneElement.IndexOf(",") + 1);
        var items = zoneElement.Split(CsdManagement.ITEM_SEPERATOR);

        var zoneId = uint.Parse(items[0]);

        using var retCsStruct = new CSMCoordinateSystemContainer();

        CsdManagement.csmGetCoordinateSystemFromCSDSelectionDefaults(zoneGroupName, zoneName, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, retCsStruct)
          .Validate($"attempting to retrieve coordinate system from CSD selection; zone group: '{zoneGroupName}', zone: {zoneName}");

        var coordinateSystem = retCsStruct.GetSelectedRecord();

        // Many of our test calibration files fail validation; is this expected or do we have a parsing problem? 
        // This validation logic was taken from TGL unit test classes, may not be correctly implemented.
        // coordinateSystem.Validate();

        var zoneID = unchecked((uint)coordinateSystem.ZoneSystemId());
        var datumID = unchecked((uint)coordinateSystem.DatumSystemId());
        var geoidID = unchecked((uint)coordinateSystem.GeoidSystemId());

        if (coordinateSystem.DatumSystemId() > 0)
        {
          return GetCSIBFromCSD(coordinateSystem);
        }
        else
        {
          var datumResult = GetDatumBySystemId(1034);
          using var csibFromIDs = new CSMCsibBlobContainer();

          CsdManagement.csmGetCSIBFromCSDSelectionById(zoneID, datumId: 1034, geoidId: 0, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, csibFromIDs);

          var csib = GetCSIB(csibFromIDs);

          if (!string.IsNullOrEmpty(csib))
          {
            return csib;
          }
        }

        throw new Exception($"Error attempting to retrieve coordinate system from CSD selection; zone group: '{zoneGroupName}', zone: {zoneName}, no datum found");
      }
    }

    /// <inheritdoc/>
    public CoordinateSystem GetCSDFromCSIB(string csibString)
    {
      if (string.IsNullOrEmpty(csibString))
      {
        throw new ArgumentNullException(nameof(csibString), "CSIB string cannot be null");
      }

      using var csContainer = new CSMCoordinateSystemContainer();

      var csmCsibData = CreateCSMCsibBlobContainer(csibString);

      lock (_lock)
      {
        CsdManagement.csmImportCoordSysFromCsib(csmCsibData, csContainer).
          Validate("attempting to import coordinate system from CSIB");
      }

      return ConvertICoordinateSystem(csContainer.GetSelectedRecord());
    }

    /// <inheritdoc/>
    public CoordinateSystem GetCSDFromDCFileContent(string dcFileStr)
    {
      if (string.IsNullOrEmpty(dcFileStr))
      {
        throw new ArgumentNullException(nameof(dcFileStr), "DC file string cannot be null");
      }

      // We may receive coordinate system file content that's been uploaded (encoded) from a web api, must decode first.
      var fileContent = dcFileStr.DecodeFromBase64();

      using var csContainer = new CSMCoordinateSystemContainer();

      lock (_lock)
      {
        CsdManagement.csmGetCoordinateSystemFromDCFile(fileContent, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, csContainer)
          .Validate("attempting to retrieve the DC file's CSD");
      }

      return ConvertICoordinateSystem(csContainer.GetSelectedRecord());
    }

    private CoordinateSystem ConvertICoordinateSystem(ICoordinateSystem csRecord)
    {
      // Many of our test calibration files fail validation; is this expected or do we have a parsing problem? 
      // This validation logic was taken from TGL unit test classes, may not be correctly implemented.
      // csRecord.Validate();

      var coordinateSystem = new CoordinateSystem
      {
        SystemName = csRecord.SystemName(),
        DatumSystemId = csRecord.DatumSystemId(),
        GeoidInfo = new GeoidInfo()
        {
          GeoidFileName = csRecord.GeoidFileName(),
          GeoidName = csRecord.GeoidName()
        },
        ZoneInfo = new ZoneInfo()
        {
          ShiftGridFileName = csRecord.ZoneShiftGridFileName(),
          SnakeGridFileName = csRecord.SnakeGridFileName()
        },
        DatumInfo = new DatumInfo()
        {
          DatumName = csRecord.DatumName(),
          DatumType = Enum.GetName(typeof(csmDatumTypes), csRecord.DatumType()).Substring("cdt".Length),
          DatumSystemId = csRecord.DatumSystemId()
          // Vertical Datum Name ?
        }
      };

      if (csRecord.HasGeoid())
      {
        // Taken from CoreX.UnitTests.TestSelectRecords.cs.
        coordinateSystem.GeoidInfo.GeoidSystemId = csRecord.GeoidSystemId() < 0 ? 0 : csRecord.GeoidSystemId();
      }

      return coordinateSystem;
    }

    private string GetCSIBFromCSD(ICoordinateSystem coordinateSystem)
    {
      using var retStructFromICoordinateSystem = new CSMCsibBlobContainer();

      lock (_lock)
      {
        CsdManagement.csmGetCSIBFromCoordinateSystem(coordinateSystem, false, Utils.FileListCallBack, Utils.EmbeddedDataCallback, retStructFromICoordinateSystem)
          .Validate("attempting to get CSMCsibBlobContainer from ICoordinateSystem");
      }

      return GetCSIB(retStructFromICoordinateSystem);
    }

    private string GetCSIB(CSMCsibBlobContainer csibBlobContainer) =>
      Convert.ToBase64String(Array.ConvertAll(Utils.IntPtrToSByte(csibBlobContainer.pCSIBData, (int)csibBlobContainer.CSIBDataLength), sb => unchecked((byte)sb)));

    #region Dispose pattern

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

    #endregion
  }
}
