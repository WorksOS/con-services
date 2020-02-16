using System;

namespace VSS.Hosted.VLCommon.Bss
{
  public class Services
  {
    public static Func<IBssServiceViewService> ServiceViews = () => new BssServiceViewService();

    public static Func<IBssCustomerService> Customers = () => new BssCustomerService();

    public static Func<IBssDeviceService> Devices = () => new BssDeviceService();

    public static Func<IBssAssetService> Assets = () => new BssAssetService();

    public static Func<IBssAssetDeviceHistoryService> AssetDeviceHistory = () => new BssAssetDeviceHistoryService();

    public static Func<IBssPLOTAService> OTAServices = () => new BssPLOTAService();
  }
}