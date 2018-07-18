/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Trimble.Coordinates.Model;

namespace Bellbird.CoordinatesAPI
{
    public interface ICoordinatesApiClient
    {
        Task<IEnumerable<MySystem>> ListMySystemsAsync();
        Task<MySystem> AddMySystemAsync(string name, string id);

        Task<CoordinateSystem> ImportFromJXLAsync(string JXLFilePath);
        Task<CoordinateSystem> ImportFromJXLDocAsync(string JXLDocument);
        Task<IEnumerable<CoordinateSystem>> SearchAsync(string searchText, double? proximityLatitude, double? proximityLongitude);
        Task<IEnumerable<ZoneGroupX>> ListZoneGroupsAsync(string searchText);
        Task<IEnumerable<ZoneX>> ListZonesInGroupAsync(ZoneGroupX zoneGroup, string searchText, double? proximityLatitude, double? proximityLongitude);
        Task<IEnumerable<ZoneX>> ListZonesAsync(string searchText, double? proximityLatitude, double? proximityLongitude);
        Task<IEnumerable<DatumX>> ListDatumsAsync(string searchText, double? proximityLatitude, double? proximityLongitude);

        Task<DatumInfo> GetZoneDefaultDatumAsync(ZoneInfo zone);
        Task<GeoidInfo> GetZoneDefaultGeoidAsync(ZoneInfo zone);

        Task<IEnumerable<GeoidX>> ListGeoidsAsync(string searchText, double? proximityLatitude, double? proximityLongitude);

        Task<int> GetGeodataFileSizeAsync(string filename);

        Task<ZoneInfo> GetZoneAsync(ZoneX zone);
        Task<DatumInfo> GetDatumAsync(DatumX datum);
        Task<GeoidInfo> GetGeoidAsync(GeoidX geoid);

        Task<CoordinateSystemWithCSIB> CreateAsync(CoordinateSystem customCoordinateSystem);

        Task DownloadGeodataFileAsync(string fileName, string downloadPath, DateTime? ifModifiedSince = null);

        Task<double> GetScaleFactorAsync(string id, LLH llh);

        Task<NEE> GetNEEAsync(string id, NEEType neeType, LLH llh);
    }
}
*/
