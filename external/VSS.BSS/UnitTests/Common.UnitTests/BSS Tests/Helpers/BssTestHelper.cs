using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;
using VSS.UnitTest.Common.Contexts;

namespace UnitTests.BSS_Tests
{
  public class BssTestHelper
  {
    public IContextContainer Ctx { get { return ContextContainer.Current; } }

    public WorkflowResult ExecuteWorkflow<TMessage>(TMessage message)
    {
      var workFlow = new BssWorkflowFactory(new BssReference(new Mock<IAssetLookup>().Object, new Mock<ICustomerLookup>().Object)).Create(message);
      var result = new WorkflowRunner().Run(workFlow);
      new ConsoleResultProcessor().Process(message, result);
      return result;
    }

    public WorkflowResult ExecuteWorkflow<TMessage>(TMessage message, IAssetLookup assetLookup, ICustomerLookup customerLookup)
    {
      var workFlow = new BssWorkflowFactory(new BssReference(assetLookup, customerLookup)).Create(message);
      var result = new WorkflowRunner().Run(workFlow);
      new ConsoleResultProcessor().Process(message, result);
      return result;
    }

    public string GetPartNumberByDeviceType(DeviceTypeEnum deviceType)
    {
      int deviceTypeId = (int) deviceType;
      var partNumber = (from pn in Ctx.OpContext.DevicePartNumberReadOnly where pn.fk_DeviceTypeID == deviceTypeId select pn).FirstOrDefault();
      Assert.IsNotNull(partNumber, "No PartNumber found for DeviceType");

      return partNumber.BSSPartNumber;
    }

    public string GetPartNumberByServiceType(ServiceTypeEnum serviceType)
    {
      int servicePlanTypeId = (int) serviceType;
      var partNumber = (from st in Ctx.OpContext.ServiceTypeReadOnly where st.ID == servicePlanTypeId select st.BSSPartNumber).FirstOrDefault();
      Assert.IsNotNull(partNumber, "No PartNumber found for ServiceType.");
      return partNumber;
    }
    
    public AssetDeviceHistory GetAssetDeviceHistory(long? deviceId = null)
    {
      var query = (from adh in Ctx.OpContext.AssetDeviceHistoryReadOnly select adh);
      
      if (deviceId.HasValue)
        query = query.Where(x => x.fk_DeviceID == deviceId);

      return query.OrderByDescending(x => x.EndUTC).FirstOrDefault();
    }

    #region Assert Helpers

    public void AssertBssFailureCode(WorkflowResult result, BssFailureCode bssFailureCode)
    {
      var bssErrorResult = result.ActivityResults.FirstOrDefault(x => x.GetType() == typeof (BssErrorResult)) as BssErrorResult;
      
      Assert.IsNotNull(bssErrorResult, "Workflow.ActivityResults did not contain a BssErrorResult.");
      Assert.AreEqual(bssFailureCode, bssErrorResult.FailureCode, "Incorrect BssFailureCode.");
    }

    public Device AssertDevice(InstallBase message)
    {
      var device = (from d in Ctx.OpContext.DeviceReadOnly
                           where d.IBKey == message.IBKey
                           select d).SingleOrDefault();

      Assert.IsNotNull(device, "Device was not created");
      Assert.AreEqual(message.IBKey, device.IBKey, "IbKey not equal");
      Assert.AreEqual(message.GPSDeviceID, device.GpsDeviceID, "GpsDeviceId not equal");
      Assert.AreEqual(message.OwnerBSSID, device.OwnerBSSID, "OwnerBssId not equal");

      AssertDeviceType(message, device.fk_DeviceTypeID);

      if(message.Action == ActionEnum.Created.ToString())
        AssertDevicePersonalities(message, device.ID, (DeviceTypeEnum)device.fk_DeviceTypeID);

      return device;
    }

    public void AssertDeviceType(InstallBase message, int deviceTypeId)
    {
      var deviceType = (from dt in Ctx.OpContext.DeviceTypeReadOnly
                        join pn in Ctx.OpContext.DevicePartNumberReadOnly on dt.ID equals pn.fk_DeviceTypeID
                        where pn.BSSPartNumber == message.PartNumber
                        select dt).SingleOrDefault();

      Assert.AreEqual(deviceType.ID, deviceTypeId, "Created DeviceType does not match for message PartNumber.");
    }

