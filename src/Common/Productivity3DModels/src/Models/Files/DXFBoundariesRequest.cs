using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Models.Models.Files
{
  public class DXFBoundariesRequest
  {
    public string CSIBFileData { get; set; }

    public ImportedFileType FileType { get; set; }

    public string DXFFileData { get; set; }

    public DxfUnitsType FileUnits { get; set; }

    public uint MaxBoundaries { get; set; }

    public bool ConvertLineStringCoordsToPolygon { get; set; }

    public DXFBoundariesRequest(string csibFileData, ImportedFileType fileType, string dxfFileData, DxfUnitsType fileUnits, uint maxBoundaries, bool convertLineStringCoordsToPolygon)
    {
      CSIBFileData = csibFileData;
      FileType = fileType;
      DXFFileData = dxfFileData;
      FileUnits = fileUnits;
      MaxBoundaries = maxBoundaries;
      ConvertLineStringCoordsToPolygon = convertLineStringCoordsToPolygon;
    }

    public void Validate()
    {
      if (FileType != ImportedFileType.SiteBoundary && FileType != ImportedFileType.Linework)
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "File type must support DXF representation"));
      }

      if (DXFFileData == null || string.IsNullOrEmpty(DXFFileData))
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "DXF file data must be provided"));
      }
    }
  }
}
