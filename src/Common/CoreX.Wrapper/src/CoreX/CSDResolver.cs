using System;
using CoreX.Wrapper.Extensions;
using CoreX.Wrapper.Types;
using CoreXModels;
using Trimble.CsdManagementWrapper;

namespace CoreX.Wrapper
{
  public class CSDResolver
  {
    

    /// <inheritdoc/>
    public CoordinateSystem GetCSDFromCSIB(string csibString)
    {
      if (string.IsNullOrEmpty(csibString))
      {
        throw new ArgumentNullException(nameof(csibString), "CSIB string cannot be null");
      }

      using var csContainer = new CSMCoordinateSystemContainer();

      var csmCsibData = CreateCSMCsibBlobContainer(csibString);

      lock (TGLLock.CsdManagementLock)
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

      lock (TGLLock.CsdManagementLock)
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

    private CSMCsibBlobContainer CreateCSMCsibBlobContainer(string csibStr)
    {
      if (string.IsNullOrEmpty(csibStr))
      {
        throw new ArgumentNullException(csibStr, $"{nameof(CreateCSMCsibBlobContainer)}: csibStr cannot be null");
      }

      var bytes = Array.ConvertAll(Convert.FromBase64String(csibStr), b => unchecked((sbyte)b));
      return new CSMCsibBlobContainer(bytes);
    }
  }
}
