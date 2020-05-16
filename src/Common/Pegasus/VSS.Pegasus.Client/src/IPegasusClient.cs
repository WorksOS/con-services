using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Pegasus.Client.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Pegasus.Client
{
  public interface IPegasusClient
  {
    Task<TileMetadata> GenerateDxfTiles(
      string dcFileName,
      string dxfFileName,
      DxfUnitsType dxfUnitsType,
      IHeaderDictionary customHeaders,
      Action<IHeaderDictionary> setJobIdAction);

    Task<TileMetadata> GenerateGeoTiffTiles(
      string geoTiffFileName,
      IHeaderDictionary customHeaders,
      Action<IHeaderDictionary> setJobIdAction);

    Task<bool> DeleteTiles(string fileName, IHeaderDictionary customHeaders);
  }
}
