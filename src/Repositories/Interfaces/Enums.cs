namespace VSS.TagFileAuth.Service.Repositories.Interfaces
{
  // todo put in table?
  public enum ServiceTypeEnumCG
  {
    Unknown = 0,
    e3DProjectMonitoring = 16,
    Manual3DProjectMonitoring = 18,
    Landfill = 23,
    ProjectMonitoring = 24
  }

  public enum ServiceTypeEnumNG
  {
    Unknown = 0,
    e3DProjectMonitoring = 13,
    Manual3DProjectMonitoring = 15,
    Landfill = 19,
    ProjectMonitoring = 20
  }

  // todo check if these are the same in NG
  public enum DeviceTypeEnumCG
  {
    MANUALDEVICE = 0,
    PL121 = 1,
    PL321 = 2,
    Series522 = 3,
    Series523 = 4,
    Series521 = 5,
    SNM940 = 6,
    CrossCheck = 7,
    TrimTrac = 8,
    PL420 = 9,
    PL421 = 10,
    TM3000 = 11,
    TAP66 = 12,
    SNM451 = 13,
    PL431 = 14,
    DCM300 = 15,
    PL641 = 16,
    PLE641 = 17,
    PLE641PLUSPL631 = 18,
    PLE631 = 19,
    PL631 = 20,
    PL241 = 21,
    PL231 = 22,
    BasicVirtualDevice = 23,
    MTHYPHEN10 = 24,
    XT5060 = 25,
    XT4860 = 26,
    TTUSeries = 27,
    XT2000 = 28,
    MTGModularGatewayHYPHENMotorEngine = 29,
    MTGModularGatewayHYPHENElectricEngine = 30,
    MCHYPHEN3 = 31,
    XT6540 = 33,
    XT65401 = 34,
    XT65402 = 35,
    THREEPDATA = 36,
    PL131 = 37,
    PL141 = 38,
    PL440 = 39,
    PLE601 = 40,
    PL161 = 41,
    PL240 = 42,
    PL542 = 43,
    PLE642 = 44,
    PLE742 = 45,
    SNM941 = 46
  }

  // todo check if these are the same in NG
  public enum SiteTypeEnumCGC
  {
    Generic = 0,
    Project = 1,
    Borrow = 2,
    Waste = 3,
    AvoidanceZone = 4,
    Stockpile = 5,
    CutZone = 6,
    FillZone = 7,
    Import = 8,
    Export = 9,
    Landfill = 10
  }
 
}
