using System;
using System.Collections.Generic;
using System.Linq;
using VSS.MasterData.Repositories.DBModels;

namespace VSS.Productivity3D.AssetMgmt3D.AssetStatus
{
  public class MockAssetStatusRepository
  {
    private static readonly List<Asset> _assets = new List<Asset>
                                                  {
                                                    new Asset
                                                    {
                                                      AssetUID = "8982e5e7-1da1-4cf8-a335-ef7b0c6758b6",
                                                      LegacyAssetID = 417113364,
                                                      EquipmentVIN = "6758B6",
                                                      SerialNumber = "EF7B0C",
                                                      AssetType = "Bulldozer",
                                                      LastActionedUtc = new DateTime(2020, 3, 26, 9, 10, 0),
                                                      Name = "Tonka Bulldozer"
                                                    },
                                                    new Asset
                                                    {
                                                      AssetUID = "6cb6fa71-9800-4700-b7ff-c62014970deb",
                                                      LegacyAssetID = 248925381,
                                                      EquipmentVIN = "970DEB",
                                                      SerialNumber = "C62014",
                                                      AssetType = "Dump Truck",
                                                      LastActionedUtc = new DateTime(2020, 3, 30, 14, 45, 4),
                                                      Name = "Tonka Dump Truck"
                                                    },
                                                    new Asset
                                                    {
                                                      AssetUID = "b93dbe5a-7123-42f1-ab20-2ce8cfefa8f6",
                                                      LegacyAssetID = 911158341,
                                                      EquipmentVIN = "2CE8CF",
                                                      SerialNumber = "EFA8F6",
                                                      AssetType = "Crawler Loader",
                                                      LastActionedUtc = new DateTime(2020, 3, 22, 13, 1, 45),
                                                      Name = "Tonka Crawler Loader"
                                                    },
                                                    new Asset
                                                    {
                                                      AssetUID = "6b4dc385-b517-4baa-9419-d9dc58f808c5",
                                                      LegacyAssetID = 1434204015,
                                                      EquipmentVIN = "F808C5",
                                                      SerialNumber = "D9DC58",
                                                      AssetType = "Scraper",
                                                      LastActionedUtc = new DateTime(2020, 3, 30, 18, 11, 23),
                                                      Name = "Tonka Scraper"
                                                    },
                                                    new Asset
                                                    {
                                                      AssetUID = "0a1c60f2-2654-450d-b919-53e806685dd3",
                                                      LegacyAssetID = 1178833682,
                                                      EquipmentVIN = "685DD3",
                                                      SerialNumber = "53E806",
                                                      AssetType = "Excavator",
                                                      LastActionedUtc = new DateTime(2020, 2, 1, 11, 2, 0),
                                                      Name = "Tonka Excavator"
                                                    }
                                                  };

    public static List<Asset> GetAssets(IEnumerable<Guid> uids) => _assets.FindAll(x => uids.Any(y => y.ToString().Contains(x.AssetUID)));
  }
}
