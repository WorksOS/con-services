using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class InstallBaseUpdatedMergeWorkflow : Workflow
  {
    public InstallBaseUpdatedMergeWorkflow(Inputs inputs) : base(inputs)
    {

      Do(new Validate<InstallBase>(
         new InstallBaseDataContractValidator(),
         new InstallBaseValidator(),
         new MakeCodeValidator()));

      Do(new MapInstallBaseToAssetDeviceContext());

      Do(new Validate<AssetDeviceContext>(
         new AssetDeviceContextValidator(),
         new AssetDeviceContextUpdatedMergeValidator()));

      TransactionStart();

      Do(new DeviceTransferOwnershipHistory(),
         new DeviceTransferOwnership(),
         new ServiceViewManagementForMergeTransfer());

      TransactionCommit();
    }
  }
}
