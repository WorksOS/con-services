using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon.Bss
{
  public class DeviceReplacementSwappedWorkflow : Workflow
  {
    public DeviceAssetContext Context { get { return Inputs.Get<DeviceAssetContext>(); } }

    public DeviceReplacementSwappedWorkflow(Inputs inputs) : base(inputs)
    {
      Do(new Validate<DeviceReplacement>(
          new DeviceReplacementDataContractValidator()));

      Do(new MapDeviceReplacementToDeviceAssetContext());

      Do(new Validate<DeviceAssetContext>(
        new DeviceAssetContextValidator(),
        new DeviceAssetContextSwappedValidator()));

      TransactionStart();

      Do(new DeviceSwapRecordAssetHistory(),
        new ServiceViewTransfer(),
        new DeviceAssetSwap());

      TransactionCommit();
    }
  }
}
