using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InstallBaseUpdatedWorkflow : Workflow
  {
    public AssetDeviceContext Context { get { return Inputs.Get<AssetDeviceContext>(); } }

    public InstallBaseUpdatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<InstallBase>(
         new InstallBaseDataContractValidator(),
         new InstallBaseValidator(),
         new MakeCodeValidator()));

      Do(new MapInstallBaseToAssetDeviceContext());

      Do(new Validate<AssetDeviceContext>(
         new AssetDeviceContextValidator(),
         new AssetDeviceContextUpdateValidator()));

      TransactionStart();

      // If we are going to transfer ownership
      // create a history record for the old owner.
      If(() => Context.IsOwnershipTransfer())
        .ThenDo(new DeviceTransferOwnershipHistory());

      // If we are going to remove the IBDevice from the 
      // Asset it is currently installed on
      // create a history record for the old Asset/Device association.
      If(() => (Context.IsDeviceTransfer() || Context.IsDeviceReplacement()) && Context.Device.AssetExists)
        .ThenDo(new DeviceRemoveFromOldAssetHistory());

      // If we are goint remove the Device the
      // IBAsset currently has installed on it
      // create a history record for the old Asset/Device association.
      If(() => (Context.IsDeviceTransfer() || Context.IsDeviceReplacement()) && Context.Asset.DeviceExists)
        .ThenDo(new AssetRemoveDeviceHistory());
      
      // If we are changing the ownership or replacing the device
      // we need to terminate the Service Views.
      // Note: Transferring active Devices is not allowed.
      If(() => Context.IsOwnershipTransfer() || Context.IsDeviceReplacement())
        .ThenDo(new ServiceViewTerminateForAsset());

      // Transfer the ownership
      If(() => Context.IsOwnershipTransfer())
        .ThenDo(new DeviceTransferOwnership());

      // If the IBDevice is currently installed on an asset
      // remove it here.
      If(() => (Context.IsDeviceTransfer() || Context.IsDeviceReplacement()) && Context.Device.AssetExists)
        .ThenDo(new DeviceRemoveFromOldAsset());

      // If the IBAsset has a device currently installed on it
      // remove it here.
      If(() => (Context.IsDeviceTransfer() || Context.IsDeviceReplacement()) && Context.Asset.DeviceExists)
        .ThenDo(new AssetRemoveDevice());

      If(() => Context.Asset.Exists)
        .ThenDo(new AssetUpdate())
        .ElseDo(new AssetCreate(),
          new AssetAddReference());

      // We need to recreate the Service Views for the new owner
      // but only if it is an Ownership Transfer
      // Creation of the Service Views for a Device Replacement
      // will happen in Device Replacement workflow.
      If(() => Context.IsOwnershipTransfer() && !(Context.IsDeviceReplacement()))
        .ThenDo(new ServiceViewCreate());

      TransactionCommit();
    }
  }
}
