namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public interface IDataRepository
  {    
    //Project LoadProject(long legacyProjectId);

    //IEnumerable<Project> LoadProjects(string customerUid, DateTime validAtDate);

    //AssetDeviceIds LoadAssetDevice(string radioSerial, string deviceType);

    //Customer LoadCustomer(string customerUid);

    //CustomerTccOrg LoadCustomerByTccOrgId(string tccOrgUid);

    //CustomerTccOrg LoadCustomerByCustomerUID(string customerUid);

    //Asset LoadAsset(long legacyAssetId);

    //IEnumerable<Subscriptions> LoadManual3DCustomerBasedSubs(string customerUid, DateTime validAtDate);

    //IEnumerable<Subscriptions> LoadAssetSubs(string assetUid, DateTime validAtDate);

    //TWGS84Point[] ParseBoundaryData(string s);
  }
}