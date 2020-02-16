using System;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InstallBaseCreatedWorkflow : Workflow
  {
    protected AssetDeviceContext Context { get { return Inputs.Get<AssetDeviceContext>(); } }

    public InstallBaseCreatedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<InstallBase>(
         new InstallBaseDataContractValidator(),
         new InstallBaseValidator(),
         new MakeCodeValidator()));

      Do(new MapInstallBaseToAssetDeviceContext());

      Do(new Validate<AssetDeviceContext>(
         new AssetDeviceContextValidator(),
         new AssetDeviceContextCreateValidator()));

      TransactionStart();
      
      //update the asset service views and asset device history if it is a device replacement
      If(() => Context.IsDeviceReplacement())
        .ThenDo(new ServiceViewTerminateForAsset(),
                new AssetRemoveDeviceHistory(),
                new AssetRemoveDevice());

      //update the asset device history if the asset exists and device doesn't exist
      If(() => !Context.IsDeviceReplacement() && Context.Asset.Exists && Context.Asset.DeviceId != 0 && !Context.Device.Exists)
        .ThenDo(new AssetRemoveDeviceHistory(), new AssetRemoveDevice());

      // Create the device and device personalities
      Do(new DeviceCreate(),
         new DevicePersonalityCreate());

      // Update or create the Asset.
      If(() => Context.Asset.Exists)
        .ThenDo(new AssetUpdate())
        // Execute AssetAliasSave whenever we create an asset.
        .ElseDo(new AssetCreate(),
          new AssetAddReference());

      TransactionCommit();
    }
  }
}
