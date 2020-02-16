using System.Xml.Serialization;

namespace VSS.Nighthawk.ExternalDataTypes.Enumerations
{
  // This is a copy of VSS.Hosted.VLCommon.DeviceTypeEnum with:
  // - XmlEnum attributes facilitating a "friendlier" schema and obscuring internal enum values from external callers
  public enum DeviceTypeEnum
  {
    [XmlEnum("None")]
    MANUALDEVICE = 0,
    [XmlEnum("PL121")]
    PL121 = 1,
    [XmlEnum("PL321")]
    PL321 = 2,
    [XmlEnum("PL522")]
    Series522 = 3,
    [XmlEnum("523")]
    Series523 = 4,
    [XmlEnum("PL521")]
    Series521 = 5,  // This device type never went to production
    [XmlEnum("SNM940")]
    SNM940 = 6,
    [XmlEnum("CrossCheck")]
    CrossCheck = 7,
    [XmlEnum("TrimTrac")]
    TrimTrac = 8,
    [XmlEnum("PL420")]
    PL420 = 9,
    [XmlEnum("PL421")]
    PL421 = 10,
    [XmlEnum("TM3000")]
    TM3000 = 11,
    [XmlEnum("TAP66")]
    TAP66 = 12,
    [XmlEnum("SNM451")]
    SNM451 = 13,
    [XmlEnum("PL431")]
    PL431 = 14,  // This device type never went to production
    [XmlEnum("DCM300")]
    DCM300 = 15,
    [XmlEnum("PL641")]
    PL641 = 16,
    [XmlEnum("PLE641")]
    PLE641 = 17,
    [XmlEnum("PLE641+PL631")]
    PLE641PLUSPL631 = 18, // Going forward device types with symbols like '+' and '-' will be replaced with equivalent English alphabets
    [XmlEnum("PLE631")]
    PLE631 = 19,
    [XmlEnum("PL631")]
    PL631 = 20,
    [XmlEnum("PL241")]
    PL241 = 21,
    [XmlEnum("PL231")]
    PL231 = 22,
    [XmlEnum("Basic Virtual Device")]
    BasicVirtualDevice = 23,
    [XmlEnum("MT - 10")]
    MTHYPHEN10 = 24,
    XT5060 = 25,
    XT4860 = 26,
    [XmlEnum("TTU Series")]
    TTUSeries = 27,
    XT2000 = 28,
    [XmlEnum("MTG Modular Gateway - Motor Engine")]
    MTGModularGatewayHYPHENMotorEngine = 29,
    [XmlEnum("MTG Modular Gateway - Electric Engine")]
    MTGModularGatewayHYPHENElectricEngine = 30,
    [XmlEnum("MC - 3")]
    MCHYPHEN3 = 31,
    XT6540 = 33,
    XT65401 = 34,
    XT65402 = 35,
    [XmlEnum("3PDATA")]
    THREEPDATA = 36,
    [XmlEnum("PL131")]
    PL131 = 37,
    [XmlEnum("PL141")]
    PL141 = 38,
    [XmlEnum("PL440")]
    PL440 = 39,
    [XmlEnum("PL240")]
    PL240 = 42,
    [XmlEnum("PL161")]
    PL161 = 41,
    [XmlEnum("PL542")]
    PL542 = 43,
    [XmlEnum("PLE642")]
    PLE642 = 44,
    [XmlEnum("PLE742")]
    PLE742 = 45,
    [XmlEnum("SNM941")]
    SNM941 = 46,
    [XmlEnum("PL240B")]
    PL240B = 47,
    [XmlEnum("EC520")]
    EC520 = 56


    }
}
