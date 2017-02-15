using System;

namespace VSS.TagFileAuth.Service.WebApi.Models
{
  // map CG/NG/CG serviceTypes if need be. Probably not as only strings passed?
  //   Family  CG                                   NG
  //   Asset   16  3D Project Monitoring            13
  //   Cust    18  Manual 3D Project Monitoring     15 
  //   Proj    23  Landfill                         19
  //   Proj    24  Project Monitoring               20

  public class Subscription
  {
    public string ServiceFamilyType { get; set; }
    public string ServiceType { get; set; }
    public string CustomerUid { get; set; }
    public string ProjectUid { get; set; }
    public string AssetUid { get; set; }
    public DateTime StartKeyDate { get; set; }
    public DateTime EndKeyDate { get; set; }


    public override bool Equals(object obj)
      {
        var otherAsset = obj as Subscription;
        if (otherAsset == null) return false;
        return otherAsset.ServiceFamilyType == ServiceFamilyType
          && otherAsset.ServiceType == ServiceType
          && otherAsset.CustomerUid == CustomerUid
          && otherAsset.ProjectUid == ProjectUid
          && otherAsset.AssetUid == AssetUid;
      }
      public override int GetHashCode() { return 0; }
    }
  }