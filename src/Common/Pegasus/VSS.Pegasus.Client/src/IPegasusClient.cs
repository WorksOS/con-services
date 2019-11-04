using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Pegasus.Client
{
  public interface IPegasusClient
  {
    Task<TileMetadata> GenerateDxfTiles(
      string dcFileName,
      string dxfFileName,
      DxfUnitsType dxfUnitsType,
      IDictionary<string, string> customHeaders,
      Action<IDictionary<string, string>> setJobIdAction);

    Task<TileMetadata> GenerateGeoTiffTiles(
      string geoTiffFileName,
      IDictionary<string, string> customHeaders,
      Action<IDictionary<string, string>> setJobIdAction);

    Task<bool> DeleteTiles(string fileName, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
  }
}