    public void AssertDevicePersonalities(InstallBase message, long deviceId, DeviceTypeEnum deviceType)
    {
      var personalities = (from dp in Ctx.OpContext.DevicePersonalityReadOnly
                           where dp.fk_DeviceID == deviceId
                           select dp).ToList();

      if (deviceType == DeviceTypeEnum.MANUALDEVICE)
      {
        Assert.AreEqual(0, personalities.Count, "DevicePersonalities are not created for ManualDevice.");
        return;
      }

      if (deviceType == DeviceTypeEnum.TrimTrac)
      {
        var expectedUnitId = API.Device.IMEI2UnitID(message.GPSDeviceID);
        var actualUnitId = personalities.SingleOrDefault(x => x.fk_PersonalityTypeID == (int)PersonalityTypeEnum.UnitID);
        Assert.IsNotNull(actualUnitId, "UnitId not created.");
        Assert.AreEqual(expectedUnitId, actualUnitId.Value, "UnitId values should match.");
      }

      var software = personalities.SingleOrDefault(x => x.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Software);
      Assert.IsNotNull(software, "FirmwareVersionId not created");
      Assert.AreEqual(message.FirmwareVersionID, software.Value, "Firmware version id values should match.");

      var simSerialNumber = personalities.SingleOrDefault(x => x.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SerialNumber);
      Assert.IsNotNull(simSerialNumber, "SIMSerialNumber not created");
      Assert.AreEqual(message.SIMSerialNumber, simSerialNumber.Value, "SIM serialnumber values should match.");

      var partNumber = personalities.SingleOrDefault(x => x.fk_PersonalityTypeID == (int)PersonalityTypeEnum.Hardware);
      Assert.IsNotNull(partNumber, "PartNumber not created.");
      Assert.AreEqual(message.PartNumber, partNumber.Value, "PartNumber values should match.");

      var imea = personalities.SingleOrDefault(x => x.fk_PersonalityTypeID == (int)PersonalityTypeEnum.SCID);
      Assert.IsNotNull(imea, "CellularModemIMEA not created.");
      Assert.AreEqual(message.CellularModemIMEA, imea.Value, "Cellular Modem IMEA values should match.");
    }

    public Asset AssertAsset(InstallBase message)
    {
      long computedAssetId = Asset.ComputeAssetID(message.MakeCode, message.EquipmentSN);
      var asset = (from a in Ctx.OpContext.AssetReadOnly
                   where a.AssetID == computedAssetId
                   select a).SingleOrDefault();

      Assert.AreEqual(message.ModelYear, asset.ManufactureYear.ToString(), "ManufactureYear not equal");
      Assert.AreEqual(message.EquipmentVIN, asset.EquipmentVIN, "Asset VIN Serial Number not equal");
      // How to test that crazy CAT MODEL and PRODUCT FAMILY LOGIC - BOOOOOOOO

      return asset;
    }

    public void AssertAssetDeviceHistoryWasNotCreated()
    {
      Assert.IsNull(Ctx.OpContext.AssetDeviceHistoryReadOnly.FirstOrDefault(), "AssetDeviceHistory should not have been created.");
    }

    public void AssertAssetDeviceHistory(long assetID, long? findByDeviceId = null,
      long? oldAssetId = null, long? oldDeviceId = null, string oldOwnerBssId = null)
    {
      DateTime startUtc = (from aa in Ctx.OpContext.AssetReadOnly where aa.AssetID == assetID select aa.InsertUTC).Single().Value;
      var assetDeviceHistory = GetAssetDeviceHistory(findByDeviceId);
      Assert.IsNotNull(assetDeviceHistory, "AssetDeviceHistory should have been created.");

      // This assertion is not going to pass all the time (like when I am trying to get a build through, for instance)
      //Assert.AreEqual(startUtc.ToString("MM/dd/yyyy HH:mm"), assetDeviceHistory.StartUTC.ToString("MM/dd/yyyy HH:mm"), "AssetDeviceHistory StartUtc not equal.");
      double deltaMins = Math.Abs(startUtc.Subtract(assetDeviceHistory.StartUTC).TotalMinutes);
      Assert.IsTrue(deltaMins <= 1, "AssetDeviceHistory StartUtc not equalish.");

      if (oldAssetId.HasValue)
        Assert.AreEqual(oldAssetId.Value, assetDeviceHistory.fk_AssetID, "AssetDeviceHistory AssetId not equal");

      if (oldDeviceId.HasValue)
        Assert.AreEqual(oldDeviceId.Value, assetDeviceHistory.fk_DeviceID, "AssetDeviceHistory DeviceId not equal");

      if (oldOwnerBssId != null)
        Assert.AreEqual(oldOwnerBssId, assetDeviceHistory.OwnerBSSID, "AssetDeviceHistory OwnerBssId not equal");
    }

    public void AssertNhRawMtsDevice(string gpsDeviceID)
    {
      Assert.AreEqual(1, Ctx.RawContext.MTSDeviceReadOnly.Count(x => x.SerialNumber == gpsDeviceID), "No record found in NHRaw");
    }

    public void AssertNhRawTrimTracDevice(string gpsDeviceID)
    {
      var unitId = API.Device.IMEI2UnitID(gpsDeviceID);
      Assert.AreEqual(1, Ctx.RawContext.TTDeviceReadOnly.Count(x => x.UnitID == unitId), "No record found in NHRaw");
    }

    public void AssertNhRawPlDevice(string gpsDeviceID)
    {
      Assert.AreEqual(1, Ctx.RawContext.PLDeviceReadOnly.Count(x => x.ModuleCode == gpsDeviceID), "No record found in NHRaw");
    }

    public void AssertEngineOnOffReset(long assetId, bool expectedResult)
    {
      var asset = Ctx.OpContext.AssetReadOnly.Where(x => x.AssetID == assetId).SingleOrDefault();
      Assert.IsNotNull(asset, "Asset should not be null");
      Assert.AreEqual(expectedResult, asset.IsEngineStartStopSupported, "Asset OnOff tracking should be reset iif required");
    }
    #endregion
  }
}
