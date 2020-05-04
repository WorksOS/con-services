namespace VSS.Common.Abstractions.Clients.CWS.Enums
{
  // device needs to be linked between WM - TCC - device
  //   process involves customer
  //       1) add device to WM by serialNumber (normally already in profileX)
  //       2) supply a deviceName
  //       3) on the actual device, type in that deviceName to link with WM
  public enum TCCDeviceStatusEnum
  {
    Unknown = 0,  // null
    Registered,   // cws passes upper camel Registered
    Pending,      // cws passes upper camel Pending
  }
}
