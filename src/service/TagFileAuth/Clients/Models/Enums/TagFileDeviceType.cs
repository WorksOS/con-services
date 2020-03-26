namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums
{
  // This is the deviceType sent from Raptor/Trex which has been extracted from the tag-file.
  // Use this and the serialNumber suffix in the CWSDeviceTypeEnum to map it to a WorksManager (cws) device type.
  public enum TagFileDeviceTypeEnum
  {
    ManualImport = 0, // this is a Raptor-specific kludge to indicate ManualImport (aka MANUALDEVICE)
    SNM940 = 6,
    SNM941 = 46, 
    EC520 = 56 
  }
}
