using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Models.Models.Files
{
  public class DXFBoundariesRequest
  {
    [JsonProperty(PropertyName = "csibFileData", Required = Required.Always)]
    public string CSIBFileData { get; set; }

    [JsonProperty(PropertyName = "fileType", Required = Required.Always)]
    public ImportedFileType FileType { get; set; }

    [JsonProperty(PropertyName = "dxfFileData", Required = Required.Always)]
    public string DXFFileData { get; set; }

    [JsonProperty(PropertyName = "fileUnits", Required = Required.Always)]
    public DxfUnitsType FileUnits { get; set; }

    [JsonProperty(PropertyName = "maxBoundaries", Required = Required.Always)]
    public uint MaxBoundaries { get; set; }

    [JsonProperty(PropertyName = "convertLineStringCoordsToPolygon", Required = Required.Default)]
    public bool ConvertLineStringCoordsToPolygon { get; set; }

    private DXFBoundariesRequest()
    {
    }

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
